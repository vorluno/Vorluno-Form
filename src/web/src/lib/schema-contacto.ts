// src/lib/schema-contacto.ts
import { z } from "zod";

/* ============================================
   SCHEMA SIMPLIFICADO PARA FORMULARIO CONTACTO
   ============================================ */

// Enums para campos de selección
export const TipoProyectoEnum = z.enum([
  "web",           // Aplicación Web
  "mobile",        // Aplicación Móvil
  "ecommerce",     // E-commerce
  "api",           // API / Backend
  "consultoria",   // Consultoría
  "otro"
], { required_error: "Selecciona un tipo de proyecto" });

export const PresupuestoEnum = z.enum([
  "menos-5k",      // Menos de $5,000
  "5k-15k",        // $5,000 - $15,000
  "15k-50k",       // $15,000 - $50,000
  "mas-50k"        // Más de $50,000
]);

export const UrgenciaEnum = z.enum([
  "inmediata",     // Necesito empezar ya
  "1-2-semanas",   // En 1-2 semanas
  "1-3-meses",     // En 1-3 meses
  "explorando"     // Solo explorando opciones
]);

export const FuenteEnum = z.enum([
  "google",
  "linkedin",
  "referido",
  "otro"
]);

/* --------------------------------
   SCHEMA DEL FORMULARIO (Frontend)
   -------------------------------- */
export const ContactoFormSchema = z.object({
  // Información básica del contacto
  Nombre: z.string()
    .min(2, "El nombre debe tener al menos 2 caracteres")
    .max(120, "El nombre es demasiado largo"),

  Email: z.string()
    .email("Email inválido")
    .min(1, "El email es requerido"),

  Telefono: z.string()
    .min(8, "El teléfono debe tener al menos 8 caracteres")
    .max(20, "El teléfono es demasiado largo"),

  Empresa: z.string()
    .max(120, "El nombre de empresa es demasiado largo")
    .optional(),

  // Tipo de proyecto
  TipoProyecto: TipoProyectoEnum,

  // Detalles opcionales del proyecto
  Presupuesto: PresupuestoEnum.optional(),

  Urgencia: UrgenciaEnum.optional(),

  // Mensaje/descripción del proyecto
  Mensaje: z.string()
    .min(20, "Por favor describe tu proyecto (mínimo 20 caracteres)")
    .max(1000, "El mensaje es demasiado largo (máximo 1000 caracteres)"),

  // ¿Cómo nos conociste?
  Fuente: FuenteEnum.optional(),

  // Aceptación de privacidad
  AceptaPrivacidad: z.boolean(),

  // Honeypot anti-bot (debe permanecer vacío)
  CodigoInterno: z.string().optional(),
});

/* --------------------------------
   SCHEMA DEL PAYLOAD (Envío al backend)
   -------------------------------- */
export const ContactoPayloadSchema = ContactoFormSchema.extend({
  // En el payload, la privacidad DEBE ser true
  AceptaPrivacidad: z.literal(true, {
    errorMap: () => ({ message: "Debes aceptar el aviso de privacidad" }),
  }),

  // El honeypot debe estar vacío
  CodigoInterno: z.string().max(0).optional(),
});

/* --------------------------------
   TYPES
   -------------------------------- */
export type ContactoForm = z.infer<typeof ContactoFormSchema>;
export type ContactoPayload = z.infer<typeof ContactoPayloadSchema>;

/* --------------------------------
   VALORES INICIALES
   -------------------------------- */
export const initialContactoValues: ContactoForm = {
  Nombre: "",
  Email: "",
  Telefono: "",
  Empresa: "",
  TipoProyecto: "web",
  Presupuesto: undefined,
  Urgencia: undefined,
  Mensaje: "",
  Fuente: undefined,
  AceptaPrivacidad: false,
  CodigoInterno: "",
};

/* --------------------------------
   LABELS/TRADUCCIONES
   -------------------------------- */
export const TIPO_PROYECTO_LABELS: Record<z.infer<typeof TipoProyectoEnum>, string> = {
  web: "Aplicación Web",
  mobile: "Aplicación Móvil",
  ecommerce: "E-commerce",
  api: "API / Backend",
  consultoria: "Consultoría",
  otro: "Otro"
};

export const PRESUPUESTO_LABELS: Record<z.infer<typeof PresupuestoEnum>, string> = {
  "menos-5k": "Menos de $5,000",
  "5k-15k": "$5,000 - $15,000",
  "15k-50k": "$15,000 - $50,000",
  "mas-50k": "Más de $50,000"
};

export const URGENCIA_LABELS: Record<z.infer<typeof UrgenciaEnum>, string> = {
  "inmediata": "Inmediata (necesito empezar ya)",
  "1-2-semanas": "En 1-2 semanas",
  "1-3-meses": "En 1-3 meses",
  "explorando": "Solo explorando opciones"
};

export const FUENTE_LABELS: Record<z.infer<typeof FuenteEnum>, string> = {
  google: "Google",
  linkedin: "LinkedIn",
  referido: "Referido por alguien",
  otro: "Otro"
};
