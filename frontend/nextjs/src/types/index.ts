export type Role = "Viewer" | "Blogger" | "Admin";

export type PostStatus = "Draft" | "PendingReview" | "Published" | "Rejected";

export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string | null;
  roles: Role[];
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface PostListItem {
  id: number;
  title: string;
  slug: string;
  summary: string;
  coverImageUrl?: string | null;
  status: PostStatus;
  authorName: string;
  categoryName?: string | null;
  createdAt: string;
  publishedAt?: string | null;
}

export interface Post extends PostListItem {
  content: string;
  authorId: string;
  categoryId?: number | null;
  rejectionReason?: string | null;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  description?: string | null;
  postCount: number;
}

export interface Notification {
  id: number;
  title: string;
  message: string;
  linkUrl?: string | null;
  isRead: boolean;
  createdAt: string;
}
