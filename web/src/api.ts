const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8000'
const STORAGE_KEY = 'auth'

export type Session = { accessToken: string; expiresAt: string }

// ASP.NET problem details: 400 has `errors`, 401/409 only `title`.
type Problem = { title?: string; errors?: Record<string, string[]> }

export function getSession(): Session | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  return raw ? (JSON.parse(raw) as Session) : null
}

export function clearSession() {
  localStorage.removeItem(STORAGE_KEY)
}

function saveSession({ accessToken, expiresAt }: Session) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ accessToken, expiresAt }))
}

function problemMessage(problem: Problem | null): string {
  if (problem?.errors) return Object.values(problem.errors).flat().join(' ')
  return problem?.title ?? 'Something went wrong.'
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getSession()?.accessToken
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(API_URL + path, { ...init, headers })
  if (res.status === 401 && token) clearSession()

  const body = await res.json().catch(() => null)
  if (!res.ok) throw new Error(problemMessage(body))
  return body as T
}

const post = <T>(path: string, data?: unknown) =>
  request<T>(path, { method: 'POST', body: data === undefined ? undefined : JSON.stringify(data) })

export function register(email: string, password: string, confirmPassword: string) {
  return post('/api/auth/register', { email, password, confirmPassword })
}

export async function login(email: string, password: string) {
  saveSession(await post<Session>('/api/auth/login', { email, password }))
}

export async function refresh() {
  saveSession(await post<Session>('/api/auth/refresh'))
}

export function getMe() {
  return request<{ userId: string; email: string; message: string }>('/api/me')
}
