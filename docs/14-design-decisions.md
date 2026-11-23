# Design Decisions

A short record of the main choices and the reasons for them. Use it to explain the project in an interview.

| # | Decision | Why | Trade-off |
|---|---|---|---|
| 1 | Layered architecture (Domain, Application, Infrastructure, Api) | Business rules sit in plain C# classes with no database or web code, so they are easy to test and easy to explain | More files than one big project |
| 2 | Business rules as small static classes (`PricingRules`, `TrackingNumber`, `ParcelStatusRules`) | Pure functions need no setup to unit test and cannot be skipped by a controller | Rules that need the database (price lookup, bin capacity) still live in the services |
| 3 | Status stored as text with a CHECK constraint | Readable in SSMS and in the API, and the database refuses a wrong value even if the code has a bug | A new status needs a script change |
| 4 | `ParcelStatusHistory` table as an audit trail | The brief and the business ask "who changed what and when"; the current status is kept on the parcel for fast reads | Two writes per change, done in one transaction |
| 5 | Unique indexes for rules that must never break (tracking number, one active route per parcel, driver and vehicle per day) | The database enforces the rule even when two requests arrive at the same time | Code must handle the "duplicate" error (tracking number retry) |
| 6 | Tracking number = date + daily sequence, with retry on duplicate | Human readable and sortable, and safe under concurrent registrations | Not random, so numbers can be guessed; the public tracking page will show only limited data |
| 7 | Dates stored in UTC, shown in local time (South Africa is UTC+2, no daylight saving) | One clear rule, no daylight saving bugs | A fixed offset constant; revisit if the company operates in other time zones |
| 8 | JWT bearer tokens with role claims | Stateless, works for a React app, easy to show in a demo | No refresh tokens yet; tokens last 8 hours |
| 9 | Passwords hashed with the ASP.NET Core Identity hasher | A proven, salted, slow hash instead of custom code | None for this scope |
| 10 | Secrets in user-secrets, demo password seeded only in Development | Nothing sensitive in Git | Each developer sets their own secrets |
| 11 | Price is calculated on the server; the screen only displays the quote | The client cannot change a price. One source of truth | One extra request while typing (debounced) |
| 12 | Volumetric weight (L x W x H / 5000) | The way real couriers bill; shows real domain modelling | Needs the dimensions to be entered |
| 13 | Plain CSS with design tokens, no component library | Full control of the blue and pink brand, fewer dependencies | More CSS to write |
| 14 | Role-based navigation in the UI plus role checks on every endpoint | The UI hides what a user cannot use, the API refuses it anyway | Roles are listed in two places (kept together in `navigation.ts` and the endpoint files) |
| 15 | Design prototypes from the team were rebuilt without fake numbers | Assessors mark data accuracy; every figure on screen now comes from the database | Some prototype features (live telemetry, GIS validation) are out of scope |
| 16 | Vertical slices (database, API and screen for one module at a time) | Something demonstrable at the end of each step | Cross-cutting clean-ups are done later |
| 17 | Migrations as numbered SQL scripts (`04_parcel_columns.sql`) that can be run twice | Simple and transparent for a student team | Not as automatic as EF migrations |

## Known gaps (planned)
- Notifications are logged, not sent (no email or SMS provider yet).
- No refresh tokens or account lockout.
- Reports and invoices PDF export are not built yet.
- Automated end-to-end browser tests are not built; the smoke script covers the API.
