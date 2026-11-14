# 📦 LogistiCore

**Parcel intake, warehouse sorting, driver dispatch and billing for Forte Courier & Logistics (FCL).**

> Capstone project for IFS303E Systems Development Project, BCom Information Systems, University of Fort Hare.
> Stack: **C# · ASP.NET Core Web API · SQL Server · React**

![status](https://img.shields.io/badge/status-in%20development-EC4899) ![dotnet](https://img.shields.io/badge/.NET-8-1D4ED8) ![license](https://img.shields.io/badge/license-MIT-blue)

## The problem
FCL manages parcel intake, warehouse sorting and driver dispatch manually. Errors, lost parcels and delayed deliveries are common.

## The solution
LogistiCore replaces the manual process with one system:

| Module | What it does |
|---|---|
| Parcel registration | Capture parcel, sender, recipient, price; generate a unique tracking number and barcode |
| Warehouse | Scan in, sort to bins/zones, track every status change |
| Dispatch | Plan routes, assign drivers and vehicles |
| Delivery | Proof of delivery (recipient name, time, notes), failed-attempt handling |
| Notifications | Customer alerts at key status changes |
| Billing | Rate cards, invoices, monthly client statements, payments |
| Reports | Daily parcel movement, driver performance, monthly revenue, lost/damaged exceptions |

## Architecture
React SPA → ASP.NET Core Web API (layered/clean architecture, JWT auth) → SQL Server (EF Core + stored procedures for reports). See [docs/05-architecture.md](docs/05-architecture.md).

## Documentation
| # | Document |
|---|---|
| 00 | [Project Initiation Document](docs/00-project-initiation-document.md) |
| 01 | [Requirements](docs/01-requirements.md) |
| 02 | [Work Breakdown Structure](docs/02-wbs.md) |
| 03 | [Use Cases and Activity Diagrams](docs/03-use-cases.md) |
| 04 | [ERD and Database Schema](docs/04-erd-and-schema.md) |
| 05 | [Architecture](docs/05-architecture.md) |
| 06 | [UI/UX Design](docs/06-ui-ux-design.md) |
| 07 | [API Specification](docs/07-api-spec.md) |
| 08 | [Testing Strategy](docs/08-testing-strategy.md) |
| 09 | [Roadmap and Sprints](docs/09-roadmap.md) |
| 10 | [Team Workflow](docs/10-team-workflow.md) |
| 11 | [Setup Guide](docs/11-setup.md) |

## Quick start
See [docs/11-setup.md](docs/11-setup.md).

## Team
| Name | Role |
|---|---|
| _Your name_ | _e.g. Tech Lead / Backend_ |
| _Teammate_ | _e.g. Frontend_ |
| _Teammate_ | _e.g. Database / QA_ |

## Screenshots
_Add dashboard, parcel registration and report screenshots here once built._

## Licence
MIT
