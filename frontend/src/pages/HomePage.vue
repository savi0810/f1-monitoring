<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { clearToken, getExpiration, loadToken } from '@/features/auth/tokenStorage'

const router = useRouter()
const maxTimeout = 2147483647
let timer: ReturnType<typeof setTimeout> | undefined

function logout() {
  if (timer) clearTimeout(timer)
  clearToken()
  router.replace({ name: 'login' })
}

function scheduleExpiration() {
  const token = loadToken()
  if (!token) {
    logout()
    return
  }

  const remaining = getExpiration(token) - Date.now()
  timer = setTimeout(scheduleExpiration, Math.min(remaining, maxTimeout))
}

onMounted(scheduleExpiration)
onUnmounted(() => {
  if (timer) clearTimeout(timer)
})
</script>

<template>
  <main class="min-h-screen bg-background p-6 text-foreground">
    <div class="mx-auto flex max-w-5xl items-center justify-between">
      <h1 class="text-2xl font-semibold">F1 Monitor</h1>
      <button class="text-sm text-primary hover:underline" @click="logout">Выйти</button>
    </div>
  </main>
</template>
