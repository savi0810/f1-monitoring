import { onScopeDispose, ref } from 'vue'
import { getSessionResults } from '../api/telemetryApi'
import type { SessionResult } from '../types'

export function useSessionResults(sessionKey: number) {
  const results = ref<SessionResult[]>([])
  const loading = ref(false)
  const error = ref('')
  let controller: AbortController | undefined

  async function loadResults() {
    controller?.abort()
    const request = new AbortController()
    controller = request
    loading.value = true
    error.value = ''

    try {
      const result = await getSessionResults(sessionKey, request.signal)
      if (!request.signal.aborted) results.value = result
    } catch (cause) {
      if (!request.signal.aborted) {
        error.value = cause instanceof Error ? cause.message : 'Не удалось загрузить результаты.'
      }
    } finally {
      if (!request.signal.aborted) loading.value = false
    }
  }

  onScopeDispose(() => controller?.abort())

  return { results, loading, error, loadResults }
}
