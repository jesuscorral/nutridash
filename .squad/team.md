# Squad Team

> nutridash

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Torete | Lead | `.squad/agents/torete/charter.md` | ✅ Active |
| Pera | Frontend Dev | `.squad/agents/pera/charter.md` | ✅ Active |
| Vaquilla | Backend Dev | `.squad/agents/vaquilla/charter.md` | ✅ Active |
| Jaro | Tester | `.squad/agents/jaro/charter.md` | ✅ Active |
| Pirri | DevOps / Infra | `.squad/agents/pirri/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Bug fixes with clear reproduction steps
- Test coverage additions and simple regressions
- Documentation and instruction file updates
- Small isolated refactors with clear acceptance criteria

**🟡 Needs review — route to @copilot but require squad review:**
- Medium features with clear specs
- Refactors across a single feature slice
- Container and compose tweaks that follow established patterns

**🔴 Not suitable — route to squad member instead:**
- Architecture changes across multiple slices
- Ambiguous product decisions
- Security-critical auth and deployment changes
- Performance work that needs profiling

## Project Context

- **Owner:** Jesus
- **Stack:** ASP.NET Core 10+ Blazor Server (interactive server rendering), EF Core 9 with SQLite, Google Gemini API, Docker
- **Description:** Aplicacion web de seguimiento nutricional y salud personal organizada por feature slices.
- **Project:** nutridash
- **Created:** 2026-04-29
