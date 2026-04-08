---
name: new-entity
description: "Scaffold a new domain entity across all Clean Architecture layers. Use when: creating a new entity, adding a new resource, scaffolding CRUD for a new model. Creates entity, interface, repository, service, DTO, validator, mapping, controller, and tests."
argument-hint: "Entity name (e.g., Customer)"
---

# New Entity Scaffold

Creates all artifacts for a new entity following Clean Architecture patterns.

## When to Use
- Adding a new business entity to the system
- Creating a new CRUD resource end-to-end

## Procedure

### 1. Domain Layer
1. Create entity in `src/Domain/Entities/{Name}.cs` inheriting `EntityBase`
2. Create DTO in `src/Domain/Dtos/{Name}Dto.cs` (use record)
3. Create validator in `src/Domain/Validators/{Name}Validator.cs` using FluentValidation
4. Create repository interface in `src/Domain/Interfaces/I{Name}Repository.cs` (if specialized) or use `IRepository<{Name}>`
5. Create service interface in `src/Domain/Interfaces/I{Name}Service.cs` (if specialized) or use `IService<{Name}>`

### 2. Data Layer
6. Create EF mapping in `src/Data/Mappings/{Name}Mapping.cs` implementing `IEntityTypeConfiguration<{Name}>`
7. Add `DbSet<{Name}>` to `src/Data/Context/ApplicationDbContext.cs`
8. Create repository in `src/Data/Repository/{Name}Repository.cs` (if specialized)
9. Create migration: `dotnet ef migrations add Add{Name} --project src/Data --startup-project src/Api`

### 3. Application Layer
10. Create service in `src/Application/Services/{Name}Service.cs` (if specialized) or rely on `Service<{Name}>`

### 4. Api Layer
11. Create controller in `src/Api/Controllers/{Name}Controller.cs` inheriting `ApiControllerBase`
12. Add CRUD endpoints: GET (list + by id), POST, PUT, DELETE

### 5. Tests
13. Create unit tests in `tests/UnitTests/Controllers/{Name}ControllerTests.cs`
14. Create integration tests in `tests/Integration/Controllers/{Name}IntegrationTests.cs`

## Reference Templates
- Entity template: check `src/Domain/Entities/Product.cs`
- Controller template: check `src/Api/Controllers/ProductController.cs`
- Mapping template: check `src/Data/Mappings/`
- Test template: check `tests/UnitTests/Controllers/`
