import { clearToken, loadToken } from '@/features/auth/tokenStorage'
import type { DriverSubscriptionDto } from '../types'

export async function getSubscriptions(signal?: AbortSignal): Promise<DriverSubscriptionDto[]> {
  const token = loadToken()
  if (!token) throw new Error('Срок авторизации истёк. Войдите снова.')

  const response = await fetch('/api/alerts/subscriptions', {
    headers: { Authorization: `Bearer ${token}` },
    signal,
  })

  if (response.status === 401) {
    clearToken()
    throw new Error('Срок авторизации истёк. Войдите снова.')
  }

  if (!response.ok) throw new Error('Не удалось загрузить подписки.')

  return await response.json() as DriverSubscriptionDto[]
}
