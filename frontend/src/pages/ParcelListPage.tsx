import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { searchParcels } from '../api/parcels'
import type { PagedResult, ParcelListItem } from '../api/parcels'
import StatusBadge from '../components/StatusBadge'
import { dateTime, money, statusLabel } from '../format'

const statuses = [
  'Registered',
  'AtWarehouse',
  'Sorted',
  'Assigned',
  'OutForDelivery',
  'Delivered',
  'FailedAttempt',
  'Returned',
  'Lost',
  'Damaged',
]

const PAGE_SIZE = 15

export default function ParcelListPage() {
  const [params, setParams] = useSearchParams()
  const q = params.get('q') ?? ''
  const status = params.get('status') ?? ''
  const page = Math.max(1, Number(params.get('page') ?? '1') || 1)

  const [result, setResult] = useState<PagedResult<ParcelListItem> | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError('')

    searchParcels({ q, status, page, pageSize: PAGE_SIZE })
      .then((data) => {
        if (!cancelled) setResult(data)
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
  }, [q, status, page])

  function updateParams(next: { q?: string; status?: string; page?: number }) {
    const merged = new URLSearchParams(params)
    for (const [key, value] of Object.entries(next)) {
      if (value === undefined || value === '' || value === 1) merged.delete(key)
      else merged.set(key, String(value))
    }
    setParams(merged)
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = String(new FormData(event.currentTarget).get('q') ?? '').trim()
    updateParams({ q: value, page: 1 })
  }

  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / PAGE_SIZE)) : 1

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Parcel directory<span className="dot">.</span>
          </h1>
          <p className="muted">Search every registered parcel.</p>
        </div>
      </div>

      <form key={q} className="filters glass-card" onSubmit={handleSubmit}>
        <label className="field field--inline">
          <span>Search</span>
          <input name="q" type="search" defaultValue={q} placeholder="Tracking number, recipient or customer" />
        </label>
        <label className="field field--inline">
          <span>Status</span>
          <select value={status} onChange={(e) => updateParams({ status: e.target.value, page: 1 })}>
            <option value="">All statuses</option>
            {statuses.map((s) => (
              <option key={s} value={s}>
                {statusLabel(s)}
              </option>
            ))}
          </select>
        </label>
        <button type="submit" className="btn btn--gradient">
          Search
        </button>
      </form>

      {error && (
        <p className="banner banner--error" role="alert">
          {error}
        </p>
      )}

      <div className="glass-card table-card">
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Tracking number</th>
                <th>Customer</th>
                <th>Recipient</th>
                <th>City</th>
                <th>Service</th>
                <th className="num">Price</th>
                <th>Status</th>
                <th>Registered</th>
              </tr>
            </thead>
            <tbody>
              {loading && (
                <tr>
                  <td colSpan={8} className="muted">
                    Loading...
                  </td>
                </tr>
              )}
              {!loading && result?.items.length === 0 && (
                <tr>
                  <td colSpan={8} className="muted">
                    No parcels match your search.
                  </td>
                </tr>
              )}
              {!loading &&
                result?.items.map((parcel) => (
                  <tr key={parcel.parcelId}>
                    <td>
                      <Link to={`/parcels/${parcel.parcelId}`} className="mono link">
                        {parcel.trackingNo}
                      </Link>
                    </td>
                    <td>{parcel.customerName}</td>
                    <td>{parcel.recipientName}</td>
                    <td>{parcel.city}</td>
                    <td>{parcel.serviceLevel}</td>
                    <td className="num">{money(parcel.price)}</td>
                    <td>
                      <StatusBadge status={parcel.currentStatus} />
                    </td>
                    <td>{dateTime(parcel.registeredAt)}</td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>

        {result && result.totalCount > 0 && (
          <div className="pagination">
            <span className="muted">
              {result.totalCount} parcel{result.totalCount === 1 ? '' : 's'} · page {page} of {totalPages}
            </span>
            <div className="pagination__buttons">
              <button type="button" className="btn btn--outline" disabled={page <= 1} onClick={() => updateParams({ page: page - 1 })}>
                Previous
              </button>
              <button
                type="button"
                className="btn btn--outline"
                disabled={page >= totalPages}
                onClick={() => updateParams({ page: page + 1 })}
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>
    </>
  )
}
