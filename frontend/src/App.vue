
<script setup lang="ts">
import { onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppLayout from '@/layouts/AppLayout.vue'
import { getExpiration, loadToken } from '@/features/auth/tokenStorage'

const route = useRoute()
const router = useRouter()
const maxTimeout = 2147483647
let timer: ReturnType<typeof setTimeout> | undefined

function scheduleExpiration() {
  if (timer) clearTimeout(timer)
  timer = undefined
  if (!route.meta.requiresAuth) return

  const token = loadToken()
  if (!token) {
    void router.replace({ name: 'login' })
    return
  }

  const remaining = getExpiration(token) - Date.now()
  timer = setTimeout(scheduleExpiration, Math.min(Math.max(remaining, 0), maxTimeout))
}

watch(() => [route.path, route.meta.requiresAuth], scheduleExpiration, { immediate: true })
onUnmounted(() => {
  if (timer) clearTimeout(timer)
})
</script>

<template>
  <RouterView v-slot="{ Component, route }">
    <AppLayout v-if="route.meta.requiresAuth">
      <component :is="Component" :key="route.matched[0]?.path" />
    </AppLayout>
    <component :is="Component" v-else :key="route.path" />
  </RouterView>
</template>
