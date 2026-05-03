# NutriDash 🥗

**Health & Nutrition Optimization System** — .NET 10 Blazor · PostgreSQL · Gemini AI · .NET Aspire · Docker

---

## 🚀 Inicio rápido

### Con Docker (Recomendado)

```bash
# 1. Copia el archivo de entorno
cp .env.example .env
# Edita .env con tu Gemini API key

# 2. Construir y lanzar
docker compose up --build

# 3. Abrir en el navegador
http://localhost:8080
```

### Con .NET Aspire (Desarrollo Local)

```bash
# 0. Asegúrate de tener Docker Desktop (o un runtime compatible de contenedores) en ejecución
#    Aspire crea PostgreSQL como recurso orquestado
#
# 1. Ejecuta el Aspire AppHost
dotnet run --project src/NutriDash.AppHost/NutriDash.AppHost.csproj

# 2. Usa la URL del dashboard que Aspire imprime en la terminal
# 3. Abre la URL HTTP/HTTPS del recurso web desde el dashboard
```

> `docker compose` levanta la app y un contenedor dedicado de PostgreSQL. Aspire levanta una base PostgreSQL propia para desarrollo orquestado y le inyecta la conexión a la app. En ambos caminos, la base se crea al arrancar, pero no se cargan filas demo/sample. Enumeraciones como `WorkoutType` siguen siendo código, no tablas lookup.

---

## 🏗️ Arquitectura

```
src/NutriDash.Web/
├── Features/           # Vertical slices (cada feature = service + page)
│   ├── Dashboard/
│   ├── DailyTracking/
│   ├── Analytics/
│   ├── MealPlan/
│   ├── ShoppingList/
│   ├── Supplements/
│   └── Settings/
├── Infrastructure/
│   ├── Data/           # EF Core, entidades y creación de esquema
│   └── Services/       # Gemini, HealthScoring, Supplements, AdaptiveNutrition
├── Components/
│   ├── Layout/         # MainLayout, NavMenu
│   └── Shared/         # MetricCard, HealthScoreBadge, SparklineChart, MacroBar
└── wwwroot/css/        # Design system completo (dark theme)
```

## 📊 Módulos

| Módulo | Descripción |
|--------|-------------|
| **Dashboard** | Métricas en tiempo real, Health Score 0–100, gráficas SVG |
| **Tracking** | Formulario diario + historial 21 días |
| **Analítica** | Estadísticas semanales, tendencias, evolución |
| **Plan de Comidas** | 7 días con macros, generado con Gemini IA |
| **Lista Compra** | Consolidación por categorías, checkboxes |
| **Suplementos** | Recomendaciones adaptativas según métricas |
| **Configuración** | Perfil personal + Gemini API key |

## 🤖 Gemini AI

La clave API se configura en `.env`. Para cambiarla:
- Desde la app: **Configuración → Gemini API Key**
- O edita el fichero `.env` y reinicia Docker

## 🐳 Docker

```yaml
# docker-compose.yml ya incluido
docker compose up --build    # primera vez
docker compose up            # reinicios
docker compose down -v       # borrar datos (volumen PostgreSQL)
```

Para desarrollo local con `dotnet watch` o depuración desde VS Code, arranca antes solo la base:

```bash
docker compose up -d postgres
```

## ⚙️ Stack

- **.NET 10** + Blazor Web App (Server Interactive)
- **EF Core 9** + PostgreSQL en contenedor Docker persistente
- **Google Gemini 1.5 Flash** API
- **CSS Dark Theme** con Inter font, sin frameworks CSS externos
