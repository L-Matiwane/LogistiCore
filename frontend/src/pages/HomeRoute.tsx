import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import DashboardPage from './DashboardPage'

export default function HomeRoute() {
  const { session } = useAuth()
  return session?.role === 'Driver' ? <Navigate to="/manifest" replace /> : <DashboardPage />
}
