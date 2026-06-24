This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.

## API Client Generation

The TypeScript API client is auto-generated from the backend's OpenAPI spec using **[Microsoft Kiota](https://microsoft.github.io/kiota/)**.

### Why Kiota?

- Auto-sync with backend changes (no manual DTO → TypeScript mapping)
- Type-safe: all API methods and response types are generated
- Single command: `npm run gen` after backend API changes
- Works with any OpenAPI-compliant backend
- Generated code is committed to git, ensuring all devs + CI have consistent types

### Generating the Client

**In local development:**

Ensure the backend API is running on the expected port (default: `http://localhost:5094`), then:

```bash
npm run gen
```

This regenerates `src/lib/generated/api/` with types and methods matching the current backend.

**In CI/deployment (staging/production):**

```bash
OPENAPI_SPEC_PATH="https://api-staging.example.com/swagger/v1/openapi.json" npm run gen
```

Then proceed with the normal build step (`npm run build`).

### Configuration

- **Kiota config:** `kiota.json` — defines output path, language, namespace, and generation options
- **Generation script:** `scripts/generate-client.js` — Node.js wrapper around Kiota CLI that reads env vars
- **Generated output:** `src/lib/generated/api/` — contains the client, models, and services
- **Default OpenAPI URL:** `http://localhost:5094/swagger/v1/openapi.json`

Override the OpenAPI spec URL by setting the `OPENAPI_SPEC_PATH` environment variable before running `npm run gen`.

### Using the Generated Client

Import the client and use it in your components:

```typescript
// Import the generated client
import { ApiClient } from '@/lib/generated/api';

// Create an instance (if not using a singleton wrapper)
const client = new ApiClient();

// All methods are fully typed — TypeScript IntelliSense will guide you:
const devices = await client.api.devices.get();
const user = await client.api.auth.login.post({ email, password });
const device = await client.api.devices['{id}'].get();
```

For a production app, wrap the client in a utility:

```typescript
// src/lib/api/client.ts
import { ApiClient } from '@/lib/generated/api';

export const apiClient = new ApiClient();
```

Then import and use globally:

```typescript
import { apiClient } from '@/lib/api';

const devices = await apiClient.api.devices.get();
```

### Important Notes

- **Do NOT edit generated code** — all changes will be overwritten on the next `npm run gen`
  - If you need to customize behavior, wrap the generated client in your own utilities
- **Commit generated code to git** — it's not a build artifact, it's a versioned dependency
  - Every dev + CI has consistent types immediately
  - git diff shows exactly what backend API changed (useful for code review)
- **After backend API changes:** Run `npm run gen` locally before committing
- **In CI/CD:** Generate the client **before** building the frontend (see the Deployment Guide for details)

### Troubleshooting

**"Module not found" errors or outdated types:**
- Run `npm run gen` to regenerate after backend changes
- Verify the OpenAPI spec URL is correct (check `kiota.json` or `OPENAPI_SPEC_PATH` env var)
- Ensure the backend API is running and responding at the spec URL

**"OpenAPI spec URL is unreachable":**
- For local dev: start the backend (`dotnet run --project src/IoTPlatform.API`)
- For CI: verify the URL in GitHub Actions secrets or env vars
- Check backend logs for `/swagger/v1/openapi.json` endpoint issues
