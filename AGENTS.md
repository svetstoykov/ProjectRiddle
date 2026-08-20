# Project Riddle Repository Instructions

This file is an index. Durable engineering rules live in `docs/ai-rules/` as bounded context files so that only the relevant guidance needs to be read for a task.

## Always read

Read these two files before doing anything else in a session:

| File | Contents |
| --- | --- |
| [`docs/ai-rules/core-principles.md`](docs/ai-rules/core-principles.md) | Product scope, approved V1 technology, repository organization, design priorities, and general coding rules. |
| [`docs/ai-rules/workflow-and-boundaries.md`](docs/ai-rules/workflow-and-boundaries.md) | Communication, security, scope, data-safety, and validation boundaries. |

## Read when the trigger applies

Read a file before starting work that matches its trigger. When several triggers apply, read all of them.

| Trigger | File |
| --- | --- |
| Adding or moving a project, changing dependencies, or changing the public/admin API, domain, persistence, authentication, or deployment boundaries | [`docs/ai-rules/architecture-boundaries.md`](docs/ai-rules/architecture-boundaries.md) |
| Writing or changing backend C# or .NET code | [`docs/ai-rules/csharp-guidelines.md`](docs/ai-rules/csharp-guidelines.md) |
| Writing or reviewing C# XML documentation comments | [`docs/ai-rules/csharp-xml-documentation.md`](docs/ai-rules/csharp-xml-documentation.md) |
| Adding or changing backend diagnostics, log statements, or logging configuration | [`docs/ai-rules/dotnet-logging.md`](docs/ai-rules/dotnet-logging.md) |
| Writing or changing React, TypeScript, or CSS code | [`docs/ai-rules/react-guidelines.md`](docs/ai-rules/react-guidelines.md) |
| Adding or changing tests, or claiming a change is complete | [`docs/ai-rules/testing.md`](docs/ai-rules/testing.md) |
| Creating, moving, naming, or substantially revising documentation | [`docs/ai-rules/documentation.md`](docs/ai-rules/documentation.md) |

## Maintaining these rules

- Add a durable rule to the file whose trigger already covers it.
- Create a new bounded rule file only when a genuinely new area appears, and add its trigger row here in the same change.
- Keep detailed, frequently changing product requirements in a product or feature specification, not in `docs/ai-rules/`.
- Keep these instructions aligned with the approved Project Riddle V1 design. Do not add rules for features or infrastructure that V1 does not contain.
