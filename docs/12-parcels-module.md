# Parcels Module (Sprint 2)

Covers requirements FR-A1 to FR-A5 (register, tracking number, barcode label, price, search).

## Screens
| Route | Roles | Purpose |
|---|---|---|
| `/` | Admin, Clerk, Dispatcher, Billing | Dashboard with live KPIs from the database |
| `/parcels/new` | Admin, WarehouseClerk | Register a parcel, live price, label preview, print label |
| `/parcels` | Staff | Search by tracking number, recipient or customer; filter by status; paging |
| `/parcels/:id` | Staff | Parcel details, status history, printable label |

## Endpoints
| Method | Route | Roles | Purpose |
|---|---|---|---|
| POST | `/api/v1/parcels/quote` | Admin, WarehouseClerk | Price for a weight, dimensions and service level |
| POST | `/api/v1/parcels` | Admin, WarehouseClerk | Register a parcel (returns 201 with the parcel) |
| GET | `/api/v1/parcels?q=&status=&page=&pageSize=` | Staff | Search, newest first |
| GET | `/api/v1/parcels/{id}` | Staff | One parcel with status history |
| GET | `/api/v1/customers` | Staff | Active customers for the picker |
| GET | `/api/v1/dashboard/summary` | Staff | Counts and the 7-day volume |

Errors: field problems return `400` (validation problem details with a field list). Business rule problems, such as no rate for a weight, return `422`.

## Business rules implemented
1. **Tracking number** `FCL-YYYYMMDD-NNNNNN`. The date is the South African date (UTC+2). The number counts up each day. If two clerks register at the same moment, the unique index rejects the duplicate and the service retries with the next number.
2. **Volumetric weight** = length × width × height (cm) ÷ 5000. **Billed weight** = the larger of actual and volumetric weight.
3. **Price** comes from the rate card: the band where `min < billed weight <= max` for the chosen service level. Weights above 30 kg are rejected.
4. A new parcel starts as `Registered`, with a first status history entry saying who registered it and when.
5. Dates are stored in UTC and shown in local time.

## Files
- `LogistiCore.Domain/Rules`: `PricingRules`, `TrackingNumber`, `LocalTime`, constants (pure logic, easy to unit test)
- `LogistiCore.Application/Parcels`: request/response models, validation, `IParcelService`
- `LogistiCore.Infrastructure/Parcels`: `ParcelService` (EF Core)
- `LogistiCore.Api/Endpoints/ParcelEndpoints.cs`
- `database/04_parcel_columns.sql`: extra columns (city, postal code, instructions, handling, billed weight)

## Test ideas (next)
- `PricingRules`: volumetric weight, billed weight, rounding
- `TrackingNumber`: format and parsing the sequence
- `ParcelValidation`: missing fields, bad phone, dimensions given partly
- Service: price band boundaries (exactly 2 kg, 10 kg, 30 kg), customer inactive
