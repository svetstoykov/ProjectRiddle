# Project Riddle V1 Subphase 4: Deployment and Readiness

**Status:** Approved in conversation; awaiting written-spec review  
**Last updated:** 2026-08-20  
**Applies to:** Project Riddle V1, subphase 4  
**Related documents:**

- [The V1 overview defines the complete product boundary](../design/2026-08-20-project-riddle-v1-design.md).
- [Subphase 1 defines persisted riddles and administrator authentication](../product/project-riddle-v1-subphase-1-authoring-and-publication.md).
- [Subphase 2 defines the public riddle experience](../product/project-riddle-v1-subphase-2-public-riddle-experience.md).
- [Subphase 3 defines the guided curriculum](../product/project-riddle-v1-subphase-3-guided-courses.md).

## 1. Purpose

This subphase defines how the completed Project Riddle capabilities become one supportable V1 application on a Raspberry Pi. It owns the production composition, persistent data boundary, startup behavior, runtime configuration, operating documentation, cross-cutting failure behavior, and lean release verification.

It is the point where authoring, public play, and courses are evaluated together. It does not add another product capability or broaden the V1 infrastructure footprint.

## 2. Operational Context

Project Riddle is a private, locally operated home-server application. The supported production target is Linux ARM64 on a Raspberry Pi, while normal development and image builds must remain possible on AMD64 machines.

V1 uses one application container and SQLite. It has no external cloud dependency, managed database, separate frontend host, distributed service, or public ingress design.

## 3. Approved Technology and Composition

- **Backend:** ASP.NET Core on .NET 10 LTS.
- **Frontend:** React with TypeScript and Vite.
- **Persistence:** EF Core 10 with SQLite.
- **Hosting:** One multi-stage Docker image serving the API and compiled SPA.
- **Runtime composition:** One Docker Compose application service and one exposed HTTP port.
- **Production target:** Linux ARM64 Raspberry Pi, with AMD64 development builds retained.

The frontend is compiled during the image build. ASP.NET Core serves the resulting static assets and owns the API routes, so production does not require separate frontend and backend containers.

## 4. Runtime Component Boundaries

The deployed application contains these logical components:

- **Riddle domain** — answer normalization, answer-pattern validation, range validation, and publication rules.
- **Public riddle API** — today's clue, archive, eligible riddle details, answer checking, letter reveal, and full reveal.
- **Admin API** — authentication and riddle authoring and publication operations.
- **Course catalog** — typed static curriculum data.
- **Riddle player** — shared React interaction for daily, archive, and course exercises.
- **Progress store** — versioned browser-storage adapter isolated from view components.
- **Admin editor** — riddle form, range-labeling interaction, preview, and publication actions.

These are application boundaries, not separately deployed services. The single-container design keeps local operation small while preserving internal separation between answer-sensitive server behavior, static course content, and browser-only progress.

## 5. Persistent Data Boundary

Docker Compose mounts a bind directory or named volume at `/data`. The persistent boundary contains:

- The SQLite database with administrator-authored riddles and publication state.
- ASP.NET Core Data Protection keys used by administrator sessions.

Both survive container replacement and restart. The application image contains neither runtime database content nor production Data Protection keys.

Browser progress remains in each visitor's browser and is not part of `/data`, SQLite backup, or server recovery.

## 6. Configuration and Secrets

Runtime configuration includes:

- An administrator password hash.
- The persistent data location.
- `Europe/Sofia` as the publication timezone.
- Normal ASP.NET Core settings required to bind the single HTTP service.

The administrator password hash and other runtime secrets live in an uncommitted environment file or equivalent local configuration. They are not baked into the image, committed to source control, returned by an endpoint, or written to logs.

The V1 service is usable over private-LAN HTTP. Secure-cookie behavior follows the request scheme as defined by the authentication design. A custom domain, TLS, reverse proxy, internet exposure, and public threat perimeter are separate deployment concerns outside V1.

## 7. Database Startup and Recovery

EF Core migrations run automatically during application startup before HTTP traffic is served. A successful migration brings the database to the schema expected by the running image.

A failed migration prevents application startup. The service must not accept requests against an unknown or partially migrated schema.

SQLite data is persistent user content. Update guidance must identify the data location and require a recoverable backup path. Backup and restore operate on the documented persistent data rather than rebuilding the database from authored source.

The design does not authorize destructive reset or automatic loss of incompatible data as an ordinary recovery strategy.

## 8. Operating Model

The repository README documents:

- First-time Raspberry Pi installation.
- Administrator password-hash generation.
- Starting and stopping the application.
- Rebuilding and updating the container.
- The persistent-data location.
- SQLite backup and restore.
- The expected behavior when startup migration fails.

Docker Compose applies a restart policy suitable for a home server. Restart behavior must not bypass migration failure or conceal a configuration problem behind an apparently healthy public page.

## 9. Cross-Cutting Failure Behavior

- No published riddle for today produces the public empty state defined in subphase 2.
- Unknown, draft, future, and unpublished riddle identifiers return `404` through public endpoints.
- Invalid answers produce ordinary incorrect results.
- Expired administrator sessions return `401` and do not expose unpublished data.
- Publication conflicts and invalid authoring forms return field-level feedback.
- Database unavailability produces a generic public failure and a server-side diagnostic that excludes secrets, answers, and unpublished content.
- A migration failure prevents startup rather than serving against an uncertain schema.
- The application never substitutes the latest previous riddle for a missing daily clue.

The frontend should preserve a useful navigation path when one capability is unavailable, but V1 does not introduce an offline mode, queue, failover database, or distributed recovery system.

## 10. Lean Verification Strategy

V1 verification is deliberately narrow and centered on high-risk service behavior:

- `ProjectRiddle.IntegrationTests` exercises Core services through their public interfaces with real Infrastructure implementations and disposable SQLite data.
- The integration suite covers answer and pattern validation, publication visibility and Sofia-date selection, administrator authentication or publication validation, and the core published-riddle flow.
- The suite controls clocks, time zones, randomness, and external dependencies so service outcomes remain deterministic.

V1 does not establish separate unit-test, controller-test, repository-only-test, EF-configuration-test, frontend automated-test, browser automation, end-to-end, snapshot, or coverage projects. Production builds, frontend type-checks, Docker packaging, and manual UI review remain release checks.

Release-level verification consists of:

- The small backend test suite passing.
- A successful frontend production build.
- A successful backend release build.
- A successful Docker image build on the available development architecture.
- An ARM64-compatible Dockerfile and a documented Raspberry Pi build and run path.

Manual UI and UX review remains necessary for usability and responsive behavior; it is not replaced by the lean automated suite.

## 11. V1 Readiness Boundary

The application is operationally ready for V1 when:

- Authoring, publication, daily play, archive play, and the complete course curriculum are available through one composed application.
- The production image serves both the API and compiled frontend through one port.
- SQLite and Data Protection keys survive container replacement through `/data`.
- Startup applies migrations before serving traffic and fails closed when migration cannot complete.
- Runtime secrets remain outside source control and the built image.
- The Dockerfile supports the ARM64 target without preventing AMD64 development builds.
- The documented Raspberry Pi procedure covers installation, credential setup, routine lifecycle operations, persistent data, backup, and restore.
- The defined backend checks and production builds pass.
- The public and administrator failure behaviors preserve the content-security boundaries defined by the earlier subphases.
- The UI is usable and responsive while remaining visually replaceable.

This readiness boundary describes the supported release state. It does not introduce a CI/CD platform, public hosting architecture, observability stack, broad test program, or step-by-step implementation plan.
