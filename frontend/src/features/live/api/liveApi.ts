import { clearToken, loadToken } from '@/features/auth/tokenStorage'
import type { CurrentLiveDto } from '../types'

export async function getCurrentLive(signal?: AbortSignal): Promise<CurrentLiveDto> {
  const token = loadToken()
  if (!token) throw new Error('Срок авторизации истёк. Войдите снова.')

  const response = await fetch('/api/live/current', {
    headers: { Authorization: `Bearer ${token}` },
    signal,
  })

  if (response.status === 401) {
    clearToken()
    throw new Error('Срок авторизации истёк. Войдите снова.')
  }

  if (!response.ok) throw new Error('Не удалось загрузить текущую гонку.')

  return await response.json() as CurrentLiveDto
}
