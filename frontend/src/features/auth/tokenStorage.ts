const TOKEN_KEY = 'f1_monitor_token'

export function getExpiration(token: string): number {
  try {
    const payload = token.split('.')[1]
    if (!payload) return 0

    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decoded = JSON.parse(atob(base64)) as { exp?: unknown }

    return typeof decoded.exp === 'number' ? decoded.exp * 1000 : 0
  } catch {
    return 0
  }
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY)
}

export function loadToken(): string | null {
  const token = localStorage.getItem(TOKEN_KEY)

  if (token && getExpiration(token) > Date.now()) return token

  clearToken()
  return null
}

export function hasValidToken(): boolean {
  return loadToken() !== null
}

export function saveToken(token: string): void {
  if (getExpiration(token) <= Date.now()) {
    throw new Error('Получен недействительный токен.')
  }

  localStorage.setItem(TOKEN_KEY, token)
}
