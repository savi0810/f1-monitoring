<script setup lang="ts">
import { Bell } from '@lucide/vue'
import { HubConnectionState } from '@microsoft/signalr'
import { computed, onMounted, onScopeDispose, watch } from 'vue'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet'
import { Spinner } from '@/components/ui/spinner'
import DriverSubscriptions from '@/features/alerts/components/DriverSubscriptions.vue'
import { useDriverSubscriptions } from '@/features/alerts/composables/useDriverSubscriptions'
import LiveStandings from '@/features/live/components/LiveStandings.vue'
import PositionAlerts from '@/features/live/components/PositionAlerts.vue'
import { useLiveRace } from '@/features/live/composables/useLiveRace'

const {
  snapshot,
  alerts,
  dismissAlert,
  sourceConnected,
  connectionState,
  loading,
  error,
  synchronize,
  connect,
} = useLiveRace()

const {
  subscriptions,
  subscribedDriverIds,
  pendingDriverIds,
  loading: subscriptionsLoading,
  loaded: subscriptionsLoaded,
  error: subscriptionsError,
  loadSubscriptions,
  subscribeDriver,
  unsubscribeDriver,
} = useDriverSubscriptions()

const drivers = computed(() => snapshot.value?.drivers ?? [])
const isFinished = computed(() =>
  snapshot.value?.completed === true || snapshot.value?.state === 'post',
)
const canSubscribe = computed(() =>
  snapshot.value?.state === 'in' && !isFinished.value,
)
const busy = computed(() =>
  loading.value
  || connectionState.value === HubConnectionState.Connecting
  || connectionState.value === HubConnectionState.Reconnecting
  || connectionState.value === HubConnectionState.Disconnecting,
)

const connectionLabels: Record<HubConnectionState, string> = {
  [HubConnectionState.Disconnected]: 'Нет соединения',
  [HubConnectionState.Connecting]: 'Подключение',
  [HubConnectionState.Connected]: 'Подключено',
  [HubConnectionState.Disconnecting]: 'Отключение',
  [HubConnectionState.Reconnecting]: 'Восстановление связи',
}

const raceStatus = computed(() => {
  if (!snapshot.value) return 'Ожидание данных о гонке'
  if (isFinished.value) return 'Гонка завершена'
  if (snapshot.value.state === 'in') return 'Гонка идёт'
  if (snapshot.value.state === 'pre') return 'Ожидание старта'
  return 'Статус гонки неизвестен'
})

const sourceStatus = computed(() => {
  if (!snapshot.value) return 'Ожидание данных'
  return sourceConnected.value ? 'Данные свежие' : 'Нет свежих данных'
})

const timeFormatter = new Intl.DateTimeFormat('ru-RU', {
  day: '2-digit',
  month: 'long',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
})

function formatTime(value: string) {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? 'Время неизвестно'
    : timeFormatter.format(date)
}

function retryConnection() {
  return connectionState.value === HubConnectionState.Disconnected
    ? connect()
    : synchronize()
}

function subscribe(driverId: string) {
  if (canSubscribe.value) return subscribeDriver(driverId)
}

function refreshSubscriptions() {
  if (!subscriptionsLoading.value) return loadSubscriptions()
}

const subscriptionsTimer = setInterval(() => {
  void refreshSubscriptions()
}, 30_000)

watch(connectionState, state => {
  if (state === HubConnectionState.Connected) {
    void refreshSubscriptions()
  }
})

watch(
  [() => snapshot.value?.sessionId, isFinished],
  () => { void refreshSubscriptions() },
)

onMounted(() => {
  void refreshSubscriptions()
  void connect()
})

onScopeDispose(() => {
  clearInterval(subscriptionsTimer)
})
</script>

<template>
  <main
    aria-label="Лайв"
    class="mx-auto flex w-full max-w-7xl flex-col gap-6 px-6 py-8"
  >
    <header class="flex flex-wrap items-start justify-between gap-6 border-l-4 border-primary pl-4">
      <div class="flex min-w-0 flex-col gap-2">
        <h1 class="text-3xl font-semibold tracking-tight">
          {{ snapshot?.sessionName || 'Прямая трансляция' }}
        </h1>
        <div class="flex items-center gap-2">
          <Badge :variant="canSubscribe ? 'default' : 'secondary'">
            {{ raceStatus }}
          </Badge>
        </div>
        <p v-if="snapshot" class="text-sm text-muted-foreground">
          Снимок:
          <time :datetime="snapshot.observedAt">
            {{ formatTime(snapshot.observedAt) }}
          </time>
        </p>
      </div>

      <div class="flex shrink-0 flex-col items-end gap-2">
        <Badge
          :variant="connectionState === HubConnectionState.Connected
            ? 'secondary'
            : 'outline'"
        >
          Связь: {{ connectionLabels[connectionState] }}
        </Badge>

        <Badge :variant="sourceConnected ? 'secondary' : 'outline'">
          Источник: {{ sourceStatus }}
        </Badge>

        <Sheet>
          <SheetTrigger as-child>
            <Button type="button">
              <Bell data-icon="inline-start" aria-hidden="true" />
              Уведомления ({{ alerts.length }})
            </Button>
          </SheetTrigger>
          <SheetContent>
            <SheetHeader class="pr-12">
              <SheetTitle>История уведомлений</SheetTitle>
              <SheetDescription>
                Последние пять событий. Закрытие панели сохраняет список.
              </SheetDescription>
            </SheetHeader>
            <div class="min-h-0 flex-1 overflow-y-auto px-4 pb-4">
              <PositionAlerts :alerts="alerts" @dismiss="dismissAlert" />
            </div>
          </SheetContent>
        </Sheet>

        <Button
          v-if="connectionState === HubConnectionState.Disconnected"
          type="button"
          size="sm"
          :disabled="busy"
          @click="retryConnection"
        >
          <Spinner v-if="busy" data-icon="inline-start" aria-hidden="true" />
          Подключиться
        </Button>
      </div>
    </header>

    <Alert v-if="error" variant="destructive">
      <AlertTitle>Не удалось обновить гонку</AlertTitle>
      <AlertDescription class="flex flex-col items-start gap-3">
        <p>{{ error }}</p>
        <Button
          type="button"
          variant="outline"
          size="sm"
          :disabled="busy"
          @click="retryConnection"
        >
          Повторить
        </Button>
      </AlertDescription>
    </Alert>

    <div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-[minmax(0,1fr)_24rem]">
      <LiveStandings
        v-if="snapshot"
        :drivers="drivers"
        :subscribed-driver-ids="subscribedDriverIds"
        :pending-driver-ids="pendingDriverIds"
        :subscriptions-loaded="subscriptionsLoaded"
        :can-subscribe="canSubscribe"
        @subscribe="subscribe"
        @unsubscribe="unsubscribeDriver"
      />

      <Card v-else>
        <CardHeader>
          <CardTitle>Позиции пилотов</CardTitle>
          <CardDescription>
            Позиции появятся после получения данных о гонке.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Empty>
            <EmptyHeader>
              <EmptyMedia v-if="loading" variant="icon">
                <Spinner aria-hidden="true" />
              </EmptyMedia>
              <EmptyTitle>
                {{ loading ? 'Загрузка текущей гонки' : 'Нет данных о гонке' }}
              </EmptyTitle>
              <EmptyDescription>
                {{ error
                  ? 'Повторите подключение, чтобы получить данные.'
                  : 'Ожидаем появления текущей гонки.' }}
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        </CardContent>
      </Card>

      <aside class="min-w-0 lg:sticky lg:top-6" aria-label="Подписки">
        <DriverSubscriptions
          :subscriptions="subscriptions"
          :drivers="drivers"
          :pending-driver-ids="pendingDriverIds"
          :loading="subscriptionsLoading"
          :loaded="subscriptionsLoaded"
          :error="subscriptionsError"
          @unsubscribe="unsubscribeDriver"
          @reload="loadSubscriptions"
        />
      </aside>
    </div>
  </main>
</template>
