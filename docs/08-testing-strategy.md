# Testing Strategy

| Level | Tool | Scope | Target |
|---|---|---|---|
| Unit | xUnit, Moq, FluentAssertions | Status transitions, pricing, capacity rules, invoice totals, validators | 70%+ of business logic |
| Integration | WebApplicationFactory + SQL Server test DB | Controllers, auth, EF queries, reports | Every endpoint, happy + failure path |
| Frontend | Vitest + Testing Library | Forms, validation, role-based nav | Key components |
| E2E (stretch) | Playwright | Register → deliver → invoice | 1-2 critical flows |
| UAT | Manual with mentor | Scripts per use case | Sign-off before demo |

## Priority test cases
1. Invalid status transition is rejected (e.g. Registered → Delivered).
2. Price calculation across weight bands and service levels (boundaries).
3. Route rejects parcel when capacity exceeded; rejects double-booked driver.
4. A parcel is invoiced once; invoice totals and VAT are correct.
5. Driver cannot confirm a parcel not on their route.
6. Customer cannot see another customer's data.
7. Reports reconcile to seed data (known expected counts).

## Defect process
Log in GitHub Issues with label `bug`, severity (S1-S3), steps to reproduce, expected vs actual. Fix on a branch, add a regression test, link the PR.

## Definition of Done
Code reviewed, tests pass in CI, validation and error handling present, docs updated, demoed to the team.
