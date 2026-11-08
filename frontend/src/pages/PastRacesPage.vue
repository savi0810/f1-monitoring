<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/sidebar'
import SessionSidebar from '@/features/telemetry/components/SessionSidebar.vue'
import { useSessions } from '@/features/telemetry/composables/useSessions'

const { sessions, loading, error, loadSessions } = useSessions()
const route = useRoute()
const selectedMeeting = computed(() =>
  sessions.value
    .flatMap(year => year.meetings)
    .find(meeting =>
      meeting.sessions.some(session => String(session.sessionKey) === route.params.sessionKey),
    ),
)
const selectedSession = computed(() =>
  selectedMeeting.value?.sessions.find(session => String(session.sessionKey) === route.params.sessionKey),
)

onMounted(() => {
  void loadSessions()
})
</script>

<template>
  <SidebarProvider>
    <SessionSidebar :years="sessions" :loading="loading" :error="error" @retry="loadSessions()" />

    <SidebarInset>
      <div class="p-4">
        <SidebarTrigger aria-label="Открыть или закрыть список сессий" />
      </div>
      <RouterView v-slot="{ Component, route }">
        <component :is="Component" v-if="Component" :key="route.params.sessionKey" :session="selectedSession" :meeting="selectedMeeting" />
        <p v-else class="p-6 text-muted-foreground">Выберите сессию в списке.</p>
      </RouterView>
    </SidebarInset>
  </SidebarProvider>
</template>
