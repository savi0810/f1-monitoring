<script setup lang="ts">
import { computed } from 'vue'
import { Button } from '@/components/ui/button'
import { Spinner } from '@/components/ui/spinner'
import { ImportStatus } from '../types'
import type { SessionImport } from '../types'

const props = defineProps<{
  sessionImport: SessionImport | null
  loading: boolean
  starting: boolean
  error: string
  importAvailable: boolean
  statusLoaded: boolean
}>()

defineEmits<{
  import: []
  refresh: []
}>()

const statusText = computed(() => {
  if (!props.sessionImport) return 'Данные ещё не загружены'

  switch (props.sessionImport.status) {
    case ImportStatus.Pending:
      return 'Данные загружаются'
    case ImportStatus.Completed:
      return 'Данные загружены'
    case ImportStatus.Empty:
      return 'Источник не вернул данные'
    case ImportStatus.Failed:
      return 'Ошибка импорта'
    default:
      return 'Неизвестный статус импорта'
  }
})

const canImport = computed(() =>
  props.importAvailable
  && props.statusLoaded
  && !props.loading
  && !props.starting
  && (!props.sessionImport || props.sessionImport.status === ImportStatus.Failed),
)
</script>

<template>
  <div class="flex flex-col items-start gap-2">
    <div v-if="loading || statusLoaded" role="status" class="flex items-center gap-2 text-sm text-muted-foreground">
      <Spinner
        v-if="loading || sessionImport?.status === ImportStatus.Pending"
        aria-hidden="true"
      />
      <span>{{ loading ? 'Загружаем состояние…' : statusText }}</span>
    </div>
    <p v-if="sessionImport?.error" class="text-sm text-destructive">
      {{ sessionImport.error }}
    </p>
    <p v-if="error" role="alert" class="text-sm text-destructive">{{ error }}</p>
    <div class="flex flex-wrap gap-2">
      <Button v-if="canImport || starting" :disabled="!canImport" @click="$emit('import')">
        <Spinner v-if="starting" data-icon="inline-start" />
        {{ starting ? 'Отправляем запрос…' : 'Загрузить результаты' }}
      </Button>
      <Button variant="outline" :disabled="loading || starting" @click="$emit('refresh')">
        Обновить
      </Button>
    </div>
  </div>
</template>
