import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { FormNotice, TextField } from '../../../shared/forms';
import { useAuth } from '../useAuth';
import { AuthPageFrame } from '../components/AuthPageFrame';
import type { AuthCapabilities } from '../types';
import './AuthPages.css';

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const wasRegistered = searchParams.get('registered') === '1';
  const explicitReturnTo = getSafeReturnTo(searchParams.get('returnTo'));

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setHasAttemptedSubmit(true);

    const trimmedEmail = email.trim();
    const validationError = validateLoginForm(trimmedEmail, password);
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      const capabilities = await login({ email: trimmedEmail, password });
      navigate(explicitReturnTo ?? resolveLandingRoute(capabilities));
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Could not log in right now.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PageSkeleton summary="Access your account and continue shopping with your saved cart." title="Log in">
      <AuthPageFrame variant="login">
        <form className="auth-form" noValidate onSubmit={submit}>
          <div className="auth-form__header">
            <span className="auth-form__eyebrow">Welcome back</span>
            <h2>Continue shopping</h2>
          </div>

          {wasRegistered ? <FormNotice variant="success">Account created. Log in to continue.</FormNotice> : null}

          {error ? <FormNotice variant="error">{error}</FormNotice> : null}

          <TextField
            autoComplete="email"
            invalid={hasAttemptedSubmit && (!email.trim() || !emailPattern.test(email.trim()))}
            label="Email"
            onChange={(event) => setEmail(event.target.value)}
            type="email"
            value={email}
          />

          <TextField
            autoComplete="current-password"
            invalid={hasAttemptedSubmit && !password}
            label="Password"
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            value={password}
          />

          <button disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Logging in...' : 'Log in'}
          </button>

          <p className="auth-form__footer">
            New here? <Link to="/register">Create an account</Link>
          </p>
        </form>
      </AuthPageFrame>
    </PageSkeleton>
  );
}

// Allow only same-origin paths; null falls back to a role-based landing route.
function getSafeReturnTo(value: string | null): string | null {
  if (value?.startsWith('/') && !value.startsWith('//')) {
    return value;
  }

  return null;
}

// Default landing page per role. Admin is checked first so admin-sellers land in admin.
function resolveLandingRoute(capabilities: AuthCapabilities): string {
  if (capabilities.isAdmin) {
    return '/admin/users';
  }
  if (capabilities.isVerifiedSeller) {
    return '/seller/products';
  }
  if (capabilities.needsSellerVerification) {
    return '/seller/verification';
  }
  return '/products';
}

function validateLoginForm(email: string, password: string): string | null {
  if (!email) {
    return 'Email is required.';
  }

  if (!emailPattern.test(email)) {
    return 'Enter a valid email address.';
  }

  if (!password) {
    return 'Password is required.';
  }

  return null;
}
