# Development Standards

## 1) Semantic Commits (English Only)

### Required format

`<type>(<scope>): <imperative summary>`

### Rules

- `type` and `scope` must be lowercase.
- Summary must be in English.
- Use imperative mood (`add`, `fix`, `refactor`, `remove`).
- Keep summary concise (recommended max: 72 chars).
- Do not end the summary with a period.

### Allowed types

- `feat`: new behavior or capability
- `fix`: bug fix
- `refactor`: internal code change without behavior change
- `perf`: performance improvement
- `docs`: documentation changes
- `test`: test additions/changes
- `build`: build/dependency/version changes
- `ci`: pipeline/automation changes
- `chore`: maintenance tasks
- `revert`: rollback of a previous commit

### Examples

- `feat(email-dispatch): add idempotent reservation for digest period`
- `fix(rabbitmq): nack to dlq after retry exhaustion`
- `refactor(template): split locale fallback greeting strategy`
- `docs(architecture): add ddd bounded context organization rules`
- `test(integration): cover duplicate dispatch race condition`

## 2) DDD Context Organization

### Principle

Organize code by **Bounded Contexts**, not by technical utility only.

Each context must own:

- its domain language
- its rules and invariants
- its application workflows
- its infrastructure adapters

### Context structure inside this solution

Within each layer project, group files by context:

- `src/Metria.EmailWorker.Domain/Contexts/<ContextName>/...`
- `src/Metria.EmailWorker.Application/Contexts/<ContextName>/...`
- `src/Metria.EmailWorker.Infrastructure/Contexts/<ContextName>/...`

Suggested folders per context:

- Domain: `Aggregates`, `Entities`, `ValueObjects`, `DomainEvents`, `DomainServices`
- Application: `UseCases`, `Commands`, `Queries`, `Dtos`, `Policies`
- Infrastructure: `Persistence`, `Messaging`, `Providers`, `Mappings`

### Context boundaries

- Do not share domain entities across contexts.
- Cross-context communication must happen through:
  - contracts (DTO/message contract), or
  - domain/integration events.
- Prefer Anti-Corruption Layer (ACL) adapters when integrating external or legacy models.

### Naming convention

- Context names must be in English and explicit (example: `EmailDispatch`, `DigestScheduling`).
- Avoid generic names like `Common`, `Helpers`, `Utils` as context containers.

### Current guidance for this worker

Primary bounded context:

- `EmailDispatch` (message ingestion, idempotency, template resolution, provider dispatch)

Secondary/support context candidates (if complexity grows):

- `DigestScheduling`
- `Observability`

## 3) Mandatory Git Rule After Each Development

After finishing any development task, execute this flow in order:

1. Run validation (build/tests/lint as applicable).
2. Create one semantic commit in English.
3. Push to remote branch.

### Minimum command sequence

```bash
git add -A
git commit -m "<type>(<scope>): <imperative summary>"
git push origin <branch-name>
```

### Additional rules

- Do not finish a development task with uncommitted local changes.
- Do not use vague messages like `update`, `fixes`, `changes`.
- If the change is large, split into logical semantic commits before pushing.
- If validation fails, do not commit/push until fixed or explicitly documented.
