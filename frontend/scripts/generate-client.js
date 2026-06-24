#!/usr/bin/env node

/**
 * Kiota client generation script.
 * Generates TypeScript API client from the backend OpenAPI spec.
 */

const { generateClient } = require("@microsoft/kiota");
const path = require("path");

async function generateApiClient() {
  const openAPIPath = process.env.OPENAPI_SPEC_PATH || "http://localhost:5094/swagger/v1/openapi.json";
  const outputPath = path.join(__dirname, "../src/lib/generated/api");
  const workingDir = path.join(__dirname, "..");

  console.log(`Generating API client from: ${openAPIPath}`);
  console.log(`Output path: ${outputPath}`);

  try {
    const result = await generateClient({
      openAPIFilePath: openAPIPath,
      clientClassName: "ApiClient",
      clientNamespaceName: "IoTPlatformApi",
      language: "typescript",
      outputPath: outputPath,
      workingDirectory: workingDir,
      operation: 1, // ConsumerOperation.Generate = 1
      cleanOutput: true,
      structuredMimeTypes: ["application/json"],
      excludePatterns: ["\\$*", "#*"],
    });

    if (result) {
      console.log("✓ API client generated successfully");
      console.log(`Generated files in: ${outputPath}`);
      process.exit(0);
    } else {
      console.error("✗ Failed to generate API client");
      process.exit(1);
    }
  } catch (error) {
    console.error("✗ Error generating API client:");
    console.error(error.message);
    process.exit(1);
  }
}

generateApiClient();
