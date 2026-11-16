import { useCallback, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { login } from '../api/client'
import { AuthContext } from './AuthContext'
import type { Session } from './AuthContext'

const STORAGE_KEY = 'logisticore.session'

function loadSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as Session) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(loadSession)

  const signIn = useCallback(async (email: string, password: string) => {
    const result = await login(email, password)
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(result))
    setSession(result)
  }, [])

  const signOut = useCallback(() => {
    sessionStorage.removeItem(STORAGE_KEY)
    setSession(null)
  }, [])

  const value = useMemo(() => ({ session, signIn, signOut }), [session, signIn, signOut])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
