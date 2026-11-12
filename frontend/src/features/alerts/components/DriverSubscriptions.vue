<script setup lang="ts">
import { BellOff } from '@lucide/vue'
import { computed } from 'vue'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/components/ui/empty'
import { Item, ItemActions, ItemContent, ItemGroup, ItemTitle } from '@/components/ui/item'
import { Spinner } from '@/components/ui/spinner'
import type { LiveDriverSnapshot } from '@/features/live/types'
import type { DriverSubscriptionDto } from '../types'

const props = defineProps<{
  subscriptions: readonly DriverSubscriptionDto[]
  drivers: readonly LiveDriverSnapshot[]
  pendingDriverIds: ReadonlySet<string>
  loading: boolean
  loaded: boolean
  error: string
}>()

const emit = defineEmits<{
  unsubscribe: [driverId: string]
  reload: []
}>()

const driverNames = computed(() =>
  new Map(props.drivers.map(driver => [driver.driverId, driver.fullName])),
)
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Мои подписки</CardTitle>
      <CardDescription>
        Уведомления об изменении позиций выбранных пилотов.
      </CardDescription>
    </CardHeader>

    <CardContent class="flex flex-col gap-4">
      <Alert v-if="error" variant="destructive">
        <AlertTitle>Не удалось обновить подписки</AlertTitle>
        <AlertDescription>
          <p>{{ error }}</p>
          <Button
            type="button"
            variant="outline"
            size="sm"
            class="mt-3"
            :disabled="loading || pendingDriverIds.size > 0"
            @click="emit('reload')"
          >
            Повторить загрузку
          </Button>
        </AlertDescription>
      </Alert>

      <div
        v-if="loading && !loaded"
        role="status"
        class="flex items-center gap-2 text-muted-foreground"
      >
        <Spinner aria-hidden="true" />
        Загрузка подписок
      </div>

      <ItemGroup
        v-else-if="subscriptions.length > 0"
        class="gap-2"
        aria-label="Мои подписки"
      >
        <Item
          v-for="subscription in subscriptions"
          :key="subscription.id"
          variant="outline"
          size="sm"
          role="listitem"
        >
          <ItemContent class="min-w-0">
            <ItemTitle class="max-w-full">
              <span class="truncate">
                {{ driverNames.get(subscription.driverId) || subscription.driverId }}
              </span>
            </ItemTitle>
          </ItemContent>

          <ItemActions>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              :disabled="pendingDriverIds.has(subscription.driverId)"
              :aria-busy="pendingDriverIds.has(subscription.driverId)"
              :aria-label="`Отменить подписку на ${driverNames.get(subscription.driverId) || subscription.driverId}`"
              @click="emit('unsubscribe', subscription.driverId)"
            >
              <Spinner
                v-if="pendingDriverIds.has(subscription.driverId)"
                data-icon="inline-start"
                aria-hidden="true"
              />
              <BellOff v-else data-icon="inline-start" aria-hidden="true" />
              Отменить
            </Button>
          </ItemActions>
        </Item>
      </ItemGroup>

      <Empty v-else-if="loaded">
        <EmptyHeader>
          <EmptyTitle>Подписок пока нет</EmptyTitle>
          <EmptyDescription>
            Выберите пилота в списке позиций, чтобы получать уведомления.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    </CardContent>

    <CardFooter v-if="loaded" class="justify-between gap-4">
      <p class="text-sm text-muted-foreground">
        Подписок: {{ subscriptions.length }}
      </p>
      <span v-if="loading" role="status" class="flex items-center gap-2">
        <Spinner aria-hidden="true" />
        Обновление
      </span>
    </CardFooter>
  </Card>
</template>
