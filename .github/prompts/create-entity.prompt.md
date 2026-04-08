---
description: "Scaffold a new CRUD entity across all Clean Architecture layers end-to-end"
agent: "agent"
argument-hint: "Entity name (e.g., Customer)"
tools: [read, search, edit, execute]
---

Create a new entity with full CRUD support across all layers:

1. **Domain**: Entity (inheriting EntityBase), DTO (record), FluentValidation validator, interfaces
2. **Data**: EF Core mapping (IEntityTypeConfiguration), DbSet registration, repository, migration
3. **Application**: Service implementation
4. **Api**: Controller (inheriting ApiControllerBase) with GET, POST, PUT, DELETE endpoints
5. **Tests**: Unit tests and integration tests

Follow the existing patterns from Product entity as reference. Ensure:
- API versioning with `[ApiVersion("1.0")]`
- CancellationToken in all async methods
- HandleResult/HandlePagedResult for responses
- FluentValidation for all input DTOs
