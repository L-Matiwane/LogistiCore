import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function ProtectedRoute({ children }: { children: ReactNode }) {
  const { session } = useAuth()
  return session ? <>{children}</> : <Navigate to="/login" replace />
}
