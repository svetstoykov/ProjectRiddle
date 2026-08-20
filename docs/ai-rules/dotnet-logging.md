# Backend Logging

Read before adding or changing backend diagnostics, log statements, or logging configuration.

- Inject `ILogger<T>` into backend services. Do not use ad hoc `Console.WriteLine`, static loggers, or provider-specific logger types for application diagnostics.
- Configure logging providers at the application composition boundary. Backend services should depend on the logging abstraction rather than a concrete provider.
- Use structured message templates with stable, descriptive property names. Do not build log messages with string interpolation.
- Record meaningful lifecycle and operation outcomes with safe identifiers and elapsed time where useful.
- Use `Debug` for detailed diagnostic context. Use `Warning` or higher for degraded, rejected, or unexpected conditions.
- Never log secrets, credentials, session material, private content, or unneeded user input.
- Log expected validation failures once at the layer with the most context. Do not repeat the same failure at every forwarding layer.
- Log unexpected exceptions once at the exception boundary and include the exception object. Do not convert normal validation failures into noisy exceptions.
- For storage, filesystem, migration, or external-process operations, log the operation and outcome at a safe level; keep raw queries, paths, and payloads out of normal operator-facing logs.
- Include request or operation correlation information when the hosting layer provides it, but do not treat correlation identifiers as secrets.
- Test logging only when it is part of a meaningful security, diagnostics, or response contract.
- The global exception handler is the single logging boundary for unexpected request exceptions. Return the same safe trace identifier in Problem Details without returning exception details or stack traces.
