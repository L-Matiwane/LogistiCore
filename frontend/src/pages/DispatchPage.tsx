import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { errorMessage } from '../api/client'
import {
  addParcelsToRoute,
  cancelRoute,
  createRoute,
  getDrivers,
  getRoutes,
  getUnassignedParcels,
  getVehicles,
  releaseRoute,
  removeParcelFromRoute,
} from '../api/dispatch'
import type { DeliveryRoute, DispatchParcel, Driver, Vehicle } from '../api/dispatch'
import StatusBadge from '../components/StatusBadge'
import { handlingLabels, kg } from '../format'

function todayIso() {
  return new Date().toLocaleDateString('en-CA')
}

export default function DispatchPage() {
  const [date, setDate] = useState(todayIso)
  const [routes, setRoutes] = useState<DeliveryRoute[]>([])
  const [parcels, setParcels] = useState<DispatchParcel[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [selected, setSelected] = useState<number[]>([])

  const [routeName, setRouteName] = useState('')
  const [driverId, setDriverId] = useState('')
  const [vehicleId, setVehicleId] = useState('')

  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let cancelled = false

    Promise.all([getRoutes(date), getUnassignedParcels(), getDrivers(), getVehicles()])
      .then(([routeList, parcelList, driverList, vehicleList]) => {
        if (cancelled) return
        setRoutes(routeList)
        setParcels(parcelList)
        setDrivers(driverList)
        setVehicles(vehicleList)
        setSelected((current) => current.filter((id) => parcelList.some((p) => p.parcelId === id)))
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(errorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [date, reloadKey])

  async function act(action: () => Promise<unknown>, success: string) {
    setBusy(true)
    setError('')
    setNotice('')
    try {
      await action()
      setNotice(success)
      setReloadKey((key) => key + 1)
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!routeName.trim() || !driverId || !vehicleId) {
      setError('Give the route a name and choose a driver and a vehicle.')
      return
    }
    void act(async () => {
      await createRoute({ name: routeName.trim(), routeDate: date, driverId: Number(driverId), vehicleId: Number(vehicleId) })
      setRouteName('')
      setDriverId('')
      setVehicleId('')
    }, 'Route created.')
  }

  function toggle(parcelId: number) {
    setSelected((current) => (current.includes(parcelId) ? current.filter((id) => id !== parcelId) : [...current, parcelId]))
  }

  const selectedKg = parcels.filter((p) => selected.includes(p.parcelId)).reduce((sum, p) => sum + p.weightKg, 0)
  const busyDrivers = new Set(routes.map((r) => r.driverId))
  const busyVehicles = new Set(routes.map((r) => r.vehicleId))

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Dispatch<span className="dot">.</span>
          </h1>
          <p className="muted">Plan routes, choose a driver and vehicle, and load sorted parcels.</p>
        </div>
        <label className="field field--inline date-field">
          <span>Route date</span>
          <input type="date" value={date} onChange={(e) => e.target.value && setDate(e.target.value)} />
        </label>
      </div>

      {error && (
        <p className="banner banner--error" role="alert">
          {error}
        </p>
      )}
      {notice && (
        <p className="banner banner--success" role="status">
          {notice}
        </p>
      )}

      <form className="glass-card panel new-route" onSubmit={handleCreate} noValidate>
        <h2 className="panel__title">New route for {date}</h2>
        <div className="new-route__fields">
          <label className="field">
            <span>Route name</span>
            <input value={routeName} onChange={(e) => setRouteName(e.target.value)} placeholder="Route A - Beacon Bay" autoComplete="off" />
          </label>
          <label className="field">
            <span>Driver</span>
            <select value={driverId} onChange={(e) => setDriverId(e.target.value)}>
              <option value="">Select a driver</option>
              {drivers.map((driver) => {
                const unavailable = !driver.isAvailable
                const onRoute = busyDrivers.has(driver.driverId)
                return (
                  <option key={driver.driverId} value={driver.driverId} disabled={unavailable || onRoute}>
                    {driver.fullName}
                    {unavailable ? ' (unavailable)' : onRoute ? ' (already on a route)' : ''}
                  </option>
                )
              })}
            </select>
          </label>
          <label className="field">
            <span>Vehicle</span>
            <select value={vehicleId} onChange={(e) => setVehicleId(e.target.value)}>
              <option value="">Select a vehicle</option>
              {vehicles.map((vehicle) => {
                const onRoute = busyVehicles.has(vehicle.vehicleId)
                return (
                  <option key={vehicle.vehicleId} value={vehicle.vehicleId} disabled={!vehicle.isActive || onRoute}>
                    {vehicle.registration} · {kg(vehicle.capacityKg)}
                    {!vehicle.isActive ? ' (not in service)' : onRoute ? ' (already on a route)' : ''}
                  </option>
                )
              })}
            </select>
          </label>
          <button type="submit" className="btn btn--gradient" disabled={busy}>
            Create route
          </button>
        </div>
      </form>

      <div className="dispatch-grid">
        <section className="glass-card panel">
          <h2 className="panel__title">Sorted parcels waiting ({parcels.length})</h2>
          {parcels.length === 0 ? (
            <p className="muted">No sorted parcels are waiting. Receive and sort parcels in the warehouse first.</p>
          ) : (
            <>
              <ul className="pick-list">
                {parcels.map((parcel) => (
                  <li key={parcel.parcelId}>
                    <label className="pick-item">
                      <input type="checkbox" checked={selected.includes(parcel.parcelId)} onChange={() => toggle(parcel.parcelId)} />
                      <span className="pick-item__main">
                        <span className="mono">{parcel.trackingNo}</span>
                        <span className="muted">
                          {parcel.recipientName}, {parcel.city} · {parcel.serviceLevel} · {kg(parcel.weightKg)}
                          {parcel.binCode ? ` · bin ${parcel.binCode}` : ''}
                        </span>
                        {parcel.specialHandling.length > 0 && (
                          <span className="pick-item__tags">{parcel.specialHandling.map((h) => handlingLabels[h] ?? h).join(', ')}</span>
                        )}
                      </span>
                    </label>
                  </li>
                ))}
              </ul>
              <p className="hint">
                {selected.length} selected · {kg(selectedKg)}
              </p>
            </>
          )}
        </section>

        <div className="stack">
          {routes.length === 0 && (
            <section className="glass-card panel">
              <p className="muted">No routes planned for {date}. Create one above.</p>
            </section>
          )}

          {routes.map((route) => {
            const percent = Math.min(100, Math.round((route.loadKg / route.capacityKg) * 100))
            const planned = route.status === 'Planned'
            const canCancel = route.status === 'Planned' || route.status === 'Released'
            return (
              <section key={route.routeId} className="glass-card panel route-card">
                <div className="route-card__head">
                  <div>
                    <h3 className="route-card__title">{route.name}</h3>
                    <p className="muted">
                      {route.driverName} · {route.vehicleRegistration}
                    </p>
                  </div>
                  <StatusBadge status={route.status} />
                </div>

                <div className="route-card__load">
                  <span>
                    Load {kg(route.loadKg)} of {kg(route.capacityKg)}
                  </span>
                  <span className="meter" aria-hidden="true">
                    <span className={`meter__fill ${percent >= 90 ? 'meter__fill--hot' : ''}`} style={{ width: `${percent}%` }} />
                  </span>
                </div>

                {route.parcels.length === 0 ? (
                  <p className="muted">No parcels yet.</p>
                ) : (
                  <ol className="stops">
                    {route.parcels.map((stop) => (
                      <li key={stop.parcelId}>
                        <span className="stops__no">{stop.stopSequence}</span>
                        <span className="stops__main">
                          <Link to={`/parcels/${stop.parcelId}`} className="mono link">
                            {stop.trackingNo}
                          </Link>
                          <span className="muted">
                            {stop.recipientName}, {stop.city} · {kg(stop.weightKg)}
                          </span>
                        </span>
                        {planned && (
                          <button
                            type="button"
                            className="btn btn--outline btn--small"
                            disabled={busy}
                            onClick={() => void act(() => removeParcelFromRoute(route.routeId, stop.parcelId), 'Parcel removed from the route.')}
                          >
                            Remove
                          </button>
                        )}
                      </li>
                    ))}
                  </ol>
                )}

                <div className="route-card__actions">
                  {planned && (
                    <button
                      type="button"
                      className="btn btn--outline btn--small"
                      disabled={busy || selected.length === 0}
                      onClick={() => void act(() => addParcelsToRoute(route.routeId, selected), `Added ${selected.length} parcel(s) to ${route.name}.`)}
                    >
                      Add selected ({selected.length})
                    </button>
                  )}
                  {planned && (
                    <button
                      type="button"
                      className="btn btn--gradient btn--small"
                      disabled={busy || route.parcels.length === 0}
                      onClick={() => void act(() => releaseRoute(route.routeId), `${route.name} released to the driver.`)}
                    >
                      Release route
                    </button>
                  )}
                  {canCancel && (
                    <button
                      type="button"
                      className="btn btn--outline btn--small btn--danger"
                      disabled={busy}
                      onClick={() => {
                        if (window.confirm(`Cancel ${route.name}? Its parcels go back to the sorted list.`)) {
                          void act(() => cancelRoute(route.routeId), `${route.name} cancelled.`)
                        }
                      }}
                    >
                      Cancel route
                    </button>
                  )}
                </div>
              </section>
            )
          })}
        </div>
      </div>
    </>
  )
}
