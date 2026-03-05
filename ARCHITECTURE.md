# Marketplace Platform Architecture

## 1. Overview

This repository is a full-stack monorepo:

- `Frontend/` contains the web UI (React + Vite + TypeScript).
- `Backend/` contains a .NET 10 REST API, split into multiple projects using a Clean Architecture style.

The backend is intentionally foundation-first: one sample entity (`SampleItem`) and system endpoints are present so new features can be added with a clear structure.

## 2. Repository Structure

```text
.
|-- Frontend/
|-- Backend/
|   |-- Backend.Api/
|   |-- Backend.Application/
|   |-- Backend.Domain/
|   |-- Backend.Infrastructure/
|   `-- tests/
|       |-- Backend.UnitTests/
|       `-- Backend.IntegrationTests/
|-- Marketplace.slnx
|-- Directory.Build.props
`-- docker-compose.yml
```

## 3. Backend Architecture (Clean Layers)

### Projects and Responsibilities

- `Backend.Domain`
  - Core business model (entities, value objects, enums, base abstractions).
  - No dependencies on other backend projects.

- `Backend.Application`
  - Use cases and application-level contracts/orchestration.
  - Depends on `Backend.Domain`.

- `Backend.Infrastructure`
  - Technical implementations (EF Core, PostgreSQL, migrations, external services).
  - Depends on `Backend.Domain`.

- `Backend.Api`
  - HTTP layer (endpoints, startup, DI composition).
  - Depends on `Backend.Application` and `Backend.Infrastructure`.

### Dependency Direction Rules

- Inward dependencies only:
  - `Api -> Application, Infrastructure`
  - `Application -> Domain`
  - `Infrastructure -> Domain`
  - `Domain -> (none)`
- `Domain` must not reference EF, ASP.NET, or infrastructure details.
- Business rules should not live in `Api`.

## 4. Frontend Architecture

Current frontend is a minimal Vite/React app:

- Entry: `Frontend/src/main.tsx`
- Root component: `Frontend/src/App.tsx`

As features grow, prefer a feature-oriented structure, for example:

- `src/features/<feature>/components`
- `src/features/<feature>/api`
- `src/features/<feature>/types`
- `src/shared/` for reusable UI/utilities

## 5. How To Implement New Backend Features

Use vertical slices while respecting layer boundaries:

1. Add/update domain model in `Backend.Domain`.
2. Add application contract/use case in `Backend.Application`.
3. Add persistence/configuration in `Backend.Infrastructure`.
4. Expose HTTP endpoint in `Backend.Api/Api/Endpoints`.
5. Register endpoints via `MapFoundationEndpoints` extension.
6. Add EF migration (if schema changed).
7. Add/update unit + integration tests.

## 6. Database and Migrations

PostgreSQL runs via Docker Compose on host port `5433`. 
Ensure you have installed Docker and have daemon running (the docker background service)

Start database:

```powershell
docker compose up -d postgres
```

Create a migration:

```powershell
dotnet ef migrations add <MigrationName> `
  --project .\Backend\Backend.Infrastructure\Backend.Infrastructure.csproj `
  --startup-project .\Backend\Backend.Api\Backend.Api.csproj
```

Apply migrations:

```powershell
dotnet ef database update `
  --project .\Backend\Backend.Infrastructure\Backend.Infrastructure.csproj `
  --startup-project .\Backend\Backend.Api\Backend.Api.csproj
```

## 7. Running the System Locally

Backend API:

```powershell
dotnet run --project .\Backend\Backend.Api\Backend.Api.csproj
```

Frontend:

```powershell
cd .\Frontend
npm install
npm run dev
```

## 8. Testing Strategy

### Unit Tests

- Project: `Backend/tests/Backend.UnitTests`
- Scope: domain/application logic in isolation.

Run:

```powershell
dotnet test Backend\tests\Backend.UnitTests\Backend.UnitTests.csproj
```

### Integration Tests

- Project: `Backend/tests/Backend.IntegrationTests`
- Scope: API behavior, routing, and host startup behavior.

Run:

```powershell
dotnet test Backend\tests\Backend.IntegrationTests\Backend.IntegrationTests.csproj
```

### Full Backend Test Run

```powershell
dotnet test Marketplace.slnx
```

## 9. Version Management

NuGet package versions are centralized in `Directory.Build.props`.

When updating common package versions, change them there once instead of editing each `.csproj`.
