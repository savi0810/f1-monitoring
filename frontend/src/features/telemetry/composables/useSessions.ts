import { onScopeDispose, ref } from 'vue'
import { getSessions } from '../api/telemetryApi'
import type { SessionCatalogYear, SessionFilters } from '../types'

export function useSessions() {
  const sessions = ref<SessionCatalogYear[]>([])
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
      if (!request.signal.aborted) {
        const now = Date.now()
        sessions.value = result
          .map(year => ({
            ...year,
            meetings: year.meetings
              .map(meeting => ({
                ...meeting,
                sessions: meeting.sessions.filter(session => {
                  const start = Date.parse(session.dateStart)
                  const end = Date.parse(session.dateEnd)
                  return !session.isCancelled && Number.isFinite(start)
                    && Number.isFinite(end) && end <= now
                }),
              }))
              .filter(meeting => meeting.sessions.length > 0),
          }))
          .filter(year => year.meetings.length > 0)
      }
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
