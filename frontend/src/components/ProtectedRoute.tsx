import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

type ProtectedRouteProps = {
  children: ReactNode
  roles?: string[]
}

export default function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const { session } = useAuth()

  if (!session) {
    return <Navigate to="/login" replace />
  }
  if (roles && !roles.includes(session.role)) {
    return <Navigate to="/" replace />
  }
  return <>{children}</>
}
