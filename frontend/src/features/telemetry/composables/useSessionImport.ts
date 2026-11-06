import { onScopeDispose, ref } from 'vue'
import { getImportStatus, getSessionLaps, startImport } from '../api/telemetryApi'
import { ImportStatus } from '../types'
import type { Lap, SessionImport } from '../types'

export function useSessionImport(sessionKey: number) {
  const laps = ref<Lap[]>([])
  const sessionImport = ref<SessionImport | null>(null)
  const loading = ref(false)
  const starting = ref(false)
  const error = ref('')
  let controller: AbortController | undefined
  let timer: ReturnType<typeof setTimeout> | undefined
  let disposed = false

  function clearTimer() {
    if (timer) clearTimeout(timer)
    timer = undefined
  }

  function scheduleRefresh() {
    clearTimer()
    if (sessionImport.value?.status === ImportStatus.Pending
      || sessionImport.value?.status === ImportStatus.Processing) {
      timer = setTimeout(() => void refresh(true), 3000)
    }
  }

  async function refresh(background = false) {
    clearTimer()
    controller?.abort()
    const request = new AbortController()
    controller = request
    if (!background) loading.value = true
    error.value = ''

    try {
      let status: SessionImport | null
      let savedLaps: Lap[] | null

      if (background) {
        status = await getImportStatus(sessionKey, request.signal)
        const active = status?.status === ImportStatus.Pending
          || status?.status === ImportStatus.Processing
        savedLaps = active ? null : await getSessionLaps(sessionKey, request.signal)
      } else {
        ;[status, savedLaps] = await Promise.all([
          getImportStatus(sessionKey, request.signal),
          getSessionLaps(sessionKey, request.signal),
        ])
      }

      if (request.signal.aborted || disposed) return
      sessionImport.value = status
      if (savedLaps) laps.value = savedLaps
      scheduleRefresh()
    } catch (cause) {
      if (!request.signal.aborted && !disposed) {
        error.value = cause instanceof Error ? cause.message : 'Не удалось загрузить данные сессии.'
      }
    } finally {
      if (!request.signal.aborted && !disposed) loading.value = false
    }
  }

  async function importSession() {
    clearTimer()
    starting.value = true
    error.value = ''

    try {
      const result = await startImport(sessionKey)
      if (disposed) return
      sessionImport.value = result

      if (result.status === ImportStatus.Completed || result.status === ImportStatus.Empty) {
        await refresh()
      } else {
        scheduleRefresh()
      }
    } catch (cause) {
      if (!disposed) {
        error.value = cause instanceof Error ? cause.message : 'Не удалось запустить импорт.'
      }
    } finally {
      if (!disposed) starting.value = false
    }
  }

  onScopeDispose(() => {
    disposed = true
    clearTimer()
    controller?.abort()
  })

  return { laps, sessionImport, loading, starting, error, refresh, importSession }
}
