# Testing

Read before adding or changing tests, or before claiming a change is complete.

## Test scope

- The default automated test boundary is service-layer integration testing in `tests/ProjectRiddle.IntegrationTests`.
- Exercise Core services through their public interfaces with real Infrastructure implementations and disposable SQLite data.
- Verify observable workflows, persistence behavior, authorization-relevant outcomes, shared Result failures, and time-sensitive rules at that boundary.
- Do not add unit-test, controller-test, repository-only-test, EF-configuration-test, browser-automation, or frontend-test projects without an explicit architectural decision.
- Do not write tests that only prove compiler behavior, constructors, interface implementations, or standard-library behavior.
- Never expose or hard-code secrets in tests. Use disposable data and dedicated test configuration.
- Control clocks, time zones, randomness, and external dependencies so service integration tests are deterministic.

## Completion verification

- Before claiming a change is complete, run the service integration tests and the relevant backend build, frontend type-check/build, formatter, lint, or packaging checks.
- Report the commands run and their results. Do not claim checks passed unless current output confirms it.
- For frontend changes, provide a concise manual UI/UX validation checklist; automated browser testing is not part of the default V1 strategy.
