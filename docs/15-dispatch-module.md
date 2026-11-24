# Dispatch Module (Sprint 3, part 2)

Covers FR-C1 to FR-C4: manage drivers and vehicles, create a route for a day, assign a driver, a vehicle and sorted parcels, with capacity and double-booking checks. The driver's own manifest (FR-C5) comes with the delivery module.

## Screens
| Route | Roles | Purpose |
|---|---|---|
| `/dispatch` | Admin, Dispatcher | Pick a date, create routes, tick sorted parcels and add them to a route, release or cancel |
| `/fleet` | Admin, Dispatcher | Drivers (mark available or unavailable) and vehicles (add, put in or out of service) |

## Endpoints (Admin, Dispatcher)
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/drivers` | Drivers |
| PUT | `/api/v1/drivers/{id}/availability` | Mark a driver available or unavailable |
| GET, POST | `/api/v1/vehicles` | List, add |
| PUT | `/api/v1/vehicles/{id}` | Edit, put in or out of service |
| GET | `/api/v1/dispatch/parcels` | Sorted parcels waiting for a route |
| GET | `/api/v1/routes?date=` | Routes for a day (cancelled ones are hidden) |
| POST | `/api/v1/routes` | Create a route |
| POST | `/api/v1/routes/{id}/parcels` | Add parcels (body: `{ "parcelIds": [1, 2] }`) |
| DELETE | `/api/v1/routes/{id}/parcels/{parcelId}` | Remove a parcel |
| POST | `/api/v1/routes/{id}/release` | Release the route to the driver |
| POST | `/api/v1/routes/{id}/cancel` | Cancel the route |

## Business rules
1. A driver can be on only one active route per day, and so can a vehicle (business rule 4). The service checks it and a filtered unique index in the database enforces it.
2. Only **Sorted** parcels can be put on a route. Adding a parcel changes its status to **Assigned** and writes a history row.
3. A parcel can be on one active route only (business rule 3, unique index on active route parcels).
4. The total weight of the parcels on a route cannot be more than the vehicle's capacity (business rule 5).
5. A route is **Planned** until it is released. Parcels can only be added or removed while it is Planned.
6. An empty route cannot be released.
7. Cancelling a Planned or Released route returns its parcels to **Sorted** and frees the driver and vehicle for that day.
8. A route cannot be planned for a day that has already passed. A vehicle on a planned or released route cannot be taken out of service.

## Files
- Domain: `Driver`, `Vehicle`, `DeliveryRoute` (the `Routes` table), `RouteParcel`, `RouteRules`
- Application: `DispatchModels.cs` (models, `IDispatchService`, validation)
- Infrastructure: `DispatchService`
- Api: `DispatchEndpoints.cs`
- Frontend: `DispatchPage`, `FleetPage`, `api/dispatch.ts`

## Not yet built
- Creating a driver together with their login (Users page, later sprint)
- Re-ordering the stops on a route
- The driver starting the route (Released to InProgress) and delivering (next module)
