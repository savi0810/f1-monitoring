async function readError(response: Response, fallback: string): Promise<string> {
  const text = await response.text()

  try {
    const body = JSON.parse(text) as {
      errors?: Record<string, string[]>
      detail?: string
    }

    if (body.errors) return Object.values(body.errors).flat().join(' ') || fallback
    return body.detail || fallback
  } catch {
    return text || fallback
  }
}

export async function login(email: string, password: string): Promise<string> {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })

  if (!response.ok) throw new Error(await readError(response, 'Не удалось войти.'))

  const body = await response.json() as { accessToken?: unknown }
  if (typeof body.accessToken !== 'string') throw new Error('Некорректный ответ сервера.')
  return body.accessToken
}

export async function register(username: string, email: string, password: string): Promise<void> {
  const response = await fetch('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, email, password }),
  })

  if (!response.ok) throw new Error(await readError(response, 'Не удалось зарегистрироваться.'))
}
