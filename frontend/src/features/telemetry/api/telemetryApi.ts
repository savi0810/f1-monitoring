import { clearToken, loadToken } from '@/features/auth/tokenStorage'
import type { Lap, Session, SessionFilters, SessionImport } from '../types'

const baseUrl = '/api/telemetry/sessions'

async function readJson<T>(response: Response, fallback: string): Promise<T> {
  if (!response.ok) {
    if (response.status === 401) {
      clearToken()
      throw new Error('Срок авторизации истёк. Войдите снова.')
    }

    if (response.status === 404) throw new Error('Сессия не найдена.')
    if (response.status === 409) throw new Error('Импорт этой сессии недоступен.')
    throw new Error(fallback)
  }

  return await response.json() as T
}

export async function getSessions(filters: SessionFilters = {}, signal?: AbortSignal): Promise<Session[]> {
  const query = new URLSearchParams()
  if (filters.year !== undefined) query.set('year', String(filters.year))
  if (filters.type) query.set('type', filters.type)

  const suffix = query.toString()
  const response = await fetch(`${baseUrl}${suffix ? `?${suffix}` : ''}`, { signal })
  return readJson<Session[]>(response, 'Не удалось загрузить каталог сессий.')
}

export async function getSessionLaps(sessionKey: number, signal?: AbortSignal): Promise<Lap[]> {
  const response = await fetch(`${baseUrl}/${sessionKey}/laps`, { signal })
  return readJson<Lap[]>(response, 'Не удалось загрузить круги.')
}

export async function getImportStatus(sessionKey: number, signal?: AbortSignal): Promise<SessionImport | null> {
  const response = await fetch(`${baseUrl}/${sessionKey}/import`, { signal })
  if (response.status === 204) return null
  return readJson<SessionImport | null>(response, 'Не удалось загрузить статус импорта.')
}

export async function startImport(sessionKey: number): Promise<SessionImport> {
  const token = loadToken()
  if (!token) throw new Error('Срок авторизации истёк. Войдите снова.')

  const response = await fetch(`${baseUrl}/${sessionKey}/import`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  })

  if (response.status === 503) {
    const body = await response.json().catch(() => null) as Partial<SessionImport> | null
    if (body?.sessionKey === sessionKey && typeof body.importId === 'string'
      && body.status === 4 && (body.error === null || typeof body.error === 'string')) {
      return {
        sessionKey: body.sessionKey,
        importId: body.importId,
        status: body.status,
        error: body.error,
      }
    }

    throw new Error('Сервис телеметрии временно недоступен.')
  }

  return readJson<SessionImport>(response, 'Не удалось запустить импорт.')
}
