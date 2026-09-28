"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { Container } from "@/components/Container";
import { RequireAuth } from "@/components/RequireAuth";
import { approvePost, getPendingPosts, rejectPost } from "@/services/posts";
import type { PostListItem } from "@/types";

function AdminContent() {
  const [posts, setPosts] = useState<PostListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);

  function load() {
    setLoading(true);
    getPendingPosts()
      .then(setPosts)
      .catch(() => setPosts([]))
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleApprove(id: number) {
    setBusyId(id);
    try {
      await approvePost(id);
      setPosts((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setBusyId(null);
    }
  }

  async function handleReject(id: number) {
    const reason = prompt("Reason for rejection:");
    if (!reason) return;
    setBusyId(id);
    try {
      await rejectPost(id, reason);
      setPosts((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <Container className="py-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Moderation</h1>
          <p className="mt-1 text-sm text-slate-500">Review posts submitted by writers.</p>
        </div>
        <span className="rounded-full bg-amber-100 px-3 py-1 text-sm font-medium text-amber-800">
          {posts.length} pending
        </span>
      </div>

      <div className="mt-6 space-y-4">
        {loading ? (
          <p className="text-sm text-slate-500">Loading…</p>
        ) : posts.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-slate-300 py-16 text-center text-slate-500">
            All caught up. Nothing to review. 🎉
          </div>
        ) : (
          posts.map((p) => (
            <div key={p.id} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div className="min-w-0">
                  {p.categoryName && (
                    <span className="text-xs font-semibold uppercase tracking-wide text-brand-600">{p.categoryName}</span>
                  )}
                  <h2 className="mt-1 text-lg font-semibold text-slate-900">{p.title}</h2>
                  <p className="mt-1 text-sm text-slate-600">{p.summary}</p>
                  <p className="mt-2 text-xs text-slate-400">by {p.authorName}</p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  <Link
                    href={`/blog/${p.slug}`}
                    className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
                  >
                    Preview
                  </Link>
                  <button
                    onClick={() => handleReject(p.id)}
                    disabled={busyId === p.id}
                    className="rounded-lg border border-rose-200 px-3 py-1.5 text-sm font-medium text-rose-600 transition hover:bg-rose-50 disabled:opacity-60"
                  >
                    Reject
                  </button>
                  <button
                    onClick={() => handleApprove(p.id)}
                    disabled={busyId === p.id}
                    className="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:opacity-60"
                  >
                    Approve
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </Container>
  );
}

export default function AdminPage() {
  return (
    <RequireAuth role="Admin">
      <AdminContent />
    </RequireAuth>
  );
}
