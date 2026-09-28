"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { StatusBadge } from "@/components/StatusBadge";
import { PostForm } from "@/components/PostForm";
import { getPostById, submitPost, updatePost, type PostInput } from "@/services/posts";
import type { Post } from "@/types";

export default function EditPostPage({ params }: { params: { id: string } }) {
  const router = useRouter();
  const id = Number(params.id);
  const [post, setPost] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    getPostById(id)
      .then(setPost)
      .catch(() => setPost(null))
      .finally(() => setLoading(false));
  }, [id]);

  async function handleUpdate(input: PostInput) {
    const updated = await updatePost(id, input);
    setPost(updated);
  }

  async function handleSubmitForReview() {
    setSubmitting(true);
    try {
      await submitPost(id);
      router.push("/dashboard/posts");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) return <p className="text-sm text-slate-500">Loading…</p>;
  if (!post) return <p className="text-sm text-slate-500">Post not found.</p>;

  return (
    <div className="mx-auto max-w-3xl">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-slate-900">Edit post</h1>
          <StatusBadge status={post.status} />
        </div>
        {(post.status === "Draft" || post.status === "Rejected") && (
          <button
            onClick={handleSubmitForReview}
            disabled={submitting}
            className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-amber-600 disabled:opacity-60"
          >
            Submit for review
          </button>
        )}
      </div>

      {post.status === "Rejected" && post.rejectionReason && (
        <div className="mt-4 rounded-lg bg-rose-50 px-4 py-3 text-sm text-rose-700">
          <strong>Rejected:</strong> {post.rejectionReason}
        </div>
      )}

      <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-6 shadow-card">
        <PostForm
          submitLabel="Save changes"
          onSubmit={handleUpdate}
          initial={{
            title: post.title,
            summary: post.summary,
            content: post.content,
            coverImageUrl: post.coverImageUrl ?? "",
            categoryId: post.categoryId ?? null
          }}
        />
      </div>
    </div>
  );
}
