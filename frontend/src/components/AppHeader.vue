<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { clearToken } from '@/features/auth/tokenStorage'

const router = useRouter()
const route = useRoute()

function logout() {
  clearToken()
  void router.replace({ name: 'login' })
}
</script>

<template>
  <header class="sticky top-0 z-20 border-b bg-background">
    <div class="flex min-h-16 items-center gap-6 px-6">
      <span class="font-semibold text-primary">F1 Monitor</span>
      <nav class="flex flex-1 items-center gap-6">
        <RouterLink
          :to="{ name: 'live' }"
          class="text-sm transition-colors hover:text-primary"
          :aria-current="route.name === 'live' ? 'page' : undefined"
          :class="{ 'font-semibold text-primary': route.name === 'live' }"
        >
          Лайв
        </RouterLink>
        <RouterLink
          :to="{ name: 'sessions' }"
          class="text-sm transition-colors hover:text-primary"
          :aria-current="route.name === 'sessions' || route.name === 'session' ? 'page' : undefined"
          :class="{ 'font-semibold text-primary': route.name === 'sessions' || route.name === 'session' }"
        >
          Предыдущие гонки
        </RouterLink>
      </nav>
      <button class="cursor-pointer text-sm transition-colors hover:text-primary" @click="logout">Выйти</button>
    </div>
  </header>
</template>
