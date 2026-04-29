import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { useAuth } from '../AuthContext';
import { AccountTypeSelector } from '../components/AccountTypeSelector';
import { AuthField, AuthTextArea } from '../components/AuthField';
import { AuthNotice } from '../components/AuthNotice';
import { AuthPageFrame } from '../components/AuthPageFrame';
import type { AccountType } from '../types';
import './AuthPages.css';

const phonePattern = String.raw`\+?[0-9][0-9\s().-]{6,24}(?:\s?(?:x|ext\.?)\s?[0-9]{1,6})?`;
const phoneValidationMessage = 'Use a valid phone number, for example +4512345678.';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [accountType, setAccountType] = useState<AccountType>('customer');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [businessName, setBusinessName] = useState('');
  const [registrationNumber, setRegistrationNumber] = useState('');
  const [payoutInformation, setPayoutInformation] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setIsSubmitting(true);
      setError(null);

      if (accountType === 'customer') {
        await register({
          accountType,
          email: email.trim(),
          password,
          firstName: firstName.trim(),
          lastName: lastName.trim(),
          phone: phone.trim(),
        });
      } else {
        await register({
          accountType,
          email: email.trim(),
          password,
          businessName: businessName.trim(),
          registrationNumber: registrationNumber.trim(),
          payoutInformation: payoutInformation.trim(),
        });
      }

      navigate('/login?registered=1');
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Could not create the account right now.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PageSkeleton summary="Choose the account type that matches how you want to use the marketplace." title="Create account">
      <AuthPageFrame variant="register">
        <form className="auth-form" onSubmit={submit}>
          <div className="auth-form__header">
            <span className="auth-form__eyebrow">Marketplace access</span>
            <h2>Choose your profile</h2>
          </div>

          {error ? <AuthNotice variant="error">{error}</AuthNotice> : null}

          <AccountTypeSelector onChange={setAccountType} value={accountType} />

          <AuthField autoComplete="email" label="Email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />

          <AuthField
            autoComplete="new-password"
            label="Password"
            minLength={8}
            onChange={(event) => setPassword(event.target.value)}
            required
            type="password"
            value={password}
          />

          {accountType === 'customer' ? (
            <>
              <AuthField autoComplete="given-name" label="First name" onChange={(event) => setFirstName(event.target.value)} required value={firstName} />
              <AuthField autoComplete="family-name" label="Last name" onChange={(event) => setLastName(event.target.value)} required value={lastName} />
              <AuthField
                autoComplete="tel"
                inputMode="tel"
                label="Phone"
                onChange={(event) => setPhone(event.target.value)}
                pattern={phonePattern}
                placeholder="+4512345678"
                required
                title={phoneValidationMessage}
                type="tel"
                value={phone}
              />
            </>
          ) : (
            <>
              <AuthField autoComplete="organization" label="Business name" onChange={(event) => setBusinessName(event.target.value)} required value={businessName} />
              <AuthField label="Registration number" onChange={(event) => setRegistrationNumber(event.target.value)} required value={registrationNumber} />
              <AuthTextArea label="Payout information" onChange={(event) => setPayoutInformation(event.target.value)} required rows={3} value={payoutInformation} />
            </>
          )}

          <button disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Creating account...' : 'Create account'}
          </button>

          <p className="auth-form__footer">
            Already registered? <Link to="/login">Log in</Link>
          </p>
        </form>
      </AuthPageFrame>
    </PageSkeleton>
  );
}
