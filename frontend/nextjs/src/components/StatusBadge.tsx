import { clsx } from "@/lib/clsx";
import type { PostStatus } from "@/types";

const styles: Record<PostStatus, string> = {
  Draft: "bg-slate-100 text-slate-700",
  PendingReview: "bg-amber-100 text-amber-800",
  Published: "bg-emerald-100 text-emerald-700",
  Rejected: "bg-rose-100 text-rose-700"
};

const labels: Record<PostStatus, string> = {
  Draft: "Draft",
  PendingReview: "Pending review",
  Published: "Published",
  Rejected: "Rejected"
};

export function StatusBadge({ status }: { status: PostStatus }) {
  return (
    <span className={clsx("inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium", styles[status])}>
      {labels[status]}
    </span>
  );
}
