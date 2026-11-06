<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import SessionImportCard from './SessionImportCard.vue'
import { formatDuration } from '@/features/telemetry/utils/formatDuration'
import { sessionLabel } from '@/features/telemetry/utils/sessionLabel'
import { useSessionImport } from '@/features/telemetry/composables/useSessionImport'
import type { Session } from '@/features/telemetry/types'

defineProps<{
  session?: Session
}>()

const route = useRoute()
const sessionKey = Number(route.params.sessionKey)
const validSessionKey = Number.isInteger(sessionKey) && sessionKey > 0 && sessionKey <= 2147483647

const { laps, sessionImport, loading, starting, error, refresh, importSession } =
  useSessionImport(sessionKey)

function formatDate(value: string | null): string {
  if (!value) return '—'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'short',
    timeStyle: 'medium',
  }).format(date)
}

onMounted(() => {
  if (validSessionKey) void refresh()
})
</script>

<template>
  <div class="p-6">
    <div class="flex flex-col gap-6">
      <header class="flex flex-wrap items-start justify-between gap-4">
        <div class="flex flex-col gap-2">
          <h1 class="text-3xl font-semibold">
            {{ session ? sessionLabel(session) : validSessionKey ? `Сессия #${sessionKey}` : 'Некорректный номер сессии' }}
          </h1>
          <p v-if="session" class="text-muted-foreground">
            {{ [session.circuitShortName, session.countryName, formatDate(session.dateStart)].filter(Boolean).join(' · ') }}
          </p>
        </div>
        <Button as-child variant="outline">
          <RouterLink :to="{ name: 'sessions' }">К каталогу</RouterLink>
        </Button>
      </header>

      <template v-if="validSessionKey">
        <SessionImportCard
          :session-import="sessionImport"
          :loading="loading"
          :starting="starting"
          :error="error"
          @import="importSession"
          @refresh="refresh()"
        />

        <Card>
          <CardHeader>
            <CardTitle>Сохранённые круги</CardTitle>
            <CardDescription>Всего кругов: {{ laps.length }}</CardDescription>
          </CardHeader>
          <CardContent>
            <p v-if="loading && laps.length === 0" role="status">
              Загружаем круги…
            </p>
            <p v-else-if="error && laps.length === 0" class="text-muted-foreground">
              Круги пока не загружены. Попробуйте обновить данные.
            </p>
            <p v-else-if="laps.length === 0" class="text-muted-foreground">
              Сохранённых кругов пока нет.
            </p>
            <Table v-else>
              <TableHeader>
                <TableRow>
                  <TableHead>Пилот</TableHead>
                  <TableHead>Круг</TableHead>
                  <TableHead>Время круга</TableHead>
                  <TableHead>Сектор 1</TableHead>
                  <TableHead>Сектор 2</TableHead>
                  <TableHead>Сектор 3</TableHead>
                  <TableHead>Выезд из боксов</TableHead>
                  <TableHead>Дата</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="lap in laps" :key="lap.eventId">
                  <TableCell>{{ lap.driverNumber ?? '—' }}</TableCell>
                  <TableCell>{{ lap.lapNumber }}</TableCell>
                  <TableCell>{{ formatDuration(lap.lapDuration) }}</TableCell>
                  <TableCell>{{ formatDuration(lap.sector1Duration) }}</TableCell>
                  <TableCell>{{ formatDuration(lap.sector2Duration) }}</TableCell>
                  <TableCell>{{ formatDuration(lap.sector3Duration) }}</TableCell>
                  <TableCell>{{ lap.isPitOutLap ? 'Да' : 'Нет' }}</TableCell>
                  <TableCell>{{ formatDate(lap.occurredAt) }}</TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </template>
    </div>
  </div>
</template>
