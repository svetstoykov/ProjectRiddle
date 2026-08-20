# Documentation Rules

These rules apply to documentation under `docs/` and to repository-level instruction files. They define how documentation is named, organized, and maintained.

## File names

- Use lowercase kebab-case for new documentation filenames, except for required repository instruction files such as `AGENTS.md` and `CLAUDE.md`.
- Use stable, descriptive names. Do not put status, dates, author names, or temporary words such as `draft` in filenames.
- Keep one source of truth for each requirement. Link to an existing document instead of creating a near-duplicate.

## Document locations

Put each document in the directory that owns its primary purpose:

| Directory | Use for |
| --- | --- |
| `docs/architecture/` | Responsibilities, contracts, boundaries, and deployment decisions. |
| `docs/decisions/` | Durable technical or product decisions and their consequences. |
| `docs/evaluation/` | Spikes, experiments, comparisons, and evidence used to inform a decision. |
| `docs/product/` | Product requirements, user outcomes, workflows, and feature specifications. |
| `docs/tech-debt/` | Deliberately deferred work and known gaps. |
| `docs/ai-rules/` | Durable instructions for agents and contributors. Keep these concise and broadly applicable. |

Do not put implementation code, generated output, operating-system metadata, or general meeting notes in `docs/`.

## Required headers for design documents

Product, architecture, decision, evaluation, and technical-debt documents should identify:

- `Status` — for example `Proposed`, `Accepted`, `In progress`, `Deferred`, or `Superseded`.
- `Last updated` — ISO date format: `YYYY-MM-DD`.
- `Applies to` when the document is limited to a release or feature.
- `Related documents` when the document changes or informs another contract.

Decision records must state the decision, context, and consequences. Proposals must clearly say that they are proposals and are not accepted constraints until approved.

## References and maintenance

- Use relative Markdown links for repository documents.
- Name the substance before the locator. Say what a section or file establishes instead of citing a bare number or filename.
- Update the source-of-truth document when requirements change; do not leave conflicting active wording in separate documents.
- Mark obsolete documents `Superseded` and link to their replacement when preserving their history is useful.
- Keep product requirements and design details out of always-on agent rules unless they are durable constraints needed for implementation.
