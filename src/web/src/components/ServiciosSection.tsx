// src/components/ServiciosSection.tsx
import { Dispatch, SetStateAction } from "react";

export type ServiceId =
  | "ph"
  | "renta"
  | "supervision"
  | "planilla"
  | "capacitaciones"
  | "otros";

export type PlanId = "emprendedor" | "pyme" | "ph" | "ecom" | "cumplimiento" | "full";

const SERVICE_OPTS: { id: ServiceId; label: string; hint: string }[] = [
  { id: "ph", label: "Asesoría para P.H.", hint: "Contabilidad y reportes para condominios/Juntas." },
  { id: "renta", label: "Declaraciones de Renta (Panamá)", hint: "Preparación y presentación de DJR." },
  { id: "supervision", label: "Supervisión contable", hint: "Revisión de informes/controles para empresas." },
  { id: "planilla", label: "Confección de Planilla", hint: "Cálculo quincenal/mensual y archivos SIPE." },
  { id: "capacitaciones", label: "Capacitaciones y Seminarios", hint: "Sesiones y talleres a medida." },
  { id: "otros", label: "Otros trámites/diligencias", hint: "RUC/avisos/diligencias varias." },
];

const PLAN_OPTS: PlanId[] = ["emprendedor", "pyme", "ph", "ecom", "cumplimiento", "full"];

export function ServiciosSection({
  services,
  setServices,
  plan,
  setPlan,
  otrosDetalle,
  setOtrosDetalle,
}: {
  services: ServiceId[];
  setServices: Dispatch<SetStateAction<ServiceId[]>>;
  plan: PlanId | null;
  setPlan: Dispatch<SetStateAction<PlanId | null>>;
  otrosDetalle: string;
  setOtrosDetalle: Dispatch<SetStateAction<string>>;
}) {
  function toggleService(id: ServiceId) {
    setServices((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  }

  return (
    <fieldset className="form-card">
      <legend className="section-title">¿En qué te ayudamos?</legend>

      {/* Chips de servicios */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
        {SERVICE_OPTS.map((s) => {
          const active = services.includes(s.id);
          return (
            <button
              key={s.id}
              type="button"
              onClick={() => toggleService(s.id)}
              title={s.hint}
              className={[
                "rounded-full border px-3 py-2 text-sm transition",
                active
                  ? "bg-white/15 border-white/40 text-white"
                  : "bg-white/5 border-white/15 text-white/80 hover:bg-white/10",
              ].join(" ")}
              aria-pressed={active}
            >
              {active ? "✓ " : ""}
              {s.label}
            </button>
          );
        })}
      </div>

      {/* Radios de plan */}
      <div className="mt-4">
        <label className="field-label">¿Prefieres un plan? (opcional)</label>
        <div className="mt-2 flex flex-wrap gap-4">
          {PLAN_OPTS.map((p) => (
            <label key={p} className="inline-flex items-center gap-2">
              <input
                type="radio"
                name="plan"
                value={p}
                checked={plan === p}
                onChange={() => setPlan(p)}
              />
              <span className="capitalize">{p}</span>
            </label>
          ))}
          <label className="inline-flex items-center gap-2">
            <input
              type="radio"
              name="plan"
              value=""
              checked={plan === null}
              onChange={() => setPlan(null)}
            />
            <span>Sin plan</span>
          </label>
        </div>
      </div>

      {/* No estoy seguro / asesoría */}
      <div className="mt-4">
        <label className="field-label" htmlFor="otrosDetalle">
          No estoy seguro / Quiero asesoría
        </label>
        <textarea
          id="otrosDetalle"
          className="field-input min-h-24"
          rows={3}
          maxLength={300}
          value={otrosDetalle}
          onChange={(e) => setOtrosDetalle(e.target.value)}
          placeholder="Cuéntanos brevemente…"
        />
        <p className="text-white/50 text-xs mt-1">Máx. 300 caracteres.</p>
      </div>
    </fieldset>
  );
}
