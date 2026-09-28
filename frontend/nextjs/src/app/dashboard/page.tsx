"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { getMyPosts } from "@/services/posts";
import type { PostListItem } from "@/types";

export default function DashboardPage() {
  const { user } = useAuth();
  const [posts, setPosts] = useState<PostListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyPosts()
      .then(setPosts)
      .catch(() => setPosts([]))
      .finally(() => setLoading(false));
  }, []);

  const stats = [
    { label: "Total posts", value: posts.length, tone: "text-slate-900" },
    { label: "Published", value: posts.filter((p) => p.status === "Published").length, tone: "text-emerald-600" },
    { label: "Pending review", value: posts.filter((p) => p.status === "PendingReview").length, tone: "text-amber-600" },
    { label: "Drafts", value: posts.filter((p) => p.status === "Draft").length, tone: "text-slate-600" }
  ];

  return (
    <div>
      <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Welcome back, {user?.displayName} 👋</h1>
          <p className="mt-1 text-sm text-slate-500">Here&apos;s what&apos;s happening with your writing.</p>
        </div>
        <Link
          href="/dashboard/posts/new"
          className="inline-flex items-center justify-center rounded-lg bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-brand-700"
        >
          + New post
        </Link>
      </div>

      <div className="mt-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        {stats.map((s) => (
          <div key={s.label} className="rounded-2xl border border-slate-200 bg-white p-5 shadow-card">
            <p className="text-sm text-slate-500">{s.label}</p>
            <p className={`mt-2 text-3xl font-bold ${s.tone}`}>{loading ? "—" : s.value}</p>
          </div>
        ))}
      </div>

      <div className="mt-8 rounded-2xl border border-slate-200 bg-white p-6 shadow-card">
        <h2 className="text-lg font-semibold text-slate-900">Recent posts</h2>
        {loading ? (
          <p className="mt-4 text-sm text-slate-500">Loading…</p>
        ) : posts.length === 0 ? (
          <p className="mt-4 text-sm text-slate-500">
            You haven&apos;t written anything yet.{" "}
            <Link href="/dashboard/posts/new" className="font-medium text-brand-600">
              Create your first post
            </Link>
            .
          </p>
        ) : (
          <ul className="mt-4 divide-y divide-slate-100">
            {posts.slice(0, 5).map((p) => (
              <li key={p.id} className="flex items-center justify-between py-3">
                <span className="truncate text-sm font-medium text-slate-800">{p.title}</span>
                <Link href={`/dashboard/posts/${p.id}/edit`} className="text-sm font-medium text-brand-600 hover:text-brand-700">
                  Edit
                </Link>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
