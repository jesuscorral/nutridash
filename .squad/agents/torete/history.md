# Torete History

## Project Context

- **Owner:** Jesus
- **Project:** nutridash
- **Stack:** ASP.NET Core 10+ Blazor Server, EF Core 9, SQLite, Google Gemini API, Docker
- **Description:** Aplicacion web de seguimiento nutricional y salud personal organizada por feature slices.
- **Seeded:** 2026-04-29T21:50:01.959+02:00

## Learnings

- Team created with a dedicated frontend, backend, tester, and container-focused infra specialist.
- Lead role includes comprehensive design reviews, decision scoping, and team consensus orchestration.
- Design decisions should document exact scope, ownership, dependency chains, and integration test plans to enable parallel team execution.
- Consolidated copilot-instructions.md serves as single source of truth for Copilot sessions — include architecture, conventions, commands, and practical guidance.
- Provider migration design reviews must specify safety constraints: provider-only swaps, no model remodeling, no lookup-table seeding unless explicitly requested.
- Design review artifacts should be recorded as decisions to preserve rationale for future team reference (schema-first migrations, seed behavior contracts).
- Solution consolidation decisions require explicit dependency mapping: Docker images must align with target framework; documentation updates follow infrastructure changes.
- Aspire resource chaining: Always separate `AddPostgres()` (returns server) from `.AddDatabase()` (returns database). WaitFor() must target the server resource, not the database, to avoid circular dependency and startup timeouts. Distribute the resources into separate variables to make dependencies explicit.
