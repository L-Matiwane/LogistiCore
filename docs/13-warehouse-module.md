# Warehouse Module (Sprint 3, part 1)

Covers FR-B1 to FR-B4: receive by scan, sort to a bin, see warehouse stock, flag lost or damaged.

## Screen
`/warehouse` (Admin, WarehouseClerk). One scan box with three modes: **Receive**, **Sort to bin**, **Lost or damaged**. The box keeps focus so a USB barcode scanner (which types the number and presses Enter) works without the mouse. Shows recent scan results, bin occupancy by zone, the parcels in the warehouse (filter by clicking a bin) and the exception list.

## Endpoints
| Method | Route | Roles | Purpose |
|---|---|---|---|
| POST | `/api/v1/warehouse/receive` | Admin, WarehouseClerk | Registered (or failed attempt) to AtWarehouse |
| POST | `/api/v1/warehouse/sort` | Admin, WarehouseClerk | AtWarehouse to Sorted, with a bin |
| POST | `/api/v1/warehouse/exceptions` | Admin, WarehouseClerk | Flag Lost or Damaged with a reason |
| GET | `/api/v1/warehouse/bins` | Staff | Bins with capacity and occupancy |
| GET | `/api/v1/warehouse/inventory?binId=` | Staff | Parcels in the warehouse |
| GET | `/api/v1/warehouse/exceptions` | Staff | Latest 50 exceptions |

## Business rules
1. Status changes follow `ParcelStatusRules` (Domain): Registered → AtWarehouse → Sorted → Assigned → OutForDelivery → Delivered or FailedAttempt. A failed attempt returns to AtWarehouse or goes to Returned. Lost or Damaged is allowed from any active status.
2. A bin cannot take more parcels than its capacity.
3. A parcel can be moved to a different bin while it is in the warehouse. The history says "Moved to bin".
4. Every change writes a status history row with who and when.
5. A lost or damaged parcel leaves its bin and gets an exception record.

## Test ideas
- `ParcelStatusRules.CanTransition` for every allowed and blocked pair
- Sort into a full bin is rejected; moving to the same bin is rejected
- Receiving a parcel twice is rejected
