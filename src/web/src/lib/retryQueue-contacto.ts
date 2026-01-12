// src/lib/retryQueue-contacto.ts
import type { ContactoPayload } from "./schema-contacto";

const KEY = "vorluno:pending-submissions";
const makeId = () =>
  (globalThis.crypto && "randomUUID" in globalThis.crypto
    ? (crypto as any).randomUUID()
    : Math.random().toString(36).slice(2));

type Pending = {
  id: string;
  createdAt: number;
  tries: number;
  payload: ContactoPayload;
};

const load = (): Pending[] => {
  try {
    return JSON.parse(localStorage.getItem(KEY) || "[]");
  } catch {
    return [];
  }
};

const save = (arr: Pending[]) => localStorage.setItem(KEY, JSON.stringify(arr));

export function enqueuePending(payload: ContactoPayload) {
  const arr = load();
  arr.push({ id: makeId(), createdAt: Date.now(), tries: 0, payload });
  save(arr);
}

export async function processPending(
  sender: (p: ContactoPayload) => Promise<unknown>
): Promise<number> {
  let arr = load();
  let sent = 0;
  for (const item of [...arr]) {
    try {
      await sender(item.payload);
      arr = arr.filter((x) => x.id !== item.id);
      sent++;
    } catch {
      // queda pendiente
    }
  }
  save(arr);
  return sent;
}

export function hasPending() {
  return load().length > 0;
}

export function registerOnlineHandler(sender: (p: ContactoPayload) => Promise<unknown>) {
  window.addEventListener("online", () => {
    void processPending(sender);
  });
}
