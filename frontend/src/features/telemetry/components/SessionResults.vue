<script setup lang="ts">
import { computed, ref } from 'vue'
import { AccordionRoot } from 'reka-ui'
import { Input } from '@/components/ui/input'
import DriverResultItem from './DriverResultItem.vue'
import type { SessionResult } from '../types'

const props = defineProps<{
  results: SessionResult[]
  sprintQualifying: boolean
}>()

const search = ref('')
const filteredResults = computed(() => {
  const query = search.value.trim().toLocaleLowerCase()
  if (!query) return props.results

  return props.results.filter(result =>
    [String(result.driverNumber), result.fullName ?? '', result.teamName ?? '']
      .some(value => value.toLocaleLowerCase().includes(query)),
  )
})

function getMembersStr(numMembers: number | undefined): string {
  if (!numMembers) return ''
  let str = ''

  const lastDigit = numMembers % 10
  const lastTwoDigits = numMembers % 100

  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    str = 'участников'
  } else if (lastDigit === 1) {
    str = 'участник'
  } else if (lastDigit >= 2 && lastDigit <= 4) {
    str = 'участника'
  } else {
    str = 'участников'
  }
  return `${numMembers} ${str}`
}
</script>

<template>
  <section class="flex flex-col gap-4">
    <div class="flex flex-wrap items-center justify-between gap-4">
      <h2 class="text-xl font-semibold">Результаты · {{ getMembersStr(results.length) }} </h2>
      <Input v-model="search" class="max-w-sm" placeholder="Поиск пилота или команды" aria-label="Поиск пилота или команды" />
    </div>

    <p v-if="filteredResults.length === 0" class="text-muted-foreground">
      По вашему запросу пилоты не найдены.
    </p>
    <AccordionRoot v-else type="multiple" class="overflow-hidden rounded-lg border">
      <DriverResultItem
        v-for="result in filteredResults"
        :key="result.driverNumber"
        :result="result"
        :sprint-qualifying="sprintQualifying"
      />
    </AccordionRoot>
  </section>
</template>
