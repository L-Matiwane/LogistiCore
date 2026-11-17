import { createContext } from 'react'

export const SESSION_KEY = 'logisticore.session'

export type Session = {
  token: string
  fullName: string
  role: string
}

export type AuthContextValue = {
  session: Session | null
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
