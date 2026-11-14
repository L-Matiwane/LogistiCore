# Setup Guide

## Prerequisites
.NET 8 SDK, Node 20+, SQL Server (LocalDB, Express or Docker), Visual Studio 2022 or VS Code, Git.

## 1. Database
Option A (Docker):
```bash
docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='YourStrong!Passw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```
Then run, in order: `database/01_schema.sql`, `02_seed.sql`, `03_reports.sql` (SSMS or `sqlcmd`).

## 2. Backend (first-time scaffolding)
```bash
cd backend
dotnet new sln -n LogistiCore
dotnet new webapi -n LogistiCore.Api -o src/LogistiCore.Api
dotnet new classlib -n LogistiCore.Application -o src/LogistiCore.Application
dotnet new classlib -n LogistiCore.Domain -o src/LogistiCore.Domain
dotnet new classlib -n LogistiCore.Infrastructure -o src/LogistiCore.Infrastructure
dotnet new xunit -n LogistiCore.UnitTests -o tests/LogistiCore.UnitTests
dotnet new xunit -n LogistiCore.IntegrationTests -o tests/LogistiCore.IntegrationTests
dotnet sln add src/*/*.csproj tests/*/*.csproj
# references
dotnet add src/LogistiCore.Application reference src/LogistiCore.Domain
dotnet add src/LogistiCore.Infrastructure reference src/LogistiCore.Application
dotnet add src/LogistiCore.Api reference src/LogistiCore.Application src/LogistiCore.Infrastructure
# packages
dotnet add src/LogistiCore.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/LogistiCore.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/LogistiCore.Api package Serilog.AspNetCore
dotnet add src/LogistiCore.Application package FluentValidation
```
Set the connection string and JWT key with user-secrets:
```bash
cd src/LogistiCore.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=LogistiCore;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<long-random-string-32+chars>"
dotnet run
```

## 3. Frontend
```bash
cd frontend
npm create vite@latest . -- --template react-ts
npm i react-router-dom @tanstack/react-query axios react-hook-form zod @hookform/resolvers
npm run dev
```
Create `.env` with `VITE_API_URL=https://localhost:5001/api/v1`.

## Demo accounts (after seeding)
| Role | Email | Password |
|---|---|---|
| Admin | admin@logisticore.test | Admin#12345 |
| Dispatcher | dispatcher@logisticore.test | Dispatch#123 |
(Seed passwords are placeholders; the API's seeder should hash real ones. Never reuse these anywhere real.)
