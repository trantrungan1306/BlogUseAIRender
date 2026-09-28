"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { Container } from "@/components/Container";
import { PostCard } from "@/components/PostCard";
import { getPublishedPosts } from "@/services/posts";
import { getCategories } from "@/services/categories";
import type { Category, PostListItem } from "@/types";

export default function HomePage() {
  const [posts, setPosts] = useState<PostListItem[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [search, setSearch] = useState("");
  const [activeCategory, setActiveCategory] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCategories().then(setCategories).catch(() => setCategories([]));
  }, []);

  useEffect(() => {
    setLoading(true);
    getPublishedPosts({ search, category: activeCategory ?? undefined, pageSize: 9 })
      .then((res) => setPosts(res.items))
      .catch(() => setPosts([]))
      .finally(() => setLoading(false));
  }, [search, activeCategory]);

  return (
    <>
      <section className="relative overflow-hidden bg-slate-900">
        <div className="absolute inset-0 bg-gradient-to-br from-brand-600 via-brand-700 to-slate-900 opacity-95" />
        <div className="absolute -right-24 -top-24 h-96 w-96 rounded-full bg-brand-400/30 blur-3xl" />
        <Container className="relative py-20 sm:py-28">
          <div className="max-w-2xl">
            <span className="inline-flex items-center rounded-full bg-white/10 px-3 py-1 text-xs font-medium text-white ring-1 ring-white/20">
              A modern publishing platform
            </span>
            <h1 className="mt-6 text-4xl font-bold tracking-tight text-white sm:text-5xl lg:text-6xl">
              Write. Share. Inspire.
            </h1>
            <p className="mt-5 max-w-xl text-lg text-brand-100">
              Discover stories from our community — thoughtfully written, carefully reviewed, and beautifully presented.
            </p>
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <a
                href="#featured"
                className="rounded-lg bg-white px-6 py-3 text-center text-sm font-semibold text-brand-700 shadow-sm transition hover:bg-brand-50"
              >
                Explore articles
              </a>
              <Link
                href="/register"
                className="rounded-lg border border-white/30 px-6 py-3 text-center text-sm font-semibold text-white transition hover:bg-white/10"
              >
                Start writing
              </Link>
            </div>
          </div>
        </Container>
      </section>

      <Container className="py-12">
        <div id="categories" className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-2xl font-bold text-slate-900" id="featured">
              Featured articles
            </h2>
            <p className="mt-1 text-sm text-slate-500">Fresh stories, hand-picked for you.</p>
          </div>
          <div className="relative w-full sm:w-72">
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search articles..."
              className="w-full rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm outline-none ring-brand-500 transition focus:ring-2"
            />
          </div>
        </div>

        <div className="mt-6 flex flex-wrap gap-2">
          <button
            onClick={() => setActiveCategory(null)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
              activeCategory === null ? "bg-brand-600 text-white" : "bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50"
            }`}
          >
            All
          </button>
          {categories.map((c) => (
            <button
              key={c.id}
              onClick={() => setActiveCategory(c.slug)}
              className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
                activeCategory === c.slug ? "bg-brand-600 text-white" : "bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50"
              }`}
            >
              {c.name}
              <span className="ml-1.5 text-xs opacity-70">{c.postCount}</span>
            </button>
          ))}
        </div>

        {loading ? (
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="h-72 animate-pulse rounded-2xl bg-slate-200/70" />
            ))}
          </div>
        ) : posts.length === 0 ? (
          <div className="mt-16 rounded-2xl border border-dashed border-slate-300 py-16 text-center">
            <p className="text-slate-500">No articles found. Try a different search.</p>
          </div>
        ) : (
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {posts.map((post) => (
              <PostCard key={post.id} post={post} />
            ))}
          </div>
        )}
      </Container>
    </>
  );
}
