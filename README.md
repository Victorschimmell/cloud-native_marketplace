# Introduction
Marketplace Platform is a monorepo with:

- `Frontend/`: React + Vite web app
- `Backend/`: .NET 10 REST API (Clean Architecture style: Api, Application, Domain, Infrastructure) + tests

The current backend is a foundation setup with one sample entity and system endpoints, ready for incremental feature development.

# Getting Started
Prerequisites:

- .NET SDK 10
- Node.js + npm
- Docker Desktop

Start PostgreSQL:

```powershell
docker compose up -d postgres
```

Run backend API:

```powershell
dotnet run --project .\Backend\Backend.Api\Backend.Api.csproj
```

Enable Olist startup import:

1. Put the required Olist CSV files in one folder.
2. Set `OlistImport:DatasetRootPath` in `Backend/Backend.Api/appsettings.Development.json` or via environment variables.
3. Set `OlistImport:Enabled=true`.
4. Start the API. Migrations will run before the import.

Olist dataset location:

- Keep the full development dataset outside source control, for example in `.data/olist/`.
- The repository only keeps small fixture CSV files for automated tests.

Useful backend URLs (Development):

- Health: `http://localhost:5053/health`
- System info: `http://localhost:5053/api/system/info`
- Scalar API docs: `http://localhost:5053/scalar`

Run frontend:

```powershell
cd .\Frontend
npm install
npm run dev
```

Architecture details: see [ARCHITECTURE.md](./ARCHITECTURE.md).

# Build and Test
Build backend solution:

```powershell
dotnet build Marketplace.slnx
```

Run backend tests:

```powershell
dotnet test Marketplace.slnx
```

Build frontend:

```powershell
cd .\Frontend
npm run build
```

# Contribute
- Keep layer boundaries (`Api -> Application/Infrastructure -> Domain`).
- Add unit and/or integration tests for behavior changes.
- Keep changes focused and open a PR with:
  - summary of changes
  - test evidence (commands + results)
  - migration notes (if schema changed)
