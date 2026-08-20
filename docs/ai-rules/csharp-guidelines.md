# C# and .NET Guidelines

Read before writing or changing backend C# or .NET code.

## Boundaries and dependency injection

- Use dependency injection for application services and external capabilities. Prefer a capability-specific interface when a service is a meaningful boundary or has more than one plausible implementation.
- Do not inject concrete service implementations merely because there is currently one implementation. Concrete immutable models, framework-owned types, and genuinely stateless utilities are reasonable exceptions.
- Avoid static service classes. Static types are appropriate for constants and small stateless helpers, not hidden application state or business workflows.
- Public Core services with dependencies have interfaces under `Core.Interfaces.Services`; pure deterministic helpers may be static. A helper that gains a dependency becomes an injectable service.
- Keep business behavior out of controllers, endpoint delegates, UI code, and ORM configuration.
- Keep request and response DTOs explicit. Do not expose persistence entities directly from HTTP endpoints.
- Public service methods return `Result` or `Result<T>` and use operation-specific input and output models. Expected failures are returned as values; unexpected exceptions reach the global exception handler.

## Application startup

- Keep startup configuration easy to follow: configuration, logging, security, persistence, application services, endpoint mapping, validation, migrations, and host execution.
- Put reusable registration bodies in capability-owned extension methods rather than making the entry point a service-registration dump.
- Fail clearly when required configuration, migrations, or service registrations are invalid. Do not serve traffic against an unknown schema.

## Backend behavior

- Validate request shape and field-level constraints at the API boundary, then enforce domain invariants in reusable backend code.
- Use asynchronous database and I/O APIs with cancellation where the surrounding operation supports it.
- Query only the data needed for the operation and keep restricted fields out of public projections.
- Use typed errors and field-level validation responses for expected failures. Reserve exceptions for unexpected failures and cancellation.
- Use the configured clock or time-zone abstraction rather than server-local time calls.
- Propagate `CancellationToken` through asynchronous service, repository, and external-I/O calls. Use `Async` suffixes for asynchronous methods.
- Keep Core independent from EF Core, ASP.NET Core, HTTP, and identity-provider types. Persist Core models through Infrastructure-owned EF configurations.

## Formatting and persistence

- Follow the repository formatter and analyzer configuration. Use spaces, consistent braces, and no decorative region separators.
- Keep one primary type per file when practical. Order members consistently within a type.
- Keep methods focused and name them after observable behavior. Avoid handlers that mix parsing, authorization, persistence, and response mapping.
- Keep comments concise and describe current behavior, invariants, security constraints, or non-obvious trade-offs.
- Configure persistence at its boundary. Do not let UI or endpoint code open connections directly.
- Use migrations as the schema source of truth and enforce critical constraints in the database as well as application validation.
- Prefer sealed classes for entities and services and sealed records for immutable inputs, outputs, requests, and responses. Keep one primary type per file, use file-scoped namespaces, and enforce the repository's 150-column formatting limit.
