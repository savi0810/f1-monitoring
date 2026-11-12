import { computed, onScopeDispose, ref } from 'vue'
import { useRouter } from 'vue-router'
import { hasValidToken } from '@/features/auth/tokenStorage'
import { createSubscription, deleteSubscription, getSubscriptions } from '../api/subscriptionsApi'
import type { DriverSubscriptionDto } from '../types'

export function useDriverSubscriptions() {
  const router = useRouter()
  const subscriptions = ref<DriverSubscriptionDto[]>([])
  const subscribedDriverIds = computed(() =>
    new Set(subscriptions.value.map(subscription => subscription.driverId)),
  )
  const pendingDriverIds = ref(new Set<string>())
  const loading = ref(false)
  const loaded = ref(false)
  const error = ref('')
  const mutationControllers = new Map<string, AbortController>()
  let loadController: AbortController | undefined
  let disposed = false

  async function reportError(cause: unknown, fallback: string) {
    if (disposed) return

    error.value = cause instanceof Error ? cause.message : fallback
    if (!hasValidToken()) await router.replace({ name: 'login' })
  }

  async function loadSubscriptions() {
    if (disposed || pendingDriverIds.value.size > 0) return

    loadController?.abort()
    const request = new AbortController()
    loadController = request
    loading.value = true
    error.value = ''

    try {
      const result = await getSubscriptions(request.signal)
      if (!request.signal.aborted) {
        subscriptions.value = result
        loaded.value = true
      }
    } catch (cause) {
      if (!request.signal.aborted) {
        await reportError(cause, 'Не удалось загрузить подписки.')
      }
    } finally {
      if (!request.signal.aborted) loading.value = false
    }
  }

  async function changeSubscription(driverId: string, action: 'subscribe' | 'unsubscribe') {
    if (disposed || !loaded.value || pendingDriverIds.value.has(driverId)) return

    loadController?.abort()
    loading.value = false

    const request = new AbortController()
    mutationControllers.set(driverId, request)
    pendingDriverIds.value.add(driverId)
    error.value = ''

    try {
      const created = action === 'subscribe'
        ? await createSubscription({ driverId }, request.signal)
        : undefined

      if (action === 'unsubscribe') {
        await deleteSubscription(driverId, request.signal)
      }

      if (!request.signal.aborted) {
        subscriptions.value = subscriptions.value.filter(
          subscription => subscription.driverId !== driverId,
        )
        if (created) subscriptions.value.push(created)
      }
    } catch (cause) {
      if (!request.signal.aborted) {
        await reportError(
          cause,
          action === 'subscribe'
            ? 'Не удалось создать подписку.'
            : 'Не удалось удалить подписку.',
        )
      }
    } finally {
      mutationControllers.delete(driverId)
      if (!disposed) pendingDriverIds.value.delete(driverId)
    }
  }

  function subscribeDriver(driverId: string) {
    return changeSubscription(driverId, 'subscribe')
  }

  function unsubscribeDriver(driverId: string) {
    return changeSubscription(driverId, 'unsubscribe')
  }

  onScopeDispose(() => {
    disposed = true
    loadController?.abort()
    for (const controller of mutationControllers.values()) controller.abort()
    mutationControllers.clear()
  })

  return {
    subscriptions,
    subscribedDriverIds,
    pendingDriverIds,
    loading,
    loaded,
    error,
    loadSubscriptions,
    subscribeDriver,
    unsubscribeDriver,
  }
}
