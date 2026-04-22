---
name: "Refactor to Clean Architecture"
description: "Guides the AI on how to refactor legacy, highly-coupled, or 'God Object' code into proper Clean Architecture layers."
---

# Refactor to Clean Architecture

Please analyze the provided legacy code and refactor it to comply with our Clean Architecture template. 

## Refactoring Guidelines

1. **Identify the Entities & DTOs**: Extract any data structures into Domain Entities (`src/Domain/Entities`) and DTOs (`src/Domain/Dtos`). Ensure Entities inherit from `EntityBase` and DTOs use `record`.
2. **Extract Validation**: Move any inline `if (string.IsNullOrEmpty(model.Name))` logic into a FluentValidation class (`src/Domain/Validators`).
3. **Isolate Database Access**: Move all database queries, DbContext usage, or SQL strings into a dedicated Repository (`src/Data/Repository`) that implements an Interface (`src/Domain/Interfaces`).
4. **Extract Business Logic**: Move the core business rules and orchestration into a Service class (`src/Application/Services`). The Service should inject the Repository Interface.
5. **Clean the Controller**: The Controller should ONLY handle HTTP routing, receiving DTOs, calling the Service, and returning `HandleResult()` or `Ok()`. It must not contain business logic or direct database access.

## Output Requirements
Please provide the refactored code divided clearly by files, showing the exact paths where each new file should be created (e.g., `src/Domain/Entities/Customer.cs`, `src/Application/Services/CustomerService.cs`).
