"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { StatusBadge } from "@/components/StatusBadge";
import { deletePost, getMyPosts, submitPost } from "@/services/posts";
import type { PostListItem } from "@/types";

export default function MyPostsPage() {
  const [posts, setPosts] = useState<PostListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);

  function load() {
    setLoading(true);
    getMyPosts()
      .then(setPosts)
      .catch(() => setPosts([]))
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleSubmit(id: number) {
    setBusyId(id);
    try {
      await submitPost(id);
      load();
    } finally {
      setBusyId(null);
    }
  }

  async function handleDelete(id: number) {
    if (!confirm("Delete this post permanently?")) return;
    setBusyId(id);
    try {
      await deletePost(id);
      setPosts((prev) => prev.filter((p) => p.id !== id));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">My posts</h1>
        <Link href="/dashboard/posts/new" className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700">
          + New post
        </Link>
      </div>

      <div className="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-card">
        {loading ? (
          <p className="p-6 text-sm text-slate-500">Loading…</p>
        ) : posts.length === 0 ? (
          <p className="p-6 text-sm text-slate-500">No posts yet.</p>
        ) : (
          <ul className="divide-y divide-slate-100">
            {posts.map((p) => (
              <li key={p.id} className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="min-w-0">
                  <div className="flex items-center gap-3">
                    <p className="truncate font-medium text-slate-900">{p.title}</p>
                    <StatusBadge status={p.status} />
                  </div>
                  <p className="mt-1 line-clamp-1 text-sm text-slate-500">{p.summary}</p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  {(p.status === "Draft" || p.status === "Rejected") && (
                    <button
                      onClick={() => handleSubmit(p.id)}
                      disabled={busyId === p.id}
                      className="rounded-lg bg-amber-500 px-3 py-1.5 text-sm font-medium text-white transition hover:bg-amber-600 disabled:opacity-60"
                    >
                      Submit
                    </button>
                  )}
                  <Link
                    href={`/dashboard/posts/${p.id}/edit`}
                    className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                  >
                    Edit
                  </Link>
                  <button
                    onClick={() => handleDelete(p.id)}
                    disabled={busyId === p.id}
                    className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-rose-600 transition hover:bg-rose-50 disabled:opacity-60"
                  >
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
