import { clearToken, loadToken } from '@/features/auth/tokenStorage'
import type { CreateDriverSubscriptionDto, DriverSubscriptionDto } from '../types'

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

export async function createSubscription(request: CreateDriverSubscriptionDto, signal?: AbortSignal): Promise<DriverSubscriptionDto> {
  const token = loadToken()
  if (!token) throw new Error('Срок авторизации истёк. Войдите снова.')

  const response = await fetch('/api/alerts/subscriptions', {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
    signal,
  })

  if (response.status === 401) {
    clearToken()
    throw new Error('Срок авторизации истёк. Войдите снова.')
  }

  if (!response.ok) throw new Error('Не удалось создать подписку.')

  return await response.json() as DriverSubscriptionDto
}

export async function deleteSubscription(driverId: string, signal?: AbortSignal): Promise<void> {
  const token = loadToken()
  if (!token) throw new Error('Срок авторизации истёк. Войдите снова.')

  const query = new URLSearchParams({ driverId })
  const response = await fetch(`/api/alerts/subscriptions?${query}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
    signal,
  })

  if (response.status === 401) {
    clearToken()
    throw new Error('Срок авторизации истёк. Войдите снова.')
  }

  if (!response.ok) throw new Error('Не удалось удалить подписку.')
}
