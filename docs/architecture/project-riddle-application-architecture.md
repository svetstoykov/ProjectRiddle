# Project Riddle Application Architecture

**Status:** Proposed; approved in conversation and awaiting written-spec review  
**Last updated:** 2026-08-20  
**Applies to:** Project Riddle V1 and future application code  
**Related documents:**

- [The V1 design defines the product boundary](../design/2026-08-20-project-riddle-v1-design.md).
- [Subphase 4 defines the supported deployment composition](./project-riddle-v1-subphase-4-deployment-and-readiness.md).
- [Core principles define the always-on engineering priorities](../ai-rules/core-principles.md).
- [Architecture boundaries define the durable dependency rules](../ai-rules/architecture-boundaries.md).

## 1. Decision

Project Riddle uses a service-driven modular monolith. The application has one deployable ASP.NET Core host, one React frontend, one SQLite database, and explicit boundaries between API delivery, Core behavior, Infrastructure, and browser presentation.

The codebase is organized by technical boundary first and capability second. It does not use vertical slices as its primary organization model. A capability such as riddles may have related models, enums, services, repositories, controllers, and frontend code, but each remains in the project and layer that owns its responsibility.

This architecture is intentionally small enough for a Raspberry Pi deployment while keeping business behavior independent from HTTP, EF Core, authentication providers, and browser APIs.

## 2. Technology baseline

- **Backend:** ASP.NET Core on .NET 10.
- **Persistence:** EF Core 10 with SQLite.
- **Frontend:** React with TypeScript and Vite.
- **Frontend HTTP:** Axios through one shared typed client.
- **Frontend server state:** TanStack Query.
- **Frontend notifications:** Sonner behind a small application notification wrapper.
- **Frontend routing:** React Router in client-side SPA mode.
- **Frontend styling:** CSS Modules and a small global stylesheet.
- **Frontend package management:** npm with a committed lockfile.
- **Deployment:** one multi-stage Docker image serving the API and compiled frontend.

These choices are implementation defaults, not permission to add unrelated infrastructure. A replacement requires a written architectural decision that explains the boundary it improves and the migration cost.

## 3. Project structure and dependency direction

The initial repository structure is:

```text
src/
  ProjectRiddle.Api/
  ProjectRiddle.Core/
  ProjectRiddle.Infrastructure/
  ProjectRiddle.Web/

tests/
  ProjectRiddle.IntegrationTests/
```

The compile-time dependency graph is:

```text
ProjectRiddle.Api -> ProjectRiddle.Core
ProjectRiddle.Api -> ProjectRiddle.Infrastructure   # composition and registration only
ProjectRiddle.Infrastructure -> ProjectRiddle.Core
ProjectRiddle.Web -> HTTP API contracts only
```

`ProjectRiddle.Core` does not reference the API, Infrastructure, EF Core, ASP.NET Core, React, Axios, or an external identity provider. The API may reference Infrastructure only to compose the application and register implementations; controller code must depend on Core contracts.

Namespaces mirror project and capability folders:

```text
ProjectRiddle.Core.Services.Riddles
ProjectRiddle.Core.Interfaces.Services
ProjectRiddle.Core.Interfaces.Repositories
ProjectRiddle.Core.Models.Riddles
ProjectRiddle.Core.Enums.Riddles
ProjectRiddle.Core.Results.Models
ProjectRiddle.Api.Controllers
ProjectRiddle.Api.Models.Riddles
ProjectRiddle.Infrastructure.Persistence
ProjectRiddle.Infrastructure.Repositories.Riddles
```

## 4. Service-driven Core organization

Core uses service-driven application behavior. A public service represents a meaningful application capability and coordinates validation, authorization decisions that require domain data, domain models, the clock, and repositories.

The usual layout is:

```text
ProjectRiddle.Core/
  Services/
    Riddles/
      RiddlesService.cs
  Interfaces/
    Services/
      IRiddlesService.cs
    Repositories/
      IRiddleRepository.cs
    Time/
      IClock.cs
    Users/
      ICurrentUser.cs
  Models/
    Riddles/
      Riddle.cs
      RiddleRange.cs
      CreateRiddleInput.cs
      CreateRiddleOutput.cs
  Enums/
    Riddles/
      RiddlePublicationState.cs
  Results/
    Models/
      Result.cs
      ResultOfT.cs
      OperationError.cs
      ErrorType.cs
```

The exact service split follows cohesion. A single `RiddlesService` may start the capability, but it must be split when it owns unrelated workflows or becomes difficult to understand. Controllers, repositories, and static helpers must not absorb service-level business behavior.

Services with injected dependencies have interfaces under `Core.Interfaces.Services`. Pure deterministic validators and helpers may be static. A helper that needs persistence, configuration, time, logging, randomness, or another service becomes an injectable service with an interface.

Public service methods use operation-specific Core contracts:

```csharp
Task<Result<CreateRiddleOutput>> CreateAsync(
    CreateRiddleInput input,
    CancellationToken cancellationToken);
```

Inputs and outputs are separate types and separate files. Domain entities are not used as a substitute for every service contract.

Core entities and value objects enforce local invariants. Services own workflows and rules that involve multiple models or dependencies. Pure validators return `Result` when their validation is user-visible; internal predicates may return `bool`.

## 5. Result and HTTP error contract

The Result model follows the existing ChangeLens shape: `Result`, `Result<T>`, `OperationError`, and `ErrorType` live under `ProjectRiddle.Core.Results.Models`. `OperationError` contains only its message, error type, and optional stable code; it does not contain HTTP types or a property path.

Every public Core service returns `Result` or `Result<T>`. Expected business, validation, authorization, conflict, not-found, and known dependency failures are values. Unexpected exceptions, cancellation, programmer errors, and unknown infrastructure failures are not disguised as successful results and reach the appropriate exception boundary.

`ErrorType.Unauthorized` means unauthenticated access and maps to `401`. `ErrorType.Forbidden` means an authenticated caller lacks permission and maps to `403`.

The default mapping is:

| Result error | HTTP status |
| --- | ---: |
| `NotFound` | 404 |
| `Validation` | 400 |
| `MalformedInput` | 400 |
| `UnprocessableInput` | 422 |
| `Conflict` | 409 |
| `InvalidOperation` | 409 |
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `Timeout` | 504 |
| `ExternalDependencyFailure` | 503 |
| `InternalError` | 500 |

Every non-success HTTP response uses `application/problem+json` and the standard Problem Details members: `type`, `title`, `status`, `detail`, and `instance`. Project Riddle also exposes the stable Result `code` and a non-sensitive `traceId` as permitted Problem Details extensions. There is no custom error envelope.

`BaseController` is the single mapping point for service results. API model-binding failures use `ValidationProblemDetails`. Authentication challenges, authorization forbids, routing failures, and service failures use the same Problem Details family. The global exception handler logs unexpected exceptions once and returns safe generic details without answers, credentials, unpublished content, or stack traces.

## 6. API boundaries

Controllers are organized by resource or capability, not by role. Public and administrative actions may share a controller, but every action has an explicit authorization boundary and an appropriate response projection.

The API uses unversioned routes until an independently deployed consumer creates a real compatibility requirement. Routes remain resource-oriented; action-style subroutes are allowed for genuine domain commands such as answer checking or revealing a letter.

API models use `Request` and `Response` names. They never serialize Core entities directly and never pass API request types into Core. Mapping methods follow this convention:

```csharp
CreateRiddleRequest.ToCoreCreateRiddleInput()
RiddleResponse.FromCoreRiddle(riddle)
CreateRiddleResponse.FromCoreCreateRiddleOutput(output)
```

Mappings are pure and live on the non-Core model. Complex mappings may move to an API-owned mapper without introducing an API dependency into Core.

Successful actions use conventional HTTP statuses: `200` for successful reads and body-returning updates, `201` with `Location` for creation, and `204` for successful deletes, bodyless updates, and bodyless commands. Controllers perform transport binding, authorization metadata, service invocation, success mapping, and status selection; they do not contain business logic or access repositories directly.

## 7. Authorization and identity

The default authorization policy requires authentication. Public endpoints explicitly opt into anonymous access. Protected operations use named policies rather than scattered role checks.

V1 uses an ASP.NET Core cookie session. External Google, Facebook, or other OpenID Connect providers may be added as authentication schemes without changing Core contracts. Provider identities are normalized into a Project Riddle user identity and local roles and policies.

`ICurrentUser` is the provider-neutral Core abstraction for services that need caller identity. Core never references `HttpContext`, `ClaimsPrincipal`, provider SDKs, or authentication cookie types. Cookie authentication, Data Protection keys, CSRF protection, and provider configuration belong to API and Infrastructure.

Credentials, tokens, password hashes, and session material never enter browser progress storage, API responses, logs, or committed source files.

## 8. Persistence

Core domain models are persisted directly by EF Core. Core models contain no EF attributes or EF references; Infrastructure owns `DbContext`, fluent configurations, migrations, and database-specific behavior.

Repositories are capability-specific and express data access needs rather than generic CRUD. They do not expose `IQueryable`, `DbSet`, EF entities, or HTTP concepts. Repository reads return Core values or nullable values for absence. Repository commands return `Task` and own their `SaveChangesAsync` operation with cancellation support.

There is no generic repository and no `IUnitOfWork` abstraction initially. A repository may use an internal EF transaction when one operation needs multiple database changes. A future cross-repository atomic workflow requires an explicit architectural decision rather than an informal workaround.

EF migrations are committed under `Infrastructure/Persistence/Migrations`, applied before the application serves traffic, and treated as the schema source of truth. Migration failure prevents startup. Automatic destructive reset is not an accepted recovery strategy.

## 9. Time, configuration, and logging

Core receives time through `IClock`. Services and models do not call server-local time or direct `DateTime.Now`/`DateTime.UtcNow`. Sofia publication-date calculations are centralized and deterministic tests inject a controlled clock.

Runtime configuration is bound to typed options and validated during startup. Core services never receive raw `IConfiguration`, environment-variable lookups, or secret configuration objects.

Backend logging uses `ILogger<T>` and structured message templates. Unexpected exceptions are logged once at the global exception boundary. Logs never contain credentials, cookies, tokens, answers, explanations, unpublished content, or unnecessary user input.

## 10. Frontend boundaries

The React application is organized by capability:

```text
ProjectRiddle.Web/src/
  app/
    routes/
    providers/
  features/
    riddles/
      api/
      components/
      pages/
      models/
    courses/
    admin/
  shared/
    api/
    components/
    notifications/
    storage/
```

One Axios client owns base URL, credentials, cancellation, serialization, and Problem Details parsing. Feature API functions call that client; components never call Axios or `fetch` directly.

TanStack Query owns server state, cache invalidation, query and mutation status, and retries. React local state owns temporary form and interaction state. A versioned typed `localStorage` adapter owns only small browser-local riddle and course progress. Malformed or obsolete data resets safely. Passwords, credentials, answers, and complete explanations are never stored there.

Sonner is mounted once at the application root behind a notification wrapper. Mutations may show success or recoverable failure toasts, but every important failure also has an inline accessible state. The HTTP client does not globally toast every error.

CSS Modules provide local styling. A small global stylesheet owns tokens and resets. No global state library, UI framework, SSR setup, or frontend test framework is required for V1.

## 11. Testing and verification

The repository has one automated test project: `ProjectRiddle.IntegrationTests`. Tests exercise Core services through their public interfaces with real Infrastructure implementations and disposable SQLite data. The project is the place to verify business workflows, persistence behavior, authorization-relevant outcomes, Result contracts, and time-sensitive rules.

V1 does not add unit-test, controller-test, repository-only-test, EF-configuration-test, browser-automation, or frontend-test projects. Backend release builds, frontend type-check and production builds, and Docker packaging checks remain required verification commands but are not additional test layers.

## 12. C# and TypeScript standards

- Use nullable reference types, analyzers, warnings-as-errors, deterministic builds, and a 150-column formatting limit.
- Use file-scoped namespaces, one primary type per file, and no decorative regions.
- Use sealed classes for entities and services and sealed records for immutable inputs, outputs, requests, and responses.
- Use `Async` suffixes and propagate `CancellationToken` through asynchronous I/O.
- Document every public C# type and member with concise, accurate XML documentation.
- Use TypeScript strict mode, no `any`, explicit API and component boundary types, ESLint, and Prettier.
- Keep React components focused, accessible, and free of duplicated derived state.

## 13. Evolution rules

Future changes must preserve the dependency direction and explicit contracts. A new dependency, project, authentication boundary, persistence technology, global state mechanism, or API compatibility strategy requires a written decision explaining why the existing boundary is insufficient.

When a folder becomes a dumping ground, split it by capability or responsibility. Do not solve growth by introducing generic `Services`, `Models`, `Repositories`, `Helpers`, or `Constants` collections with no owning capability.

This document defines code architecture. Product behavior, Bulgarian content, and release-specific acceptance criteria remain in the product and deployment documents linked above.
