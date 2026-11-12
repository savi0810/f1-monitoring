<script setup lang="ts">
import 'vue-sonner/style.css'
import AppHeader from '@/components/AppHeader.vue'
import { Toaster } from '@/components/ui/sonner'
</script>

<template>
  <div class="min-h-screen bg-background text-foreground">
    <AppHeader />
    <slot />
    <Toaster theme="dark" position="top-center" :duration="3000" close-button />
  </div>
</template>
