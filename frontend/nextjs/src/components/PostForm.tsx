"use client";

import { useEffect, useState } from "react";
import { getCategories } from "@/services/categories";
import type { Category } from "@/types";
import type { PostInput } from "@/services/posts";

interface Props {
  initial?: Partial<PostInput>;
  submitLabel: string;
  onSubmit: (input: PostInput) => Promise<void>;
}

export function PostForm({ initial, submitLabel, onSubmit }: Props) {
  const [title, setTitle] = useState(initial?.title ?? "");
  const [summary, setSummary] = useState(initial?.summary ?? "");
  const [content, setContent] = useState(initial?.content ?? "");
  const [coverImageUrl, setCoverImageUrl] = useState(initial?.coverImageUrl ?? "");
  const [categoryId, setCategoryId] = useState<number | "">(initial?.categoryId ?? "");
  const [categories, setCategories] = useState<Category[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    getCategories().then(setCategories).catch(() => setCategories([]));
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSaving(true);
    try {
      await onSubmit({
        title,
        summary,
        content,
        coverImageUrl: coverImageUrl || null,
        categoryId: categoryId === "" ? null : Number(categoryId)
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      {error && <div className="rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>}

      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700">Title</label>
        <input
          required
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm outline-none ring-brand-500 transition focus:ring-2"
          placeholder="An engaging headline"
        />
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700">Category</label>
          <select
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value === "" ? "" : Number(e.target.value))}
            className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm outline-none ring-brand-500 transition focus:ring-2"
          >
            <option value="">Uncategorized</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700">Cover image URL</label>
          <input
            value={coverImageUrl ?? ""}
            onChange={(e) => setCoverImageUrl(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm outline-none ring-brand-500 transition focus:ring-2"
            placeholder="https://…"
          />
        </div>
      </div>

      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700">Summary</label>
        <textarea
          required
          rows={2}
          value={summary}
          onChange={(e) => setSummary(e.target.value)}
          className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm outline-none ring-brand-500 transition focus:ring-2"
          placeholder="A short teaser that appears on cards."
        />
      </div>

      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700">Content (Markdown supported)</label>
        <textarea
          required
          rows={14}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          className="w-full rounded-lg border border-slate-300 px-4 py-2.5 font-mono text-sm outline-none ring-brand-500 transition focus:ring-2"
          placeholder="# Heading\n\nWrite your story…"
        />
      </div>

      <button
        type="submit"
        disabled={saving}
        className="rounded-lg bg-brand-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-brand-700 disabled:opacity-60"
      >
        {saving ? "Saving…" : submitLabel}
      </button>
    </form>
  );
}
