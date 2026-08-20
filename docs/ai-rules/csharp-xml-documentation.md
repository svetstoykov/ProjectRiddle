# C# XML Documentation Comments

Read before writing, reviewing, or maintaining XML documentation comments on C# code.

The goal is quick understanding, not exhaustive prose. Use plain, direct language and document behavior, constraints, and context that matter to a maintainer or caller.

## Required coverage

- Document every non-private type and member, including parameters, type parameters, return values, and explicitly thrown exceptions.
- Document private members only when their purpose, invariant, side effect, algorithm, workaround, or security implication is not obvious from the name and signature.
- Keep XML well formed. Every `cref` and `name` attribute must identify the actual type, member, or parameter.

## Tags and phrasing

- Use `<summary>` for one short complete sentence answering what the member does. End it with a period.
- Use `<remarks>` for why the member exists, important constraints, side effects, lifetime, ordering, or thread-safety behavior.
- Use `<param>` and `<typeparam>` to explain what each input represents and its null/empty constraints.
- Use `<returns>` to explain what a returned value represents, not just its type.
- Use `<exception cref="...">` for exceptions explicitly thrown by the member, stating the condition that causes each one.
- Use `<inheritdoc />` for overrides and interface implementations whose contract is unchanged.
- Use `<see cref="..." />` for types and members, `<see langword="..." />` for C# keywords, and `<c>...</c>` for short literals or conventions.

Use natural canonical phrasing:

```csharp
/// <summary>
///     Asynchronously processes the supplied request.
/// </summary>
/// <param name="request">The request to process. Cannot be <see langword="null" />.</param>
/// <returns>A task that represents the asynchronous processing operation.</returns>
```

- Summaries for methods should use a third-person present-tense verb and should not begin with “This method”.
- Async summaries may begin with “Asynchronously”; document the task result when the task produces a value.
- Boolean properties should say “Gets or sets a value indicating whether…” and use `<see langword="true" />` and `<see langword="false" />`.
- Type summaries may use “Represents” for domain objects, “Defines” for contracts/enums, and “Provides” for helper types.
- Use one `<para>` per distinct idea in multi-part remarks. Do not narrate historical behavior.

## Review checklist

Review in this order:

1. Required members are documented and private comments are justified.
2. Each summary is short, accurate, grammatical, and ends with a period.
3. Parameters, type parameters, returns, and explicit exceptions are complete.
4. Cross-reference tags are correct and keywords use `<see langword="..." />`.
5. Remarks explain behavior and constraints without leaking implementation history.
