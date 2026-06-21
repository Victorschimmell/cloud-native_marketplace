# Example: Adding an Endpoint
Use the existing clean architecture split:
- `Backend.Domain`: entities and business rules
- `Backend.Application`: use cases, DTOs, validators
- `Backend.Infrastructure`: EF Core and external implementations
- `Backend.Api`: controllers and HTTP mapping

If you add a new endpoint, create only what the use case needs.

## What to add
For a new feature like `GET /sample-items` or `POST /sample-items`, the usual additions are:
1. `Domain`
   - Add or update the entity if the business model changes.

2. `Application`
   - Create a `Query` for reads or a `Command` for writes.
   - Add request/response DTOs if the API contract should not expose domain entities directly.
   - Add a validator when input needs rules.

3. `Infrastructure`
   - Add or update EF configuration, queries, repositories, and `DbSet<>` if persistence changes.

4. `Api`
   - Add or update a controller action under `Backend.Api/Controllers/`.
   - The controller should call the application use case, not contain business logic.
   - See `implementation-for-mapping-in-api-layer.md` when implementing mapping between API contracts and application DTOs.

5. `Tests`
   - Unit tests for application/domain logic.
   - Integration tests for the endpoint.

6. `Migration`
   - Add an EF migration if the schema changed.

## Rules
- Use `Query` for reads.
- Use `Command` for writes.
- Use DTOs for API input/output when returning the domain entity would leak persistence or internal structure.
- Add validators for commands/queries that accept client input.
- Keep controllers thin.
- Keep business logic out of `Api` and persistence details out of `Domain`.