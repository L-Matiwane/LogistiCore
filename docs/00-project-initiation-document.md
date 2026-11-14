# Project Initiation Document (PID)

**Project:** LogistiCore | **Client:** Forte Courier & Logistics (FCL) | **Module:** IFS303E | **Version:** 0.1 (draft)

## 1. Problem definition
Parcel intake, warehouse sorting and driver dispatch at FCL are manual. Consequences: transcription errors, lost parcels, delayed deliveries, no customer visibility, slow and error-prone billing.

## 2. Objectives (SMART)
| ID | Objective | Measure |
|---|---|---|
| O1 | Every parcel gets a unique tracking number and barcode at registration | 100% of parcels |
| O2 | Every status change is recorded with who, when, where | Full audit trail per parcel |
| O3 | Dispatchers assign parcels to routes and drivers in the system | No paper dispatch sheets |
| O4 | Delivery is confirmed in-system with proof | Delivery record per delivered parcel |
| O5 | Customers are notified at key milestones | Notification logged per milestone |
| O6 | Invoices and monthly statements are generated automatically | Statement per client per month |
| O7 | The four required reports are available and accurate | Reconciles to source data |

## 3. Scope
**In scope (core FCL processes, not modified):** A. parcel registration and barcode generation; B. sorting and warehouse management; C. route planning and driver assignment; D. delivery confirmation workflows; E. customer notifications; F. billing and statements; reports 1-4; role-based security.

**Minor enhancements (allowed by the brief):** dashboard KPIs, public tracking page, CSV/PDF export of reports.

**Out of scope:** live GPS tracking, payment gateway integration, native mobile apps, real SMS/email delivery (simulated via a notification service with a log; a provider can be plugged in later).

## 4. Stakeholders and personas
| Stakeholder | Interest | Persona |
|---|---|---|
| FCL Owner/Manager | Revenue, performance, control | **Nomsa, 45, Operations Manager.** Wants fewer lost parcels and monthly numbers she trusts |
| Dispatcher | Efficient routes and assignments | **Sipho, 32.** Plans routes on paper today; needs a clear daily board |
| Warehouse Clerk | Fast scan-in and sorting | **Thandi, 26.** Needs a quick barcode workflow, minimal typing |
| Driver | Clear manifest, easy confirmation | **Lwazi, 29.** Uses a phone in the field; needs big buttons |
| Billing Clerk | Accurate invoices | **Ayanda, 38.** Wants statements without spreadsheets |
| Customer (business client) | Visibility and correct bills | **Zola, 35, shop owner.** Wants to know where parcels are |
| Mentor / AI Client | Requirements and sign-off | - |

(Personas are working assumptions. Refine them after the requirements sessions.)

## 5. High-level requirements
See [01-requirements.md](01-requirements.md) and [02-wbs.md](02-wbs.md).

## 6. Deliverables
PID; analysis and design documentation (use cases, ERD, schema, UI designs, workflows); working prototype; final demo at the examination and expo.

## 7. Assumptions, constraints, risks
| Type | Item | Mitigation |
|---|---|---|
| Constraint | Fixed academic deadline | Sprint plan, MVP first |
| Constraint | Technologies taught in the degree | Stay on C#/.NET/SQL Server/React |
| Risk | Requirements change after AI Client sessions | Change log; version the docs |
| Risk | Merge conflicts in a team | Short-lived branches, PR reviews |
| Risk | Scope creep | Strict MoSCoW; Should/Could only after MVP |
| Risk | Team member unavailable | Pair on every module; shared ownership |

## 8. Success criteria
The demo shows a parcel travelling from registration to delivery and onto an invoice, with all four reports reconciling. Assessed on functionality, usability, data accuracy, complexity, workflow completeness and professionalism.

## 9. Sign-off
| Name | Role | Date |
|---|---|---|
| | Mentor | |
