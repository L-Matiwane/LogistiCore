# UI/UX Design

## Brand
**Name:** LogistiCore. **Personality:** reliable, fast, friendly. Blue = trust and operations; pink = action and highlights.

## Design tokens
| Token | Value | Use |
|---|---|---|
| `--blue-900` | `#0B1F4B` | Sidebar, headings |
| `--blue-700` | `#1D4ED8` | Primary buttons, links |
| `--blue-500` | `#3B82F6` | Focus, charts |
| `--blue-100` | `#DBEAFE` | Selected rows, badges |
| `--pink-600` | `#DB2777` | Accent actions, alerts of interest |
| `--pink-500` | `#EC4899` | Highlights, active nav marker |
| `--pink-100` | `#FCE7F3` | Soft backgrounds |
| `--neutral-900` | `#111827` | Text |
| `--neutral-100` | `#F3F4F6` | Page background |
| `--success` | `#16A34A` | Delivered |
| `--warning` | `#F59E0B` | Failed attempt/delayed |
| `--danger` | `#DC2626` | Lost/damaged |

Check contrast: white text on `#1D4ED8` and `#DB2777` passes WCAG AA. Use pink as accent, not for long body text.

**Status colours:** Registered grey, AtWarehouse blue-500, Sorted blue-700, Assigned pink-500, OutForDelivery pink-600, Delivered green, Failed amber, Lost/Damaged red.

**Typography:** Inter (UI), JetBrains Mono (tracking numbers). **Spacing:** 4px grid. **Radius:** 8px. **Logo idea:** a box icon with a pink route line over a blue square.

## UX principles (Nielsen-based)
1. Visibility of system status: status badges and progress timeline on every parcel.
2. Match the real world: warehouse and dispatch vocabulary.
3. Error prevention: disable invalid actions, confirm destructive ones.
4. Recognition over recall: scan/search first; dropdowns for known values.
5. Consistency: same layout for list, detail, form screens.
6. Efficiency: keyboard-friendly scan input, 3-click rule for core tasks.

## Navigation by role
| Role | Menu |
|---|---|
| Admin/Manager | Dashboard, Parcels, Warehouse, Dispatch, Billing, Reports, Users, Rates |
| Warehouse Clerk | Register parcel, Receive/Sort, Parcels, Exceptions |
| Dispatcher | Dispatch board, Routes, Drivers, Vehicles |
| Driver | My manifest (mobile-first) |
| Billing | Invoices, Statements, Payments |
| Customer | Track parcel, My parcels, My invoices |

## Screens to wireframe (Figma or paper)
1. Login
2. Dashboard (KPI cards: parcels today, in warehouse, out for delivery, delivered, exceptions; chart of weekly volume)
3. Register parcel (form + live price + label preview)
4. Parcel list and detail (timeline)
5. Receive/sort (scan field, bin selector)
6. Dispatch board (unassigned parcels left, routes right)
7. Driver manifest (mobile)
8. Invoice and statement
9. Reports (filters + table + export)
10. Public tracking page

## Wireframe: dispatch board
```
+--------------------------------------------------------------+
| LogistiCore            Dispatch                  [Sipho ▾]   |
+---------+----------------------------------------------------+
| Nav     | Date [14 Nov ▾]      [+ New route]                 |
|         +--------------------+-------------------------------+
|         | Sorted parcels (24)| Route A - Thabo - Van 2       |
|         | [ ] FCL-...-0012   |  load 120/400 kg   [Release]  |
|         | [ ] FCL-...-0013   |  1. FCL-...-0004              |
|         | [Add to route →]   |  2. FCL-...-0009              |
+---------+--------------------+-------------------------------+
```

---

## Brand update v2 (from the team's prototype)

The team's prototype uses a **dark navy base with a blue-to-pink gradient**. This section supersedes the colour usage above where they differ.

| Element | Specification |
|---|---|
| Background | Deep navy (`#0B1330` to `#13204F`); optional night-highway hero photo with a dark overlay |
| Cards | Glass effect: `rgba(15,23,60,.72)`, 1px light border, 24px radius, backdrop blur |
| Primary button | `linear-gradient(90deg, #1D4ED8, #DB2777)`, white uppercase bold text, 12px radius |
| Secondary button | Transparent with a white outline, white uppercase text |
| Headline | Uppercase, light weight; accent line in a light blue-to-pink gradient; **pink full stop** |
| Logo | Wordmark `LOGISTICORE` with a blue-to-pink gradient, `COURIER SERVICE` underneath in spaced capitals |
| Typeface | Inter |
| Tagline | "Precision in motion." / "Tracking parcels across South Africa in real time." |

Accessibility: keep body text white or `#CBD5E1` on navy (passes WCAG AA). Use pink for accents, not for long text.
