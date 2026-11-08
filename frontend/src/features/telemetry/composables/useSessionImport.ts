import { onScopeDispose, ref } from 'vue'
import { getImportStatus, startImport } from '../api/telemetryApi'
import { ImportStatus } from '../types'
import type { SessionImport } from '../types'

export function useSessionImport(sessionKey: number, onCompleted: () => void) {
  const sessionImport = ref<SessionImport | null>(null)
  const statusLoaded = ref(false)
  const loading = ref(false)
  const starting = ref(false)
  const error = ref('')
  let controller: AbortController | undefined
  let startController: AbortController | undefined
  let timer: ReturnType<typeof setTimeout> | undefined
  let disposed = false

  function clearTimer() {
    if (timer) clearTimeout(timer)
    timer = undefined
  }

  function isActive(status: SessionImport | null) {
    return status?.status === ImportStatus.Pending || status?.status === ImportStatus.Processing
  }

  function isFinished(status: SessionImport | null) {
    return status?.status === ImportStatus.Completed || status?.status === ImportStatus.Empty
  }

  function scheduleRefresh() {
    clearTimer()
    if (isActive(sessionImport.value)) {
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
      const previousStatus = sessionImport.value
      const status = await getImportStatus(sessionKey, request.signal)
      if (request.signal.aborted || disposed) return

      sessionImport.value = status
      statusLoaded.value = true
      if (isActive(previousStatus) && isFinished(status)) onCompleted()
      scheduleRefresh()
    } catch (cause) {
      if (!request.signal.aborted && !disposed) {
        error.value = cause instanceof Error ? cause.message : 'Не удалось загрузить статус импорта.'
        scheduleRefresh()
      }
    } finally {
      if (!request.signal.aborted && !disposed) loading.value = false
    }
  }

  async function importSession() {
    clearTimer()
    controller?.abort()
    loading.value = false
    startController?.abort()
    const request = new AbortController()
    startController = request
    starting.value = true
    error.value = ''

    try {
      const result = await startImport(sessionKey, request.signal)
      if (request.signal.aborted || disposed) return
      sessionImport.value = result
      statusLoaded.value = true

      if (isFinished(result)) onCompleted()
      else scheduleRefresh()
    } catch (cause) {
      if (!request.signal.aborted && !disposed) {
        error.value = cause instanceof Error ? cause.message : 'Не удалось запустить импорт.'
      }
    } finally {
      if (!request.signal.aborted && !disposed) starting.value = false
    }
  }

  onScopeDispose(() => {
    disposed = true
    clearTimer()
    controller?.abort()
    startController?.abort()
  })

  return { sessionImport, statusLoaded, loading, starting, error, refresh, importSession }
}
