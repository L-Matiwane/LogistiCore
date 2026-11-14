# Roadmap and Sprints

Two-week sprints assumed. Adjust dates to your module calendar and the expo date. Work vertically: each sprint ends with something demonstrable.

| Sprint | Goal | Deliverables |
|---|---|---|
| 0 - Setup | Team ready | Repo, branch protection, CI, backlog, PID signed off, requirements sessions held |
| 1 - Foundations | Skeleton runs end to end | DB schema + seed, API solution, login + roles, React shell with theme and role nav |
| 2 - Parcels | Module A | Register parcel, tracking no., barcode label, search/list/detail, status history |
| 3 - Warehouse + Dispatch | Modules B, C | Receive/sort, bins, drivers, vehicles, routes, assignment rules |
| 4 - Delivery + Notifications | Modules D, E | Driver manifest, confirm/fail, notification log, public tracking |
| 5 - Billing + Reports | Module F, reports | Invoices, statements, payments, 4 reports, CSV/PDF export, dashboard |
| 6 - Hardening | Quality | Test coverage, bug fixes, UX polish, accessibility, performance, security review |
| 7 - Demo | Presentation | Demo data, rehearsed script, final docs, README screenshots, recorded demo video |

## MVP (must be done by end of Sprint 4)
Login → register parcel → receive/sort → route/assign → deliver → notification logged.

## Backlog management
GitHub Projects board: Backlog → Ready → In progress → In review → Done. Every issue has acceptance criteria and links to a requirement ID (e.g. FR-C4).

## Rituals
- Sprint planning (1 hr), 15-min stand-ups (or async in chat), review/demo with mentor, retrospective.
- Keep a `docs/CHANGELOG.md` of requirement changes.

## Suggested roles (rotate where possible)
| Role | Focus |
|---|---|
| Tech lead | Architecture, PR reviews, merge |
| Backend dev | API, business rules |
| Frontend dev | React, design system |
| Database/QA | Schema, procedures, tests, UAT scripts |
| Everyone | Write docs for what you build; commit regularly so contribution is visible |
