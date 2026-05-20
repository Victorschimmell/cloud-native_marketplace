import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageSkeleton from '../../../components/PageSkeleton';
import { ApiError } from '../../../shared/api/request';
import { FormNotice, TextField } from '../../../shared/forms';
import { useAuth } from '../useAuth';
import { AccountTypeSelector } from '../components/AccountTypeSelector';
import { AuthPageFrame } from '../components/AuthPageFrame';
import type { AccountType } from '../types';
import './AuthPages.css';

const phonePattern = String.raw`\+?[0-9]{7,25}`;
const digitsOnlyPattern = String.raw`\d*`;
const phoneValidationMessage = 'Use a valid phone number, for example +4512345678.';
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phoneRegex = new RegExp(`^${phonePattern}$`);
const digitsOnlyRegex = /^\d+$/;

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [accountType, setAccountType] = useState<AccountType>('customer');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [businessName, setBusinessName] = useState('');
  const [registrationNumber, setRegistrationNumber] = useState('');
  const [payoutInformation, setPayoutInformation] = useState('');
  const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setHasAttemptedSubmit(true);

    const formValues = {
      accountType,
      businessName: businessName.trim(),
      email: email.trim(),
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      password,
      confirmPassword,
      payoutInformation: normalizeDigitsInput(payoutInformation),
      phone: normalizePhoneInput(phone),
      registrationNumber: normalizeDigitsInput(registrationNumber),
    };
    const validationError = validateRegisterForm(formValues);
    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      if (accountType === 'customer') {
        await register({
          accountType,
          email: formValues.email,
          password,
          firstName: formValues.firstName,
          lastName: formValues.lastName,
          phone: formValues.phone,
        });
      } else {
        await register({
          accountType,
          email: formValues.email,
          password,
          businessName: formValues.businessName,
          registrationNumber: formValues.registrationNumber,
          payoutInformation: formValues.payoutInformation,
        });
      }

      navigate(accountType === 'customer' ? '/products' : '/login?registered=1');
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Could not create the account right now.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PageSkeleton summary="Choose the account type that matches how you want to use the marketplace." title="Create account">
      <AuthPageFrame variant="register">
        <form className="auth-form" noValidate onSubmit={submit}>
          <div className="auth-form__header">
            <span className="auth-form__eyebrow">Marketplace access</span>
            <h2>Choose your profile</h2>
          </div>

          {error ? <FormNotice variant="error">{error}</FormNotice> : null}

          <AccountTypeSelector onChange={setAccountType} value={accountType} />

          <TextField
            autoComplete="email"
            invalid={hasAttemptedSubmit && (!email.trim() || !emailPattern.test(email.trim()))}
            label="Email"
            onChange={(event) => setEmail(event.target.value)}
            type="email"
            value={email}
          />

          <TextField
            autoComplete="new-password"
            invalid={hasAttemptedSubmit && (!password || password.length < 8)}
            label="Password"
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            value={password}
          />
          <TextField
            autoComplete="new-password"
            invalid={hasAttemptedSubmit && (!confirmPassword || confirmPassword !== password)}
            label="Confirm password"
            onChange={(event) => setConfirmPassword(event.target.value)}
            type="password"
            value={confirmPassword}
          />

          {accountType === 'customer' ? (
            <>
              <TextField
                autoComplete="given-name"
                invalid={hasAttemptedSubmit && !firstName.trim()}
                label="First name"
                onChange={(event) => setFirstName(event.target.value)}
                value={firstName}
              />
              <TextField
                autoComplete="family-name"
                invalid={hasAttemptedSubmit && !lastName.trim()}
                label="Last name"
                onChange={(event) => setLastName(event.target.value)}
                value={lastName}
              />
              <TextField
                autoComplete="tel"
                inputMode="tel"
                invalid={hasAttemptedSubmit && (!phone.trim() || !phoneRegex.test(phone.trim()))}
                label="Phone"
                maxLength={26}
                onChange={(event) => setPhone(normalizePhoneInput(event.target.value))}
                pattern={phonePattern}
                placeholder="+4512345678"
                type="tel"
                value={phone}
              />
            </>
          ) : (
            <>
              <TextField
                autoComplete="organization"
                invalid={hasAttemptedSubmit && !businessName.trim()}
                label="Business name"
                onChange={(event) => setBusinessName(event.target.value)}
                value={businessName}
              />
              <TextField
                inputMode="numeric"
                invalid={hasAttemptedSubmit && !digitsOnlyRegex.test(registrationNumber)}
                label="Registration number"
                onChange={(event) => setRegistrationNumber(normalizeDigitsInput(event.target.value))}
                pattern={digitsOnlyPattern}
                value={registrationNumber}
              />
              <TextField
                inputMode="numeric"
                invalid={hasAttemptedSubmit && !digitsOnlyRegex.test(payoutInformation)}
                label="Payout information"
                onChange={(event) => setPayoutInformation(normalizeDigitsInput(event.target.value))}
                pattern={digitsOnlyPattern}
                value={payoutInformation}
              />
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

interface RegisterFormValues {
  accountType: AccountType;
  businessName: string;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  payoutInformation: string;
  phone: string;
  registrationNumber: string;
}

function normalizePhoneInput(value: string): string {
  const hasLeadingPlus = value.trimStart().startsWith('+');
  const digits = value.replace(/\D/g, '');

  return hasLeadingPlus ? `+${digits}` : digits;
}

function normalizeDigitsInput(value: string): string {
  return value.replace(/\D/g, '');
}

function validateRegisterForm(values: RegisterFormValues): string | null {
  if (!values.email) {
    return 'Email is required.';
  }

  if (!emailPattern.test(values.email)) {
    return 'Enter a valid email address.';
  }

  if (!values.password) {
    return 'Password is required.';
  }

  if (values.password.length < 8) {
    return 'Password must be at least 8 characters.';
  }

  if (!values.confirmPassword) {
    return 'Confirm password is required.';
  }

  if (values.confirmPassword !== values.password) {
    return 'Passwords must match.';
  }

  if (values.accountType === 'customer') {
    if (!values.firstName || !values.lastName) {
      return 'First name and last name are required.';
    }

    if (!values.phone) {
      return 'Phone is required.';
    }

    if (!phoneRegex.test(values.phone)) {
      return phoneValidationMessage;
    }

    return null;
  }

  if (!values.businessName) {
    return 'Business name is required.';
  }

  if (!values.registrationNumber) {
    return 'Registration number is required.';
  }

  if (!digitsOnlyRegex.test(values.registrationNumber)) {
    return 'Registration number can only contain numbers.';
  }

  if (!values.payoutInformation) {
    return 'Payout information is required.';
  }

  if (!digitsOnlyRegex.test(values.payoutInformation)) {
    return 'Payout information can only contain numbers.';
  }

  return null;
}
