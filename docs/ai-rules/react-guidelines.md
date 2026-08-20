# React and Frontend Guidelines

Read before writing or changing React, TypeScript, or CSS code.

## Component design

- Prefer small, focused functional components with clear responsibilities.
- Keep state as local as possible; lift it only when multiple components genuinely need it.
- Derive values during rendering instead of storing duplicated state.
- Use `useEffect` only to synchronize with external systems such as API calls, browser storage, or subscriptions—not for ordinary calculations or event handling.
- Prefer composition over large components with many configuration props.
- Use clear names for components, props, hooks, and event handlers.
- Use stable identifiers for list keys; never use array indexes when items can change order.
- Avoid premature memoization. Add it only for a measured rendering need.
- Keep TypeScript types explicit at component and API boundaries. Avoid `any`, untyped JSON, and duplicated request/response shapes.
- Reuse existing components and patterns before introducing abstractions.
- Organize the frontend by capability under `features/`, with shared API, notification, storage, and UI code under `shared/`.
- Use one centralized Axios client for HTTP. Components must not call Axios or `fetch` directly.
- Use TanStack Query for server state and cache invalidation. Use local React state for temporary interaction state; do not add a global state library without an explicit decision.

## Data, behavior, and accessibility

- Treat the server as authoritative for authorization and protected operations.
- Do not include restricted data in initial page data, generic API caches, or client bundles before an authorized operation returns it.
- Handle loading, empty, unavailable, validation-error, and server-error states explicitly.
- Preserve responsive behavior, keyboard access, visible focus, readable contrast, and reduced-motion support.
- Use semantic HTML, associated labels, keyboard-operable controls, and appropriate ARIA attributes.
- Do not hide important state changes in color alone; make them understandable in text and structure.
- Parse the backend Problem Details contract centrally. Use a single application notification host for recoverable action feedback, while rendering important failures inline and accessibly.
- Store only versioned, typed browser-local progress through an isolated storage adapter. Never store credentials, tokens, answers, or protected content in browser progress.
- Use CSS Modules for component styling and keep global styles limited to resets, tokens, and application-wide rules.

## Verification

- Run the available formatter, lint, type-check, and production-build commands that are relevant to the change.
- Provide a focused manual UI/UX checklist for the user when browser validation is needed.
