# Team Decisions

## Decision 1: PostgreSQL + Docker Compose Migration

**Date:** 2026-05-02T20:54:56.093+02:00  
**Status:** Team consensus  
**Leads:** Torete (design), Vaquilla (implementation), Jaro (validation)  
**Topic:** Database provider switch and compose topology

### Summary

Migrate from SQLite to PostgreSQL as the primary database runtime, with Docker Compose providing a dedicated PostgreSQL service alongside the app container.

### Scope

**Backend:**
- Replace `Microsoft.EntityFrameworkCore.Sqlite` with `Npgsql.EntityFrameworkCore.PostgreSQL`
- Switch `Program.cs` from `UseSqlite()` to `UseNpgsql()`
- Remove SQLite-specific column mappings from `AppDbContext` (`.HasColumnType("TEXT")`)
- Remove SQLite path parsing and fallback logic; fail fast if connection string is missing

**Database Initialization:**
- For now, keep `EnsureCreated()` for schema creation (avoids creating migrations in this pass)
- Reduce `SeedData` to no-op: create schema only, insert no demo business rows

**Infrastructure:**
- Add dedicated PostgreSQL service to `docker-compose.yml` with:
  - Pinned PostgreSQL image version (not `latest`)
  - Named volume for data persistence
  - Environment variables for database name, user, password
  - Healthcheck using PostgreSQL readiness
- App container depends on postgres health before initialization
- Connection string uses compose service DNS: `Host=postgres;Port=5432;Database=nutridash;Username=...;Password=...`

**Configuration:**
- Updated `appsettings.json` fallback to match dev Postgres defaults: `nutridash` database, `nutridash_dev_password` credential placeholder
- VS Code debug/watch tasks use same local PostgreSQL defaults via compose

### Seed Behavior

**No enum lookup tables.** `WorkoutType`, `MealType`, and `IngredientCategory` remain code-only enums, not database lookup tables.

**No demo business data.** Startup creates schema and enumerators only. User creates first `UserMetric`, `AppSettings`, and `Gemini` key rows via Settings UI.

### Out of Scope

- EF Core migrations folder (will be added in future maintenance)
- Relational lookup-table remodeling
- `Ingredient.Category` string→enum normalization (pre-existing issue)
- Gemini integration behavior changes

### Notes

- Local `.squad/` tooling assumes Postgres is available via compose or local service on port 5432
- Dev credentials remain placeholders in committed files only
- Postgres volume persists data across `docker-compose` restarts

---

## Decision 2: .NET 10 + .slnx Consolidation

**Date:** 2026-05-02T19:50:51.761+02:00  
**Status:** Team consensus  
**Leads:** Torete (design), Pirri (solution structure), Jaro (Docker), Vaquilla (docs)  
**Topic:** Target framework alignment and solution file modernization

### Summary

Standardize on .NET 10 runtime and `NutriDash.slnx` as the primary solution file, updating Docker images and tooling to match.

### Current State

- **Directory.Packages.props** already declares `net10.0` as default target
- **NutriDash.Web.csproj** correctly inherits `net10.0`
- **EF Core packages** (v9.0.4) are compatible with .NET 10
- **Dockerfile** currently uses .NET 9 SDK/runtime (mismatch)
- **NutriDash.slnx** exists but is empty (needs regeneration from `.sln`)

### Scope

**Docker Images:**
- Update Dockerfile `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build`
- Update Dockerfile `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final`
- Ensure `Directory.Packages.props` is copied into build context for central package management

**Solution File:**
- Regenerate `NutriDash.slnx` from `NutriDash.sln` using `dotnet sln convert` or Visual Studio
- Verify all projects and configurations are present in the new file
- Keep `NutriDash.sln` for backward compatibility (gradual migration pattern)

**VS Code / Tooling:**
- `.vscode/launch.json` already references `net10.0` binary path (correct)
- Verify `.vscode/tasks.json` (if exists) uses correct SDK version
- Update `dotnet.defaultSolution` setting to point at `NutriDash.slnx` in VS Code C# Dev Kit

**Documentation:**
- Update README.md to note .NET 10 migration as complete milestone
- Update copilot-instructions.md to remove Docker version mismatch warning (after Docker fix)

### Out of Scope

- EF Core version upgrades beyond 9.0.4
- Target framework changes for individual projects
- Solution folder reorganization
- Appsettings or environment configuration changes

---

## Decision 3: Documentation Consolidation

**Date:** 2026-04-29T21:50:01.959+02:00  
**Status:** Implemented  
**Lead:** Torete  
**Topic:** Copilot instructions and team communications

### Summary

Consolidated `.github/copilot-instructions.md` as single source of truth for future Copilot sessions. Updated and tightened `README.md` to match runtime reality.

### Content

**copilot-instructions.md** now covers:
1. Build & run commands (dotnet CLI and Docker)
2. Database behavior (PostgreSQL, migrations, seeding)
3. Architecture (feature slice pattern, DI, startup)
4. Conventions (Spanish UI, feature folder naming, configuration precedence)
5. Testing & linting status (none currently configured)
6. Container details (Docker Compose topology, health checks)
7. Practical guidance (feature additions, DB changes, Gemini integration)

**README.md** tightened to clarify:
- Docker Compose runs app with dedicated PostgreSQL container
- Startup creates schema only; no demo rows are seeded
- Enums (WorkoutType, MealType, IngredientCategory) remain code-only

### Rationale

- Single entry point for future Copilot sessions
- Complete but concise; practical facts only
- No breaking changes; consolidation and expansion of existing docs

---

## Decision 4: Seed Behavior Alignment

**Date:** 2026-05-02T20:54:56.093+02:00  
**Status:** Team consensus  
**Leads:** Vaquilla (implementation), Torete (design gate)  
**Topic:** Startup database initialization contract

### Summary

Startup creates schema only. No demo business rows are seeded. Enums remain code-only.

### Details

- `SeedData.Initialize()` is now a no-op after schema creation
- Removed insertion of:
  - `UserMetric` (user creates first via Settings)
  - `AppSettings` (created as needed by services)
  - `DailyTracking`, `MealPlan`, `Meal`, `Ingredient` (no demo data)
  - Gemini API key row (created via Settings UI)

- `WorkoutType`, `MealType`, `IngredientCategory` remain C# enums, not lookup tables

### Rationale

- Reduces startup coupling to schema changes
- Clarifies that the app must handle an empty database gracefully
- Makes later migrations safer (no seed data to reconcile)

### Out of Scope

- Relational normalization of enum types
- Sample nutrition data or demo content

---

## Notes

**Team alignment:** All decisions reflect consensus from Torete (Lead), Jaro (Tester), Vaquilla (Backend Dev), and Pirri (DevOps/Infra) regarding the PostgreSQL + .NET 10 migration scope.

**Cross-cutting:** PostgreSQL + Docker Compose (Decision 1) and .NET 10 + .slnx (Decision 2) are interdependent. Documentation (Decision 3) and seed behavior (Decision 4) follow from those primary decisions.
