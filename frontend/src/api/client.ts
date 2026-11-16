const API_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5027'

export type LoginResponse = {
  token: string
  fullName: string
  role: string
}

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
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
