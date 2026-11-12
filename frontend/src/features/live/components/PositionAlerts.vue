<script setup lang="ts">
import { ArrowDown, ArrowUp, X } from '@lucide/vue'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/components/ui/empty'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'
import type { PositionAlertTriggered } from '../types'

defineProps<{
  alerts: readonly PositionAlertTriggered[]
}>()

const emit = defineEmits<{
  dismiss: [alertId: string]
}>()

const timeFormatter = new Intl.DateTimeFormat('ru-RU', {
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
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Уведомления</CardTitle>
      <CardDescription>
        Последние пять уведомлений об изменениях позиций.
      </CardDescription>
    </CardHeader>

    <CardContent>
      <Empty v-if="alerts.length === 0">
        <EmptyHeader>
          <EmptyTitle>Уведомлений пока нет</EmptyTitle>
          <EmptyDescription>
            Здесь появятся изменения позиций пилотов, на которых вы подписаны.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>

      <TooltipProvider v-else>
        <div class="flex flex-col gap-3">
          <Alert v-for="alert in alerts" :key="alert.alertId" role="status">
            <ArrowUp
              v-if="alert.currentPosition < alert.previousPosition"
              aria-hidden="true"
            />
            <ArrowDown v-else aria-hidden="true" />

            <AlertTitle class="pr-8">
              Пилот изменил позицию
            </AlertTitle>
            <AlertDescription class="pr-8">
              <p>
                {{ alert.driverName }}:
                {{ alert.previousPosition }} → {{ alert.currentPosition }}
              </p>
              <time :datetime="alert.observedAt">
                {{ formatTime(alert.observedAt) }}
              </time>
            </AlertDescription>

            <Tooltip>
              <TooltipTrigger as-child>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-xs"
                  class="absolute right-2 top-2"
                  :aria-label="`Закрыть уведомление: ${alert.driverName}`"
                  @click="emit('dismiss', alert.alertId)"
                >
                  <X aria-hidden="true" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Закрыть уведомление</TooltipContent>
            </Tooltip>
          </Alert>
        </div>
      </TooltipProvider>
    </CardContent>

    <CardFooter>
      <p class="text-sm text-muted-foreground">
        История доступна до ухода со страницы.
      </p>
    </CardFooter>
  </Card>
</template>
