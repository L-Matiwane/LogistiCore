# Requirements (draft v0.1, to be validated with mentor and AI Client)

Priority uses MoSCoW: **M**ust, **S**hould, **C**ould.

## Functional requirements

### A. Parcel registration and barcode generation
| ID | Requirement | P |
|---|---|---|
| FR-A1 | Register a parcel with sender, recipient, address, weight, dimensions, service level | M |
| FR-A2 | Generate a unique tracking number (e.g. `FCL-20261114-000123`) | M |
| FR-A3 | Generate a Code128 barcode and printable label | M |
| FR-A4 | Calculate price from rate card (weight band x service level) | M |
| FR-A5 | Search and filter parcels (tracking no., customer, status, date) | M |
| FR-A6 | Edit/cancel a parcel before dispatch | S |

### B. Sorting and warehouse management
| ID | Requirement | P |
|---|---|---|
| FR-B1 | Receive a parcel at the warehouse by barcode scan/entry | M |
| FR-B2 | Assign parcel to a zone/bin | M |
| FR-B3 | View warehouse inventory by zone/status | M |
| FR-B4 | Flag a parcel as lost or damaged with a reason | M |

### C. Route planning and driver assignment
| ID | Requirement | P |
|---|---|---|
| FR-C1 | Manage drivers and vehicles | M |
| FR-C2 | Create a route for a date with ordered stops | M |
| FR-C3 | Assign parcels to a route; assign driver and vehicle | M |
| FR-C4 | Prevent double assignment (driver/vehicle/parcel) and over-capacity loads | M |
| FR-C5 | Driver sees own manifest | M |

### D. Delivery confirmation
| ID | Requirement | P |
|---|---|---|
| FR-D1 | Driver marks delivered with recipient name, time, notes | M |
| FR-D2 | Failed attempt with reason; reschedule or return to warehouse | M |
| FR-D3 | Signature/photo capture | C |

### E. Customer notifications
| ID | Requirement | P |
|---|---|---|
| FR-E1 | Notify on registered, out for delivery, delivered, failed, delayed | M |
| FR-E2 | Notification log with channel and send status | M |
| FR-E3 | Public tracking page by tracking number | S |

### F. Billing and statements
| ID | Requirement | P |
|---|---|---|
| FR-F1 | Generate an invoice from delivered parcels for a client | M |
| FR-F2 | Monthly statement per client: opening balance, charges, payments, closing balance | M |
| FR-F3 | Record payments | M |
| FR-F4 | Export invoice/statement as PDF | S |

### Reports
| ID | Requirement | P |
|---|---|---|
| FR-R1 | Daily parcel movement report | M |
| FR-R2 | Driver performance summary (deliveries, success rate, failed attempts, average time) | M |
| FR-R3 | Monthly revenue report | M |
| FR-R4 | Lost/damaged parcel exceptions list | M |

### Security and administration
| ID | Requirement | P |
|---|---|---|
| FR-S1 | Login with JWT; roles: Admin, Dispatcher, WarehouseClerk, Driver, Billing, Customer | M |
| FR-S2 | Role-based authorisation on every endpoint | M |
| FR-S3 | User management (Admin) | M |
| FR-S4 | Audit log of status changes and sensitive actions | S |

## Non-functional requirements
| ID | Requirement |
|---|---|
| NFR-1 | Usability: core tasks (register parcel, scan-in, confirm delivery) in 3 clicks or fewer after login |
| NFR-2 | Performance: list/search responses under 2 s for 10,000 parcels |
| NFR-3 | Security: hashed passwords, HTTPS, parameterised queries, no secrets in the repo |
| NFR-4 | Reliability: validation on client and server; consistent error format |
| NFR-5 | Data integrity: foreign keys, constraints, transactions on multi-step operations |
| NFR-6 | Maintainability: layered code, 70%+ unit test coverage on business logic |
| NFR-7 | Accessibility: WCAG AA colour contrast, keyboard navigation |

## Business rules
1. A parcel has exactly one current status; every change is appended to its history.
2. Valid transitions: Registered → AtWarehouse → Sorted → Assigned → OutForDelivery → Delivered; OutForDelivery → FailedAttempt → AtWarehouse or Returned; any → Lost/Damaged.
3. A parcel can be on one active route only.
4. A driver cannot have two active routes on the same date; a vehicle likewise.
5. Route load cannot exceed vehicle capacity (kg).
6. Only Delivered parcels are billable; each parcel is invoiced once.
7. Three failed attempts → parcel returned to sender (confirm the number with the client).

## Open questions for the mentor / AI Client
1. Which service levels and rate bands does FCL use?
2. VAT rate and invoice numbering format?
3. Maximum delivery attempts?
4. Do clients pay per parcel or on a monthly account?
5. Which areas/zones does FCL serve?
