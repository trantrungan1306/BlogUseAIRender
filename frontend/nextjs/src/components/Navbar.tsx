"use client";

import Link from "next/link";
import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { Container } from "@/components/Container";
import { clsx } from "@/lib/clsx";

const navLinks = [
  { href: "/", label: "Home" },
  { href: "/#featured", label: "Articles" },
  { href: "/#categories", label: "Categories" }
];

export function Navbar() {
  const { user, isAuthenticated, hasRole, logout } = useAuth();
  const [open, setOpen] = useState(false);

  return (
    <header className="sticky top-0 z-40 border-b border-slate-200/70 bg-white/80 backdrop-blur">
      <Container className="flex h-16 items-center justify-between">
        <Link href="/" className="flex items-center gap-2" onClick={() => setOpen(false)}>
          <span className="grid h-9 w-9 place-items-center rounded-xl bg-brand-600 text-lg font-bold text-white">S</span>
          <span className="text-lg font-semibold tracking-tight text-slate-900">SimpleBlog</span>
        </Link>

        <nav className="hidden items-center gap-8 md:flex">
          {navLinks.map((l) => (
            <Link key={l.href} href={l.href} className="text-sm font-medium text-slate-600 transition hover:text-slate-900">
              {l.label}
            </Link>
          ))}
        </nav>

        <div className="hidden items-center gap-3 md:flex">
          {isAuthenticated ? (
            <>
              <Link href="/dashboard" className="text-sm font-medium text-slate-600 hover:text-slate-900">
                Dashboard
              </Link>
              {hasRole("Admin") && (
                <Link href="/admin" className="text-sm font-medium text-slate-600 hover:text-slate-900">
                  Admin
                </Link>
              )}
              <div className="flex items-center gap-2 rounded-full bg-slate-100 py-1 pl-1 pr-3">
                <span className="grid h-7 w-7 place-items-center rounded-full bg-brand-600 text-xs font-semibold text-white">
                  {user?.displayName?.charAt(0).toUpperCase()}
                </span>
                <span className="text-sm font-medium text-slate-700">{user?.displayName}</span>
              </div>
              <button
                onClick={logout}
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <Link href="/login" className="text-sm font-medium text-slate-600 hover:text-slate-900">
                Login
              </Link>
              <Link
                href="/register"
                className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-brand-700"
              >
                Get started
              </Link>
            </>
          )}
        </div>

        <button
          className="inline-flex h-10 w-10 items-center justify-center rounded-lg text-slate-700 md:hidden"
          onClick={() => setOpen((v) => !v)}
          aria-label="Toggle menu"
        >
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            {open ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
          </svg>
        </button>
      </Container>

      <div className={clsx("border-t border-slate-200 bg-white md:hidden", open ? "block" : "hidden")}>
        <Container className="flex flex-col gap-1 py-3">
          {navLinks.map((l) => (
            <Link
              key={l.href}
              href={l.href}
              onClick={() => setOpen(false)}
              className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
            >
              {l.label}
            </Link>
          ))}
          <div className="my-2 h-px bg-slate-200" />
          {isAuthenticated ? (
            <>
              <Link href="/dashboard" onClick={() => setOpen(false)} className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
                Dashboard
              </Link>
              {hasRole("Admin") && (
                <Link href="/admin" onClick={() => setOpen(false)} className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
                  Admin
                </Link>
              )}
              <button
                onClick={() => {
                  logout();
                  setOpen(false);
                }}
                className="rounded-lg px-3 py-2 text-left text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <Link href="/login" onClick={() => setOpen(false)} className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
                Login
              </Link>
              <Link
                href="/register"
                onClick={() => setOpen(false)}
                className="rounded-lg bg-brand-600 px-3 py-2 text-center text-sm font-semibold text-white"
              >
                Get started
              </Link>
            </>
          )}
        </Container>
      </div>
    </header>
  );
}
