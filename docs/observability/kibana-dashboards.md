# Kibana Dashboards
The local Kibana setup includes two dashboards for the observability case study.

## Create Or Update Dashboards
Start Elasticsearch and Kibana first:
```powershell
docker compose up -d elasticsearch kibana
```

Then run:
```powershell
.\docs\observability\create-kibana-dashboards.ps1
```

The script is idempotent. It creates or updates:
- the `Marketplace Backend Logs` data view;
- reusable Kibana visualizations;
- the `Marketplace System Overview` dashboard;
- the `Marketplace Checkout Observability` dashboard.

By default, the script connects to `http://localhost:5601`. To use another Kibana URL:
```powershell
.\docs\observability\create-kibana-dashboards.ps1 -KibanaUrl "http://localhost:5601"
```

## Marketplace System Overview
URL:
```text
http://localhost:5601/app/dashboards#/view/dashboard-marketplace-system-overview
```

Purpose:
- show total backend request volume;
- show failed request count;
- show requests over time grouped by HTTP status code;
- show the busiest API endpoints;
- show average request duration by endpoint using the structured `metadata.DurationMs` field;
- show log volume by severity level.

This dashboard is useful during load simulation because it answers which routes receive the most traffic and which routes become slower.

## Marketplace Checkout Observability
URL:
```text
http://localhost:5601/app/dashboards#/view/dashboard-marketplace-checkout-observability
```

Purpose:
- show completed checkout count;
- show failed checkout count;
- show checkout operations over time;
- show average duration by checkout operation;
- show checkout failures grouped by `ErrorType`;
- show recent checkout events grouped by `CorrelationId`, operation and outcome.

This dashboard is useful for the customer case study because checkout is the main flow where load, validation failures, inventory failures and persistence behavior can be observed.

## Expected Data
The dashboards use the shared data view:

```text
logs-marketplace-backend-*
```

This includes logs from both common local setups:
- Docker backend logs in `logs-marketplace-backend-production`;
- Visual Studio or `dotnet run` backend logs in `logs-marketplace-backend-development`.

Some panels may be empty until matching events exist. For example, `Checkout: Failures by Error Type` needs at least one failed checkout log with `metadata.ErrorType`.

## Useful Investigation Flow
1. Open `Marketplace Checkout Observability`.
2. Look for a spike in failed checkout events.
3. Filter by the dominant `metadata.ErrorType`.
4. Copy one `labels.CorrelationId`.
5. Open Discover and search for that correlation ID:

```text
labels.CorrelationId: "your-correlation-id"
```

This should show the request-level and checkout-level logs that explain the failure sequence.
