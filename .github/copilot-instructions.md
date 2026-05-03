# Copilot Instructions for NutriDash

**Project:** Health & Nutrition Optimization System  
**Stack:** ASP.NET Core 10 + Blazor Server, EF Core 9, PostgreSQL, Google Gemini API, .NET Aspire  
**Last Updated:** 2026-05-03T17:22:57.635+02:00

---

## Build & Run

### Local Development with .NET Aspire
```bash
# Ensure Docker Desktop (or another compatible container runtime) is running first
# Aspire provisions PostgreSQL as an orchestrated container resource
#
# Run the Aspire distributed application host
dotnet run --project src/NutriDash.AppHost/NutriDash.AppHost.csproj

# Aspire prints the dashboard URL in the terminal
# Open the web endpoint from the dashboard resources view
```

### Local Development (dotnet CLI only, without Aspire)
```bash
# Build
dotnet build NutriDash.slnx
dotnet build NutriDash.slnx --configuration Release

# Start PostgreSQL only
docker compose up -d postgres

# Run the web app directly
dotnet run --project src/NutriDash.Web/NutriDash.Web.csproj
```

### Docker (Production Path)
```bash
# First time: build and start
docker compose up --build

# Subsequent runs
docker compose up

# Teardown (keeps PostgreSQL volume by default)
docker compose down

# Teardown + delete all data
docker compose down -v
```

**Important:** Docker runs on port **8080**; direct local dev runs on **50757 (HTTPS) / 50758 (HTTP)**; Aspire assigns dev-time endpoints dynamically and shows them in the dashboard.

---

## Aspire Architecture

- **AppHost Project** (`src/NutriDash.AppHost/`): Orchestrates services and resources (PostgreSQL, web project)
- **ServiceDefaults Project** (`src/NutriDash.ServiceDefaults/`): Shared configuration for HTTP resilience, health checks, and service discovery
- **Web Project** (`src/NutriDash.Web/`): Blazor Server application that uses ServiceDefaults extensions

The AppHost creates a `nutridash` PostgreSQL database and wires the web project to use it with the named connection `DefaultConnection`.

---

## Database

- **Engine:** PostgreSQL  
- **Aspire:** The AppHost injects the `DefaultConnection` value for the web project automatically from the orchestrated PostgreSQL resource.  
- **Docker Compose connection string:** `Host=postgres;Port=5432;Database=${POSTGRES_DB:-nutridash};Username=${POSTGRES_USER:-nutridash};Password=${POSTGRES_PASSWORD:-nutridash_dev_password}`  
- **Local dev (non-Aspire):** VS Code watch/debug use `Host=localhost;Port=5432;Database=nutridash;Username=nutridash;Password=nutridash_dev_password` unless overridden via environment variable `ConnectionStrings__DefaultConnection`
- **Docker storage:** PostgreSQL data lives in named volume `postgres_data`  
- **Initialization:** `SeedData.Initialize()` runs at app startup but currently seeds nothing. No demo/sample rows are inserted, and enums like `WorkoutType` stay in code instead of database lookup tables.
- **No migrations folder:** Schema changes apply via entity updates + `EnsureCreated()` at startup

**Connection string resolution order:**
1. Environment variable `ConnectionStrings__DefaultConnection` (if set)
2. appsettings.json / app defaults

---

## Gemini API Key

Resolution order (first non-empty wins):
1. Environment variable `GEMINI_API_KEY`
2. `AppSettings` table row with key `"GeminiApiKey"`
3. appsettings.json entry `Gemini:ApiKey`

In Docker, the key comes from `.env` → `docker-compose.yml` environment. Users can also update via the **Settings** page in the app.

---

## Architecture

**Core pattern:** Vertical feature slices. Each feature folder contains a `Service` (scoped, per-request business logic) and a `.razor` page component.

**Layers:**

| Layer | Responsibility | DI Lifetime |
|-------|---|---|
| **Features/** (Dashboard, DailyTracking, Analytics, MealPlan, ShoppingList, Supplements, Settings) | Feature-specific service + Razor page | Scoped per request/circuit |
| **Infrastructure/Services/** (GeminiService, HealthScoringService, SupplementRecommendationService, AdaptiveNutritionService) | Cross-cutting, reusable business logic | Singleton |
| **Infrastructure/Data/** (AppDbContext, entities, SeedData) | Database context + schema init hook | Context scoped per request |
| **Components/** (MainLayout, NavMenu, shared widgets) | Layout and reusable UI pieces | Stateless or scoped to page |

**DI Registration (Program.cs):**
- `builder.AddServiceDefaults()` registers Aspire service discovery, HTTP resilience, and health checks
- `builder.AddNpgsqlDbContext<AppDbContext>("DefaultConnection")` registers the database context with Aspire-aware configuration
- Infrastructure singletons are registered once and shared across all requests
- Feature services are scoped (created fresh per request/Blazor circuit)
- All services use constructor injection; no service locator pattern

**No separate API layer:** Blazor Server is interactive; pages call services directly. No repository abstraction; services work directly with `DbContext`.

---

## Key Conventions

### UI & Routing
- **Language:** Spanish copy (routes, labels, navigation)
- **Routing:** Registered in `Routes.razor`; follow the existing feature-slice convention
- **CSS:** Custom dark theme in `wwwroot/css/`; no external frameworks; CSS variables for consistency

### Startup / Schema Init
- `SeedData.Initialize(db)` is called once in `Program.cs` at app startup
- It is currently a no-op; do not assume any reference/demo rows are inserted
- `ctx.Database.EnsureCreated()` creates schema from entities (no EF Core migrations)
- Enums such as `WorkoutType` remain code-only unless the team explicitly introduces persisted lookup tables

### Services & Configuration
- Configuration-dependent services use `IConfiguration` via constructor injection (e.g., `GeminiService`, `HealthScoringService`). Not all services require it.
- Runtime-editable values (like Gemini API key) can live in the `AppSettings` table, which is checked on each use.
- Gemini API calls go through `GeminiService` (handles retries, key resolution, model selection)
- No configuration hardcoding; use `IConfiguration` and `AppSettings` table for runtime changes

### Database Constraints
- `DailyTracking.Date` has a unique index (one entry per day)
- `AppSetting.Key` has a unique index (one row per config key)
- MealPlan → Meal → Ingredient relationships have cascading deletes

---

## Testing

**Status:** No test project, no test framework package configured. Do not invent single-test commands. State that none currently exist if testing is needed.

---

## Linting & Formatting

**Status:** No lint/analyzer setup (no `.editorconfig`, no analyzer packages in `.csproj`). Do not add linters without team consensus. Maintain existing code style (standard C# conventions, feature-slice naming).

---

## Container Details

### Image & Build
- **Build base:** .NET SDK 10.0 (used in Dockerfile)
- **Runtime base:** ASP.NET 10.0 (used in Dockerfile)
- **Target framework:** `net10.0` (defined in Directory.Packages.props as `DefaultTargetFramework`)
- **Multi-stage:** SDK → publish to `/app/publish`, then copy to final runtime image
- **Runtime helper:** `curl` installed in runtime image for container health checks
- **Entry point:** `dotnet NutriDash.Web.dll`

### Container Runtime (Production)
- **Port:** 8080
- **Environment:** `ASPNETCORE_ENVIRONMENT=Production`
- **URLs:** `http://+:8080`
- **Health check:** `GET http://localhost:8080/` (30s interval, 3 retries)

### Environment Variables (docker compose)
| Variable | Value | Notes |
|----------|-------|-------|
| `GEMINI_API_KEY` | From `.env` | Required for AI features |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Fixed in compose |
| `POSTGRES_DB` | From `.env` / placeholder | PostgreSQL database name |
| `POSTGRES_USER` | From `.env` / placeholder | PostgreSQL username |
| `POSTGRES_PASSWORD` | From `.env` / placeholder | PostgreSQL password |
| `ConnectionStrings__DefaultConnection` | Derived Postgres connection string | Points app container at `postgres:5432` |

---

## Local Dev vs. Docker vs. Aspire

| Aspect | Local Dev (dotnet) | Docker | Aspire |
|--------|---|---|---|
| **Port** | 50757 (HTTPS) / 50758 (HTTP) | 8080 | Dynamic, shown in dashboard |
| **Environment** | Development | Production | Development |
| **Database** | PostgreSQL on `localhost:5432` | PostgreSQL on `postgres:5432` (`postgres_data` volume) | PostgreSQL in Aspire orchestration |
| **Use case** | Active development | Staging, production, exact config | Development with orchestration insights |

---

## Quick Docker Commands

```bash
# Build and start
docker compose up --build

# Start (after built)
docker compose up

# Stop (keep data)
docker compose down

# Stop and remove volume
docker compose down -v

# View logs
docker compose logs -f nutridash

# Start only PostgreSQL for local dotnet watch/debug
docker compose up -d postgres
```

---

## For Copilot Sessions

1. **New features:** Add folder under `Features/`, include `{FeatureName}Service.cs` (scoped) and `{FeatureName}.razor` page. Register service in `Program.cs`.
2. **Database changes:** Update entity definitions in `Infrastructure/Data/Entities/`. No migrations folder; changes apply on next startup via `EnsureCreated()`. Do not assume seed rows or enum lookup tables already exist.
3. **Gemini integration:** Use `GeminiService.GenerateAsync()`; it handles retries and key resolution. Do not make direct HTTP calls.
4. **Configuration:** Configuration-dependent services (e.g., `GeminiService`) use `IConfiguration` in constructor. For user-editable settings, store in `AppSettings` table.
5. **Cross-feature work:** Consult the team (`.squad/`) before changing DI lifetimes, startup order, or seeding logic.
6. **Container runtime:** Keep the Dockerfile aligned with the `net10.0` target so local and container builds stay on the same runtime.
7. **Aspire development:** Use `dotnet run --project src/NutriDash.AppHost/NutriDash.AppHost.csproj` for orchestrated local development with dashboard insights.
