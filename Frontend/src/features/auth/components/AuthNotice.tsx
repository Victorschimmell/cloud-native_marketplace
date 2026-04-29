interface AuthNoticeProps {
  children: string;
  variant: 'error' | 'success';
}

export function AuthNotice({ children, variant }: AuthNoticeProps) {
  return (
    <p className={`auth-form__notice auth-form__notice--${variant}`} role={variant === 'error' ? 'alert' : 'status'}>
      {children}
    </p>
  );
}
