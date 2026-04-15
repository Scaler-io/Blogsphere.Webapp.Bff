# Feature test coverage

This command drives **scanning the codebase**, **comparing features to existing tests**, and **adding only the tests that are missing**—without duplicating work or bloating the suite.

Use it when new handlers, services, or API behavior land and you want unit (or focused integration) tests aligned with current patterns in the repo.

---

## Quality bar (every new or extended test must satisfy)

1. **Readable and intent-revealing** — Test names describe behavior and expected outcome (e.g. `Handle_when_scope_is_unknown_returns_bad_request`), not internal names only.
2. **Focused on a single behavior** — One logical outcome per test method; split scenarios instead of asserting everything in one test.
3. **Independent and isolated** — No order dependency; use mocks/fakes for I/O, time, and external services so tests do not hit real networks, databases, or shared mutable state.
4. **Deterministic and repeatable** — No reliance on wall-clock randomness unless explicitly controlled; same result every run.
5. **Fast execution** — Prefer unit tests with mocks; reserve slow integration tests for HTTP/host boundaries and keep them few and purposeful.
6. **Moq used appropriately** — Mock only boundaries (providers, cache, logger when needed); avoid mocking the class under test; do not verify every trivial interaction—only when the contract matters.
7. **Clear and precise assertions** — Prefer FluentAssertions (or project conventions) with specific expected values, not vague truthiness.
8. **Minimal and clean setup** — Reuse project test helpers (`TestHelpers`, `TestData`, shared mappers); extract Arrange only when it stays readable.
9. **Edge cases and error paths** — Include failure branches, empty/null inputs, and “happy path” where they add confidence—not exhaustive combinatorics unless justified.
10. **Behavior, not implementation** — Assert observable outcomes (returned `Result`, DTO fields, cache calls when behavior requires it); avoid tying tests to private method names or refactor-only details.
11. **xUnit features** — Use `[Fact]` for single scenarios; `[Theory]` with `[InlineData]` or `MemberData` for parameterized cases; use descriptive parameter names.
12. **Interaction verification only when necessary** — Verify `Times.Never` / `Times.Once` when the test’s purpose is to prove delegation, skipping work, or side effects; otherwise prefer state/output assertions.

---

## Instructions

Follow these steps **in order**. Prefer **repository tools** (search, glob, read) over guessing file locations.

### Step 1: Map testable features

Scan the codebase and build a short inventory of **what should have tests** (prioritize in this order):

- **Application layer**: CQRS handlers (`*QueryHandler`, `*CommandHandler`), domain services, orchestration (`*ScopeHandler`, `*Enricher`, `*Contributor`), and non-trivial static helpers used in business paths.
- **Skip or lower priority**: pure DTOs, AutoMapper profiles without logic, DI registration-only code, generated code.

Record for each candidate: **type name**, **file path**, and **primary responsibilities** (one line each).

### Step 2: Inventory existing tests

Discover test projects (e.g. `*UnitTests*`, `*IntegrationTests*`) and list test files that correspond to features:

- Glob `**/*Tests*.cs` (or project-specific patterns).
- For each feature type from Step 1, determine if a test class exists (same feature area / naming convention as the repo).
- Count **test methods** (`[Fact]`, `[Theory]`) per relevant test class—give a **numeric summary** (e.g. “`GetDashboardQueryHandler`: 0 tests; `GetApiRouteByIdQueryHandler`: 1 test class, 3 methods”).

### Step 3: Gap analysis

Produce a **gap table** (markdown table or bullet list):

| Feature / SUT | Test class present? | Approx. # tests | Missing scenarios (behaviors/errors) |
|---------------|---------------------|-----------------|--------------------------------------|

Mark items **complete**, **partial**, or **missing**. Do not re-implement tests that already cover a behavior unless they are wrong or violate the quality bar.

### Step 4: Implement only what is missing

For each **partial** or **missing** row:

1. Read the **production code** under test and **one similar existing test file** in the same project to match style, usings, and helper usage.
2. Add or extend tests so the **observable behaviors** are covered per the quality bar.
3. Keep diffs **minimal**—new files or small targeted edits; no unrelated refactors.
4. For integration tests (if the repo uses them for controllers): follow `WebApplicationFactory` / existing endpoint test patterns; do not add secrets to `appsettings`.

### Step 5: Run tests and fix failures

Run the appropriate test command for the solution or project, e.g.:

```bash
dotnet test path/to/Solution.sln
```

Fix compilation or test failures introduced by the change. If the host locks build outputs, run unit-test projects only or stop the running API process, then re-run.

### Step 6: Report back

Summarize for the user:

- Features scanned and test counts **before** vs **after**
- New or updated test files
- Any areas **still** uncovered (with brief justification—e.g. needs full integration environment)
- Confirmation that tests were run successfully

---

## Rules

- Do **not** commit secrets or real keys into tests or tracked config; use fakes, test doubles, or integration-test overrides already used in the repo.
- Do **not** add markdown documentation files unless the user asks.
- Match **existing** test stack (xUnit, Moq, FluentAssertions in this repo).
- Prefer **unit tests** for handlers with mocked dependencies; use **integration tests** sparingly for HTTP/API contracts.
