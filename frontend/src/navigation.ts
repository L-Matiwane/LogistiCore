export type NavItem = {
  to: string
  label: string
  icon: string
  roles: string[]
  end?: boolean
  soon?: boolean
}

export const intakeRoles = ['Admin', 'WarehouseClerk']
export const staffRoles = ['Admin', 'WarehouseClerk', 'Dispatcher', 'Billing']

export const navItems: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: 'dashboard', roles: staffRoles, end: true },
  { to: '/parcels/new', label: 'Register parcel', icon: 'inventory_2', roles: intakeRoles },
  { to: '/parcels', label: 'Parcel directory', icon: 'local_shipping', roles: staffRoles, end: true },
  { to: '/warehouse', label: 'Warehouse', icon: 'qr_code_scanner', roles: intakeRoles },
  { to: '/dispatch', label: 'Dispatch & routes', icon: 'alt_route', roles: ['Admin', 'Dispatcher'] },
  { to: '/fleet', label: 'Drivers & vehicles', icon: 'directions_car', roles: ['Admin', 'Dispatcher'] },
  { to: '/manifest', label: 'My manifest', icon: 'person_pin_circle', roles: ['Driver'], soon: true },
  { to: '/billing', label: 'Billing', icon: 'receipt_long', roles: ['Admin', 'Billing'], soon: true },
  { to: '/reports', label: 'Reports', icon: 'analytics', roles: ['Admin'], soon: true },
  { to: '/settings', label: 'Users & rates', icon: 'settings', roles: ['Admin'], soon: true },
]
