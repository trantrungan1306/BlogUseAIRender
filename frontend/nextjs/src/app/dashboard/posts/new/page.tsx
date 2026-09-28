"use client";

import { useRouter } from "next/navigation";
import { PostForm } from "@/components/PostForm";
import { createPost, type PostInput } from "@/services/posts";

export default function NewPostPage() {
  const router = useRouter();

  async function handleCreate(input: PostInput) {
    await createPost(input);
    router.push("/dashboard/posts");
  }

  return (
    <div className="mx-auto max-w-3xl">
      <h1 className="text-2xl font-bold text-slate-900">Create a new post</h1>
      <p className="mt-1 text-sm text-slate-500">Saved as a draft. Submit it for review when you&apos;re ready.</p>
      <div className="mt-6 rounded-2xl border border-slate-200 bg-white p-6 shadow-card">
        <PostForm submitLabel="Save draft" onSubmit={handleCreate} />
      </div>
    </div>
  );
}
