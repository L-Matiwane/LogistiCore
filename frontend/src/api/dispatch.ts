import { request } from './client'

export type Driver = { driverId: number; fullName: string; licenceNo: string; phone: string | null; isAvailable: boolean }

export type Vehicle = {
  vehicleId: number
  registration: string
  description: string | null
  capacityKg: number
  isActive: boolean
}

export type DispatchParcel = {
  parcelId: number
  trackingNo: string
  recipientName: string
  city: string
  serviceLevel: string
  weightKg: number
  binCode: string | null
  specialHandling: string[]
}

export type RouteParcel = {
  parcelId: number
  stopSequence: number
  trackingNo: string
  recipientName: string
  deliveryAddress: string
  city: string
  serviceLevel: string
  weightKg: number
  currentStatus: string
}

export type DeliveryRoute = {
  routeId: number
  name: string
  routeDate: string
  driverId: number
  driverName: string
  vehicleId: number
  vehicleRegistration: string
  capacityKg: number
  loadKg: number
  status: string
  parcels: RouteParcel[]
}

export type SaveVehicle = { registration: string; description: string | null; capacityKg: number; isActive: boolean }

export const getDrivers = () => request<Driver[]>('/api/v1/drivers')

export const setDriverAvailability = (driverId: number, isAvailable: boolean) =>
  request<Driver>(`/api/v1/drivers/${driverId}/availability`, { method: 'PUT', body: JSON.stringify({ isAvailable }) })

export const getVehicles = () => request<Vehicle[]>('/api/v1/vehicles')

export const createVehicle = (body: SaveVehicle) =>
  request<Vehicle>('/api/v1/vehicles', { method: 'POST', body: JSON.stringify(body) })

export const updateVehicle = (vehicleId: number, body: SaveVehicle) =>
  request<Vehicle>(`/api/v1/vehicles/${vehicleId}`, { method: 'PUT', body: JSON.stringify(body) })

export const getUnassignedParcels = () => request<DispatchParcel[]>('/api/v1/dispatch/parcels')

export const getRoutes = (date: string) => request<DeliveryRoute[]>(`/api/v1/routes?date=${date}`)

export const createRoute = (body: { name: string; routeDate: string; driverId: number; vehicleId: number }) =>
  request<DeliveryRoute>('/api/v1/routes', { method: 'POST', body: JSON.stringify(body) })

export const addParcelsToRoute = (routeId: number, parcelIds: number[]) =>
  request<DeliveryRoute>(`/api/v1/routes/${routeId}/parcels`, { method: 'POST', body: JSON.stringify({ parcelIds }) })

export const removeParcelFromRoute = (routeId: number, parcelId: number) =>
  request<DeliveryRoute>(`/api/v1/routes/${routeId}/parcels/${parcelId}`, { method: 'DELETE' })

export const releaseRoute = (routeId: number) =>
  request<DeliveryRoute>(`/api/v1/routes/${routeId}/release`, { method: 'POST' })

export const cancelRoute = (routeId: number) =>
  request<DeliveryRoute>(`/api/v1/routes/${routeId}/cancel`, { method: 'POST' })
