import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { useAuth } from '../AuthContext';
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
      await login({ email, password });
      navigate('/products');
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Could not log in right now.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PageSkeleton summary="Access your account and continue shopping with your saved cart." title="Log in">
      <div className="auth-page">
        <form className="auth-form" onSubmit={submit}>
          {wasRegistered ? (
            <p className="auth-form__notice auth-form__notice--success" role="status">
              Account created. Log in to continue.
            </p>
          ) : null}

          {error ? (
            <p className="auth-form__notice auth-form__notice--error" role="alert">
              {error}
            </p>
          ) : null}

          <label>
            Email
            <input autoComplete="email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />
          </label>

          <label>
            Password
            <input autoComplete="current-password" onChange={(event) => setPassword(event.target.value)} required type="password" value={password} />
          </label>

          <button disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Logging in...' : 'Log in'}
          </button>

          <p className="auth-form__footer">
            New here? <Link to="/register">Create an account</Link>
          </p>
        </form>
      </div>
    </PageSkeleton>
  );
}
