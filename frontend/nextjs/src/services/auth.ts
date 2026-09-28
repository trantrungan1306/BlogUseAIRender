import { apiFetch, setToken } from "@/lib/api";
import type { AuthResponse, User } from "@/types";

export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await apiFetch<AuthResponse>("/auth/login", { method: "POST", body: { email, password } });
  setToken(res.token);
  return res;
}

export async function register(email: string, password: string, displayName: string): Promise<AuthResponse> {
  const res = await apiFetch<AuthResponse>("/auth/register", {
    method: "POST",
    body: { email, password, displayName }
  });
  setToken(res.token);
  return res;
}

export async function me(): Promise<User> {
  return apiFetch<User>("/auth/me", { auth: true });
}

export function logout() {
  setToken(null);
}
