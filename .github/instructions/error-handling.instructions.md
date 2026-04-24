---
description: "Use when creating controllers, services, or handling business validations to ensure consistent error responses using RFC 7807 Problem Details."
applyTo: ["src/Server/Application/**", "src/Server/Api/**", "src/Server/Domain/**"]
---

# Error Handling Rules

## ProblemDetails (RFC 7807)
All API error responses must adhere to the RFC 7807 `ProblemDetails` format.
- DO NOT return raw strings like `return BadRequest("error message")`.
- DO NOT use `return NotFound("not found")` with raw text.

## Domain Exceptions
- All business rule violations must be handled by throwing exceptions that inherit from `DomainExceptions` (located in `src/Server/Domain/Exceptions/`).
- Example: `throw new BusinessException("O saldo é insuficiente.");`
- Example: `throw new NotFoundException("Produto não encontrado.");`

## Controllers
- Controllers MUST NOT catch domain exceptions or formatting errors. Let the exceptions bubble up.
- The `GlobalExceptionHandler` middleware will automatically intercept `DomainExceptions` (and others) and translate them into a standardized `ProblemDetails` response with the correct HTTP Status Code (e.g., 400 for BusinessException, 404 for NotFoundException).

## FluentValidation
- Input validation (DTOs) happens automatically via FluentValidation.
- Do not manually validate `ModelState` in controllers. The pipeline will automatically intercept invalid models and return a 400 Bad Request with a `ValidationProblemDetails` object detailing the specific field errors.

## Internal Errors
- Never expose stack traces or internal technical details to the client in Production. The `GlobalExceptionHandler` masks unhandled exceptions (HTTP 500) behind a generic message, while logging the original error with its trace ID for the DevOps team.
