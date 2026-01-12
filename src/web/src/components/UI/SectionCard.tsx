import React from 'react';

export const SectionCard: React.FC<{ title?: string; children: React.ReactNode; className?: string }> =
  ({ title, children, className }) => (
    <fieldset className={`form-card ${className ?? ''}`}>
      {title && <legend className="section-title">{title}</legend>}
      {children}
    </fieldset>
  );
