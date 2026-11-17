import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { errorMessage } from '../api/client'
import { getParcel } from '../api/parcels'
import type { Parcel } from '../api/parcels'
import LabelPreview from '../components/LabelPreview'
import StatusBadge from '../components/StatusBadge'
import { dateTime, handlingLabels, kg, money } from '../format'

export default function ParcelDetailPage() {
  const { id } = useParams()
  const [parcel, setParcel] = useState<Parcel | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelled = false
    const parcelId = Number(id)

    if (!Number.isInteger(parcelId)) {
      setError('That parcel link is not valid.')
      return
    }

    getParcel(parcelId)
      .then((data) => {
        if (!cancelled) setParcel(data)
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(errorMessage(err))
      })

    return () => {
      cancelled = true
    }
  }, [id])

  if (error) {
    return (
      <>
        <p className="banner banner--error" role="alert">
          {error}
        </p>
        <Link to="/parcels" className="link">
          Back to the parcel directory
        </Link>
      </>
    )
  }
  if (!parcel) {
    return <p className="muted">Loading...</p>
  }

  const dims =
    parcel.lengthCm && parcel.widthCm && parcel.heightCm
      ? `${parcel.lengthCm} × ${parcel.widthCm} × ${parcel.heightCm} cm`
      : 'Not captured'

  return (
    <>
      <div className="page-head">
        <div>
          <Link to="/parcels" className="link muted">
            ← Parcel directory
          </Link>
          <h1 className="page-title mono">{parcel.trackingNo}</h1>
          <p className="muted">Registered {dateTime(parcel.registeredAt)}</p>
        </div>
        <div className="page-head__actions">
          <StatusBadge status={parcel.currentStatus} />
          <button type="button" className="btn btn--outline" onClick={() => window.print()}>
            Print label
          </button>
        </div>
      </div>

      <div className="detail-grid">
        <div className="stack">
          <section className="glass-card panel">
            <h2 className="panel__title">Recipient</h2>
            <dl className="facts">
              <dt>Name</dt>
              <dd>{parcel.recipientName}</dd>
              <dt>Phone</dt>
              <dd>{parcel.recipientPhone}</dd>
              <dt>Email</dt>
              <dd>{parcel.recipientEmail ?? 'Not captured'}</dd>
              <dt>Address</dt>
              <dd>
                {parcel.deliveryAddress}, {parcel.city} {parcel.postalCode}
              </dd>
              <dt>Instructions</dt>
              <dd>{parcel.deliveryInstructions ?? 'None'}</dd>
            </dl>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">Shipment</h2>
            <dl className="facts">
              <dt>Customer</dt>
              <dd>{parcel.customerName}</dd>
              <dt>Service</dt>
              <dd>{parcel.serviceLevel}</dd>
              <dt>Weight</dt>
              <dd>
                {kg(parcel.weightKg)} actual, {kg(parcel.billedWeightKg)} billed
              </dd>
              <dt>Dimensions</dt>
              <dd>{dims}</dd>
              <dt>Handling</dt>
              <dd>
                {parcel.specialHandling.length === 0
                  ? 'None'
                  : parcel.specialHandling.map((h) => handlingLabels[h] ?? h).join(', ')}
              </dd>
              <dt>Price</dt>
              <dd className="mono">{money(parcel.price)}</dd>
            </dl>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">Status history</h2>
            <ol className="timeline">
              {parcel.history.map((item, index) => (
                <li key={`${item.status}-${index}`}>
                  <StatusBadge status={item.status} />
                  <span className="muted">
                    {dateTime(item.changedAt)}
                    {item.changedByName ? ` · ${item.changedByName}` : ''}
                    {item.note ? ` · ${item.note}` : ''}
                  </span>
                </li>
              ))}
            </ol>
          </section>
        </div>

        <LabelPreview
          trackingNo={parcel.trackingNo}
          fromName={parcel.customerName}
          toName={parcel.recipientName}
          address={parcel.deliveryAddress}
          city={parcel.city}
          postalCode={parcel.postalCode}
          actualWeightKg={parcel.weightKg}
          billedWeightKg={parcel.billedWeightKg}
          serviceLevel={parcel.serviceLevel}
          handling={parcel.specialHandling}
        />
      </div>
    </>
  )
}
