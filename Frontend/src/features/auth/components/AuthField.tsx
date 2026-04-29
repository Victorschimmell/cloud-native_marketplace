import type { InputHTMLAttributes, TextareaHTMLAttributes } from 'react';

interface AuthFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
}

interface AuthTextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
}

export function AuthField({ label, ...inputProps }: AuthFieldProps) {
  return (
    <label className="auth-field">
      <span>{label}</span>
      <input {...inputProps} />
    </label>
  );
}

export function AuthTextArea({ label, ...textareaProps }: AuthTextAreaProps) {
  return (
    <label className="auth-field">
      <span>{label}</span>
      <textarea {...textareaProps} />
    </label>
  );
}
