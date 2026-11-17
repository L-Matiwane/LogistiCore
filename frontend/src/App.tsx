import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthProvider'
import AppLayout from './components/AppLayout'
import ProtectedRoute from './components/ProtectedRoute'
import { intakeRoles, navItems, staffRoles } from './navigation'
import ComingSoonPage from './pages/ComingSoonPage'
import HomeRoute from './pages/HomeRoute'
import LoginPage from './pages/LoginPage'
import ParcelDetailPage from './pages/ParcelDetailPage'
import ParcelListPage from './pages/ParcelListPage'
import RegisterParcelPage from './pages/RegisterParcelPage'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<HomeRoute />} />
            <Route
              path="parcels"
              element={
                <ProtectedRoute roles={staffRoles}>
                  <ParcelListPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="parcels/new"
              element={
                <ProtectedRoute roles={intakeRoles}>
                  <RegisterParcelPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="parcels/:id"
              element={
                <ProtectedRoute roles={staffRoles}>
                  <ParcelDetailPage />
                </ProtectedRoute>
              }
            />
            {navItems
              .filter((item) => item.soon)
              .map((item) => (
                <Route
                  key={item.to}
                  path={item.to.slice(1)}
                  element={
                    <ProtectedRoute roles={item.roles}>
                      <ComingSoonPage title={item.label} />
                    </ProtectedRoute>
                  }
                />
              ))}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
