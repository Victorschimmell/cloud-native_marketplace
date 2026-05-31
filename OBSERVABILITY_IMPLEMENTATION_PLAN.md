# Observability Implementation Plan

## Goal

Improve the logging setup so the marketplace platform can support an industrial-style observability case study. The system should allow a developer or DevOps engineer to inspect behavior under simulated load, identify which parts of the system receive the most traffic, where latency increases, and where failures occur.

The lean approach is to build on the existing Serilog setup and add centralized structured logging with Elasticsearch and Kibana.

## Proposed Stack

- Serilog for structured application logging.
- Elasticsearch for centralized log storage and querying.
- Kibana for dashboards, filtering, and investigation.
- Existing database-backed audit logs for business/audit events.
- Optional future extension: OpenTelemetry, Elastic APM, distributed tracing, and infrastructure metrics.

## Phase 1: Define What We Want To Observe

Status: completed.

The first observability scope is defined in [docs/observability/logging-schema.md](docs/observability/logging-schema.md).

This includes:

- case study questions;
- primary checkout components;
- standard operation names;
- common structured fields;
- known failure categories;
- metrics to derive from logs;
- an example failed-checkout investigation flow.

## Phase 2: Add Elasticsearch And Kibana

Status: completed.

Add Elasticsearch and Kibana to `docker-compose.yml` as a local/demo observability stack.

Expected local services:

- Elasticsearch: `http://localhost:9200`
- Kibana: `http://localhost:5601`

The backend should eventually send Serilog events to Elasticsearch using the Docker service name:

```text
http://elasticsearch:9200
```

## Phase 3: Configure Serilog For Elasticsearch

Status: completed.

The backend API references the official `Elastic.Serilog.Sinks` package and conditionally writes Serilog events to Elasticsearch when `Elasticsearch:Uri` is configured.

Current Docker Compose value:

```text
Elasticsearch:Uri = http://elasticsearch:9200
```

Current local development value:

```text
Elasticsearch:Uri = http://localhost:9200
```

This means logs are sent to Elasticsearch from both common setups:

- Docker Compose backend: uses the internal Docker service name `elasticsearch`.
- Visual Studio or `dotnet run` backend: uses the host port `localhost:9200`.

The sink writes to the following data stream:

```text
logs-marketplace-backend-{environment}
```

Bootstrap is configured in silent mode so the backend does not crash if Elasticsearch is still warming up during local startup. File and console logging remain enabled as fallbacks.

## Phase 4: Add Correlation ID Support

Status: completed.

The backend now ensures every HTTP request has a correlation ID.

Behavior:

1. If `X-Correlation-ID` is provided and valid, the backend uses it.
2. If the header is missing or invalid, the backend generates a new ID.
3. The ID is assigned to `HttpContext.TraceIdentifier`.
4. The ID is returned in the `X-Correlation-ID` response header.
5. The ID is pushed into Serilog's log context as `CorrelationId`.

This allows Kibana users to filter all operational logs for one request flow.

## Phase 5: Improve Request-Level Logging

Status: completed.

Serilog request completion logs now include structured request fields:

- `Component = HttpPipeline`
- `Operation = HttpRequest.Completed` or `HttpRequest.Failed`
- `Outcome = Succeeded` or `Failed`
- `CorrelationId`
- `StatusCode`
- `RequestMethod`
- `RequestPath`
- `UserId`, when available

## Phase 6: Add Structured Checkout Logs

Status: completed.

`CheckoutService` now emits structured observability events for the main checkout path through a dedicated `ICheckoutObservability` helper. This keeps the business flow responsible for deciding what happened while the helper owns log formatting, duration calculation, and structured field consistency.

Successful checkout operations include:

- `Checkout.Started`
- `Checkout.CartLoaded`
- `Checkout.CustomerValidated`
- `Checkout.ShippingAddressValidated`
- `Checkout.InventoryValidated`
- `Checkout.PaymentValidated`
- `Checkout.OrderCreated`
- `Checkout.PaymentRecorded`
- `Checkout.StockDeducted`
- `Checkout.CartConverted`
- `Checkout.OrderItemsCreated`
- `Checkout.AuditLogWritten`
- `Checkout.ChangesSaved`
- `Checkout.Completed`

Failure events use the same operation naming where possible and include a stable `ErrorType`, for example:

- `BuyerRestricted`
- `InvalidCurrency`
- `MissingCartIdentifier`
- `CartNotFound`
- `UnauthenticatedCustomer`
- `CustomerProfileNotFound`
- `InvalidShippingAddress`
- `MissingPayment`
- `UnsupportedPaymentType`
- `EmptyCart`
- `ListingNotFound`
- `InsufficientInventory`
- `PaymentAmountMismatch`
- `PaymentProcessingFailed`
- `UnexpectedException`

Events include `Component`, `Operation`, `Outcome`, `DurationMs`, request identifiers, checkout identifiers, and safe business context such as item count, payment count, total amount, currency, and inventory quantities. Sensitive address and card details are not logged.

## Phase 7: Create Kibana Dashboards

Status: completed.

The Kibana dashboard setup is documented in [docs/observability/kibana-dashboards.md](docs/observability/kibana-dashboards.md).

The repo now includes an idempotent dashboard bootstrap script:

```powershell
.\docs\observability\create-kibana-dashboards.ps1
```

The script creates or updates:

- `Marketplace Backend Logs` data view;
- `Marketplace System Overview`;
- `Marketplace Checkout Observability`.

The dashboards show request volume, failed requests, status code trends, top endpoints, request duration by endpoint, checkout completion and failure counts, checkout operation duration, failure categories, and correlation-oriented checkout event grouping.

## Phase 8: Add Load Simulation

Status: completed.

The load simulation is implemented with k6 in [docs/observability/load-tests/marketplace-load-test.js](docs/observability/load-tests/marketplace-load-test.js).

It creates repeatable traffic for:

- browse-heavy flows;
- successful checkout flows;
- controlled checkout failures.

Run instructions are documented in [docs/observability/load-tests/README.md](docs/observability/load-tests/README.md).

## Remaining Phases

1. Run the load simulation and capture observations.
2. Write the final case study results.

## Success Criteria

The implementation is successful when:

- backend logs appear in Elasticsearch;
- Kibana can search and filter logs by `CorrelationId`;
- checkout logs include operation, outcome, and duration fields;
- known checkout failures are categorized by `ErrorType`;
- dashboards show request volume, errors, and checkout duration;
- a load simulation produces enough data for a case study;
- a developer can inspect one failed checkout and understand the sequence of events that caused it.
