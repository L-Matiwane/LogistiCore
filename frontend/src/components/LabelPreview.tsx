import BarcodeSvg from './BarcodeSvg'
import { handlingLabels, kg } from '../format'

type LabelPreviewProps = {
  trackingNo: string | null
  fromName: string
  toName: string
  address: string
  city: string
  postalCode: string
  actualWeightKg: number | null
  billedWeightKg: number | null
  serviceLevel: string
  handling: string[]
}

export default function LabelPreview(props: LabelPreviewProps) {
  const { trackingNo } = props

  return (
    <div className="label-sheet print-area">
      <div className="label-head">
        <div>
          <div className="label-brand">
            FORTE<span>COURIER</span>
          </div>
          <div className="label-sub">Courier &amp; Logistics</div>
        </div>
        <div className="label-service">{props.serviceLevel.toUpperCase()}</div>
      </div>

      <div className="label-cols">
        <div>
          <div className="label-key">From</div>
          <div className="label-val">{props.fromName || '—'}</div>
        </div>
        <div>
          <div className="label-key">Ship to</div>
          <div className="label-val label-val--big">{props.toName || '—'}</div>
          <div className="label-val">{props.address || '—'}</div>
          <div className="label-val label-mono">
            {[props.city, props.postalCode].filter(Boolean).join(' ').toUpperCase() || '—'}
          </div>
        </div>
      </div>

      <div className="label-specs">
        <div>
          <span>Actual</span>
          <strong>{props.actualWeightKg === null ? '—' : kg(props.actualWeightKg)}</strong>
        </div>
        <div>
          <span>Billed</span>
          <strong>{props.billedWeightKg === null ? '—' : kg(props.billedWeightKg)}</strong>
        </div>
        <div>
          <span>Handling</span>
          <strong>
            {props.handling.length === 0 ? 'None' : props.handling.map((h) => handlingLabels[h] ?? h).join(', ')}
          </strong>
        </div>
      </div>

      <div className="label-barcode">
        {trackingNo ? (
          <>
            <BarcodeSvg value={trackingNo} />
            <div className="label-tracking">{trackingNo}</div>
          </>
        ) : (
          <div className="label-pending">Tracking number and barcode are created when you register the parcel.</div>
        )}
      </div>
    </div>
  )
}
