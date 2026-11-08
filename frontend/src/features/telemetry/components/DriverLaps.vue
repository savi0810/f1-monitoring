<script setup lang="ts">
import { computed } from 'vue'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import type { SessionResultLap } from '../types'
import { formatLapTime } from '../utils/formatResultTime'

const props = defineProps<{
  laps: SessionResultLap[]
}>()

const bestLap = computed(() => {
  const durations = props.laps
    .map(lap => lap.lapDuration)
    .filter((duration): duration is number =>
      duration !== null && Number.isFinite(duration) && duration > 0,
    )

  return durations.length ? Math.min(...durations) : null
})
</script>

<template>
  <div class="flex flex-col gap-3">
    <p class="text-sm text-muted-foreground">
      Лучший сохранённый круг: {{ formatLapTime(bestLap) }}
    </p>
    <p class="text-sm text-muted-foreground">
      Сохранено кругов: {{ laps.length }}
    </p>

    <p v-if="laps.length === 0" class="text-sm text-muted-foreground">
      Сохранённых кругов пока нет.
    </p>
    <Table v-else class="tabular-nums">
      <TableHeader>
        <TableRow>
          <TableHead>Круг</TableHead>
          <TableHead>Время</TableHead>
          <TableHead>Сектор 1</TableHead>
          <TableHead>Сектор 2</TableHead>
          <TableHead>Сектор 3</TableHead>
          <TableHead>Выезд из боксов</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="lap in laps" :key="lap.lapNumber">
          <TableCell>{{ lap.lapNumber }}</TableCell>
          <TableCell>{{ formatLapTime(lap.lapDuration) }}</TableCell>
          <TableCell>{{ formatLapTime(lap.sector1Duration) }}</TableCell>
          <TableCell>{{ formatLapTime(lap.sector2Duration) }}</TableCell>
          <TableCell>{{ formatLapTime(lap.sector3Duration) }}</TableCell>
          <TableCell>{{ lap.isPitOutLap ? 'Да' : 'Нет' }}</TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
