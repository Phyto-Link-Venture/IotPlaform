/**
 * API Client Module
 *
 * Main export file that re-exports the Kiota client wrapper and provides
 * authentication utilities.
 */

export { getApiClient, createKiotaApiClient, apiClient } from './client';
export * from '../generated/api/apiClient';
export * from '../generated/api/api/index';

/**
 * Utility to check if the user is authenticated.
 * Since we use httpOnly cookies for JWT, the browser will automatically
 * send the token with requests, but we can't read it from JavaScript.
 * This function provides a simple heuristic check.
 */
export function isAuthenticated(): boolean {
  // In SSR context, we can't check cookies from JS
  if (typeof document === 'undefined') {
    return false;
  }

  // Check if a JWT cookie exists by looking at the document.cookie string
  // Common cookie names: 'jwt', 'token', 'access_token'
  const cookieStr = document.cookie || '';
  return (
    cookieStr.includes('jwt=') ||
    cookieStr.includes('token=') ||
    cookieStr.includes('access_token=')
  );
}

/**
 * Utility to clear authentication (logout).
 * In a real implementation, this would also call a backend logout endpoint.
 */
export function clearAuth(): void {
  // The server will clear the httpOnly cookie via Set-Cookie header
  // when the logout endpoint is called. We don't need to do anything on the client.
  // This function is a placeholder for future use.
}
