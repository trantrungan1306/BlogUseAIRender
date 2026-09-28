"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/context/AuthContext";
import type { Role } from "@/types";

export function RequireAuth({ role, children }: { role?: Role; children: React.ReactNode }) {
  const { isAuthenticated, loading, hasRole } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (loading) return;
    if (!isAuthenticated) {
      router.replace("/login");
    } else if (role && !hasRole(role)) {
      router.replace("/dashboard");
    }
  }, [loading, isAuthenticated, role, hasRole, router]);

  if (loading || !isAuthenticated || (role && !hasRole(role))) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-2 border-slate-300 border-t-brand-600" />
      </div>
    );
  }

  return <>{children}</>;
}
