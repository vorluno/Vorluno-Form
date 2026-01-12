// ClientApp/src/components/UI/Toast.tsx
import React, { createContext, useCallback, useContext, useMemo, useRef, useState } from "react";

type ToastType = "success" | "error" | "info";
type ToastAction = { label: string; onClick: () => void };
type ToastItem = {
  id: string;
  title: string;
  type: ToastType;
  timeout?: number; // ms (default 3500). 0 = no autohide
  actions?: ToastAction[];
};

type ToastContextType = {
  push: (t: Omit<ToastItem, "id" | "type"> & { type?: ToastType }) => void;
};

const ToastCtx = createContext<ToastContextType | null>(null);

const makeId = () =>
(globalThis.crypto && "randomUUID" in globalThis.crypto
  ? (crypto as any).randomUUID()
  : Math.random().toString(36).slice(2));

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const timers = useRef<Record<string, any>>({});

  const remove = useCallback((id: string) => {
    setToasts((arr) => arr.filter((t) => t.id !== id));
    if (timers.current[id]) {
      clearTimeout(timers.current[id]);
      delete timers.current[id];
    }
  }, []);

  const push = useCallback<ToastContextType["push"]>((opts) => {
    const id = makeId();
    const item: ToastItem = {
      id,
      type: opts.type ?? "info",
      title: opts.title,
      actions: opts.actions,
      timeout: opts.timeout ?? 3500,
    };
    setToasts((arr) => [...arr, item]);
    if (item.timeout && item.timeout > 0) {
      timers.current[id] = setTimeout(() => remove(id), item.timeout);
    }
  }, [remove]);

  const value = useMemo(() => ({ push }), [push]);

  return (
    <ToastCtx.Provider value={value}>
      {children}
      <div className="fixed bottom-6 right-6 z-50 flex flex-col gap-3">
        {toasts.map((t) => {
          const theme =
            t.type === "success"
              ? "border-emerald-300 bg-emerald-50 text-emerald-800"
              : t.type === "error"
                ? "border-rose-300 bg-rose-50 text-rose-800"
                : "border-sky-300 bg-sky-50 text-sky-800";
          return (
            <div
              key={t.id}
              className={`max-w-[360px] rounded-lg border shadow-lg px-4 py-3 ${theme}`}
              role="status"
              aria-live="polite"
            >
              <div className="flex items-start gap-3">
                <div className="mt-0.5 text-sm">{t.title}</div>
                <button
                  onClick={() => remove(t.id)}
                  className="ml-auto text-sm opacity-60 hover:opacity-100"
                  aria-label="Cerrar"
                >
                  ✕
                </button>
              </div>
              {t.actions && t.actions.length > 0 && (
                <div className="mt-2 flex gap-2">
                  {t.actions.map((a, i) => (
                    <button
                      key={i}
                      onClick={() => { remove(t.id); a.onClick(); }}
                      className="rounded-md border px-2 py-1 text-xs hover:bg-black/5"
                    >
                      {a.label}
                    </button>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </ToastCtx.Provider>
  );
};

export function useToast() {
  const ctx = useContext(ToastCtx);
  if (!ctx) throw new Error("useToast must be used within <ToastProvider>");
  return { toast: ctx.push };
}
