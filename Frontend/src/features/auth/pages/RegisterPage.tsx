import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { useAuth } from '../AuthContext';
import './AuthPages.css';

type AccountType = 'customer' | 'seller';

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
      <div className="auth-page">
        <form className="auth-form" onSubmit={submit}>
          {error ? (
            <p className="auth-form__notice auth-form__notice--error" role="alert">
              {error}
            </p>
          ) : null}

          <fieldset className="auth-form__account-type">
            <legend>Account type</legend>
            <label>
              <input checked={accountType === 'customer'} name="accountType" onChange={() => setAccountType('customer')} type="radio" />
              Customer
            </label>
            <label>
              <input checked={accountType === 'seller'} name="accountType" onChange={() => setAccountType('seller')} type="radio" />
              Seller
            </label>
          </fieldset>

          <label>
            Email
            <input autoComplete="email" onChange={(event) => setEmail(event.target.value)} required type="email" value={email} />
          </label>

          <label>
            Password
            <input autoComplete="new-password" minLength={8} onChange={(event) => setPassword(event.target.value)} required type="password" value={password} />
          </label>

          {accountType === 'customer' ? (
            <>
              <label>
                First name
                <input autoComplete="given-name" onChange={(event) => setFirstName(event.target.value)} required value={firstName} />
              </label>
              <label>
                Last name
                <input autoComplete="family-name" onChange={(event) => setLastName(event.target.value)} required value={lastName} />
              </label>
              <label>
                Phone
                <input
                  autoComplete="tel"
                  inputMode="tel"
                  onChange={(event) => setPhone(event.target.value)}
                  pattern={phonePattern}
                  placeholder="+4512345678"
                  required
                  title={phoneValidationMessage}
                  type="tel"
                  value={phone}
                />
              </label>
            </>
          ) : (
            <>
              <label>
                Business name
                <input autoComplete="organization" onChange={(event) => setBusinessName(event.target.value)} required value={businessName} />
              </label>
              <label>
                Registration number
                <input onChange={(event) => setRegistrationNumber(event.target.value)} required value={registrationNumber} />
              </label>
              <label>
                Payout information
                <textarea onChange={(event) => setPayoutInformation(event.target.value)} required rows={3} value={payoutInformation} />
              </label>
            </>
          )}

          <button disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Creating account...' : 'Create account'}
          </button>

          <p className="auth-form__footer">
            Already registered? <Link to="/login">Log in</Link>
          </p>
        </form>
      </div>
    </PageSkeleton>
  );
}
