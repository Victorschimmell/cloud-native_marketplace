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

# Local pipeline setup
Because we do not have dedicated pipeline server, pipelines must be run on local agents. Pipelines triggers: pull request created, pull request merged, manual trigger.

### Setup steps:
1. Follow instructions in:
https://dev.azure.com/SEA2026/Cloud%20Native%20Platform/_settings/agentqueues?queueId=10&view=agents
2. In local terminal, login with Docker credentials provided in Azure DevOps>Pipelines>Library>Credentials

Outcomes of running a pipeline:
- When you create a PR to branch "release/v1.0.0", backend and frontend will be built, linted, and tested automatically
- When you merge the PR to branch "release/v1.0.0", the same steps run again, and on success the Docker image is built and pushed to DockerHub
- You will see red/green on the pipeline
- You will get email if pipeline run failed/is successful
- Docker image is **only** pushed after a merge (never during a PR build)


# Download latest Docker image of solution
Run:
```
docker pull adminkusofteng2026/marketplace-platform
```