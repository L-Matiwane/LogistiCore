import { SESSION_KEY } from '../auth/AuthContext'

const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5027'

export type FieldErrors = Record<string, string[]>

export type LoginResponse = {
  token: string
  fullName: string
  role: string
}

export class ApiError extends Error {
  status: number
  fieldErrors: FieldErrors

  constructor(message: string, status: number, fieldErrors: FieldErrors = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

type ProblemBody = {
  title?: string
  detail?: string
  message?: string
  errors?: FieldErrors
}

/** A message that is safe to show a person. Prefers the first field error, then the server message. */
export function errorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const first = Object.values(err.fieldErrors)[0]?.[0]
    return first ?? err.message
  }
  return 'Something went wrong. Please try again.'
}

function readToken(): string | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    return raw ? ((JSON.parse(raw) as { token?: string }).token ?? null) : null
  } catch {
    return null
  }
}

export async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = readToken()
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) {
    headers.set('Content-Type', 'application/json')
  }
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  let response: Response
  try {
    response = await fetch(`${API_URL}${path}`, { ...init, headers })
  } catch {
    throw new ApiError('Cannot reach the server. Is the API running?', 0)
  }

  if (response.status === 401 && token) {
    sessionStorage.removeItem(SESSION_KEY)
    window.location.assign('/login')
    throw new ApiError('Your session has expired. Please sign in again.', 401)
  }
  if (response.status === 403) {
    throw new ApiError('You do not have permission to do that.', 403)
  }
  if (response.status === 404) {
    throw new ApiError('That record was not found.', 404)
  }

  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as ProblemBody | null
    const fieldErrors = body?.errors ?? {}
    const hasFieldErrors = Object.keys(fieldErrors).length > 0
    const message = hasFieldErrors
      ? 'Please fix the highlighted fields.'
      : (body?.detail ?? body?.message ?? 'Something went wrong. Please try again.')
    throw new ApiError(message, response.status, fieldErrors)
  }

  return (await response.json()) as T
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  let response: Response

  try {
    response = await fetch(`${API_URL}/api/v1/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    })
  } catch {
    throw new ApiError('Cannot reach the server. Is the API running?', 0)
  }

  if (response.status === 401) {
    throw new ApiError('Incorrect email or password.', 401)
  }
  if (!response.ok) {
    throw new ApiError('Something went wrong. Please try again.', response.status)
  }

  return (await response.json()) as LoginResponse
}
