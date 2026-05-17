import type { ReactNode } from 'react';
import './forms.css';

interface FormNoticeProps {
  children: ReactNode;
  variant: 'error' | 'info' | 'success';
}

export function FormNotice({ children, variant }: FormNoticeProps) {
  return (
    <p className={`form-notice form-notice--${variant}`} role={variant === 'error' ? 'alert' : 'status'}>
      {children}
    </p>
  );
}
