<script setup lang="ts">
import { Bell, BellRing } from '@lucide/vue'
import { computed } from 'vue'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/components/ui/empty'
import { Item, ItemActions, ItemContent, ItemGroup, ItemTitle } from '@/components/ui/item'
import { Spinner } from '@/components/ui/spinner'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'
import type { LiveDriverSnapshot } from '../types'

const props = defineProps<{
  drivers: readonly LiveDriverSnapshot[]
  subscribedDriverIds: ReadonlySet<string>
  pendingDriverIds: ReadonlySet<string>
  subscriptionsLoaded: boolean
  canSubscribe: boolean
}>()

const emit = defineEmits<{
  subscribe: [driverId: string]
  unsubscribe: [driverId: string]
}>()

const sortedDrivers = computed(() => [...props.drivers].sort((left, right) =>
  (left.position ?? Number.MAX_SAFE_INTEGER)
  - (right.position ?? Number.MAX_SAFE_INTEGER),
))

function isDisabled(driverId: string) {
  return !props.subscriptionsLoaded
    || props.pendingDriverIds.has(driverId)
    || (!props.canSubscribe && !props.subscribedDriverIds.has(driverId))
}

function actionLabel(driver: LiveDriverSnapshot) {
  if (props.pendingDriverIds.has(driver.driverId)) {
    return `Изменение подписки: ${driver.fullName}`
  }

  return props.subscribedDriverIds.has(driver.driverId)
    ? `Отменить подписку на ${driver.fullName}`
    : `Подписаться на ${driver.fullName}`
}

function toggleSubscription(driverId: string) {
  if (isDisabled(driverId)) return

  if (props.subscribedDriverIds.has(driverId)) {
    emit('unsubscribe', driverId)
  } else {
    emit('subscribe', driverId)
  }
}
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Позиции пилотов</CardTitle>
      <CardDescription>
        Подпишитесь на изменения позиций выбранных пилотов.
      </CardDescription>
    </CardHeader>

    <CardContent>
      <Empty v-if="sortedDrivers.length === 0">
        <EmptyHeader>
          <EmptyTitle>Позиции пока недоступны</EmptyTitle>
          <EmptyDescription>Ожидаем данные о пилотах.</EmptyDescription>
        </EmptyHeader>
      </Empty>

      <TooltipProvider v-else>
        <ItemGroup class="relative gap-2" aria-label="Позиции пилотов">
          <TransitionGroup
            move-class="transition-transform duration-[280ms] ease-out motion-reduce:transition-none"
            enter-active-class="transition-opacity duration-150 ease-out motion-reduce:transition-none"
            leave-active-class="absolute w-full transition-opacity duration-150 ease-out motion-reduce:transition-none"
            enter-from-class="opacity-0"
            leave-to-class="opacity-0"
          >
            <Item
              v-for="driver in sortedDrivers"
              :key="driver.driverId"
              variant="outline"
              size="sm"
              role="listitem"
            >
              <span class="w-8 shrink-0 text-center tabular-nums">
                {{ driver.position ?? '—' }}
              </span>

              <ItemContent class="min-w-0">
                <ItemTitle class="max-w-full">
                  <span class="truncate">{{ driver.fullName }}</span>
                </ItemTitle>
              </ItemContent>

              <ItemActions>
                <Tooltip>
                  <TooltipTrigger as-child>
                    <Button
                      type="button"
                      :variant="subscribedDriverIds.has(driver.driverId) ? 'default' : 'outline'"
                      size="sm"
                      :disabled="isDisabled(driver.driverId)"
                      :aria-label="actionLabel(driver)"
                      :aria-busy="pendingDriverIds.has(driver.driverId)"
                      @click="toggleSubscription(driver.driverId)"
                    >
                      <Spinner
                        v-if="pendingDriverIds.has(driver.driverId)"
                        data-icon="inline-start"
                        aria-hidden="true"
                      />
                      <BellRing
                        v-else-if="subscribedDriverIds.has(driver.driverId)"
                        data-icon="inline-start"
                        aria-hidden="true"
                      />
                      <Bell v-else data-icon="inline-start" aria-hidden="true" />

                      <span>
                        {{ pendingDriverIds.has(driver.driverId)
                          ? 'Сохранение'
                          : subscribedDriverIds.has(driver.driverId)
                            ? 'Подписан'
                            : 'Подписаться' }}
                      </span>
                    </Button>
                  </TooltipTrigger>

                  <TooltipContent>
                    {{ actionLabel(driver) }}
                  </TooltipContent>
                </Tooltip>
              </ItemActions>
            </Item>
          </TransitionGroup>
        </ItemGroup>
      </TooltipProvider>
    </CardContent>

    <CardFooter v-if="!canSubscribe">
      <p class="text-sm text-muted-foreground">
        Новые подписки доступны во время гонки.
      </p>
    </CardFooter>
  </Card>
</template>
