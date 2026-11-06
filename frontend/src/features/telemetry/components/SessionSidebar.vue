<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Sidebar, SidebarContent, SidebarHeader } from '@/components/ui/sidebar'
import SessionTree from './SessionTree.vue'
import type { SessionYear } from '../types'

defineProps<{
  years: SessionYear[]
  loading: boolean
  error: string
}>()

defineEmits<{
  retry: []
}>()
</script>

<template>
  <Sidebar class="md:top-16 md:h-[calc(100svh-4rem)]">
    <SidebarHeader>
      <h1 class="px-2 py-1 font-semibold">Сессии</h1>
    </SidebarHeader>
    <SidebarContent>
      <p v-if="loading" class="p-4 text-sm" role="status">Загружаем сессии…</p>
      <div v-else-if="error" class="flex flex-col items-start gap-3 p-4" role="alert">
        <p class="text-sm">{{ error }}</p>
        <Button variant="outline" @click="$emit('retry')">Попробовать снова</Button>
      </div>
      <p v-else-if="years.length === 0" class="p-4 text-sm text-muted-foreground">
        Завершённых сессий пока нет.
      </p>
      <SessionTree v-else :years="years" />
    </SidebarContent>
  </Sidebar>
</template>
