<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/sidebar'
import { buildSessionTree } from '@/features/telemetry/utils/buildSessionTree'
import SessionSidebar from '@/features/telemetry/components/SessionSidebar.vue'
import { useSessions } from '@/features/telemetry/composables/useSessions'

const { sessions, loading, error, loadSessions } = useSessions()
const route = useRoute()
const years = computed(() => buildSessionTree(sessions.value))
const selectedSession = computed(() =>
  sessions.value.find(session => String(session.sessionKey) === route.params.sessionKey),
)

onMounted(() => {
  void loadSessions()
})
</script>

<template>
  <SidebarProvider>
    <SessionSidebar :years="years" :loading="loading" :error="error" @retry="loadSessions()" />

    <SidebarInset>
      <div class="p-4">
        <SidebarTrigger aria-label="Открыть или закрыть список сессий" />
      </div>
      <RouterView v-slot="{ Component, route }">
        <component :is="Component" v-if="Component" :key="route.params.sessionKey" :session="selectedSession" />
        <p v-else class="p-6 text-muted-foreground">Выберите сессию в списке.</p>
      </RouterView>
    </SidebarInset>
  </SidebarProvider>
</template>
