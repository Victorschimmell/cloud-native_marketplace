import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { FormNotice, TextField } from '../../../shared/forms';
import { useAuth } from '../AuthContext';
import { AuthPageFrame } from '../components/AuthPageFrame';
import './AuthPages.css';

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const wasRegistered = searchParams.get('registered') === '1';

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedEmail = email.trim();
    const validationError = validateLoginForm(trimmedEmail, password);
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await login({ email: trimmedEmail, password });
      navigate('/products');
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

          <TextField autoComplete="email" label="Email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />

          <TextField
            autoComplete="current-password"
            label="Password"
            onChange={(event) => setPassword(event.target.value)}
            required
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
