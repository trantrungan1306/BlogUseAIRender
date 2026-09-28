import Link from "next/link";
import type { PostListItem } from "@/types";

function formatDate(value?: string | null) {
  if (!value) return "";
  return new Date(value).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

export function PostCard({ post }: { post: PostListItem }) {
  return (
    <Link
      href={`/blog/${post.slug}`}
      className="group flex flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-card transition hover:-translate-y-1 hover:shadow-lg"
    >
      <div className="relative aspect-[16/9] overflow-hidden bg-slate-100">
        {post.coverImageUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={post.coverImageUrl}
            alt={post.title}
            className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-brand-500 to-brand-700 text-3xl font-bold text-white">
            {post.title.charAt(0)}
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col p-5">
        {post.categoryName && (
          <span className="mb-2 text-xs font-semibold uppercase tracking-wide text-brand-600">{post.categoryName}</span>
        )}
        <h3 className="text-lg font-semibold text-slate-900 group-hover:text-brand-700">{post.title}</h3>
        <p className="mt-2 line-clamp-2 flex-1 text-sm text-slate-600">{post.summary}</p>
        <div className="mt-4 flex items-center justify-between text-xs text-slate-500">
          <span>{post.authorName}</span>
          <span>{formatDate(post.publishedAt ?? post.createdAt)}</span>
        </div>
      </div>
    </Link>
  );
}
