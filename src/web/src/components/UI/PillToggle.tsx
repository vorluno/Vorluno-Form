import React from 'react';

type Option = { value: string; label: string; icon?: React.ReactNode };

export const PillToggle: React.FC<{
  value: string; onChange: (v: string) => void; options: Option[]; className?: string;
}> = ({ value, onChange, options, className }) => (
  <div className={className}>
    {options.map(o => (
      <button key={o.value}
        type="button"
        className="pill"
        aria-pressed={o.value === value}
        onClick={() => onChange(o.value)}>
        {o.icon}{o.label}
      </button>
    ))}
  </div>
);
