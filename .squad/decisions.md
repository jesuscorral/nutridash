# Squad Decisions

## Active Decisions

### 1. .NET 10 Migration & .slnx Consolidation Design Review
**Date:** 2026-05-02T19:50:51.761+02:00  
**Author:** Torete (Lead)  
**Status:** Approved by team consensus

#### Executive Summary
Convert project to .NET 10 and consolidate on `.slnx` only. Codebase is 90% ready; critical blocker: Dockerfile uses .NET 9 SDK/runtime while project targets net10.0. `.slnx` file exists but is empty and needs regeneration.

#### Scope
1. **Container Runtime** (Owner: Jaro) — Update Dockerfile to .NET 10 SDK and ASP.NET 10.0 images
2. **Solution File Consolidation** (Owner: Pirri) — Regenerate `NutriDash.slnx` from `.sln` and retire `.sln`
3. **Documentation & Tooling** (Owner: Vaquilla) — Update launch.json, tasks.json, copilot-instructions.md, README.md

#### Implementation Order
1. Jaro updates Docker (parallel)
2. Pirri regenerates `.slnx` (parallel)
3. Vaquilla updates documentation (after #1 and #2)
4. Integration test: `dotnet build NutriDash.slnx` locally and `docker-compose up --build`

#### Files Affected
- `Dockerfile` — Update base images to 10.0
- `NutriDash.slnx` — Regenerate from .sln
- `.github/copilot-instructions.md` — Update warnings
- `README.md` — Note .NET 10 migration complete
- `.vscode/launch.json` — Verify net10.0 references
- `.vscode/tasks.json` — Verify SDK references

---

### 2. Standardize on NutriDash.slnx (Pirri's Decision)
**Date:** 2026-05-02T19:50:51.761+02:00  
**Author:** Pirri (Backend/DevOps)  
**Status:** Approved

#### Summary
Standardize repo-level solution workflows on `NutriDash.slnx` and retire `NutriDash.sln`. Docker builds must copy `Directory.Packages.props` for correct central package management inside SDK 10 image.

#### Decision
- Solution-oriented tooling points at `NutriDash.slnx`
- .sln is removed after successful .slnx migration
- Docker Dockerfile includes `Directory.Packages.props` copy step

---

### 3. .NET 10 + SLNX Alignment (Vaquilla's Decision)
**Date:** 2026-05-02T19:50:51.761+02:00  
**Author:** Vaquilla (Frontend/DevExp)  
**Status:** Approved

#### Summary
Keep `NutriDash.slnx` as sole solution file to avoid `.sln`/`.slnx` ambiguity. Point VS Code's `dotnet.defaultSolution` at `.slnx`. Align Docker with existing net10.0 target.

#### Decision
- `NutriDash.slnx` is the only solution file
- VS Code configured to use `NutriDash.slnx` via `dotnet.defaultSolution`
- Docker uses .NET 10 SDK and ASP.NET 10.0 runtime images

---

### 4. Copilot Instructions Consolidated
**Date:** 2026-04-29T21:50:01.959+02:00  
**Decided by:** Torete (Lead)  
**Status:** Implemented

#### Summary
Consolidated `.github/copilot-instructions.md` as single source of truth. Now covers build & run commands (dotnet + Docker), database behavior, architecture (feature slice pattern), conventions (Spanish UI, feature folders), testing/linting status, container details, and practical guidance for future development.

#### Rationale
- Single entry point for Copilot sessions
- Complete but concise (no marketing language)
- Reflects verified team findings
- Enables accurate, consistent future development guidance

#### No Breaking Changes
Consolidation and expansion of existing documentation; does not alter codebase, DI registration, or runtime behavior.

### 5. Aspire Support Implementation (Pirri's Work Summary)
**Date:** 2026-05-02T21:13:51.285+02:00  
**Author:** Pirri (Backend/DevOps)  
**Status:** Implemented

#### Summary
Added .NET Aspire orchestration framework to NutriDash. The AppHost now manages PostgreSQL database and web application lifecycle. Consolidated solution artifacts to use only `NutriDash.slnx`, removing the legacy `.sln` file.

#### Key Changes
- Created `src/NutriDash.ServiceDefaults/` — shared configuration library with health checks and resilience patterns
- Created `src/NutriDash.AppHost/` — distributed application host orchestrating PostgreSQL and web service
- Updated `Directory.Packages.props` with Aspire framework packages (v13.2.4)
- Updated `NutriDash.slnx` to include AppHost and ServiceDefaults projects
- Removed `NutriDash.sln` (superseded by .slnx)
- Updated `README.md` and `.github/copilot-instructions.md` with Aspire documentation

#### Development Paths Now Available
1. **Docker Compose (Production):** `docker compose up --build` → port 8080
2. **Aspire (Orchestrated Development):** `dotnet run --project src/NutriDash.AppHost/` → Aspire dashboard
3. **Direct dotnet CLI (Minimal):** `docker compose up -d postgres` + `dotnet run --project src/NutriDash.Web/` → ports 50757/50758

---

### 6. Aspire AppHost Startup Dependency Clarification (Pirri's Decision)
**Date:** 2026-05-03T17:22:57.635+02:00  
**Author:** Pirri (Backend/DevOps)  
**Status:** Accepted & Implemented

#### Decision
Keep PostgreSQL server and database resources split in `src/NutriDash.AppHost/Program.cs` for clarity. Web project both references and waits for the database resource returned by `AddDatabase()`.

#### Rationale
Official Aspire guidance treats `PostgresDatabaseResource` as the readiness boundary for consumers. `AddDatabase()` explicitly documents that dependents waiting on it block until the database is available. Using separate variables for server and database without moving `WaitFor()` off the database resource provides clarity while aligning with platform guidance.

---

### 7. Aspire Startup Path Decision (Jaro's Decision)
**Date:** 2026-05-03T17:22:57.635+02:00  
**Author:** Jaro (Tester)  
**Status:** Accepted & Implemented

#### Decision
Use Aspire's default PostgreSQL credential generation for local AppHost startup instead of declaring an unsatisfied `postgres-password` secret parameter. Document that Aspire startup requires Docker Desktop or another compatible local container runtime to be running first.

#### Rationale
The checked-in AppHost with an unsatisfied secret parameter makes the out-of-the-box startup path fragile. The existing team skill already treats server/database separation as the safe dependency model, so the code and docs should match that path.

---

### 8. .NET 10 Migration & .slnx Consolidation Design Review (Torete's Review)
**Date:** 2026-05-02T19:50:51.761+02:00  
**Author:** Torete (Lead)  
**Status:** Approved by team consensus

#### Executive Summary
Convert project to .NET 10 and consolidate on `.slnx` only. Codebase is 90% ready; critical blocker: Dockerfile uses .NET 9 SDK/runtime while project targets net10.0. `.slnx` file exists but is empty and needs regeneration.

#### Scope
1. **Container Runtime** (Owner: Jaro) — Update Dockerfile to .NET 10 SDK and ASP.NET 10.0 images
2. **Solution File Consolidation** (Owner: Pirri) — Regenerate `NutriDash.slnx` from `.sln` and retire `.sln`
3. **Documentation & Tooling** (Owner: Vaquilla) — Update launch.json, tasks.json, copilot-instructions.md, README.md

#### Implementation Order
1. Jaro updates Docker (parallel)
2. Pirri regenerates `.slnx` (parallel)
3. Vaquilla updates documentation (after #1 and #2)
4. Integration test: `dotnet build NutriDash.slnx` locally and `docker-compose up --build`

---

### 9. PostgreSQL + Docker Compose Migration Scope (Torete's Design Review)
**Date:** 2026-05-02T20:54:56.093+02:00  
**Author:** Torete (Lead)  
**Status:** Approved as contract-tightening change only

#### Approved Scope
1. Replace EF Core SQLite provider with PostgreSQL provider
2. Add PostgreSQL as a second service in `docker-compose.yml`
3. Remove SQLite-only startup/model assumptions
4. Stop seeding sample business data
5. Treat enums as code-level enums only (no lookup-table seeding)

#### Initialization Strategy
Approved migration from `EnsureCreated()` to **EF Core migrations + `Database.Migrate()`** to establish a persistent relational service contract.

#### Seed Contract Decision
- **Do seed:** Schema creation only
- **Do NOT seed:** Demo business rows, `UserMetric`, `AppSetting`, `DailyTracking`, `MealPlan` entries
- **Keep in code:** `WorkoutType`, `MealType`, `IngredientCategory` remain code-only enums (no database lookup tables)

#### Routing
- **Backend owner:** Pirri — provider switch, migrations, seed reduction
- **Infra owner:** Jaro — compose PostgreSQL topology and container contract
- **Validation/docs owner:** Vaquilla — local run docs, VS Code task/launch connection-string updates

---

### 10. Aspire Support + .slnx Consolidation Design Review (Torete's Design Review)
**Date:** 2026-05-02T21:13:51.285+02:00  
**Author:** Torete (Lead)  
**Status:** Approved by team consensus

#### Executive Summary
Add .NET Aspire orchestration for local dev, consolidate to `.slnx` only, and retire `.sln`. The NutriDash stack is lean (one Blazor Web app + Postgres dependency), making this a straightforward Aspire pilot. Docker Compose remains as production orchestration alongside Aspire for local dev.

#### Design Decisions
1. **Aspire Project Structure:** Minimal — create AppHost + ServiceDefaults projects
2. **Docker Compose Strategy:** Keep unchanged for production; Aspire is local dev only
3. **Solution File Consolidation:** Regenerate `.slnx` to include AppHost + ServiceDefaults; delete `.sln`
4. **Documentation Updates:** Add Aspire section to copilot-instructions.md and README.md

#### Team Consensus
- **Jaro (Infra/Container):** Owns Aspire projects, Docker test validation
- **Pirri (Backend/Solution):** Owns `.slnx` regeneration, `.sln` retirement
- **Vaquilla (Frontend/DevExp):** Owns documentation updates, IDE config
- **Torete (Lead):** Design review, gating, decision routing

---

### 11. Aspire AppHost Startup — Resource Dependency Bug (Torete's Decision)
**Date:** 2026-05-03T17:22:57.635+02:00  
**Author:** Torete (Lead)  
**Status:** Rejected — Critical bug found and fixed

#### Bug Found
The AppHost Program.cs has a resource dependency bug: chaining `.AddPostgres().AddDatabase()` into a single variable, then calling `.WaitFor()` on the database resource instead of the server resource creates an implicit circular dependency.

#### How Fixed
Separate server and database resources; wait for the server, not the database:
```csharp
var postgresServer = builder.AddPostgres("postgres", password: postgresPassword);
var postgresDatabase = postgresServer.AddDatabase("DefaultConnection", "nutridash");

builder.AddProject<Projects.NutriDash_Web>("web")
    .WithReference(postgresDatabase)
    .WaitFor(postgresServer)  // ← Wait for server, not database
    .WithExternalHttpEndpoints();
```

#### Affected File
- `src/NutriDash.AppHost/Program.cs`

---

### 12. .NET 10 + SLNX Alignment (Vaquilla's Decision)
**Date:** 2026-05-02T19:50:51.761+02:00  
**Author:** Vaquilla (Frontend/DevExp)  
**Status:** Approved

#### Decision
- Keep `NutriDash.slnx` as sole solution file to avoid `.sln`/`.slnx` ambiguity
- Point VS Code's `dotnet.defaultSolution` at `.slnx`
- Align Docker with existing net10.0 target

---

### 13. Aspire Integration for NutriDash.Web (Vaquilla's Work Summary)
**Date:** 2026-05-02T21:13:51.285+02:00  
**Author:** Vaquilla (Frontend/DevExp)  
**Status:** Completed

#### Changes
1. **NutriDash.Web.csproj:**
   - Added reference to `NutriDash.ServiceDefaults`
   - Replaced `Npgsql.EntityFrameworkCore.PostgreSQL` with `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`

2. **Program.cs:**
   - Added `builder.AddServiceDefaults()` — registers Aspire health checks, service discovery, and resilience policies
   - Replaced manual connection string retrieval with `AddNpgsqlDbContext<AppDbContext>("DefaultConnection")`
   - Added `app.MapDefaultEndpoints()` — exposes `/health` and `/health/ready` endpoints

#### Compatibility
✅ Aspire Orchestration, Docker Compose, Local Development, EF Migrations, Seeding all preserved

---

### 14. PostgreSQL Wiring and Seed Contract (Vaquilla's Decision)
**Date:** 2026-05-02T20:54:56.093+02:00  
**Author:** Vaquilla (Frontend/DevExp)  
**Status:** Pending team consensus

#### Decision
- Switched app-level EF Core provider contract from SQLite to PostgreSQL
- Kept startup schema initialization explicit with `EnsureCreated()` in `Program.cs`
- Reduced `SeedData` to a no-op so startup creates schema only and inserts no demo rows

#### Rationale
- Current enum types remain code enums (nothing relational to seed)
- Moving schema creation into startup keeps database bootstrap explicit
- Keeps the migration surgical and aligned with backend-only scope

---

### 15. Documentation & Seed Behavior Alignment (Pirri's Decision)
**Date:** 2026-05-03T17:22:57.635+02:00  
**Author:** Pirri (Backend/DevOps)  
**Status:** Implemented

#### Changes
- Tightened `README.md` and `.github/copilot-instructions.md` to match runtime reality
- Documented that Docker Compose runs the app with a dedicated PostgreSQL container
- Clarified that startup creates schema only: no demo/sample rows are seeded
- Clarified that enums like `WorkoutType` remain code-only, not database lookup tables

---

### 16. Postgres Compose Baseline (Pirri's Decision)
**Date:** 2026-05-02T20:54:56.093+02:00  
**Author:** Pirri (Backend/DevOps)  
**Status:** Implemented

#### Decision
Local and container runtime should converge on a dedicated PostgreSQL service in `docker-compose.yml` instead of a SQLite file mount. Container wiring, VS Code tasks, and operator-facing docs now assume Postgres connection strings and volume-backed database persistence.

#### Notes
- Compose owns PostgreSQL startup and health before the app container starts
- Dev credentials remain placeholders in committed files only
- Expected startup seed scope is enumerators/catalogs only, not demo business records

---

### 17. Aspire Support Validation Plan (Jaro's Planning)
**Date:** 2026-05-02T21:13:51.285+02:00  
**Author:** Jaro (Tester)  
**Status:** Plan ready for execution

#### Summary
Prepared a comprehensive validation strategy for Aspire migration with **38 discrete checks** across 7 validation domains: build, AppHost startup, service endpoints, database wiring, local debug workflow, container orchestration, and documentation.

#### Validation Plan Outline
- **Phase 1:** Baseline snapshot (7 checks) before implementation
- **Phase 2:** Post-migration validation (31 checks) grouped into 7 categories
  - Build & project structure (4 checks)
  - AppHost startup (4 checks)
  - Service endpoints (3 checks)
  - Database connectivity (5 checks)
  - Local debug workflow (5 checks)
  - Container orchestration (5 checks)
  - Documentation & tooling (5 checks)

#### High-Risk Areas Identified
1. **Stray .sln References** → Build Failure
2. **AppHost Port Assignment** → Debug Confusion
3. **Postgres Startup Race** → Data Loss Risk
4. **Connection String Misconfiguration** → Silent Failure
5. **Hot Reload Breaks** → Developer Friction
6. **Container Build Uses Old .sln Path** → CI/CD Breakage

---

### 18. Aspire Support Validation Summary (Jaro's Summary)
**Date:** 2026-05-02T21:13:51.285+02:00  
**Author:** Jaro (Tester)  
**Status:** Summary ready for team reference

#### Key Findings
- **Baseline:** 7 checks covering build, local dev, container, debugging, and file status
- **Critical dependencies to protect:**
  - AppHost must declare Postgres as a **blocking** (not async) resource
  - Port assignments must be **deterministic** or clearly documented
  - Connection string injection from AppHost → must populate `ConnectionStrings__DefaultConnection` env var
  - Hot reload in local dev → essential workflow, must not break

#### Sign-Off Criteria
A migration is **ready for merge** when:
1. ✅ All Phase 2 checks pass or are explicitly documented as deferred
2. ✅ No .sln references remain (grep output clean)
3. ✅ Regressions documented (if any edge cases found, logged with workarounds)
4. ✅ Team documentation updated (copilot-instructions.md, README, VS Code configs aligned)
5. ✅ Both local and container paths verified end-to-end

---

### 19. Aspire Local Startup Requirements (Jaro's Decision)
**Date:** 2026-05-03T17:22:57.635+02:00  
**Author:** Jaro (Tester)  
**Status:** Accepted & Implemented

#### Decision
Document that Aspire startup requires Docker Desktop or another local container runtime to be running before Aspire AppHost startup. Updated `README.md` and `.github/copilot-instructions.md` to require Docker Desktop or another local container runtime before Aspire startup.

#### Rationale
Aspire orchestration manages container lifecycle; without a running container runtime, the PostgreSQL container cannot be provisioned and the AppHost startup will fail. This is a prerequisite, not an optional dependency.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
