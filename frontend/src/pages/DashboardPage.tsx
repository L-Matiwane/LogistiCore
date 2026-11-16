import { useAuth } from '../auth/useAuth'
import Logo from '../components/Logo'

const modules = [
  { name: 'Parcels', text: 'Register parcels, generate tracking numbers and barcodes.' },
  { name: 'Warehouse', text: 'Receive, sort and track parcels inside the warehouse.' },
  { name: 'Dispatch', text: 'Plan routes and assign drivers and vehicles.' },
  { name: 'Deliveries', text: 'Confirm deliveries and record failed attempts.' },
  { name: 'Billing', text: 'Invoices, monthly statements and payments.' },
  { name: 'Reports', text: 'Daily movement, driver performance, revenue and exceptions.' },
]

export default function DashboardPage() {
  const { session, signOut } = useAuth()

  return (
    <div className="app-shell">
      <header className="topbar">
        <Logo size="small" />
        <div className="topbar__user">
          <span className="role-badge">{session?.role}</span>
          <span className="topbar__name">{session?.fullName}</span>
          <button type="button" className="btn btn--outline" onClick={signOut}>
            Sign out
          </button>
        </div>
      </header>

      <main className="content">
        <h1 className="page-title">
          Welcome, {session?.fullName?.split(' ')[0]}
          <span className="dot">.</span>
        </h1>
        <p className="muted">You are signed in. These modules are being built sprint by sprint.</p>

        <div className="module-grid">
          {modules.map((module) => (
            <article key={module.name} className="glass-card module-card">
              <h2>{module.name}</h2>
              <p>{module.text}</p>
              <span className="tag">Coming soon</span>
            </article>
          ))}
        </div>
      </main>
    </div>
  )
}
