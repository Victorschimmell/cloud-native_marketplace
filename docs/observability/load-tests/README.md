# Marketplace Load Test

This folder contains a k6 load simulation for the observability case study.

The script generates three kinds of traffic:

- browse-heavy traffic against products, categories, details, and reviews;
- successful checkout flows;
- controlled checkout failures such as invalid currency and payment mismatch.

The goal is not to benchmark absolute production capacity. The goal is to create repeatable data in Elasticsearch/Kibana so the dashboards can show request volume, slow endpoints, checkout operation duration, failure categories, and correlation-based investigation.

## Prerequisites

Start the Docker stack:

```powershell
docker compose up -d --build
```

Create or update the Kibana dashboards:

```powershell
.\docs\observability\create-kibana-dashboards.ps1
```

The test expects:

- frontend reachable through Docker Compose;
- backend reachable through the frontend `/api` proxy;
- database seeded with at least one in-stock product;
- currency endpoint supporting the selected currency, default `BRL`.

The test creates temporary customer accounts and successful checkout flows deduct inventory. For clean repeated case-study runs, start from a freshly seeded database or reseed products between runs.

## Run With Docker Compose

Recommended from the repository root:

```powershell
docker compose run --rm loadtest
```

This uses the optional `loadtest` Compose profile/service and runs k6 inside the same Docker network as the frontend and backend.

To change profile or currency:

```powershell
$env:LOAD_PROFILE = "smoke"
$env:LOADTEST_CURRENCY = "BRL"
docker compose run --rm loadtest
```

Because this command runs k6 inside the Docker network, `BASE_URL=http://frontend` exercises the same frontend Nginx `/api` proxy that browser users use in the containerized setup.

## Run With Docker Directly

Recommended from the repository root:

```powershell
docker run --rm `
  --network marketplace-platform_default `
  -e BASE_URL=http://frontend `
  -e LOAD_PROFILE=case-study `
  -v "${PWD}/docs/observability/load-tests:/scripts:ro" `
  grafana/k6 run /scripts/marketplace-load-test.js
```

## Run With Local k6

If k6 is installed locally:

```powershell
$env:BASE_URL = "http://localhost"
$env:LOAD_PROFILE = "case-study"
k6 run .\docs\observability\load-tests\marketplace-load-test.js
```

## Configuration

Environment variables:

| Variable | Default | Purpose |
| --- | --- | --- |
| `BASE_URL` | `http://localhost` | Root URL to exercise. Use `http://frontend` inside the Compose network or `http://localhost` from the host. |
| `CURRENCY` | `BRL` | Preferred display/payment currency used by browse and checkout flows. If it is not available, the script tries `BRL`, `DKK`, and `USD` in that order. |
| `LOAD_PROFILE` | `case-study` | One of `smoke`, `case-study`, or `stress`. |
| `USER_POOL_SIZE` | `8` | Number of temporary customer users created during setup. |
| `PRODUCT_PAGE_SIZE` | `50` | Number of products discovered during setup. |
| `SLEEP_MIN_SECONDS` | `0.2` | Minimum think time between flows. |
| `SLEEP_MAX_SECONDS` | `1.2` | Maximum think time between flows. |

## Suggested Profiles

Smoke test:

```powershell
$env:LOAD_PROFILE = "smoke"
docker compose run --rm loadtest
```

Case-study run:

```powershell
$env:LOAD_PROFILE = "case-study"
docker compose run --rm loadtest
```

Stress run:

```powershell
$env:LOAD_PROFILE = "stress"
docker compose run --rm loadtest
```

## What To Look For In Kibana

Open:

- `Marketplace System Overview`
- `Marketplace Checkout Observability`

Useful searches:

```text
labels.CorrelationId: k6-*
```

```text
labels.Operation: "Checkout.Failed"
```

```text
error.type: "PaymentAmountMismatch"
```

Expected observations:

- request volume increases during the ramp-up stages;
- `/api/products`, `/api/cart/items`, and `/api/checkout` appear in top endpoints;
- request duration appears through `metadata.DurationMs`;
- checkout failures appear with stable `error.type` values;
- individual failed flows can be followed through `labels.CorrelationId`.
