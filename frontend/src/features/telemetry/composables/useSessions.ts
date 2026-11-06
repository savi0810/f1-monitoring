import { onScopeDispose, ref } from 'vue'
import { getSessions } from '../api/telemetryApi'
import type { Session, SessionFilters } from '../types'

export function useSessions() {
  const sessions = ref<Session[]>([])
  const loading = ref(false)
  const error = ref('')
  let controller: AbortController | undefined

  async function loadSessions(filters: SessionFilters = {}) {
    controller?.abort()
    const request = new AbortController()
    controller = request
    loading.value = true
    error.value = ''

    try {
      const result = await getSessions(filters, request.signal)
      if (!request.signal.aborted) sessions.value = result
    } catch (cause) {
      if (!request.signal.aborted) {
        sessions.value = []
        error.value = cause instanceof Error ? cause.message : 'Не удалось загрузить каталог сессий.'
      }
    } finally {
      if (!request.signal.aborted) loading.value = false
    }
  }

  onScopeDispose(() => controller?.abort())

  return { sessions, loading, error, loadSessions }
}
