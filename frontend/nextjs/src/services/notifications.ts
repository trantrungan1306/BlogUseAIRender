import { apiFetch } from "@/lib/api";
import type { Notification } from "@/types";

export function getNotifications(): Promise<Notification[]> {
  return apiFetch<Notification[]>("/notifications", { auth: true });
}

export function markNotificationRead(id: number): Promise<void> {
  return apiFetch<void>(`/notifications/${id}/read`, { method: "POST", auth: true });
}
