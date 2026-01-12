// src/lib/api-contacto.ts
import axios from "axios";
import type { ContactoPayload } from "./schema-contacto";

// Configurar axios con baseURL relativa (para que funcione en dev y prod)
const api = axios.create({
  baseURL: "/api",
  headers: { "Content-Type": "application/json" },
});

/**
 * Envía el formulario de contacto al backend.
 */
export async function enviarContacto(payload: ContactoPayload): Promise<void> {
  await api.post("/contacto", payload);
}
