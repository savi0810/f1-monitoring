<script setup lang="ts">
import { computed } from 'vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { ImportStatus } from '../types'
import type { SessionImport } from '../types'

const props = defineProps<{
  sessionImport: SessionImport | null
  loading: boolean
  starting: boolean
  error: string
}>()

defineEmits<{
  import: []
  refresh: []
}>()

const statusText = computed(() => {
  if (!props.sessionImport) return 'Импорт ещё не запускался.'

  switch (props.sessionImport.status) {
    case ImportStatus.Pending:
      return 'Импорт ожидает обработки.'
    case ImportStatus.Processing:
      return 'Импорт выполняется.'
    case ImportStatus.Completed:
      return 'Импорт завершён.'
    case ImportStatus.Empty:
      return 'Импорт завершён: источник не вернул круги.'
    case ImportStatus.Failed:
      return 'Импорт завершился с ошибкой.'
    default:
      return 'Неизвестный статус импорта.'
  }
})

const canImport = computed(() =>
  !props.loading
  && !props.starting
  && (!props.sessionImport || props.sessionImport.status === ImportStatus.Failed),
)
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Импорт сессии</CardTitle>
      <CardDescription>
        Сохранённые круги и статус обновляются автоматически во время импорта.
      </CardDescription>
    </CardHeader>
    <CardContent class="flex flex-col gap-2">
      <p role="status">{{ loading ? 'Загружаем состояние…' : statusText }}</p>
      <p v-if="sessionImport?.error" class="text-sm text-destructive">
        {{ sessionImport.error }}
      </p>
      <p v-if="error" role="alert" class="text-sm text-destructive">{{ error }}</p>
    </CardContent>
    <CardFooter class="flex flex-wrap gap-2">
      <Button :disabled="!canImport" @click="$emit('import')">
        {{ starting ? 'Отправляем запрос…' : 'Импортировать круги' }}
      </Button>
      <Button variant="outline" :disabled="loading || starting" @click="$emit('refresh')">
        Обновить
      </Button>
    </CardFooter>
  </Card>
</template>
