# Marketplace Platform
Marketplace Platform is a monorepo with a React/Vite frontend, a .NET 10 backend API, PostgreSQL and Elasticsearch/Kibana.

## Prerequisites
- Docker Desktop
- .NET SDK 10
- Node.js and npm

## Option 1: Run the Project With Docker
This is the simplest way to run the full project.

From the repository root:
```powershell
docker compose up --build
```

Open:
- Frontend: http://localhost
- Backend API base URL: http://localhost:8080
- Kibana: http://localhost:5601

The backend does not have a page at `/`, so `http://localhost:8080` can show a 404 in the browser. Use the frontend URL for the application.

Stop the project:
```powershell
docker compose down
```

Reset local Docker data if needed:
```powershell
docker compose down -v
```

## Option 2: Run the Project Locally
Use this if you want to run the backend and frontend from source.

Start the backing services:
```powershell
docker compose up -d postgres elasticsearch kibana
```

Start the backend API:
```powershell
dotnet run --project .\Backend\Backend.Api\Backend.Api.csproj --launch-profile http
```

The backend runs at http://localhost:5094. Migrations are applied automatically in development.

Start the frontend in a second terminal:
```powershell
cd .\Frontend
npm install
npm run dev
```

The frontend runs at http://localhost:5173 and proxies API calls to http://localhost:5094.

## Optional Olist Import

The project can import Olist CSV data on backend startup.
1. Put the Olist CSV files from https://www.kaggle.com/datasets/olistbr/brazilian-ecommerce in `<repo-root>\.data\olist`.
2. Set `OlistImport:Enabled` to `true` in `Backend\Backend.Api\appsettings.Development.json`.
3. Start the backend API.

The configured default path is `.data\olist`, which is resolved relative to the backend process working directory. If you start the backend from Visual Studio, that may resolve to `<repo-root>\Backend\Backend.Api\.data\olist`. To avoid ambiguity, set `OlistImport:DatasetRootPath` to the absolute path of `<repo-root>\.data\olist` on your machine.

## Use Cases
Text here