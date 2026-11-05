<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '@/features/auth/authApi'
import { saveToken } from '@/features/auth/tokenStorage'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldGroup, FieldLabel, FieldError } from '@/components/ui/field'
import { Input } from '@/components/ui/input'

const router = useRouter()

const email = ref('')
const password = ref('')
const error = ref('')
const invalidEmail = ref(false)
const invalidPassword = ref(false)
const isSubmitting = ref(false)

async function handleLogin() {
  error.value = ''
  invalidEmail.value = !email.value
  invalidPassword.value = !password.value

  if (invalidEmail.value || invalidPassword.value) {
    error.value = 'Пожалуйста, заполните все поля.'
    return
  }

  isSubmitting.value = true
  try {
    const token = await login(email.value, password.value)
    saveToken(token)
    await router.push({ name: 'home' })
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Неизвестная ошибка'
    invalidEmail.value = true
    invalidPassword.value = true
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="flex min-h-screen items-center justify-center bg-background">
    <Card class="w-full max-w-sm">
      <CardHeader>
        <CardTitle class="text-xl">Вход</CardTitle>
      </CardHeader>

      <CardContent>
        <form id="login-form" novalidate @submit.prevent="handleLogin">
          <FieldGroup>
            <Field :data-invalid="invalidEmail || undefined">
              <FieldLabel for="login-email">Электронная почта</FieldLabel>
              <Input id="login-email" v-model="email" type="email" placeholder="mail@example.com" autocomplete="email" :aria-invalid="invalidEmail" />
            </Field>

            <Field :data-invalid="invalidPassword || undefined">
              <FieldLabel for="login-password">Пароль</FieldLabel>
              <Input id="login-password" v-model="password" type="password" placeholder="••••••••" autocomplete="current-password" :aria-invalid="invalidPassword" />
              <FieldError v-if="error">{{ error }}</FieldError>
            </Field>
          </FieldGroup>
        </form>

        <p class="mt-4 text-center text-sm text-muted-foreground">
          Нет аккаунта?
          <Button variant="link" class="h-auto p-0" @click="router.push({ name: 'register' })">
            Зарегистрироваться
          </Button>
        </p>
      </CardContent>

      <CardFooter>
        <Button class="w-full gap-2" type="submit" form="login-form" :disabled="isSubmitting">
          {{ isSubmitting ? 'Входим...' : 'Войти' }}
        </Button>
      </CardFooter>
    </Card>
  </main>
</template>
