# IoTPlatform — Entity-Relationship Diagram

This diagram is the source-of-truth visualization of the database schema described in
[`CLAUDE.md` §7](../CLAUDE.md). It renders automatically on GitHub.

## Key conventions

- **Every entity** carries audit fields (`created_at/by`, `updated_at/by`, `is_deleted`,
  `deleted_at/by`) via `AuditableEntity`; tenant-scoped entities add `company_id` via `TenantEntity`.
- **Permissions bind to controller action methods** — `code = "{Controller}.{actionMethod}"`
  (e.g. `Users.createUser`). There is no `PermissionAction` enum.
- **Telemetry posts directly at the device level.** The sensor layer is hidden — there is no
  `sensors` table. Each reading is identified by `dataname_symbol` (e.g. `temperature_C`),
  validated on ingestion.

```mermaid
erDiagram
    COMPANIES ||--o{ DEPARTMENTS : "1:many"
    COMPANIES ||--o{ USER_GROUPS : "1:many"
    COMPANIES ||--o{ USERS : "1:many"
    COMPANIES ||--o{ DEVICES : "1:many"
    COMPANIES ||--o{ AI_PROVIDER_CONFIGS : "1:many"
    COMPANIES ||--o{ AI_CONVERSATIONS : "1:many"
    COMPANIES ||--o{ MCP_CLIENTS : "1:many"
    COMPANIES ||--o{ AUDIT_LOGS : "1:many"
    COMPANIES ||--o{ SYSTEM_LOGS : "1:many"

    DEPARTMENTS ||--o{ USERS : "1:many"
    DEPARTMENTS ||--o{ DEVICES : "1:many"
    DEPARTMENTS ||--o{ USER_GROUPS : "1:many"

    USER_GROUPS ||--o{ USER_GROUP_MEMBERS : "1:many"
    USER_GROUPS ||--o{ USER_GROUP_PERMISSIONS : "1:many"

    USERS ||--o{ USER_GROUP_MEMBERS : "1:many"
    USERS ||--o{ AI_CONVERSATIONS : "1:many"
    USERS ||--o{ AI_MESSAGES : "1:many"
    USERS ||--o{ AUDIT_LOGS : "1:many"
    USERS ||--o{ DEVICE_COMMANDS : "1:many"

    MODULES ||--o{ PERMISSIONS : "1:many"
    PERMISSIONS ||--o{ USER_GROUP_PERMISSIONS : "1:many"

    DEVICE_TYPES ||--o{ DEVICES : "1:many"

    DEVICES ||--o{ TELEMETRY_DATA : "1:many"
    DEVICES ||--o{ DEVICE_COMMANDS : "1:many"

    AI_CONVERSATIONS ||--o{ AI_MESSAGES : "1:many"

    COMPANY {
        uuid id PK
        string name
        string code
        text description
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    DEPARTMENTS {
        uuid id PK
        uuid company_id FK
        string name
        text description
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    USERS {
        uuid id PK
        uuid company_id FK
        uuid department_id FK
        string email UK
        string first_name
        string last_name
        string password_hash
        boolean is_super_admin
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    USER_GROUPS {
        uuid id PK
        uuid company_id FK
        uuid department_id FK
        string name
        text description
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    USER_GROUP_MEMBERS {
        uuid id PK
        uuid user_id FK
        uuid user_group_id FK
    }

    MODULES {
        uuid id PK
        string code UK
        string name
        text description
        boolean is_active
    }

    PERMISSIONS {
        uuid id PK
        uuid module_id FK
        string code UK "Controller.actionMethod"
        string name
        string action_method "controller method binding"
        timestamp created_at
        uuid created_by
    }

    USER_GROUP_PERMISSIONS {
        uuid id PK
        uuid user_group_id FK
        uuid permission_id FK
    }

    DEVICE_TYPES {
        uuid id PK
        string code UK
        string name
        text description
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    DEVICES {
        uuid id PK
        uuid company_id FK
        uuid department_id FK
        uuid device_type_id FK
        string device_key UK
        string name
        string status
        timestamp last_seen_at
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    TELEMETRY_DATA {
        bigint id PK
        uuid device_id FK
        string dataname_symbol "^[a-z][a-z0-9]*_[A-Za-z0-9%]+$"
        double value
        text raw_payload
        timestamp timestamp
        jsonb metadata
    }

    DEVICE_COMMANDS {
        uuid id PK
        uuid device_id FK
        string command
        jsonb payload
        string status
        uuid issued_by FK
        timestamp issued_at
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    AI_PROVIDER_CONFIGS {
        uuid id PK
        uuid company_id FK
        string provider "OpenAI, Anthropic, etc"
        string model
        string api_key_encrypted "Data Protection API"
        string endpoint
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    AI_CONVERSATIONS {
        uuid id PK
        uuid company_id FK
        uuid user_id FK
        string title
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    AI_MESSAGES {
        uuid id PK
        uuid conversation_id FK
        string role "user, assistant, system"
        text content
        jsonb tool_calls "null if no tools"
        integer token_usage
        string model "model used"
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
    }

    MCP_CLIENTS {
        uuid id PK
        uuid company_id FK
        string name
        string api_key_hash "never plaintext"
        jsonb scopes "JSON array"
        boolean is_active
        timestamp created_at
        uuid created_by
        timestamp updated_at
        uuid updated_by
        boolean is_deleted
        timestamp deleted_at
        uuid deleted_by
    }

    AUDIT_LOGS {
        uuid id PK
        uuid company_id FK
        uuid user_id FK
        string entity_type
        uuid entity_id
        string action
        jsonb old_value
        jsonb new_value
        timestamp timestamp
        string ip_address
    }

    SYSTEM_LOGS {
        uuid id PK
        string level "Debug, Info, Warning, Error"
        text message
        text exception
        string source
        jsonb properties
        uuid company_id FK
        uuid user_id FK
        timestamp timestamp
    }
```
