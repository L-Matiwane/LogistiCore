import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { errorMessage } from '../api/client'
import { createVehicle, getDrivers, getVehicles, setDriverAvailability, updateVehicle } from '../api/dispatch'
import type { Driver, Vehicle } from '../api/dispatch'
import { kg } from '../format'

export default function FleetPage() {
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [registration, setRegistration] = useState('')
  const [description, setDescription] = useState('')
  const [capacity, setCapacity] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let cancelled = false
    Promise.all([getDrivers(), getVehicles()])
      .then(([driverList, vehicleList]) => {
        if (!cancelled) {
          setDrivers(driverList)
          setVehicles(vehicleList)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(errorMessage(err))
      })
    return () => {
      cancelled = true
    }
  }, [reloadKey])

  async function act(action: () => Promise<unknown>) {
    setBusy(true)
    setError('')
    try {
      await action()
      setReloadKey((key) => key + 1)
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  function handleAdd(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const capacityKg = Number(capacity)
    if (!registration.trim() || !Number.isFinite(capacityKg) || capacityKg <= 0) {
      setError('Enter a registration number and a capacity in kg.')
      return
    }
    void act(async () => {
      await createVehicle({ registration: registration.trim(), description: description.trim() || null, capacityKg, isActive: true })
      setRegistration('')
      setDescription('')
      setCapacity('')
    })
  }

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Drivers and vehicles<span className="dot">.</span>
          </h1>
          <p className="muted">The fleet that dispatch can put on routes.</p>
        </div>
      </div>

      {error && (
        <p className="banner banner--error" role="alert">
          {error}
        </p>
      )}

      <section className="glass-card table-card">
        <div className="table-head">
          <h2 className="panel__title">Drivers</h2>
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Licence</th>
                <th>Phone</th>
                <th>Availability</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {drivers.map((driver) => (
                <tr key={driver.driverId}>
                  <td>{driver.fullName}</td>
                  <td className="mono">{driver.licenceNo}</td>
                  <td>{driver.phone ?? '—'}</td>
                  <td>
                    <span className={`status ${driver.isAvailable ? 'status--delivered' : 'status--failedattempt'}`}>
                      {driver.isAvailable ? 'Available' : 'Unavailable'}
                    </span>
                  </td>
                  <td>
                    <button
                      type="button"
                      className="btn btn--outline btn--small"
                      disabled={busy}
                      onClick={() => void act(() => setDriverAvailability(driver.driverId, !driver.isAvailable))}
                    >
                      Mark {driver.isAvailable ? 'unavailable' : 'available'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <p className="hint table-hint">New drivers are created together with their login on the Users page, which is planned for a later sprint.</p>
      </section>

      <section className="glass-card table-card section-gap">
        <div className="table-head">
          <h2 className="panel__title">Vehicles</h2>
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Registration</th>
                <th>Description</th>
                <th className="num">Capacity</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {vehicles.map((vehicle) => (
                <tr key={vehicle.vehicleId}>
                  <td className="mono">{vehicle.registration}</td>
                  <td>{vehicle.description ?? '—'}</td>
                  <td className="num">{kg(vehicle.capacityKg)}</td>
                  <td>
                    <span className={`status ${vehicle.isActive ? 'status--delivered' : 'status--returned'}`}>
                      {vehicle.isActive ? 'In service' : 'Out of service'}
                    </span>
                  </td>
                  <td>
                    <button
                      type="button"
                      className="btn btn--outline btn--small"
                      disabled={busy}
                      onClick={() =>
                        void act(() =>
                          updateVehicle(vehicle.vehicleId, {
                            registration: vehicle.registration,
                            description: vehicle.description,
                            capacityKg: vehicle.capacityKg,
                            isActive: !vehicle.isActive,
                          }),
                        )
                      }
                    >
                      {vehicle.isActive ? 'Take out of service' : 'Put in service'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <form className="fleet-form" onSubmit={handleAdd} noValidate>
          <label className="field">
            <span>Registration</span>
            <input value={registration} onChange={(e) => setRegistration(e.target.value)} placeholder="EL 789 EC" autoComplete="off" />
          </label>
          <label className="field">
            <span>Description (optional)</span>
            <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Panel van" autoComplete="off" />
          </label>
          <label className="field">
            <span>Capacity (kg)</span>
            <input type="number" min="1" value={capacity} onChange={(e) => setCapacity(e.target.value)} />
          </label>
          <button type="submit" className="btn btn--gradient" disabled={busy}>
            Add vehicle
          </button>
        </form>
      </section>
    </>
  )
}
