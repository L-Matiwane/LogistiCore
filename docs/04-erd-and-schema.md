# ERD and Database Schema

Executable script: [`database/01_schema.sql`](../database/01_schema.sql). Seed: `02_seed.sql`. Reports: `03_reports.sql`.

## ERD
```mermaid
erDiagram
  ROLES ||--o{ USERS : has
  USERS ||--o| DRIVERS : "is a"
  CUSTOMERS ||--o{ PARCELS : sends
  RATE_CARDS ||--o{ PARCELS : prices
  WAREHOUSE_BINS ||--o{ PARCELS : stores
  PARCELS ||--o{ PARCEL_STATUS_HISTORY : logs
  PARCELS ||--o{ PARCEL_EXCEPTIONS : raises
  ROUTES ||--o{ ROUTE_PARCELS : contains
  PARCELS ||--o{ ROUTE_PARCELS : "assigned via"
  DRIVERS ||--o{ ROUTES : drives
  VEHICLES ||--o{ ROUTES : used_in
  ROUTE_PARCELS ||--o| DELIVERIES : results_in
  PARCELS ||--o{ NOTIFICATIONS : triggers
  CUSTOMERS ||--o{ INVOICES : billed
  INVOICES ||--o{ INVOICE_LINES : has
  PARCELS ||--o| INVOICE_LINES : billed_as
  CUSTOMERS ||--o{ PAYMENTS : makes
  INVOICES ||--o{ PAYMENTS : settled_by
```

## Tables and key attributes
| Table | Purpose | Key attributes |
|---|---|---|
| Roles | Security roles | RoleId, Name |
| Users | Login accounts | UserId, Email, PasswordHash, RoleId, IsActive |
| Customers | Business/individual clients | CustomerId, Name, Email, Phone, BillingAddress, AccountNo, UserId (optional portal login) |
| RateCards | Pricing | RateCardId, ServiceLevel, MinWeightKg, MaxWeightKg, Price |
| WarehouseBins | Storage locations | BinId, Zone, Code, Capacity |
| Parcels | Core entity | ParcelId, TrackingNo, CustomerId, RecipientName, RecipientPhone, DeliveryAddress, WeightKg, ServiceLevel, Price, CurrentStatus, BinId, IsInvoiced |
| ParcelStatusHistory | Audit trail | HistoryId, ParcelId, Status, ChangedByUserId, ChangedAt, Note |
| ParcelExceptions | Lost/damaged | ExceptionId, ParcelId, Type, Reason, ReportedBy, ReportedAt, Resolved |
| Drivers | Driver profile | DriverId, UserId, LicenceNo, Phone, IsAvailable |
| Vehicles | Fleet | VehicleId, Registration, CapacityKg, IsActive |
| Routes | Daily route | RouteId, RouteDate, DriverId, VehicleId, Status, Name |
| RouteParcels | Parcel on a route | RouteParcelId, RouteId, ParcelId, StopSequence |
| Deliveries | Proof / attempt result | DeliveryId, RouteParcelId, Outcome, RecipientName, FailureReason, AttemptedAt |
| Notifications | Customer alerts | NotificationId, ParcelId, Channel, Message, SentAt, Status |
| Invoices | Billing header | InvoiceId, InvoiceNo, CustomerId, IssueDate, DueDate, Subtotal, Vat, Total, Status |
| InvoiceLines | Billing lines | LineId, InvoiceId, ParcelId, Amount |
| Payments | Receipts | PaymentId, CustomerId, InvoiceId, Amount, PaidAt, Method |

## Design decisions
- Current status lives on `Parcels` for fast reads; `ParcelStatusHistory` is the source of truth for audit.
- A parcel can appear in `RouteParcels` multiple times across failed attempts, but only one row may be on an active route (enforced in service logic and a filtered unique index).
- Reports use views/stored procedures so they can be tested against seed data independent of the API.
