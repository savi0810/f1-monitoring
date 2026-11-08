<script setup lang="ts">
import { ChevronDown } from '@lucide/vue'
import { AccordionContent, AccordionHeader, AccordionItem, AccordionTrigger } from 'reka-ui'
import DriverLaps from './DriverLaps.vue'
import type { SessionResult } from '../types'
import { formatGap, formatLapTime, formatTotalTime } from '../utils/formatResultTime'

defineProps<{
  result: SessionResult
  sprintQualifying: boolean
}>()
</script>

<template>
  <AccordionItem :value="String(result.driverNumber)" class="border-b last:border-b-0">
    <AccordionHeader>
      <AccordionTrigger class="group flex w-full cursor-pointer items-center gap-4 px-4 py-4 text-left transition-colors hover:bg-primary/20 focus-visible:outline-2 focus-visible:outline-offset-[-2px] motion-reduce:transition-none">
        <span class="w-8 shrink-0 tabular-nums">{{ result.position ?? '—' }}</span>
        <span class="min-w-0 flex-1">
          <span class="block truncate font-medium">
            #{{ result.driverNumber }} {{ result.fullName || 'Пилот' }}
          </span>
          <span class="block truncate text-sm text-muted-foreground">
            {{ result.teamName || 'Команда неизвестна' }}
          </span>
        </span>

        <span v-if="result.race" class="text-right text-sm tabular-nums">
          <span v-if="result.position === 1 && result.race.totalTimeSeconds !== null" class="block">
            {{ formatTotalTime(result.race.totalTimeSeconds) }}
          </span>
          <span v-else-if="result.race.gapToLeaderSeconds !== null" class="block">
            {{ formatGap(result.race.gapToLeaderSeconds) }}
          </span>
          <span v-if="result.race.gapToLeaderText" class="block">
            {{ result.race.gapToLeaderText }}
          </span>
          <span v-if="result.race.gapToLeaderSeconds === null && !result.race.gapToLeaderText">
            {{ formatTotalTime(result.race.totalTimeSeconds) }}
          </span>
        </span>
        <span v-else-if="result.practice" class="text-right text-sm tabular-nums">
          {{ formatLapTime(result.practice.bestLapSeconds) }}
          <span class="block text-muted-foreground">{{ formatGap(result.practice.gapToLeaderSeconds) }}</span>
        </span>
        <span v-else-if="result.qualifying" class="text-right text-sm tabular-nums">
          <span v-for="phase in 3" :key="phase" class="block">
            {{ sprintQualifying ? 'SQ' : 'Q' }}{{ phase }}:
            {{ formatLapTime(result.qualifying.phases.find(item => item.phase === phase)?.bestLapSeconds ?? null) }}
          </span>
        </span>

        <span class="text-sm tabular-nums">
          Круги: {{ result.numberOfLaps ?? '—' }}
        </span>
        <span v-if="result.didNotFinish" class="text-sm">DNF</span>
        <span v-if="result.didNotStart" class="text-sm">DNS</span>
        <span v-if="result.disqualified" class="text-sm">DSQ</span>
        <ChevronDown class="shrink-0 transition-transform motion-reduce:transition-none group-data-[state=open]:rotate-180" />
      </AccordionTrigger>
    </AccordionHeader>

    <AccordionContent class="driver-laps-content bg-muted/30">
      <div class="px-4 py-4">
        <DriverLaps :laps="result.laps" />
      </div>
    </AccordionContent>
  </AccordionItem>
</template>

<style scoped>
.driver-laps-content {
  overflow: hidden;
}

@media (prefers-reduced-motion: no-preference) {
  .driver-laps-content[data-state='open'] {
    animation: expand 160ms ease-out;
  }

  .driver-laps-content[data-state='closed'] {
    animation: collapse 160ms ease-in;
  }
}

@keyframes expand {
  from { height: 0; }
  to { height: var(--reka-accordion-content-height); }
}

@keyframes collapse {
  from { height: var(--reka-accordion-content-height); }
  to { height: 0; }
}
</style>
