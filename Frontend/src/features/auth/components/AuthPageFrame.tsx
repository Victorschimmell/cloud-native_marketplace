import type { ReactNode } from 'react';

interface AuthPageFrameProps {
  children: ReactNode;
  variant: 'login' | 'register';
}

export function AuthPageFrame({ children, variant }: AuthPageFrameProps) {
  return (
    <div className="auth-page">
      <aside className={`auth-page__panel auth-page__panel--${variant}`} aria-hidden="true">
        <div className="auth-page__mark">
          <span />
          <span />
          <span />
        </div>
      </aside>
      {children}
    </div>
  );
}
