# API Specification (v1)

Base URL `/api/v1`. JSON. Auth: `Authorization: Bearer <jwt>`. Errors use `application/problem+json`.

## Auth
| Method | Route | Roles | Purpose |
|---|---|---|---|
| POST | /auth/login | Public | Email + password → JWT |
| GET | /auth/me | Any | Current user |

## Parcels
| Method | Route | Roles | Purpose |
|---|---|---|---|
| POST | /parcels | Clerk, Admin | Register parcel |
| GET | /parcels?status=&customerId=&from=&to=&q= | Staff | Search, paged |
| GET | /parcels/{id} | Staff | Detail with history |
| PUT | /parcels/{id} | Clerk, Admin | Edit before dispatch |
| GET | /parcels/{id}/label | Clerk | Barcode label (PNG/PDF) |
| GET | /track/{trackingNo} | Public | Limited public status |

## Warehouse
| POST | /warehouse/receive | Clerk | Receive by tracking no. |
| POST | /warehouse/sort | Clerk | Assign bin |
| POST | /parcels/{id}/exceptions | Clerk, Admin | Flag lost/damaged |
| GET | /exceptions | Admin | List |

## Routes, drivers, vehicles
| GET/POST/PUT | /drivers, /vehicles | Admin, Dispatcher | CRUD |
| POST | /routes | Dispatcher | Create route |
| POST | /routes/{id}/parcels | Dispatcher | Add parcels (capacity check) |
| POST | /routes/{id}/release | Dispatcher | Release to driver |
| GET | /routes/mine | Driver | Own manifest |

## Deliveries
| POST | /deliveries/{routeParcelId}/confirm | Driver | Mark delivered |
| POST | /deliveries/{routeParcelId}/fail | Driver | Failed attempt + reason |

## Billing
| POST | /invoices | Billing | Generate for customer + period |
| GET | /invoices, /invoices/{id}, /invoices/{id}/pdf | Billing, Customer (own) | |
| GET | /customers/{id}/statement?month=YYYY-MM | Billing, Customer (own) | Statement |
| POST | /payments | Billing | Record payment |

## Reports (Admin, Manager)
| GET | /reports/daily-movement?date= | Daily parcel movement |
| GET | /reports/driver-performance?from=&to= | Driver performance |
| GET | /reports/monthly-revenue?month= | Monthly revenue |
| GET | /reports/exceptions?from=&to= | Lost/damaged list |
All support `?format=csv`.

## Example
```http
POST /api/v1/parcels
{
  "customerId": 3,
  "recipientName": "Anele Dlamini",
  "recipientPhone": "0821234567",
  "deliveryAddress": "12 Oxford St, East London",
  "weightKg": 4.5,
  "serviceLevel": "Express"
}
```
```json
201 Created
{ "parcelId": 101, "trackingNo": "FCL-20261114-000101", "price": 145.00, "status": "Registered" }
```
