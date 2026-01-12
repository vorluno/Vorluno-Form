import React, { forwardRef, InputHTMLAttributes, SelectHTMLAttributes, TextareaHTMLAttributes } from 'react';

const cx = (...a: (string | false | undefined)[]) => a.filter(Boolean).join(' ');

export const FieldLabel: React.FC<React.LabelHTMLAttributes<HTMLLabelElement>> =
  (p) => <label {...p} className={cx('field-label', p.className)} />;

export const FieldInput = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement>>(
  (p, ref) => <input ref={ref} {...p} className={cx('field-input', p.className)} />
);
FieldInput.displayName = 'FieldInput';

export const FieldSelect = forwardRef<HTMLSelectElement, SelectHTMLAttributes<HTMLSelectElement>>(
  (p, ref) => <select ref={ref} {...p} className={cx('field-input', p.className)} />
);
FieldSelect.displayName = 'FieldSelect';

export const FieldTextArea = forwardRef<HTMLTextAreaElement, TextareaHTMLAttributes<HTMLTextAreaElement>>(
  (p, ref) => <textarea ref={ref} {...p} className={cx('field-input', p.className)} />
);
FieldTextArea.displayName = 'FieldTextArea';
