# Architecture Boundaries

Read before adding or moving a project, changing dependencies, or changing an API, domain, persistence, authentication, or deployment boundary.

## Responsibilities

- Keep domain behavior independent from delivery mechanisms, user interfaces, and storage providers.
- Keep API and presentation layers responsible for transport concerns, request validation, and contract mapping.
- Keep persistence responsible for storage access, migrations, and database constraints; it must not decide authorization or presentation behavior.
- Keep frontend code responsible for presentation and interaction state. It must not access server files or databases directly.
- Keep HTTP-specific DTO mapping at the API boundary and database-specific mapping at the persistence boundary.

## Dependency direction

When code is split into layers or projects, preserve this direction:

```text
presentation -> application and domain behavior
infrastructure -> application and domain abstractions
domain -> no dependency on presentation, transport, or storage frameworks
```

The exact project names may change, but dependencies must point inward toward behavior and abstractions.

For the current application structure, the dependency direction is:

```text
API -> Core
API -> Infrastructure    # composition and registration only
Infrastructure -> Core
Web -> HTTP API contracts only
```

Core must not reference ASP.NET Core, EF Core, Infrastructure, HTTP, browser APIs, or external identity providers. Controllers depend on Core service contracts; Infrastructure implements Core repository and capability interfaces.

Organize backend code by technical project and capability folders. This is a service-driven architecture, not a vertical-slice architecture. Public Core services use operation-specific input and output models and return `Result` or `Result<T>`.

## Data and authorization boundaries

- Public projections expose only data that is authorized for public use.
- Restricted operations must have named, typed contracts and explicit authorization boundaries.
- Do not add generic endpoints that accept arbitrary actions or arbitrary data access.
- Keep authorization failures from disclosing protected resources or credentials.
- Use policy-based authorization with a secure-by-default API policy. Public actions explicitly opt into anonymous access. Core receives caller identity only through a provider-neutral abstraction.
- Keep API request and response DTOs separate from Core models. Non-Core models own pure `ToCore...` and `FromCore...` mappings; controllers never serialize Core entities directly.
- Keep all non-success HTTP responses in the standard Problem Details format. Map expected service failures in `BaseController`; handle unexpected exceptions once in a global exception boundary.

## Time, configuration, and deployment

- Centralize time, clock, and time-zone behavior. Do not scatter server-local time calls through application code.
- Keep runtime configuration and secrets outside committed source files.
- Treat deployment, external exposure, and hosting changes as explicit architectural decisions.
- Centralize time behind `IClock`, bind runtime configuration through typed options validated at startup, and keep EF migrations and database-specific configuration in Infrastructure.
