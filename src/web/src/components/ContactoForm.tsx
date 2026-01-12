import { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  ContactoPayloadSchema,
  ContactoFormSchema,
  type ContactoForm,
  type ContactoPayload,
  initialContactoValues,
  TIPO_PROYECTO_LABELS,
  PRESUPUESTO_LABELS,
  URGENCIA_LABELS,
  FUENTE_LABELS,
} from "../lib/schema-contacto";
import { enviarContacto } from "../lib/api-contacto";
import { FieldLabel, FieldInput, FieldSelect, FieldTextArea } from "./UI/Field";
import { SectionCard } from "./UI/SectionCard";
import { useToast } from "./UI/Toast";
import { enqueuePending, processPending, registerOnlineHandler } from "../lib/retryQueue-contacto";

/** Timeout de envío */
function submitWithTimeout<T>(p: Promise<T>, ms = 15000): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const t = setTimeout(() => reject(new Error("Tiempo de espera agotado. Intenta nuevamente.")), ms);
    p.then((v) => { clearTimeout(t); resolve(v); }).catch((e) => { clearTimeout(t); reject(e); });
  });
}

export default function ContactoForm() {
  const { toast } = useToast();

  const [enviadoOk, setEnviadoOk] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const form = useForm<ContactoForm>({
    resolver: zodResolver(ContactoFormSchema),
    defaultValues: initialContactoValues,
    mode: "onBlur",
    reValidateMode: "onChange",
  });

  const {
    register,
    handleSubmit,
    watch,
    control,
    formState: { errors, isSubmitting },
    reset,
  } = form;

  const aceptoPriv = !!watch("AceptaPrivacidad");

  // Reintentar pendientes cuando vuelva la conexión
  useEffect(() => { registerOnlineHandler(enviarContacto); }, []);

  // Post-éxito: reset + toast
  const afterSuccess = () => {
    setEnviadoOk(true);
    toast({ title: "¡Mensaje enviado! Te contactaremos pronto.", type: "success" });
    reset(initialContactoValues);
  };

  // Envío
  const onSubmit = async (raw: ContactoForm) => {
    let parsed: ContactoPayload;

    try {
      parsed = ContactoPayloadSchema.parse(raw);
    } catch {
      setError("Revisa los campos resaltados.");
      toast({ title: "Revisa los campos resaltados.", type: "error" });
      return;
    }

    try {
      setError(null);
      await submitWithTimeout(enviarContacto(parsed));
      afterSuccess();
    } catch (e: any) {
      const msg = e?.message ?? "No se pudo enviar. Intenta nuevamente.";
      setError(msg);
      setEnviadoOk(false);

      toast({
        title: msg,
        type: "error",
        actions: [
          {
            label: "Reintentar ahora",
            onClick: async () => {
              try {
                await submitWithTimeout(enviarContacto(parsed));
                afterSuccess();
              } catch {
                toast({ title: "Sigue fallando. Guardado para reintento.", type: "error" });
                enqueuePending(parsed);
              }
            },
          },
          {
            label: "Guardar y reintentar después",
            onClick: () => {
              enqueuePending(parsed);
              toast({ title: "Guardado. Se reintentará al volver la conexión.", type: "info" });
              void processPending(enviarContacto);
            },
          },
        ],
        timeout: 0,
      });
    }
  };

  const onInvalid = () => {
    const bad = document.querySelector('[aria-invalid="true"]') as HTMLElement | null;
    if (bad) {
      bad.focus({ preventScroll: false });
      bad.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit, onInvalid)} className="space-y-6" noValidate>
      {/* Información de Contacto */}
      <SectionCard title="Información de Contacto">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <FieldLabel htmlFor="Nombre" className="required">Tu nombre completo</FieldLabel>
            <FieldInput
              id="Nombre"
              {...register("Nombre")}
              placeholder="Ej.: Juan Pérez"
              autoComplete="name"
              autoCapitalize="words"
              aria-invalid={!!errors?.Nombre}
            />
            {errors.Nombre && <p className="error-text">{String(errors.Nombre.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Email" className="required">Email de contacto</FieldLabel>
            <FieldInput
              id="Email"
              type="email"
              {...register("Email")}
              placeholder="ejemplo@dominio.com"
              autoComplete="email"
              aria-invalid={!!errors?.Email}
            />
            {errors.Email && <p className="error-text">{String(errors.Email.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Telefono" className="required">Teléfono</FieldLabel>
            <FieldInput
              id="Telefono"
              type="tel"
              {...register("Telefono")}
              placeholder="+507 6000-0000"
              inputMode="tel"
              autoComplete="tel"
              aria-invalid={!!errors?.Telefono}
            />
            {errors.Telefono && <p className="error-text">{String(errors.Telefono.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Empresa">Empresa (opcional)</FieldLabel>
            <FieldInput
              id="Empresa"
              {...register("Empresa")}
              placeholder="Nombre de tu empresa"
              autoComplete="organization"
              aria-invalid={!!errors?.Empresa}
            />
            {errors.Empresa && <p className="error-text">{String(errors.Empresa.message)}</p>}
          </div>
        </div>
      </SectionCard>

      {/* Detalles del Proyecto */}
      <SectionCard title="Detalles del Proyecto">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <FieldLabel htmlFor="TipoProyecto" className="required">Tipo de proyecto</FieldLabel>
            <FieldSelect
              id="TipoProyecto"
              {...register("TipoProyecto")}
              aria-invalid={!!errors?.TipoProyecto}
            >
              {Object.entries(TIPO_PROYECTO_LABELS).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </FieldSelect>
            {errors.TipoProyecto && <p className="error-text">{String(errors.TipoProyecto.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Presupuesto">Presupuesto estimado</FieldLabel>
            <FieldSelect
              id="Presupuesto"
              {...register("Presupuesto")}
              aria-invalid={!!errors?.Presupuesto}
            >
              <option value="">-- Seleccione --</option>
              {Object.entries(PRESUPUESTO_LABELS).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </FieldSelect>
            {errors.Presupuesto && <p className="error-text">{String(errors.Presupuesto.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Urgencia">¿Cuándo necesitas empezar?</FieldLabel>
            <FieldSelect
              id="Urgencia"
              {...register("Urgencia")}
              aria-invalid={!!errors?.Urgencia}
            >
              <option value="">-- Seleccione --</option>
              {Object.entries(URGENCIA_LABELS).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </FieldSelect>
            {errors.Urgencia && <p className="error-text">{String(errors.Urgencia.message)}</p>}
          </div>

          <div>
            <FieldLabel htmlFor="Fuente">¿Cómo nos conociste?</FieldLabel>
            <FieldSelect
              id="Fuente"
              {...register("Fuente")}
              aria-invalid={!!errors?.Fuente}
            >
              <option value="">-- Seleccione --</option>
              {Object.entries(FUENTE_LABELS).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </FieldSelect>
            {errors.Fuente && <p className="error-text">{String(errors.Fuente.message)}</p>}
          </div>

          <div className="md:col-span-2">
            <FieldLabel htmlFor="Mensaje" className="required">Cuéntanos sobre tu proyecto</FieldLabel>
            <FieldTextArea
              id="Mensaje"
              rows={6}
              {...register("Mensaje")}
              placeholder="Describe tu proyecto, objetivos, requerimientos específicos, etc..."
              aria-invalid={!!errors?.Mensaje}
            />
            {errors.Mensaje && <p className="error-text">{String(errors.Mensaje.message)}</p>}
            <p className="helper-text">Mínimo 20 caracteres, máximo 1000.</p>
          </div>
        </div>
      </SectionCard>

      {/* Honeypot anti-bot */}
      <input type="text" className="hidden" autoComplete="off" {...register("CodigoInterno")} />

      {/* Aceptación + Botón de Envío */}
      <div className="form-sticky flex items-center justify-between gap-4 flex-wrap">
        <label className="inline-flex items-center gap-3">
          <Controller
            name="AceptaPrivacidad"
            control={control}
            render={({ field }) => (
              <input
                type="checkbox"
                className="h-4 w-4"
                checked={!!field.value}
                onChange={(e) => field.onChange(e.target.checked)}
                onBlur={field.onBlur}
                name={field.name}
                ref={field.ref}
              />
            )}
          />
          <span>He leído y acepto el aviso de privacidad.</span>
        </label>

        {errors.AceptaPrivacidad && <p className="error-text">{String(errors.AceptaPrivacidad.message)}</p>}

        <button
          type="submit"
          className="vorluno-btn inline-flex items-center gap-2"
          disabled={isSubmitting || !aceptoPriv}
        >
          {isSubmitting ? <span className="vorluno-spinner" aria-hidden /> : null}
          {isSubmitting ? "Enviando…" : "Enviar solicitud"}
        </button>
      </div>

      {/* Regiones vivas accesibles */}
      <p aria-live="polite" className="sr-only">{enviadoOk ? "Formulario enviado" : ""}</p>
      <p aria-live="assertive" className="sr-only">{error ?? ""}</p>

      {error && (
        <p className="p-3 rounded-lg border border-rose-200 bg-rose-50 text-rose-700 text-sm">
          {error}
        </p>
      )}
      {enviadoOk && (
        <p className="p-3 rounded-lg border border-emerald-200 bg-emerald-50 text-emerald-700 text-sm">
          ¡Gracias! Hemos recibido tu mensaje. Te contactaremos pronto.
        </p>
      )}
    </form>
  );
}
