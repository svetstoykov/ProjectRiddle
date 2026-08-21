# Repository Instructions

This file indexes the durable AI rules in `docs/ai-rules/`. Read the always-on files before acting, then read every file whose trigger applies to the task.

## Repository invariants

- The repository-root `docs/` tree is intentionally ignored. Keep `/docs/` in `.gitignore` and do not add negation patterns that re-include any path beneath it. Transient planning names such as phases and versions live only in that untracked tree and must not leak into tracked artifacts.

## Always read

| File | Contents |
| --- | --- |
| [`docs/ai-rules/core-principles.md`](docs/ai-rules/core-principles.md) | Agent authority, general engineering principles, organization, and code-quality rules. |
| [`docs/ai-rules/workflow-and-boundaries.md`](docs/ai-rules/workflow-and-boundaries.md) | Communication, security, scope, data-safety, and validation boundaries. |

## Read when the trigger applies

| Trigger | File |
| --- | --- |
| Adding or moving a project, changing dependencies, or changing an API, domain, persistence, authentication, or deployment boundary | [`docs/ai-rules/architecture-boundaries.md`](docs/ai-rules/architecture-boundaries.md) |
| Writing or changing backend C# or .NET code | [`docs/ai-rules/csharp-guidelines.md`](docs/ai-rules/csharp-guidelines.md) |
| Writing or reviewing C# XML documentation comments | [`docs/ai-rules/csharp-xml-documentation.md`](docs/ai-rules/csharp-xml-documentation.md) |
| Adding or changing backend diagnostics, log statements, or logging configuration | [`docs/ai-rules/dotnet-logging.md`](docs/ai-rules/dotnet-logging.md) |
| Writing or changing React, TypeScript, or CSS code | [`docs/ai-rules/react-guidelines.md`](docs/ai-rules/react-guidelines.md) |
| Adding or changing tests, or claiming a change is complete | [`docs/ai-rules/testing.md`](docs/ai-rules/testing.md) |
| Creating, moving, naming, or substantially revising documentation | [`docs/ai-rules/documentation.md`](docs/ai-rules/documentation.md) |

## Maintaining these rules

- Add a durable rule to the file whose trigger already covers it.
- Create a new bounded rule file only when a genuinely new area appears, and add its trigger row here in the same change.
- Keep product requirements, technology choices, and detailed design decisions outside `docs/ai-rules/`.
