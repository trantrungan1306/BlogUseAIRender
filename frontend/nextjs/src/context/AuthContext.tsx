"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { getToken } from "@/lib/api";
import * as authService from "@/services/auth";
import type { Role, User } from "@/types";

interface AuthContextValue {
  user: User | null;
  loading: boolean;
  isAuthenticated: boolean;
  hasRole: (role: Role) => boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  loginWithGoogle: (idToken: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      setLoading(false);
      return;
    }
    authService
      .me()
      .then(setUser)
      .catch(() => authService.logout())
      .finally(() => setLoading(false));
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const res = await authService.login(email, password);
    setUser(res.user);
  }, []);

  const register = useCallback(async (email: string, password: string, displayName: string) => {
    const res = await authService.register(email, password, displayName);
    setUser(res.user);
  }, []);

  const loginWithGoogle = useCallback(async (idToken: string) => {
    const res = await authService.googleLogin(idToken);
    setUser(res.user);
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setUser(null);
  }, []);

  const hasRole = useCallback((role: Role) => user?.roles.includes(role) ?? false, [user]);

  const value = useMemo<AuthContextValue>(
    () => ({ user, loading, isAuthenticated: !!user, hasRole, login, register, loginWithGoogle, logout }),
    [user, loading, hasRole, login, register, loginWithGoogle, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
