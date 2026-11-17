import { request } from './client'

export type ServiceLevel = 'Standard' | 'Express' | 'Overnight'

export type CustomerOption = { customerId: number; accountNo: string; name: string }

export type QuoteRequest = {
  weightKg: number
  lengthCm: number | null
  widthCm: number | null
  heightCm: number | null
  serviceLevel: ServiceLevel
}

export type PriceQuote = {
  actualWeightKg: number
  volumetricWeightKg: number
  billedWeightKg: number
  serviceLevel: string
  price: number
}

export type RegisterParcelRequest = QuoteRequest & {
  customerId: number
  recipientName: string
  recipientPhone: string
  recipientEmail: string | null
  deliveryAddress: string
  city: string
  postalCode: string
  deliveryInstructions: string | null
  specialHandling: string[]
}

export type StatusHistoryItem = {
  status: string
  changedByName: string | null
  changedAt: string
  note: string | null
}

export type Parcel = {
  parcelId: number
  trackingNo: string
  customerId: number
  customerName: string
  recipientName: string
  recipientPhone: string
  recipientEmail: string | null
  deliveryAddress: string
  city: string
  postalCode: string
  deliveryInstructions: string | null
  specialHandling: string[]
  weightKg: number
  lengthCm: number | null
  widthCm: number | null
  heightCm: number | null
  billedWeightKg: number
  serviceLevel: string
  price: number
  currentStatus: string
  registeredAt: string
  history: StatusHistoryItem[]
}

export type ParcelListItem = {
  parcelId: number
  trackingNo: string
  customerName: string
  recipientName: string
  deliveryAddress: string
  city: string
  serviceLevel: string
  price: number
  currentStatus: string
  registeredAt: string
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export type DashboardSummary = {
  parcelsToday: number
  inWarehouse: number
  outForDelivery: number
  deliveredToday: number
  exceptions: number
  weeklyVolume: { date: string; count: number }[]
}

export type ParcelSearchParams = {
  q?: string
  status?: string
  page?: number
  pageSize?: number
}

export const getCustomers = () => request<CustomerOption[]>('/api/v1/customers')

export const quoteParcel = (body: QuoteRequest) =>
  request<PriceQuote>('/api/v1/parcels/quote', { method: 'POST', body: JSON.stringify(body) })

export const registerParcel = (body: RegisterParcelRequest) =>
  request<Parcel>('/api/v1/parcels', { method: 'POST', body: JSON.stringify(body) })

export const getParcel = (id: number) => request<Parcel>(`/api/v1/parcels/${id}`)

export function searchParcels(params: ParcelSearchParams) {
  const query = new URLSearchParams()
  if (params.q) query.set('q', params.q)
  if (params.status) query.set('status', params.status)
  query.set('page', String(params.page ?? 1))
  query.set('pageSize', String(params.pageSize ?? 20))
  return request<PagedResult<ParcelListItem>>(`/api/v1/parcels?${query.toString()}`)
}

export const getDashboardSummary = () => request<DashboardSummary>('/api/v1/dashboard/summary')
