import { HttpError, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { onScopeDispose, ref } from 'vue'
import { useRouter } from 'vue-router'
import { clearToken, hasValidToken, loadToken } from '@/features/auth/tokenStorage'
import { getCurrentLive } from '../api/liveApi'
import type { LiveRaceSnapshot } from '../types'

export function useLiveRace() {
  const router = useRouter()
  const snapshot = ref<LiveRaceSnapshot | null>(null)
  const sourceConnected = ref(false)
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

  connection.on('TimingUpdated', (incoming: LiveRaceSnapshot) => {
    const current = snapshot.value
    if (disposed || !current || incoming.sessionId !== current.sessionId) return

    const observedAt = Date.parse(incoming.observedAt)
    if (!Number.isFinite(observedAt)
      || observedAt <= Date.parse(current.observedAt)) return

    snapshot.value = incoming
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

        sourceConnected.value = result.isConnected
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
    controller?.abort()
    connection.off('TimingUpdated')
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
