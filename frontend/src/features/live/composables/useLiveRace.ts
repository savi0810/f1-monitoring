import { HttpError, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { computed, onScopeDispose, ref } from 'vue'
import { useRouter } from 'vue-router'
import { clearToken, hasValidToken, loadToken } from '@/features/auth/tokenStorage'
import { getCurrentLive } from '../api/liveApi'
import type { LiveRaceSnapshot, SourceUpdated } from '../types'

function parseUpdateInterval(value: string | null): number {
  const match = value?.match(/^(-)?(?:(\d+)\.)?(\d{2}):(\d{2}):(\d{2}(?:\.\d{1,7})?)$/)
  if (!match) return 0

  const [, sign, days, hours, minutes, seconds] = match
  const milliseconds = (
    Number(days ?? 0) * 86400
    + Number(hours) * 3600
    + Number(minutes) * 60
    + Number(seconds)
  ) * 1000

  return sign ? -milliseconds : milliseconds
}

export function useLiveRace() {
  const router = useRouter()
  const snapshot = ref<LiveRaceSnapshot | null>(null)
  const now = ref(Date.now())
  const lastSourceObservedAt = ref<number | null>(null)
  const sourceConnected = computed(() => {
    if (!snapshot.value || lastSourceObservedAt.value === null) return false

    const interval = parseUpdateInterval(snapshot.value.expectedUpdateInterval)
    const maxAge = Math.max(30_000, interval + 30_000)

    return now.value - lastSourceObservedAt.value <= maxAge
  })
  const connectionState = ref(HubConnectionState.Disconnected)
  const loading = ref(false)
  const error = ref('')
  const connection = new HubConnectionBuilder()
    .withUrl('/livehub', {
      accessTokenFactory: () => loadToken() ?? '',
    })
    .withAutomaticReconnect()
    .build()

  let controller: AbortController | undefined
  let disposed = false
  let connecting = false

  const freshnessTimer = setInterval(() => {
    now.value = Date.now()
  }, 1000)

  function observeSource(value: string) {
    const observedAt = Date.parse(value)
    if (!Number.isFinite(observedAt)) return

    lastSourceObservedAt.value = Math.max(
      lastSourceObservedAt.value ?? observedAt,
      observedAt,
    )
    now.value = Date.now()
  }

  connection.on('SourceUpdated', (incoming: SourceUpdated) => {
    if (disposed || incoming.sessionId !== snapshot.value?.sessionId) return

    observeSource(incoming.observedAt)
  })

  connection.on('TimingUpdated', (incoming: LiveRaceSnapshot) => {
    const current = snapshot.value
    if (disposed || !current || incoming.sessionId !== current.sessionId) return

    const observedAt = Date.parse(incoming.observedAt)
    if (!Number.isFinite(observedAt)
      || observedAt <= Date.parse(current.observedAt)) return

    snapshot.value = incoming
    observeSource(incoming.observedAt)
  })

  connection.onreconnecting(() => {
    if (!disposed) connectionState.value = HubConnectionState.Reconnecting
  })

  connection.onreconnected(async () => {
    if (disposed) return

    try {
      await loadCurrent()
      if (disposed) return
      if (error.value) throw new Error(error.value)

      await subscribeSession()
      if (!disposed) connectionState.value = HubConnectionState.Connected
    } catch (cause) {
      await handleConnectionError(cause)
    }
  })

  connection.onclose(() => {
    if (!disposed) connectionState.value = HubConnectionState.Disconnected
  })

  async function loadCurrent() {
    if (disposed) return

    controller?.abort()
    const request = new AbortController()
    controller = request
    loading.value = true
    error.value = ''

    try {
      const result = await getCurrentLive(request.signal)
      if (!request.signal.aborted) {
        const current = snapshot.value
        const incoming = result.snapshot

        if (!current || !incoming || incoming.sessionId !== current.sessionId
          || Date.parse(incoming.observedAt) > Date.parse(current.observedAt)) {
          snapshot.value = incoming
        }

        if (current?.sessionId !== incoming?.sessionId) {
          lastSourceObservedAt.value = null
        }

        if (incoming) observeSource(incoming.observedAt)
      }
    } catch (cause) {
      if (!request.signal.aborted) {
        error.value = cause instanceof Error
          ? cause.message
          : 'Не удалось загрузить текущую гонку.'

        if (!hasValidToken()) await router.replace({ name: 'login' })
      }
    } finally {
      if (!request.signal.aborted) loading.value = false
    }
  }

  async function subscribeSession() {
    if (disposed) return

    const sessionId = snapshot.value?.sessionId
    if (sessionId) await connection.invoke('SubscribeSession', sessionId)
  }

  async function handleConnectionError(cause: unknown) {
    if (disposed) return

    if (cause instanceof HttpError && cause.statusCode === 401) clearToken()

    error.value = cause instanceof Error
      ? cause.message
      : 'Не удалось подключиться к обновлениям гонки.'

    await connection.stop()
    if (disposed) return

    connectionState.value = HubConnectionState.Disconnected
    if (!hasValidToken()) await router.replace({ name: 'login' })
  }

  async function connect() {
    if (disposed || connecting
      || connection.state !== HubConnectionState.Disconnected) return

    connecting = true

    try {
      await loadCurrent()
      if (disposed || error.value) return

      connectionState.value = HubConnectionState.Connecting
      await connection.start()
      if (disposed) return

      await subscribeSession()
      if (!disposed) connectionState.value = HubConnectionState.Connected
    } catch (cause) {
      await handleConnectionError(cause)
    } finally {
      connecting = false
    }
  }

  onScopeDispose(() => {
    disposed = true
    clearInterval(freshnessTimer)
    controller?.abort()
    connection.off('TimingUpdated')
    connection.off('SourceUpdated')
    void connection.stop()
  })

  return {
    snapshot,
    sourceConnected,
    connectionState,
    loading,
    error,
    loadCurrent,
    connect,
  }
}
