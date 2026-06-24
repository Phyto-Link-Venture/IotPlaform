/**
 * Kiota API Client Wrapper
 *
 * This module instantiates the auto-generated Kiota client and wires up JWT
 * authentication from httpOnly cookies. The JWT is automatically injected into
 * all outbound requests via the browser's automatic cookie handling.
 */

import { createApiClient, type ApiClient } from '../generated/api/apiClient';
import type { RequestAdapter } from '@microsoft/kiota-abstractions';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';

// Default base URL for local development; override via environment variables
const DEFAULT_BASE_URL = process.env.NEXT_PUBLIC_API_URL || process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

/**
 * Initialize the API client with authentication.
 * Uses the generated Kiota client with a custom request adapter.
 *
 * Note: httpOnly cookies are automatically sent by the browser with fetch()
 * when credentials: 'include' is set. The JWT token is handled transparently
 * by the browser and doesn't need to be manually injected into headers.
 */
export function createKiotaApiClient(): ApiClient {
  // Use anonymous auth provider since JWT is handled via httpOnly cookies
  const authProvider = new AnonymousAuthenticationProvider();

  // Create a fetch-based request adapter with the anonymous auth provider
  const requestAdapter = new FetchRequestAdapter(authProvider);

  // Set the base URL from environment or default
  requestAdapter.baseUrl = DEFAULT_BASE_URL;

  // Instantiate the Kiota client with the configured adapter
  const client = createApiClient(requestAdapter);

  return client;
}

// Export a singleton instance for use throughout the app
let _apiClient: ApiClient | null = null;

export function getApiClient(): ApiClient {
  if (!_apiClient) {
    _apiClient = createKiotaApiClient();
  }
  return _apiClient;
}

/**
 * Alias for backward compatibility and convenience
 */
export const apiClient = new Proxy(
  {},
  {
    get: (target, prop) => {
      return (getApiClient() as any)[prop];
    },
  }
) as ApiClient;
