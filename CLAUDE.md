# CLAUDE.md — IoTPlatform

This file is the architecture & contribution guide for the IoTPlatform project. It documents the
rules, conventions, and structure every contributor (human or AI) must follow.

---

## 1. Project Overview

IoTPlatform is a **multi-tenant IoT platform** that ingests data from devices (primarily via MQTT),
exposes a REST API, stores telemetry and management data in PostgreSQL, and provides an integrated
**AI assistant** for chatting, analysis, and tool-driven actions. It also exposes an **MCP server**
so external LLMs can integrate with the platform.

- **Backend:** C# / ASP.NET Core (.NET), EF Core, MQTTnet, Microsoft.Extensions.AI
- **Database:** PostgreSQL (via EF Core — provider-swappable)
- **Frontend:** Next.js (separate folder in this monorepo — see §11)
- **Ingestion:** MQTT (MQTTnet), REST API, extensible to other sources

---

## 2. Tech Stack & Key Libraries

| Concern            | Choice                                        |
|--------------------|-----------------------------------------------|
| Runtime            | .NET (LTS)                                     |
| Web                | ASP.NET Core (Minimal APIs + Controllers)     |
| ORM                | EF Core + Npgsql (PostgreSQL)                  |
| MQTT               | MQTTnet                                        |
| AI                 | Microsoft.Extensions.AI (provider-agnostic)   |
| MCP                | ModelContextProtocol (C# SDK)                  |
| Auth               | JWT Bearer + ASP.NET Core Identity (custom)   |
| Logging            | Serilog (structured) → console + DB sink       |
| Validation         | FluentValidation                              |
| Mapping            | Mapperly (source-gen) or manual                |
| Testing            | xUnit + FluentAssertions + Testcontainers     |
| Containerization   | Docker + docker-compose                       |

---

## 3. Solution Structure

```
IoTPlatform/
├── src/
│   ├── IoTPlatform.API/            # ASP.NET Core host: controllers, DI, middleware, MQTT hosted service, MCP endpoint
│   ├── IoTPlatform.Models/         # Domain entities, DTOs, enums (no external deps)
│   ├── IoTPlatform.Services/       # Business logic, interfaces, AI assistant orchestration, MCP tools
│   ├── IoTPlatform.Infrastructure/ # EF Core DbContext, migrations, repositories, MQTT client, AI provider wiring
│   ├── IoTPlatform.Common/         # Cross-cutting: constants, extensions, result types, exceptions
│   └── IoTPlatform.Tests/          # Unit + integration tests
├── frontend/                       # Next.js app
├── docker/                         # Dockerfiles, mosquitto config, init scripts
├── docker-compose.yml              # postgres + mosquitto (+ api for full-stack)
├── .github/workflows/              # CI (build/test) + deploy
├── CLAUDE.md
├── README.md
└── IoTPlatform.sln
```

### Dependency direction (Clean / layered architecture)

```
API ──▶ Services ──▶ Infrastructure ──▶ Models
                └────────────────────────▶ Models
Common is referenced by all. Models has NO project dependencies.
```

- **API** never talks to EF Core directly — only through **Services**.
- **Services** depend on **interfaces** (e.g. `IDeviceRepository`, `IChatClient`) implemented in **Infrastructure**.
- **Models** is dependency-free so it can be shared safely.

---

## 4. Coding Conventions

- **Language:** C# latest, `nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` in CI.
- **Naming:**
  - PascalCase for types, methods, properties, public members.
  - camelCase for locals & parameters; `_camelCase` for private fields.
  - Interfaces prefixed `I` (`IDeviceService`).
  - Async methods suffixed `Async`.
- **Namespaces:** file-scoped, match folder structure under `IoTPlatform.<Project>`.
- **One public type per file**, filename = type name.
- **Folders by feature** inside each project where it scales (e.g. `Services/Devices/`, `Services/Ai/`).
- **DTOs** for all API input/output — never expose EF entities over the wire.
- **Result pattern** (`Result<T>` in Common) for service returns; map to HTTP at the controller boundary.
- **No business logic in controllers** — thin controllers, fat services.
- **CancellationToken** flows through all async I/O.

---

## 5. Database Conventions

- **Provider:** PostgreSQL via Npgsql. EF Core keeps it swappable — provider-specific SQL is avoided.
- **Naming:** snake_case tables & columns (configured globally via naming convention); plural table names.
- **Primary keys:** `Guid` (`uuid`), generated app-side (sequential GUIDs) for index friendliness.
- **Migrations:** EF Core Code-First. Migrations live in `IoTPlatform.Infrastructure/Migrations`.
- **Every entity inherits `AuditableEntity`** (see §6) → audit fields + soft delete.
- **Soft delete:** `is_deleted` flag + global query filter; rows are never hard-deleted by default.
- **Multi-tenancy:** every tenant-scoped entity has `company_id`. A **global query filter** auto-filters by
  the current tenant (resolved from `ITenantContext`). SuperAdmin can bypass via an explicit "ignore filter" path.
- **Time-series telemetry:** stored in `telemetry_data`, indexed on `(device_id, sensor_id, timestamp)`.
  Designed so it can later move to TimescaleDB / partitioning without schema upheaval.

---

## 6. Cross-Cutting Entity Base Classes

```csharp
public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

public abstract class TenantEntity : AuditableEntity
{
    public Guid CompanyId { get; set; }
}
```

Audit fields are populated automatically in `SaveChangesAsync` via an EF Core interceptor that reads the
current user from `ICurrentUser` and the tenant from `ITenantContext`.

---

## 7. Data Model (Schema)

### Multi-tenancy core
- **companies** (id, name, code, description, is_active, +audit)
- **departments** (id, company_id, name, description, is_active, +audit)
- **user_groups** (id, company_id, department_id, name, description, is_active, +audit)
- **users** (id, company_id, email, first_name, last_name, password_hash, is_super_admin, is_active, +audit)
- **user_group_members** (id, user_id, user_group_id) — many-to-many; a user can be in multiple groups

### Access control (permissions granted at the **user group** level)
- **modules** (id, code, name, description, is_active) — Dashboard, Settings, Reports, Devices, etc.
- **permissions** (id, module_id, code, name, action) — action ∈ {View, Create, Edit, Delete, Control, Export}
- **user_group_permissions** (id, user_group_id, permission_id)

> A user's effective permissions = union of permissions across all their user groups.
> Device visibility is scoped by **department** (a user sees devices in departments they belong to),
> on top of the company-level tenant filter.

### IoT core
- **device_types** (id, code, name, description, is_active, +audit)
- **devices** (id, company_id, department_id, device_type_id, device_key/identifier, name, status, last_seen_at, +audit)
- **sensors** (id, device_id, sensor_type, name, unit, status, +audit)
- **telemetry_data** (id, device_id, sensor_id, value, raw_payload, timestamp, metadata jsonb)
- **device_commands** (id, device_id, command, payload, status, issued_by, issued_at, +audit) — commands sent back to devices

### AI & MCP
- **ai_provider_configs** (id, company_id, provider, model, api_key_encrypted, endpoint, is_active, +audit)
- **ai_conversations** (id, company_id, user_id, title, created_at, +audit)
- **ai_messages** (id, conversation_id, role, content, tool_calls jsonb, token_usage, model, +audit)
- **mcp_clients** (id, company_id, name, api_key_hash, scopes, is_active, +audit) — external LLMs authorized to call the MCP server

### Audit & logging
- **audit_logs** (id, company_id, user_id, entity_type, entity_id, action, old_value jsonb, new_value jsonb, timestamp, ip_address)
- **system_logs** (id, level, message, exception, source, properties jsonb, company_id?, user_id?, timestamp)

> **All AI/LLM activity is logged** to both `ai_messages` (full conversation) and `audit_logs`
> (who invoked what, which model, token usage, tool calls). MCP calls from external LLMs are
> likewise recorded in `audit_logs` + `system_logs`.

---

## 8. Security & Authorization

- **AuthN:** JWT Bearer. Access + refresh tokens. Passwords hashed with ASP.NET Core Identity hasher (PBKDF2).
- **AuthZ:** permission-based via a policy provider. `[HasPermission("Device.Control")]` maps to a policy that
  checks the user's effective group permissions.
- **Tenant isolation:** enforced at the data layer (global query filters), not just the UI. The frontend
  **global filter** (company/department selector) only narrows what an already-authorized user sees;
  it never widens access. SuperAdmin / cross-company managers see multiple tenants and can switch context.
- **Secrets:** API keys (AI providers) encrypted at rest; never logged in plaintext.

---

## 9. AI Assistant (Microsoft.Extensions.AI)

- Uses the **provider-agnostic `IChatClient` abstraction** so any provider/key (OpenAI, Azure OpenAI,
  Anthropic, Ollama, etc.) can be plugged via `ai_provider_configs`.
- **Skills = tools** exposed to the model via `AIFunction` (function calling): e.g. `GetDeviceStatus`,
  `QueryTelemetry`, `ListAlerts`, `SummarizeReport`. Tools live in `Services/Ai/Tools/`.
- Conversations + every message (incl. tool calls + token usage) persisted and audited (§7).

## 10. MCP Server

- The platform hosts an **MCP server** (`ModelContextProtocol` C# SDK) so external LLMs can connect and
  use platform tools/resources under scoped, audited access (`mcp_clients`).
- MCP tools reuse the same underlying services as the AI assistant skills — single source of truth.

---

## 11. Frontend (Next.js)

- Lives in `frontend/`. Talks to the API over REST (+ optional WebSocket/SignalR for live telemetry).
- Houses the **global tenant filter** (company/department context switcher).
- Auth via JWT stored per app convention (httpOnly cookie preferred).

> **Monorepo decision:** frontend + backend in one repo for atomic full-stack changes, shared CI, and
> simpler coordination. Can be split later if needed.

---

## 12. Local Development

```bash
docker compose up -d            # postgres + mosquitto
dotnet ef database update -p src/IoTPlatform.Infrastructure -s src/IoTPlatform.API
dotnet run --project src/IoTPlatform.API
cd frontend && npm install && npm run dev
```

- **docker-compose** provides: PostgreSQL 16, Mosquitto (MQTT broker). API can also run in-compose.
- Config via `appsettings.json` + `appsettings.Development.json` + env vars (12-factor). No secrets committed.

---

## 13. Testing

- **Unit tests** for services (mock repositories/IChatClient).
- **Integration tests** with **Testcontainers** (real PostgreSQL + Mosquitto) for repositories, MQTT ingest, API.
- Run: `dotnet test`. CI runs the full suite on every PR.

---

## 14. CI/CD (GitHub Actions)

- **ci.yml:** restore → build (warnings as errors) → test on every PR/push.
- **deploy.yml:** build & publish Docker images on push to `main` (target environment TBD).
- Frontend lint/build included.

---

## 15. Git & Contribution Rules

- Conventional-commit style messages (`feat:`, `fix:`, `chore:`, `docs:` …).
- Feature branches; no direct pushes to `main`.
- Keep changes scoped; update CLAUDE.md when conventions change.
- Do not commit secrets, `.env`, API keys, or build artifacts (see `.gitignore`).

---

## 16. Open Decisions / TODO

- Target .NET version pin.
- Deployment target for `deploy.yml`.
- Whether telemetry moves to TimescaleDB at scale.
- Refresh-token storage strategy.
