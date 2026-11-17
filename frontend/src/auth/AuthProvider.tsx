import { useCallback, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { login } from '../api/client'
import { AuthContext, SESSION_KEY } from './AuthContext'
import type { Session } from './AuthContext'

function loadSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    return raw ? (JSON.parse(raw) as Session) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(loadSession)

  const signIn = useCallback(async (email: string, password: string) => {
    const result = await login(email, password)
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(result))
    setSession(result)
  }, [])

  const signOut = useCallback(() => {
    sessionStorage.removeItem(SESSION_KEY)
    setSession(null)
  }, [])

  const value = useMemo(() => ({ session, signIn, signOut }), [session, signIn, signOut])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
