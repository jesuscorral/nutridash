# Vaquilla History

## Project Context

- **Owner:** Jesus
- **Project:** nutridash
- **Stack:** ASP.NET Core 10+ Blazor Server, EF Core 9, SQLite, Google Gemini API, Docker
- **Description:** Aplicacion web de seguimiento nutricional y salud personal organizada por feature slices.
- **Seeded:** 2026-04-29T21:50:01.959+02:00

## Learnings

- Backend ownership includes EF Core, SQLite, ASP.NET Core application logic, and Gemini API integration.
- Developer experience role extends to configuration management (VS Code settings, launch.json, tasks.json) and removing documentation warnings once dependencies are resolved.
- Point VS Code's dotnet.defaultSolution to the canonical solution file to prevent tool ambiguity when multiple solution formats exist during migration.
- Provider migration is surgical: swap EF provider package + connection string method (UseSqlite → UseNpgsql), remove provider-specific mappings (.HasColumnType), but preserve service fallback logic to handle gracefully missing demo data.
- Seed behavior reduction must maintain backward compatibility: services should not assume demo rows exist; use fallback defaults (missing UserMetric, GeminiApiKey) to preserve functional correctness.
