import type { ReactNode } from 'react';
import './StatusMessage.css';

interface StatusMessageProps {
  children: ReactNode;
  variant?: 'info' | 'error';
}

export default function StatusMessage({ children, variant = 'info' }: StatusMessageProps) {
  return (
    <div className={`status-message status-message--${variant}`} role={variant === 'error' ? 'alert' : undefined}>
      {children}
    </div>
  );
}
