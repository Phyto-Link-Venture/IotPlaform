# IoTPlatform

A multi-tenant IoT platform: ingests device data via **MQTT** and REST, stores telemetry and
management data in **PostgreSQL**, exposes a **REST API**, an integrated **AI assistant**
(Microsoft.Extensions.AI), and an **MCP server** for external LLM integration.

> Architecture, conventions, and the full data model live in **[CLAUDE.md](./CLAUDE.md)** —
> read it before contributing.

## Stack

- **Backend:** C# / ASP.NET Core (.NET 10 LTS), EF Core + Npgsql, MQTTnet, Microsoft.Extensions.AI, ModelContextProtocol
- **Database:** PostgreSQL (provider-swappable via EF Core)
- **Frontend:** Next.js (in `frontend/`)
- **Auth:** JWT Bearer + permission-based authorization (granted at the user-group level)

## Solution layout

```
src/
  IoTPlatform.API/            ASP.NET Core host: controllers, auth, MQTT hosted service, MCP endpoint
  IoTPlatform.Models/         Entities, enums (no external deps)
  IoTPlatform.Services/       Business logic, AI assistant, AI tools
  IoTPlatform.Infrastructure/ EF Core, migrations, MQTT client, security
  IoTPlatform.Common/         Result type, abstractions, constants
  IoTPlatform.Tests/          xUnit tests
frontend/                     Next.js app
docker/                       Dockerfiles, mosquitto config
```

## Local development

```bash
# 1. Start infrastructure (PostgreSQL + Mosquitto MQTT broker)
docker compose up -d

# 2. Apply migrations
dotnet tool restore
dotnet dotnet-ef database update -p src/IoTPlatform.Infrastructure -s src/IoTPlatform.API

# 3. Run the API (Swagger at /swagger)
dotnet run --project src/IoTPlatform.API

# 4. Run the frontend
cd frontend && npm install && npm run dev   # http://localhost:3000
```

Copy `.env.example` → `.env` and set real values (JWT signing key, DB password, etc.).
**Never commit secrets.**

## Key concepts

- **Multi-tenancy:** every tenant-scoped entity carries `company_id`; a global query filter
  isolates data per company. SuperAdmin can view across tenants.
- **Permissions:** assigned to **user groups**; a user's effective permissions are the union
  across their groups. Endpoints use `[HasPermission("Device.Control")]`.
- **Global filter:** the frontend company/department switcher sends `X-Company-Id` /
  `X-Department-Id` headers — it only narrows visibility, never widens access.
- **AI & MCP:** the assistant and the MCP server share the same tool implementations; all LLM
  activity is persisted (`ai_messages`) and audited (`audit_logs`).

## Testing

```bash
dotnet test
```

## CI/CD

- `.github/workflows/ci.yml` — backend build (warnings-as-errors) + tests, frontend lint + build.
- Deployment to the self-hosted VPS is configured per environment (staging / production).
