import { apiFetch } from "@/lib/api";
import type { PagedResult, Post, PostListItem } from "@/types";

export interface PostQueryParams {
  search?: string;
  category?: string;
  page?: number;
  pageSize?: number;
}

export function getPublishedPosts(params: PostQueryParams = {}): Promise<PagedResult<PostListItem>> {
  const query = new URLSearchParams();
  if (params.search) query.set("search", params.search);
  if (params.category) query.set("category", params.category);
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 9));
  return apiFetch<PagedResult<PostListItem>>(`/posts?${query.toString()}`);
}

export function getPostBySlug(slug: string): Promise<Post> {
  return apiFetch<Post>(`/posts/slug/${slug}`);
}

export function getPostById(id: number): Promise<Post> {
  return apiFetch<Post>(`/posts/${id}`, { auth: true });
}

export function getMyPosts(): Promise<PostListItem[]> {
  return apiFetch<PostListItem[]>("/posts/mine", { auth: true });
}

export function getPendingPosts(): Promise<PostListItem[]> {
  return apiFetch<PostListItem[]>("/posts/pending", { auth: true });
}

export interface PostInput {
  title: string;
  summary: string;
  content: string;
  coverImageUrl?: string | null;
  categoryId?: number | null;
}

export function createPost(input: PostInput): Promise<Post> {
  return apiFetch<Post>("/posts", { method: "POST", body: input, auth: true });
}

export function updatePost(id: number, input: PostInput): Promise<Post> {
  return apiFetch<Post>(`/posts/${id}`, { method: "PUT", body: input, auth: true });
}

export function submitPost(id: number): Promise<Post> {
  return apiFetch<Post>(`/posts/${id}/submit`, { method: "POST", auth: true });
}

export function approvePost(id: number): Promise<Post> {
  return apiFetch<Post>(`/posts/${id}/approve`, { method: "POST", auth: true });
}

export function rejectPost(id: number, reason: string): Promise<Post> {
  return apiFetch<Post>(`/posts/${id}/reject`, { method: "POST", body: { reason }, auth: true });
}

export function deletePost(id: number): Promise<void> {
  return apiFetch<void>(`/posts/${id}`, { method: "DELETE", auth: true });
}
