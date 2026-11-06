<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { register } from '@/features/auth/authApi'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldGroup, FieldLabel, FieldError } from '@/components/ui/field'
import { Input } from '@/components/ui/input'

const router = useRouter()

const username = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const invalidUsername = ref(false)
const invalidEmail = ref(false)
const invalidPassword = ref(false)
const isSubmitting = ref(false)

async function handleRegister() {
  error.value = ''
  invalidUsername.value = !username.value
  invalidEmail.value = !email.value
  invalidPassword.value = !password.value

  if (invalidUsername.value || invalidEmail.value || invalidPassword.value) {
    error.value = 'Пожалуйста, заполните все поля.'
    return
  }

  if (!email.value.includes('@')) {
    invalidEmail.value = true
    error.value = 'Неверный формат электронной почты.'
    return
  }

  isSubmitting.value = true
  try {
    await register(username.value, email.value, password.value)
    await router.push({ name: 'login' })
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Неизвестная ошибка'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <main class="flex min-h-screen items-center justify-center bg-background">
    <Card class="w-full max-w-sm">
      <CardHeader>
        <CardTitle class="text-xl">Регистрация</CardTitle>
      </CardHeader>

      <CardContent>
        <form id="register-form" novalidate @submit.prevent="handleRegister">
          <FieldGroup>
            <Field :data-invalid="invalidUsername || undefined">
              <FieldLabel for="reg-username">Имя пользователя</FieldLabel>
              <Input id="reg-username" v-model="username" type="text" placeholder="username" autocomplete="username" :aria-invalid="invalidUsername" />
            </Field>

            <Field :data-invalid="invalidEmail || undefined">
              <FieldLabel for="reg-email">Электронная почта</FieldLabel>
              <Input id="reg-email" v-model="email" type="email" placeholder="mail@example.com" autocomplete="email" :aria-invalid="invalidEmail" />
            </Field>

            <Field :data-invalid="invalidPassword || undefined">
              <FieldLabel for="reg-password">Пароль</FieldLabel>
              <Input id="reg-password" v-model="password" type="password" placeholder="••••••••" autocomplete="new-password" :aria-invalid="invalidPassword" />
              <FieldError v-if="error">{{ error }}</FieldError>
            </Field>
          </FieldGroup>
        </form>

        <p class="mt-4 text-center text-sm text-muted-foreground">
          Уже есть аккаунт?
          <Button variant="link" class="h-auto p-0" @click="router.push({ name: 'login' })">
            Войти
          </Button>
        </p>
      </CardContent>

      <CardFooter>
        <Button class="w-full gap-2" type="submit" form="register-form" :disabled="isSubmitting">
          {{ isSubmitting ? 'Создаём...' : 'Создать аккаунт' }}
        </Button>
      </CardFooter>
    </Card>
  </main>
</template>
