import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { getDashboardSummary, searchParcels } from '../api/parcels'
import type { DashboardSummary, ParcelListItem } from '../api/parcels'
import { useAuth } from '../auth/useAuth'
import StatusBadge from '../components/StatusBadge'
import { dateTime, money } from '../format'

function weekday(isoDate: string) {
  return new Date(`${isoDate}T12:00:00`).toLocaleDateString('en-ZA', { weekday: 'short' })
}

export default function DashboardPage() {
  const { session } = useAuth()
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [recent, setRecent] = useState<ParcelListItem[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false

    Promise.all([getDashboardSummary(), searchParcels({ page: 1, pageSize: 5 })])
      .then(([summaryResult, recentResult]) => {
        if (!cancelled) {
          setSummary(summaryResult)
          setRecent(recentResult.items)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(errorMessage(err))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  const canRegister = session?.role === 'Admin' || session?.role === 'WarehouseClerk'
  const maxDay = Math.max(1, ...(summary?.weeklyVolume.map((d) => d.count) ?? [1]))
  const weekTotal = summary?.weeklyVolume.reduce((sum, d) => sum + d.count, 0) ?? 0

  const cards = summary
    ? [
        { label: 'Registered today', value: summary.parcelsToday, tone: 'blue' },
        { label: 'In warehouse', value: summary.inWarehouse, tone: 'blue' },
        { label: 'Out for delivery', value: summary.outForDelivery, tone: 'pink' },
        { label: 'Delivered today', value: summary.deliveredToday, tone: 'green' },
        { label: 'Exceptions', value: summary.exceptions, tone: 'red' },
      ]
    : []

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Operations overview<span className="dot">.</span>
          </h1>
          <p className="muted">Live figures from the LogistiCore database.</p>
        </div>
        {canRegister && (
          <Link to="/parcels/new" className="btn btn--gradient">
            Register parcel
          </Link>
        )}
      </div>

      {error && (
        <p className="banner banner--error" role="alert">
          {error}
        </p>
      )}
      {loading && <p className="muted">Loading...</p>}

      {summary && (
        <>
          <section className="kpi-grid" aria-label="Key figures">
            {cards.map((card) => (
              <article key={card.label} className={`glass-card kpi kpi--${card.tone}`}>
                <span className="kpi__label">{card.label}</span>
                <span className="kpi__value">{card.value}</span>
              </article>
            ))}
          </section>

          <div className="dash-grid">
            <section className="glass-card panel">
              <h2 className="panel__title">Parcels registered, last 7 days</h2>
              {weekTotal === 0 ? (
                <p className="muted">No parcels registered this week yet.</p>
              ) : (
                <div className="bars" role="img" aria-label="Parcels registered per day for the last 7 days">
                  {summary.weeklyVolume.map((day) => (
                    <div key={day.date} className="bars__col">
                      <span className="bars__count">{day.count}</span>
                      <div className="bars__bar" style={{ height: `${Math.max(4, (day.count / maxDay) * 100)}%` }} />
                      <span className="bars__day">{weekday(day.date)}</span>
                    </div>
                  ))}
                </div>
              )}
            </section>

            <section className="glass-card panel">
              <h2 className="panel__title">Latest parcels</h2>
              {recent.length === 0 ? (
                <p className="muted">Nothing here yet. Register the first parcel to see it listed.</p>
              ) : (
                <ul className="recent">
                  {recent.map((parcel) => (
                    <li key={parcel.parcelId}>
                      <Link to={`/parcels/${parcel.parcelId}`} className="recent__link">
                        <span className="mono">{parcel.trackingNo}</span>
                        <span className="muted">
                          {parcel.recipientName}, {parcel.city} · {money(parcel.price)} · {dateTime(parcel.registeredAt)}
                        </span>
                      </Link>
                      <StatusBadge status={parcel.currentStatus} />
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </div>
        </>
      )}
    </>
  )
}
