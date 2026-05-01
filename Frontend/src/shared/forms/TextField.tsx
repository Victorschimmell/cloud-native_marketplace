import { useId, type InputHTMLAttributes } from 'react';
import './forms.css';

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  error?: string | null;
  hint?: string;
  label: string;
}

export function TextField({ className, error, hint, id, label, ...inputProps }: TextFieldProps) {
  const generatedId = useId();
  const inputId = id ?? `${generatedId}-input`;
  const hintId = hint ? `${inputId}-hint` : undefined;
  const errorId = error ? `${inputId}-error` : undefined;
  const describedBy = [hintId, errorId].filter(Boolean).join(' ') || undefined;

  return (
    <label className="form-field" htmlFor={inputId}>
      <span className="form-field__label">{label}</span>
      <input
        {...inputProps}
        aria-describedby={describedBy}
        aria-invalid={error ? true : undefined}
        className={['form-field__control', className].filter(Boolean).join(' ')}
        id={inputId}
      />
      {hint ? (
        <p className="form-field__hint" id={hintId}>
          {hint}
        </p>
      ) : null}
      {error ? (
        <p className="form-field__error" id={errorId}>
          {error}
        </p>
      ) : null}
    </label>
  );
}
