import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { useAuth } from '../AuthContext';
import { AuthField } from '../components/AuthField';
import { AuthNotice } from '../components/AuthNotice';
import { AuthPageFrame } from '../components/AuthPageFrame';
import './AuthPages.css';

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

    try {
      setIsSubmitting(true);
      setError(null);
      await login({ email: email.trim(), password });
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
        <form className="auth-form" onSubmit={submit}>
          <div className="auth-form__header">
            <span className="auth-form__eyebrow">Welcome back</span>
            <h2>Sign in</h2>
          </div>

          {wasRegistered ? <AuthNotice variant="success">Account created. Log in to continue.</AuthNotice> : null}

          {error ? <AuthNotice variant="error">{error}</AuthNotice> : null}

          <AuthField autoComplete="email" label="Email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />

          <AuthField
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
