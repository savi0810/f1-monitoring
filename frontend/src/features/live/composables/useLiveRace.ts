import { HubConnectionBuilder } from '@microsoft/signalr'
import { onScopeDispose, ref } from 'vue'
import { useRouter } from 'vue-router'
import { hasValidToken, loadToken } from '@/features/auth/tokenStorage'
import { getCurrentLive } from '../api/liveApi'
import type { LiveRaceSnapshot } from '../types'

export function useLiveRace() {
  const router = useRouter()
  const snapshot = ref<LiveRaceSnapshot | null>(null)
  const sourceConnected = ref(false)
  const loading = ref(false)
  const error = ref('')
  const connection = new HubConnectionBuilder()
    .withUrl('/livehub', {
      accessTokenFactory: () => loadToken() ?? '',
    })
    .withAutomaticReconnect()
    .build()
  let controller: AbortController | undefined

  async function loadCurrent() {
    controller?.abort()
    const request = new AbortController()
    controller = request
    loading.value = true
    error.value = ''

    try {
      const result = await getCurrentLive(request.signal)
      if (!request.signal.aborted) {
        snapshot.value = result.snapshot
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

  onScopeDispose(() => {
    controller?.abort()
    void connection.stop()
  })

  return { snapshot, sourceConnected, loading, error, loadCurrent }
}
