# Pirri History

## Project Context

- **Owner:** Jesus
- **Project:** nutridash
- **Stack:** ASP.NET Core 10+ Blazor Server, EF Core 9, SQLite, Google Gemini API, Docker
- **Description:** Aplicacion web de seguimiento nutricional y salud personal organizada por feature slices.
- **Seeded:** 2026-04-29T21:50:01.959+02:00

## Learnings

- Infrastructure ownership is centered on Docker image generation and docker-compose-based local execution.
- DevOps role includes managing base image versions, ensuring central package management (Directory.Packages.props) is copied in Docker builds, and aligning build artifacts across local and containerized environments.
- Solution file consolidation (.sln to .slnx) requires validation: compare project references line-by-line, run `dotnet sln list` on both formats, and verify rebuild succeeds.
- Compose service topology for databases must include healthchecks: app service should depend on database health, not just startup order, to ensure migration/initialization safety.
- Docker image updates are tightly coupled to target framework alignment: .NET 10 target requires SDK 10.0 and ASP.NET 10.0 runtime images; mismatch will cause build failures or binary incompatibility.
- Aspire AppHost PostgreSQL wiring is clearest when server and database resources use separate variables, but the consuming app should still `WaitFor()` the database resource returned by `AddDatabase()` because Aspire treats that child resource as the readiness boundary for connection availability.
