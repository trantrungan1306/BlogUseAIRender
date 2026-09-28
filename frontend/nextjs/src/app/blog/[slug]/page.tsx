import Link from "next/link";
import { notFound } from "next/navigation";
import { Container } from "@/components/Container";
import { Markdown } from "@/components/Markdown";
import { getPostBySlug } from "@/services/posts";
import type { Post } from "@/types";

function formatDate(value?: string | null) {
  if (!value) return "";
  return new Date(value).toLocaleDateString(undefined, { year: "numeric", month: "long", day: "numeric" });
}

export default async function BlogDetailPage({ params }: { params: { slug: string } }) {
  let post: Post;
  try {
    post = await getPostBySlug(params.slug);
  } catch {
    notFound();
  }

  return (
    <article className="pb-16">
      <div className="border-b border-slate-200 bg-white">
        <Container className="py-10">
          <Link href="/" className="text-sm font-medium text-brand-600 hover:text-brand-700">
            ← Back to articles
          </Link>
          {post!.categoryName && (
            <span className="mt-6 block text-xs font-semibold uppercase tracking-wide text-brand-600">
              {post!.categoryName}
            </span>
          )}
          <h1 className="mt-2 max-w-3xl text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">{post!.title}</h1>
          <p className="mt-4 max-w-2xl text-lg text-slate-600">{post!.summary}</p>
          <div className="mt-6 flex items-center gap-3 text-sm text-slate-500">
            <span className="grid h-9 w-9 place-items-center rounded-full bg-brand-600 text-sm font-semibold text-white">
              {post!.authorName.charAt(0).toUpperCase()}
            </span>
            <div>
              <p className="font-medium text-slate-800">{post!.authorName}</p>
              <p>{formatDate(post!.publishedAt ?? post!.createdAt)}</p>
            </div>
          </div>
        </Container>
      </div>

      {post!.coverImageUrl && (
        <Container className="py-8">
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={post!.coverImageUrl}
            alt={post!.title}
            className="aspect-[21/9] w-full rounded-2xl object-cover shadow-card"
          />
        </Container>
      )}

      <Container className="max-w-3xl">
        <Markdown content={post!.content} />
      </Container>
    </article>
  );
}
