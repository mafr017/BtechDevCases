const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8000'
const STORAGE_KEY = 'auth'
const WELCOME_KEY = 'welcome'
const WALLET_KEY = 'wallet'

const TIMEOUT_MS = 10_000
const RETRY_DELAYS_MS = [1_000, 3_000]

export type Session = { accessToken: string; expiresAt: string }

export type Transfer = {
  id: string
  direction: 'sent' | 'received'
  counterparty: string
  amount: number
  notes: string | null
  createdAt: string
}

export type Wallet = { balance: number; transfers: Transfer[] }

export type CachedWallet = Wallet & { updatedAt: string }

type Problem = { title?: string; errors?: Record<string, string[]> }

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

const networkError = () => new ApiError('No connection to the server. Check your signal and try again.', 0)

export const isNetworkError = (err: unknown) => err instanceof ApiError && err.status === 0
export const isUnauthorized = (err: unknown) => err instanceof ApiError && err.status === 401

export function getSession(): Session | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  return raw ? (JSON.parse(raw) as Session) : null
}

export function clearSession() {
  ;[STORAGE_KEY, WELCOME_KEY, WALLET_KEY].forEach((key) => localStorage.removeItem(key))
}

function saveSession({ accessToken, expiresAt }: Session) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ accessToken, expiresAt }))
}

function problemMessage(problem: Problem | null): string {
  if (problem?.errors) return Object.values(problem.errors).flat().join(' ')
  return problem?.title ?? 'Something went wrong.'
}

async function send<T>(path: string, init: RequestInit): Promise<T> {
  const token = getSession()?.accessToken
  const headers: Record<string, string> = { 'Content-Type': 'application/json', ...(init.headers as Record<string, string>) }
  if (token) headers.Authorization = `Bearer ${token}`

  let res: Response
  let body: unknown
  try {
    res = await fetch(API_URL + path, { ...init, headers, signal: AbortSignal.timeout(TIMEOUT_MS) })
    body = await res.json().catch(() => (res.ok ? undefined : null))
  } catch {
    throw networkError()
  }

  if (res.status === 401 && token) clearSession()
  if (!res.ok) throw new ApiError(problemMessage(body as Problem | null), res.status)
  if (body === undefined) throw networkError()
  return body as T
}

async function sendWithRetry<T>(path: string, init: RequestInit = {}): Promise<T> {
  for (let attempt = 0; ; attempt++) {
    try {
      return await send<T>(path, init)
    } catch (err) {
      const retryable = err instanceof ApiError && (err.status === 0 || err.status >= 500)
      if (!retryable || attempt >= RETRY_DELAYS_MS.length) throw err
      await new Promise((resolve) => setTimeout(resolve, RETRY_DELAYS_MS[attempt]))
    }
  }
}

const post = <T>(path: string, data?: unknown) =>
  send<T>(path, { method: 'POST', body: data === undefined ? undefined : JSON.stringify(data) })

export function register(email: string, password: string, confirmPassword: string) {
  return post('/api/auth/register', { email, password, confirmPassword })
}

export async function login(email: string, password: string) {
  saveSession(await post<Session>('/api/auth/login', { email, password }))
}

export async function refresh() {
  saveSession(await post<Session>('/api/auth/refresh'))
}

export async function getMe() {
  const me = await sendWithRetry<{ userId: string; email: string; message: string }>('/api/me')
  localStorage.setItem(WELCOME_KEY, me.message)
  return me
}

export const getCachedWelcome = () => localStorage.getItem(WELCOME_KEY)

function cacheWallet(wallet: Wallet): CachedWallet {
  const cached = { ...wallet, updatedAt: new Date().toISOString() }
  localStorage.setItem(WALLET_KEY, JSON.stringify(cached))
  return cached
}

export function getCachedWallet(): CachedWallet | null {
  const raw = localStorage.getItem(WALLET_KEY)
  return raw ? (JSON.parse(raw) as CachedWallet) : null
}

export async function getWallet() {
  return cacheWallet(await sendWithRetry<Wallet>('/api/wallet'))
}

export async function transfer(recipient: string, amount: number, notes: string, idempotencyKey: string) {
  const wallet = await sendWithRetry<Wallet>('/api/wallet/transfers', {
    method: 'POST',
    headers: { 'Idempotency-Key': idempotencyKey },
    body: JSON.stringify({ recipient, amount, notes }),
  })
  return cacheWallet(wallet)
}
