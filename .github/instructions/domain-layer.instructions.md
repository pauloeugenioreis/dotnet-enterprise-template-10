---
description: "Use when creating or modifying domain entities, interfaces, validators, DTOs, or exceptions. Covers Clean Architecture domain layer rules."
applyTo: "src/Domain/**"
---

# Domain Layer Rules

## Zero Dependencies
The Domain layer has NO external project references. It defines contracts (interfaces) that other layers implement.

## Entities
- All entities inherit `EntityBase`
- Place in `Domain/Entities/`
- Keep entities behavior-rich when appropriate (domain logic in the entity)

## Interfaces
- Place in `Domain/Interfaces/`
- `IRepository<T>` — generic repository contract
- `IService<T>` — generic service contract
- Specialized interfaces for complex operations (e.g., `IOrderRepository`)

## Validators
- Use FluentValidation for all input DTOs
- Place in `Domain/Validators/`
- One validator per DTO class

## DTOs
- Place in `Domain/Dtos/`
- Use records for immutable DTOs when possible
- Separate request/response DTOs (never expose entities directly)

## Exceptions
- Place in `Domain/Exceptions/`
- Inherit from `DomainExceptions` base class
- Throw domain exceptions for business rule violations
- `GlobalExceptionHandler` translates these to proper HTTP responses
