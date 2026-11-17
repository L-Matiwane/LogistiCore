import { useState } from 'react'
import type { FormEvent } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { navItems } from '../navigation'
import Logo from './Logo'

export default function AppLayout() {
  const { session, signOut } = useAuth()
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)

  const role = session?.role ?? ''
  const items = navItems.filter((item) => item.roles.includes(role))
  const initials = (session?.fullName ?? '?')
    .split(' ')
    .map((part) => part[0])
    .slice(0, 2)
    .join('')
    .toUpperCase()

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = String(new FormData(event.currentTarget).get('q') ?? '').trim()
    navigate(value ? `/parcels?q=${encodeURIComponent(value)}` : '/parcels')
    setMenuOpen(false)
  }

  const canSearch = ['Admin', 'WarehouseClerk', 'Dispatcher', 'Billing'].includes(role)

  return (
    <div className="shell">
      <aside className={`sidebar ${menuOpen ? 'sidebar--open' : ''}`}>
        <div className="sidebar__brand">
          <Logo size="small" />
        </div>
        <nav className="sidebar__nav" aria-label="Main">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) => `nav-link ${isActive ? 'nav-link--active' : ''}`}
              onClick={() => setMenuOpen(false)}
            >
              <span className="material-symbols-outlined" aria-hidden="true">
                {item.icon}
              </span>
              <span>{item.label}</span>
              {item.soon && <span className="nav-link__soon">Soon</span>}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="main-area">
        <header className="topbar2">
          <button
            type="button"
            className="icon-btn menu-btn"
            aria-label="Open menu"
            aria-expanded={menuOpen}
            onClick={() => setMenuOpen((open) => !open)}
          >
            <span className="material-symbols-outlined">menu</span>
          </button>

          {canSearch ? (
            <form className="searchbox" onSubmit={handleSearch} role="search">
              <span className="material-symbols-outlined" aria-hidden="true">
                search
              </span>
              <input name="q" type="search" placeholder="Search tracking number, recipient or customer" aria-label="Search parcels" />
            </form>
          ) : (
            <div className="searchbox-spacer" />
          )}

          <div className="topbar2__user">
            <span className="role-badge">{role}</span>
            <span className="avatar" aria-hidden="true">
              {initials}
            </span>
            <span className="topbar2__name">{session?.fullName}</span>
            <button type="button" className="icon-btn" onClick={signOut} title="Sign out" aria-label="Sign out">
              <span className="material-symbols-outlined">logout</span>
            </button>
          </div>
        </header>

        <main className="page">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
