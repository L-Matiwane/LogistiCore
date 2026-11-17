import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ApiError, errorMessage } from '../api/client'
import { getCustomers, quoteParcel, registerParcel } from '../api/parcels'
import type { CustomerOption, Parcel, PriceQuote, ServiceLevel } from '../api/parcels'
import LabelPreview from '../components/LabelPreview'
import { kg, money } from '../format'

type FormState = {
  customerId: string
  recipientName: string
  recipientPhone: string
  recipientEmail: string
  deliveryAddress: string
  city: string
  postalCode: string
  deliveryInstructions: string
  weightKg: string
  lengthCm: string
  widthCm: string
  heightCm: string
  serviceLevel: ServiceLevel
  handling: string[]
}

const emptyForm: FormState = {
  customerId: '',
  recipientName: '',
  recipientPhone: '',
  recipientEmail: '',
  deliveryAddress: '',
  city: '',
  postalCode: '',
  deliveryInstructions: '',
  weightKg: '',
  lengthCm: '',
  widthCm: '',
  heightCm: '',
  serviceLevel: 'Standard',
  handling: [],
}

const tiers: { value: ServiceLevel; title: string; note: string }[] = [
  { value: 'Standard', title: 'Standard', note: '2 to 3 business days' },
  { value: 'Express', title: 'Express', note: 'Next business day' },
  { value: 'Overnight', title: 'Overnight', note: 'Priority, next morning' },
]

const handlingOptions = [
  { value: 'Fragile', label: 'Fragile', icon: 'broken_image' },
  { value: 'KeepUpright', label: 'Keep upright', icon: 'north' },
  { value: 'Perishable', label: 'Perishable', icon: 'ac_unit' },
  { value: 'SignatureRequired', label: 'Signature required', icon: 'draw' },
]

function toNumber(value: string): number | null {
  if (value.trim() === '') return null
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : null
}

export default function RegisterParcelPage() {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [customers, setCustomers] = useState<CustomerOption[]>([])
  const [customersError, setCustomersError] = useState('')
  const [quote, setQuote] = useState<PriceQuote | null>(null)
  const [quoteError, setQuoteError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({})
  const [submitError, setSubmitError] = useState('')
  const [saving, setSaving] = useState(false)
  const [registered, setRegistered] = useState<Parcel | null>(null)

  const weight = toNumber(form.weightKg)
  const length = toNumber(form.lengthCm)
  const width = toNumber(form.widthCm)
  const height = toNumber(form.heightCm)
  const serviceLevel = form.serviceLevel

  useEffect(() => {
    let cancelled = false
    getCustomers()
      .then((list) => {
        if (!cancelled) setCustomers(list)
      })
      .catch((err: unknown) => {
        if (!cancelled) setCustomersError(errorMessage(err))
      })
    return () => {
      cancelled = true
    }
  }, [])

  // Ask the server for a price shortly after the person stops typing.
  useEffect(() => {
    if (weight === null || weight <= 0) return

    let cancelled = false
    const timer = window.setTimeout(() => {
      quoteParcel({ weightKg: weight, lengthCm: length, widthCm: width, heightCm: height, serviceLevel })
        .then((result) => {
          if (!cancelled) {
            setQuote(result)
            setQuoteError('')
          }
        })
        .catch((err: unknown) => {
          if (!cancelled) {
            setQuote(null)
            setQuoteError(errorMessage(err))
          }
        })
    }, 400)

    return () => {
      cancelled = true
      window.clearTimeout(timer)
    }
  }, [weight, length, width, height, serviceLevel])

  const hasWeight = weight !== null && weight > 0
  const shownQuote = hasWeight ? quote : null
  const shownQuoteError = hasWeight ? quoteError : ''

  function setField<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((current) => ({ ...current, [key]: value }))
    setFieldErrors((current) => {
      if (!current[key]) return current
      const next = { ...current }
      delete next[key]
      return next
    })
  }

  function toggleHandling(tag: string) {
    setField('handling', form.handling.includes(tag) ? form.handling.filter((t) => t !== tag) : [...form.handling, tag])
  }

  function validateLocally(): Record<string, string[]> {
    const errors: Record<string, string[]> = {}
    if (!form.customerId) errors.customerId = ['Choose a customer account.']
    if (!form.recipientName.trim()) errors.recipientName = ['Recipient name is required.']
    if (!form.recipientPhone.trim()) errors.recipientPhone = ['Phone number is required.']
    if (!form.deliveryAddress.trim()) errors.deliveryAddress = ['Street address is required.']
    if (!form.city.trim()) errors.city = ['City is required.']
    if (!form.postalCode.trim()) errors.postalCode = ['Postal code is required.']
    if (!hasWeight) errors.weightKg = ['Enter the parcel weight in kg.']
    return errors
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError('')

    const local = validateLocally()
    if (Object.keys(local).length > 0) {
      setFieldErrors(local)
      setSubmitError('Please fix the highlighted fields.')
      return
    }

    setSaving(true)
    setFieldErrors({})

    try {
      const created = await registerParcel({
        customerId: Number(form.customerId),
        recipientName: form.recipientName.trim(),
        recipientPhone: form.recipientPhone.trim(),
        recipientEmail: form.recipientEmail.trim() || null,
        deliveryAddress: form.deliveryAddress.trim(),
        city: form.city.trim(),
        postalCode: form.postalCode.trim(),
        deliveryInstructions: form.deliveryInstructions.trim() || null,
        weightKg: weight ?? 0,
        lengthCm: length,
        widthCm: width,
        heightCm: height,
        serviceLevel: form.serviceLevel,
        specialHandling: form.handling,
      })
      setRegistered(created)
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setFieldErrors(err.fieldErrors)
      }
      setSubmitError(errorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  function startAnother() {
    setForm(emptyForm)
    setQuote(null)
    setQuoteError('')
    setFieldErrors({})
    setSubmitError('')
    setRegistered(null)
  }

  const customerName = customers.find((c) => String(c.customerId) === form.customerId)?.name ?? ''
  const err = (field: string) => fieldErrors[field]?.[0]

  const label = registered
    ? {
        trackingNo: registered.trackingNo,
        fromName: registered.customerName,
        toName: registered.recipientName,
        address: registered.deliveryAddress,
        city: registered.city,
        postalCode: registered.postalCode,
        actualWeightKg: registered.weightKg,
        billedWeightKg: registered.billedWeightKg,
        serviceLevel: registered.serviceLevel,
        handling: registered.specialHandling,
      }
    : {
        trackingNo: null,
        fromName: customerName,
        toName: form.recipientName,
        address: form.deliveryAddress,
        city: form.city,
        postalCode: form.postalCode,
        actualWeightKg: shownQuote?.actualWeightKg ?? null,
        billedWeightKg: shownQuote?.billedWeightKg ?? null,
        serviceLevel: form.serviceLevel,
        handling: form.handling,
      }

  return (
    <>
      <div className="page-head">
        <div>
          <h1 className="page-title">
            Register parcel<span className="dot">.</span>
          </h1>
          <p className="muted">Capture the consignment, check the price and create the tracking label.</p>
        </div>
      </div>

      {registered && (
        <section className="banner banner--success" role="status">
          <div>
            <strong>Parcel registered.</strong> Tracking number <span className="mono">{registered.trackingNo}</span>
          </div>
          <div className="banner__actions">
            <button type="button" className="btn btn--outline" onClick={() => window.print()}>
              Print label
            </button>
            <Link to={`/parcels/${registered.parcelId}`} className="btn btn--outline">
              View parcel
            </Link>
            <button type="button" className="btn btn--gradient" onClick={startAnother}>
              Register another
            </button>
          </div>
        </section>
      )}

      {customersError && (
        <p className="banner banner--error" role="alert">
          {customersError}
        </p>
      )}

      <form className="register-grid" onSubmit={handleSubmit} noValidate>
        <fieldset className="stack form-sections" disabled={registered !== null || saving}>
          <section className="glass-card panel">
            <h2 className="panel__title">1. Customer account</h2>
            <label className="field">
              <span>Customer</span>
              <select value={form.customerId} onChange={(e) => setField('customerId', e.target.value)} aria-invalid={!!err('customerId')}>
                <option value="">Select a customer</option>
                {customers.map((c) => (
                  <option key={c.customerId} value={c.customerId}>
                    {c.name} ({c.accountNo})
                  </option>
                ))}
              </select>
              {err('customerId') && <em className="field-error">{err('customerId')}</em>}
            </label>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">2. Recipient and address</h2>
            <div className="form-grid">
              <label className="field">
                <span>Recipient name</span>
                <input value={form.recipientName} onChange={(e) => setField('recipientName', e.target.value)} autoComplete="off" />
                {err('recipientName') && <em className="field-error">{err('recipientName')}</em>}
              </label>
              <label className="field">
                <span>Mobile phone</span>
                <input
                  type="tel"
                  value={form.recipientPhone}
                  onChange={(e) => setField('recipientPhone', e.target.value)}
                  placeholder="082 123 4567"
                  autoComplete="off"
                />
                {err('recipientPhone') && <em className="field-error">{err('recipientPhone')}</em>}
              </label>
              <label className="field form-grid__wide">
                <span>Email (optional)</span>
                <input
                  type="email"
                  value={form.recipientEmail}
                  onChange={(e) => setField('recipientEmail', e.target.value)}
                  autoComplete="off"
                />
                {err('recipientEmail') && <em className="field-error">{err('recipientEmail')}</em>}
              </label>
              <label className="field form-grid__wide">
                <span>Street address</span>
                <input value={form.deliveryAddress} onChange={(e) => setField('deliveryAddress', e.target.value)} autoComplete="off" />
                {err('deliveryAddress') && <em className="field-error">{err('deliveryAddress')}</em>}
              </label>
              <label className="field">
                <span>City</span>
                <input value={form.city} onChange={(e) => setField('city', e.target.value)} autoComplete="off" />
                {err('city') && <em className="field-error">{err('city')}</em>}
              </label>
              <label className="field">
                <span>Postal code</span>
                <input value={form.postalCode} onChange={(e) => setField('postalCode', e.target.value)} autoComplete="off" />
                {err('postalCode') && <em className="field-error">{err('postalCode')}</em>}
              </label>
              <label className="field form-grid__wide">
                <span>Delivery instructions (optional)</span>
                <input
                  value={form.deliveryInstructions}
                  onChange={(e) => setField('deliveryInstructions', e.target.value)}
                  placeholder="Gate code, delivery hours, where to leave the parcel"
                  autoComplete="off"
                />
                {err('deliveryInstructions') && <em className="field-error">{err('deliveryInstructions')}</em>}
              </label>
            </div>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">3. Cargo</h2>
            <div className="form-grid form-grid--four">
              <label className="field">
                <span>Weight (kg)</span>
                <input
                  type="number"
                  inputMode="decimal"
                  min="0"
                  step="0.1"
                  value={form.weightKg}
                  onChange={(e) => setField('weightKg', e.target.value)}
                />
                {err('weightKg') && <em className="field-error">{err('weightKg')}</em>}
              </label>
              <label className="field">
                <span>Length (cm)</span>
                <input type="number" inputMode="decimal" min="0" value={form.lengthCm} onChange={(e) => setField('lengthCm', e.target.value)} />
              </label>
              <label className="field">
                <span>Width (cm)</span>
                <input type="number" inputMode="decimal" min="0" value={form.widthCm} onChange={(e) => setField('widthCm', e.target.value)} />
              </label>
              <label className="field">
                <span>Height (cm)</span>
                <input type="number" inputMode="decimal" min="0" value={form.heightCm} onChange={(e) => setField('heightCm', e.target.value)} />
              </label>
            </div>
            {err('dimensions') && <em className="field-error">{err('dimensions')}</em>}
            <p className="hint">Dimensions are optional. When given, the parcel is billed on whichever is larger: actual or volumetric weight (L × W × H ÷ 5000).</p>

            <div className="chips" role="group" aria-label="Special handling">
              {handlingOptions.map((option) => {
                const active = form.handling.includes(option.value)
                return (
                  <button
                    key={option.value}
                    type="button"
                    className={`chip ${active ? 'chip--active' : ''}`}
                    aria-pressed={active}
                    onClick={() => toggleHandling(option.value)}
                  >
                    <span className="material-symbols-outlined" aria-hidden="true">
                      {option.icon}
                    </span>
                    {option.label}
                  </button>
                )
              })}
            </div>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">4. Service level</h2>
            <div className="tier-grid" role="radiogroup" aria-label="Service level">
              {tiers.map((tier) => (
                <button
                  key={tier.value}
                  type="button"
                  role="radio"
                  aria-checked={form.serviceLevel === tier.value}
                  className={`tier ${form.serviceLevel === tier.value ? 'tier--active' : ''}`}
                  onClick={() => setField('serviceLevel', tier.value)}
                >
                  <strong>{tier.title}</strong>
                  <span>{tier.note}</span>
                </button>
              ))}
            </div>
          </section>
        </fieldset>

        <aside className="stack side-column">
          <section className="glass-card panel">
            <h2 className="panel__title">Price</h2>
            {shownQuote ? (
              <dl className="facts facts--tight">
                <dt>Service</dt>
                <dd>{shownQuote.serviceLevel}</dd>
                <dt>Actual weight</dt>
                <dd>{kg(shownQuote.actualWeightKg)}</dd>
                <dt>Volumetric weight</dt>
                <dd>{shownQuote.volumetricWeightKg > 0 ? kg(shownQuote.volumetricWeightKg) : 'Not used'}</dd>
                <dt>Billed weight</dt>
                <dd>{kg(shownQuote.billedWeightKg)}</dd>
              </dl>
            ) : (
              <p className="muted">{shownQuoteError || 'Enter the weight to see the price.'}</p>
            )}
            <div className="total">
              <span>Total</span>
              <strong>{shownQuote ? money(shownQuote.price) : '—'}</strong>
            </div>
            <p className="hint">Price comes from the rate card for the billed weight and service level. VAT is added on the invoice.</p>
          </section>

          <section className="glass-card panel">
            <h2 className="panel__title">Label preview</h2>
            <LabelPreview {...label} />
          </section>

          {submitError && !registered && (
            <p className="banner banner--error" role="alert">
              {submitError}
            </p>
          )}

          <div className="actions">
            <button type="submit" className="btn btn--gradient btn--block" disabled={saving || registered !== null}>
              {saving ? 'Registering...' : 'Register parcel'}
            </button>
            <button type="button" className="btn btn--outline btn--block" onClick={startAnother} disabled={saving}>
              Clear form
            </button>
          </div>
        </aside>
      </form>
    </>
  )
}
