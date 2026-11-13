<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Spinner } from '@/components/ui/spinner'
import SessionImportCard from './SessionImportCard.vue'
import SessionResults from './SessionResults.vue'
import { sessionLabel } from '@/features/telemetry/utils/sessionLabel'
import { useSessionImport } from '@/features/telemetry/composables/useSessionImport'
import { useSessionResults } from '@/features/telemetry/composables/useSessionResults'
import { ImportStatus } from '@/features/telemetry/types'
import type { Session, SessionCatalogMeeting } from '@/features/telemetry/types'

const props = defineProps<{
  session?: Session
  meeting?: SessionCatalogMeeting
}>()

const route = useRoute()
const sessionKey = Number(route.params.sessionKey)
const validSessionKey = Number.isInteger(sessionKey) && sessionKey > 0 && sessionKey <= 2147483647

const { results, loading: resultsLoading, error: resultsError, loadResults } = useSessionResults(sessionKey)
const { sessionImport, statusLoaded, loading, starting, error, refresh, importSession } =
  useSessionImport(sessionKey, () => void loadResults())

const sprintQualifying = computed(() => {
  if (!props.session) return false
  const description = `${props.session.sessionName} ${props.session.sessionType}`.toLowerCase()
  return description.includes('sprint')
    && (description.includes('qualifying') || description.includes('shootout'))
})

function formatDate(value: string | null): string {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'short',
    timeStyle: 'medium',
  }).format(date)
}

function refreshAll() {
  void refresh()
  void loadResults()
}

onMounted(() => {
  if (validSessionKey) refreshAll()
})
</script>

<template>
  <div class="p-6">
    <div class="flex flex-col gap-6">
      <nav v-if="session && meeting" aria-label="Путь к сессии" class="text-sm text-muted-foreground">
        {{ session.year }} / {{ meeting.meetingName || `Уикенд #${meeting.meetingKey}` }} / {{ sessionLabel(session) }}
      </nav>
      <header class="flex flex-wrap items-start justify-between gap-4">
        <div class="flex flex-col gap-2">
          <h1 class="text-3xl font-semibold">
            {{ session ? sessionLabel(session) : validSessionKey ? `Сессия #${sessionKey}` : 'Некорректный номер сессии' }}
          </h1>
          <p v-if="session" class="text-muted-foreground">
            {{ [session.circuitShortName, session.countryName, formatDate(session.dateStart)].filter(Boolean).join(' · ') }}
          </p>
        </div>
        <div class="flex flex-col items-start gap-4">
          <SessionImportCard
            v-if="validSessionKey"
            :session-import="sessionImport"
            :loading="loading"
            :starting="starting"
            :error="error"
            :import-available="!!session && !session.isCancelled"
            :status-loaded="statusLoaded"
            @import="importSession"
            @refresh="refreshAll"
          />
        </div>
      </header>

      <template v-if="validSessionKey">
        <p v-if="resultsError && resultsError !== error" role="alert" class="text-sm text-destructive">
          {{ resultsError }}
        </p>
        <div v-if="resultsLoading" class="flex items-center gap-2">
          <Spinner />
          <span class="text-sm text-muted-foreground">Загружаем результаты…</span>
        </div>
        <div v-else-if="!resultsError && results.length === 0" class="flex flex-col items-start gap-3 rounded-lg border p-6">
          <p class="font-medium">Результатов пока нет</p>
          <p v-if="sessionImport?.status === ImportStatus.Pending" class="text-sm text-muted-foreground">
            Импорт выполняется. Результаты появятся после его завершения.
          </p>
          <p v-else-if="sessionImport?.status === ImportStatus.Completed || sessionImport?.status === ImportStatus.Empty" class="text-sm text-muted-foreground">
            Импорт завершён, но результаты пилотов отсутствуют.
          </p>
          <p v-else class="text-sm text-muted-foreground">
            Загрузите данные сессии, чтобы увидеть результаты пилотов.
          </p>
        </div>
        <SessionResults v-if="results.length > 0" :results="results" :sprint-qualifying="sprintQualifying" />
      </template>
    </div>
  </div>
</template>
