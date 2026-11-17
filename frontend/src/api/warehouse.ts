import { request } from './client'

export type Bin = { binId: number; zone: string; code: string; capacity: number; occupied: number }

export type ScanResult = {
  parcelId: number
  trackingNo: string
  recipientName: string
  city: string
  serviceLevel: string
  currentStatus: string
  binCode: string | null
  message: string
}

export type InventoryItem = {
  parcelId: number
  trackingNo: string
  recipientName: string
  city: string
  serviceLevel: string
  currentStatus: string
  binCode: string | null
}

export type ExceptionItem = {
  exceptionId: number
  trackingNo: string
  customerName: string
  type: string
  reason: string
  reportedByName: string | null
  reportedAt: string
  resolved: boolean
}

export const getBins = () => request<Bin[]>('/api/v1/warehouse/bins')

export const getInventory = (binId: number | null) =>
  request<InventoryItem[]>(`/api/v1/warehouse/inventory${binId ? `?binId=${binId}` : ''}`)

export const getExceptions = () => request<ExceptionItem[]>('/api/v1/warehouse/exceptions')

export const receiveParcel = (trackingNo: string) =>
  request<ScanResult>('/api/v1/warehouse/receive', { method: 'POST', body: JSON.stringify({ trackingNo }) })

export const sortParcel = (trackingNo: string, binId: number) =>
  request<ScanResult>('/api/v1/warehouse/sort', { method: 'POST', body: JSON.stringify({ trackingNo, binId }) })

export const reportException = (trackingNo: string, type: 'Lost' | 'Damaged', reason: string) =>
  request<ScanResult>('/api/v1/warehouse/exceptions', {
    method: 'POST',
    body: JSON.stringify({ trackingNo, type, reason }),
  })
