// Thin API client for the IoTPlatform backend.
//
// The "global filter" (company / department context switcher) is sent as request
// headers — it only narrows what an already-authorized user sees and never widens
// access (the backend enforces tenant isolation regardless).

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080";

export interface TenantFilter {
  companyId?: string;
  departmentId?: string;
}

export interface ApiOptions extends RequestInit {
  token?: string;
  filter?: TenantFilter;
}

export async function apiFetch<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const { token, filter, headers, ...rest } = options;

  const finalHeaders: Record<string, string> = {
    "Content-Type": "application/json",
    ...(headers as Record<string, string>),
  };

  if (token) finalHeaders["Authorization"] = `Bearer ${token}`;
  if (filter?.companyId) finalHeaders["X-Company-Id"] = filter.companyId;
  if (filter?.departmentId) finalHeaders["X-Department-Id"] = filter.departmentId;

  const res = await fetch(`${API_BASE_URL}${path}`, { ...rest, headers: finalHeaders });

  if (!res.ok) {
    const body = await res.text();
    throw new Error(`API ${res.status}: ${body || res.statusText}`);
  }

  return (res.status === 204 ? undefined : await res.json()) as T;
}

// --- Typed endpoints ---

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
}

export function login(email: string, password: string) {
  return apiFetch<LoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
}

export interface DeviceDto {
  id: string;
  deviceKey: string;
  name: string;
  deviceType: string;
  status: number;
  lastSeenAt: string | null;
  departmentId: string | null;
}

export function listDevices(token: string, filter?: TenantFilter) {
  return apiFetch<DeviceDto[]>("/api/devices", { token, filter });
}

export interface ChatResponseDto {
  conversationId: string;
  content: string;
  promptTokens: number | null;
  completionTokens: number | null;
  model: string | null;
}

export function chat(
  token: string,
  message: string,
  conversationId?: string,
  filter?: TenantFilter,
) {
  return apiFetch<ChatResponseDto>("/api/ai/chat", {
    method: "POST",
    token,
    filter,
    body: JSON.stringify({ conversationId, message }),
  });
}
