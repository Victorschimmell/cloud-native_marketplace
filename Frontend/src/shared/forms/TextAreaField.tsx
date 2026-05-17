import { useId, type TextareaHTMLAttributes } from 'react';
import './forms.css';

interface TextAreaFieldProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: string | null;
  hint?: string;
  label: string;
}

export function TextAreaField({ className, error, hint, id, label, ...textareaProps }: TextAreaFieldProps) {
  const generatedId = useId();
  const textareaId = id ?? `${generatedId}-textarea`;
  const hintId = hint ? `${textareaId}-hint` : undefined;
  const errorId = error ? `${textareaId}-error` : undefined;
  const describedBy = [hintId, errorId].filter(Boolean).join(' ') || undefined;

  return (
    <label className="form-field" htmlFor={textareaId}>
      <span className="form-field__label">{label}</span>
      <textarea
        {...textareaProps}
        aria-describedby={describedBy}
        aria-invalid={error ? true : undefined}
        className={['form-field__control', className].filter(Boolean).join(' ')}
        id={textareaId}
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
