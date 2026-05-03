# Jaro History

## Project Context

- **Owner:** Jesus
- **Project:** nutridash
- **Stack:** ASP.NET Core 10+ Blazor Server, EF Core 9, SQLite, Google Gemini API, Docker
- **Description:** Aplicacion web de seguimiento nutricional y salud personal organizada por feature slices.
- **Seeded:** 2026-04-29T21:50:01.959+02:00

## Learnings

- Testing ownership covers regression checks and edge cases across frontend, backend, and infrastructure changes.
- Tester role includes verifying tooling compatibility (dotnet CLI, project file formats) and identifying migration paths before implementation.
- Integration testing spans both local build paths (dotnet CLI) and container paths (docker-compose up --build) to catch platform-specific mismatches early.
- Validation checklists should cover both baseline (pre-change) and post-change conditions; environment constraints should be documented but don't block team execution.
- Risk assessment for infrastructure changes requires attention to version alignment: Docker images must support target frameworks; healthchecks must validate service readiness before dependent services start.
- Aspire AppHost startup should keep PostgreSQL server and database resources in separate variables, reference the database, and wait on the server resource.
- Aspire local startup depends on a running container runtime because PostgreSQL is provisioned as an orchestrated container.
- An explicit Aspire secret parameter for the PostgreSQL password is a startup risk unless the value is actually supplied through configuration or secrets.
