import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { getBins, getExceptions, getInventory, receiveParcel, reportException, sortParcel } from '../api/warehouse'
import type { Bin, ExceptionItem, InventoryItem } from '../api/warehouse'
import StatusBadge from '../components/StatusBadge'
import { dateTime } from '../format'

type Mode = 'receive' | 'sort' | 'exception'
type ScanEntry = { id: number; ok: boolean; text: string; time: string }

const modes: { value: Mode; label: string; icon: string }[] = [
  { value: 'receive', label: 'Receive', icon: 'move_to_inbox' },
  { value: 'sort', label: 'Sort to bin', icon: 'shelves' },
  { value: 'exception', label: 'Lost or damaged', icon: 'report_problem' },
]

export default function WarehousePage() {
  const [mode, setMode] = useState<Mode>('receive')
  const [trackingNo, setTrackingNo] = useState('')
  const [binId, setBinId] = useState('')
  const [exceptionType, setExceptionType] = useState<'Lost' | 'Damaged'>('Damaged')
  const [reason, setReason] = useState('')
  const [busy, setBusy] = useState(false)
  const [scans, setScans] = useState<ScanEntry[]>([])

  const [bins, setBins] = useState<Bin[]>([])
  const [inventory, setInventory] = useState<InventoryItem[]>([])
  const [exceptions, setExceptions] = useState<ExceptionItem[]>([])
  const [binFilter, setBinFilter] = useState<number | null>(null)
  const [reloadKey, setReloadKey] = useState(0)
  const [loadError, setLoadError] = useState('')

  const inputRef = useRef<HTMLInputElement>(null)
  const nextId = useRef(1)

  useEffect(() => {
    let cancelled = false

    Promise.all([getBins(), getInventory(binFilter), getExceptions()])
      .then(([binList, items, exceptionList]) => {
        if (!cancelled) {
          setBins(binList)
          setInventory(items)
          setExceptions(exceptionList)
          setLoadError('')
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) setLoadError(errorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [reloadKey, binFilter])

  useEffect(() => {
    inputRef.current?.focus()
  }, [mode])

  function addScan(ok: boolean, text: string) {
    const entry: ScanEntry = {
      id: nextId.current++,
      ok,
      text,
      time: new Date().toLocaleTimeString('en-ZA', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
    }
    setScans((current) => [entry, ...current].slice(0, 8))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const code = trackingNo.trim().toUpperCase()
    if (!code) {
      addScan(false, 'Scan or type a tracking number first.')
      return
    }

    setBusy(true)
    try {
      if (mode === 'receive') {
        const result = await receiveParcel(code)
        addScan(true, result.message)
      } else if (mode === 'sort') {
        if (!binId) {
          addScan(false, 'Choose a bin before scanning.')
          return
        }
        const result = await sortParcel(code, Number(binId))
        addScan(true, result.message)
      } else {
        if (!reason.trim()) {
          addScan(false, 'Describe what happened before reporting.')
          return
        }
        const result = await reportException(code, exceptionType, reason.trim())
        addScan(true, result.message)
        setReason('')
      }
      setTrackingNo('')
      setReloadKey((key) => key + 1)
    } catch (err) {
      addScan(false, errorMessage(err))
    } finally {
      setBusy(false)
      inputRef.current?.focus()
    }
  }

  const zones = bins.reduce<Record<string, Bin[]>>((groups, bin) => {
    ;(groups[bin.zone] ??= []).push(bin)
    return groups
  }, {})

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Warehouse<span className="dot">.</span>
          </h1>
          <p className="muted">Scan parcels in, sort them to bins and report problems.</p>
        </div>
      </div>

      {loadError && (
        <p className="banner banner--error" role="alert">
          {loadError}
        </p>
      )}

      <div className="warehouse-grid">
        <div className="stack">
          <section className="glass-card panel">
            <div className="tabs" role="tablist" aria-label="Warehouse action">
              {modes.map((item) => (
                <button
                  key={item.value}
                  type="button"
                  role="tab"
                  aria-selected={mode === item.value}
                  className={`tab ${mode === item.value ? 'tab--active' : ''}`}
                  onClick={() => setMode(item.value)}
                >
                  <span className="material-symbols-outlined" aria-hidden="true">
                    {item.icon}
                  </span>
                  {item.label}
                </button>
              ))}
            </div>

            <form onSubmit={handleSubmit} noValidate>
              {mode === 'sort' && (
                <label className="field">
                  <span>Destination bin</span>
                  <select value={binId} onChange={(e) => setBinId(e.target.value)}>
                    <option value="">Select a bin</option>
                    {bins.map((bin) => (
                      <option key={bin.binId} value={bin.binId} disabled={bin.occupied >= bin.capacity}>
                        {bin.code} · {bin.zone} ({bin.occupied}/{bin.capacity})
                        {bin.occupied >= bin.capacity ? ' · full' : ''}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              {mode === 'exception' && (
                <>
                  <label className="field">
                    <span>What happened</span>
                    <select value={exceptionType} onChange={(e) => setExceptionType(e.target.value as 'Lost' | 'Damaged')}>
                      <option value="Damaged">Damaged</option>
                      <option value="Lost">Lost</option>
                    </select>
                  </label>
                  <label className="field">
                    <span>Reason</span>
                    <input value={reason} onChange={(e) => setReason(e.target.value)} placeholder="For example: box crushed, contents exposed" />
                  </label>
                </>
              )}

              <label className="field">
                <span>Tracking number</span>
                <input
                  ref={inputRef}
                  className="scan-input"
                  value={trackingNo}
                  onChange={(e) => setTrackingNo(e.target.value)}
                  placeholder="Scan the barcode"
                  autoComplete="off"
                  spellCheck={false}
                />
              </label>

              <button type="submit" className="btn btn--gradient btn--block" disabled={busy}>
                {busy ? 'Working...' : mode === 'receive' ? 'Receive parcel' : mode === 'sort' ? 'Sort parcel' : 'Report problem'}
              </button>
            </form>
            <p className="hint">A USB barcode scanner types the number and presses Enter for you, so keep this box selected.</p>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">Recent scans</h2>
            {scans.length === 0 ? (
              <p className="muted">Nothing scanned yet.</p>
            ) : (
              <ul className="scan-log" aria-live="polite">
                {scans.map((scan) => (
                  <li key={scan.id} className={scan.ok ? 'scan-log__ok' : 'scan-log__err'}>
                    <span className="material-symbols-outlined" aria-hidden="true">
                      {scan.ok ? 'check_circle' : 'error'}
                    </span>
                    <span>{scan.text}</span>
                    <time>{scan.time}</time>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>

        <div className="stack">
          <section className="glass-card panel">
            <h2 className="panel__title">Bins</h2>
            {Object.entries(zones).map(([zone, zoneBins]) => (
              <div key={zone} className="zone">
                <h3 className="zone__title">{zone}</h3>
                <div className="bin-grid">
                  {zoneBins.map((bin) => {
                    const percent = Math.min(100, Math.round((bin.occupied / bin.capacity) * 100))
                    const active = binFilter === bin.binId
                    return (
                      <button
                        key={bin.binId}
                        type="button"
                        className={`bin ${active ? 'bin--active' : ''}`}
                        aria-pressed={active}
                        onClick={() => setBinFilter(active ? null : bin.binId)}
                      >
                        <strong className="mono">{bin.code}</strong>
                        <span>
                          {bin.occupied} of {bin.capacity}
                        </span>
                        <span className="meter" aria-hidden="true">
                          <span className={`meter__fill ${percent >= 90 ? 'meter__fill--hot' : ''}`} style={{ width: `${percent}%` }} />
                        </span>
                      </button>
                    )
                  })}
                </div>
              </div>
            ))}
            <p className="hint">Select a bin to list only its parcels.</p>
          </section>
        </div>
      </div>

      <section className="glass-card table-card section-gap">
        <div className="table-head">
          <h2 className="panel__title">In the warehouse ({inventory.length})</h2>
          {binFilter !== null && (
            <button type="button" className="btn btn--outline" onClick={() => setBinFilter(null)}>
              Show all bins
            </button>
          )}
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Tracking number</th>
                <th>Recipient</th>
                <th>City</th>
                <th>Service</th>
                <th>Bin</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {inventory.length === 0 && (
                <tr>
                  <td colSpan={6} className="muted">
                    No parcels in the warehouse right now.
                  </td>
                </tr>
              )}
              {inventory.map((item) => (
                <tr key={item.parcelId}>
                  <td>
                    <Link to={`/parcels/${item.parcelId}`} className="mono link">
                      {item.trackingNo}
                    </Link>
                  </td>
                  <td>{item.recipientName}</td>
                  <td>{item.city}</td>
                  <td>{item.serviceLevel}</td>
                  <td className="mono">{item.binCode ?? 'Not sorted'}</td>
                  <td>
                    <StatusBadge status={item.currentStatus} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="glass-card table-card section-gap">
        <div className="table-head">
          <h2 className="panel__title">Lost and damaged parcels</h2>
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Tracking number</th>
                <th>Customer</th>
                <th>Type</th>
                <th>Reason</th>
                <th>Reported by</th>
                <th>Reported</th>
              </tr>
            </thead>
            <tbody>
              {exceptions.length === 0 && (
                <tr>
                  <td colSpan={6} className="muted">
                    No exceptions reported.
                  </td>
                </tr>
              )}
              {exceptions.map((item) => (
                <tr key={item.exceptionId}>
                  <td>
                    <span className="mono">{item.trackingNo}</span>
                  </td>
                  <td>{item.customerName}</td>
                  <td>
                    <StatusBadge status={item.type} />
                  </td>
                  <td className="wrap">{item.reason}</td>
                  <td>{item.reportedByName ?? '—'}</td>
                  <td>{dateTime(item.reportedAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </>
  )
}
