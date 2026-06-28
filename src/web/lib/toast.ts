import { signal } from "@preact/signals";

export type ToastType = "success" | "error" | "info";

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
}

export const toasts = signal<Toast[]>([]);

export function showToast(message: string, type: ToastType = "info") {
  const id = Math.random().toString(36).slice(2);
  const newToast = { id, message, type };

  toasts.value = [...toasts.value, newToast];

  globalThis.setTimeout(() => {
    toasts.value = toasts.value.filter((t) => t.id !== id);
  }, 3000);
}

export const toast = {
  success: (m: string) => showToast(m, "success"),
  error: (m: string) => showToast(m, "error"),
};
