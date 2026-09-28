"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Container } from "@/components/Container";
import { RequireAuth } from "@/components/RequireAuth";
import { useAuth } from "@/context/AuthContext";
import { clsx } from "@/lib/clsx";

const links = [
  { href: "/dashboard", label: "Overview", exact: true },
  { href: "/dashboard/posts", label: "My Posts" },
  { href: "/dashboard/posts/new", label: "Create Post" }
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { hasRole } = useAuth();

  return (
    <RequireAuth>
      <Container className="py-8">
        <div className="grid gap-8 lg:grid-cols-[220px_1fr]">
          <aside className="lg:sticky lg:top-20 lg:h-fit">
            <nav className="flex gap-1 overflow-x-auto rounded-xl border border-slate-200 bg-white p-2 lg:flex-col">
              {links.map((l) => {
                const active = l.exact ? pathname === l.href : pathname.startsWith(l.href);
                return (
                  <Link
                    key={l.href}
                    href={l.href}
                    className={clsx(
                      "whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium transition",
                      active ? "bg-brand-600 text-white" : "text-slate-600 hover:bg-slate-100"
                    )}
                  >
                    {l.label}
                  </Link>
                );
              })}
              {hasRole("Admin") && (
                <Link
                  href="/admin"
                  className="whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
                >
                  Pending Reviews
                </Link>
              )}
            </nav>
          </aside>
          <div>{children}</div>
        </div>
      </Container>
    </RequireAuth>
  );
}
