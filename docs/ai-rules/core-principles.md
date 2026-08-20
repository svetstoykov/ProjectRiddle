# Core Principles

Always in effect. Read this file at the start of every session.

## Agent authority

- Do not invoke a Superpowers skill unless the user explicitly confirms its use for the current task. A request to perform a task does not by itself authorize a Superpowers skill.
- Follow repository instructions and the user's explicit directions. When they conflict, ask for clarification before acting.
- Do not expand a focused request into unrelated refactoring, dependencies, infrastructure, or features.

## Engineering priorities

Apply these principles in priority order:

1. Simplicity: keep implementations and interfaces small.
2. Correctness: preserve observable behavior and enforce important invariants.
3. Consistency: use the same rules and vocabulary at equivalent boundaries.
4. Completeness: cover reasonably expected cases without adding speculative complexity.

## Code organization

- Use the repository structure that exists. Organize code by capability before technical role when practical.
- Use the approved service-driven modular-monolith structure: API delivery, Core behavior, Infrastructure, and frontend presentation remain separate responsibilities. Do not reorganize the backend around vertical slices.
- Group capability-specific types beneath their owning technical area. Keep services, interfaces, models, enums, repositories, controllers, and frontend code in the project that owns the boundary rather than creating project-wide dumping grounds.
- Keep domain behavior, API contracts, persistence records, and presentation concerns distinct.
- Keep public contracts separate from persistence entities. Do not expose storage records directly at an API boundary.
- Public service methods use explicit operation inputs and outputs and return the shared Result contract for expected outcomes.
- Do not create project-wide dumping grounds for models, services, interfaces, or constants.
- Do not create an interface mechanically for every class. Use one when it represents a meaningful boundary or substitution point.
- Use concrete types or explicit polymorphism in application contracts. Avoid `object`, `dynamic`, `any`, and untyped dictionaries.
- Keep one primary type per file where the language and layout support it. Avoid nested classes and decorative region separators.
- Keep stable non-prose literals close to their capability rather than in a project-wide constants dump.
- Keep one-off human-readable messages at their call sites unless they are reused or form a stable external contract.
- Write comments and documentation about current behavior; let version history explain changes.
