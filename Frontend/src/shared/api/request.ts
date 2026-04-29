import { getStoredAccessToken } from '../../features/auth/authStorage';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export class ApiError extends Error {
  public readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

interface ValidationProblemDetails {
  title?: string;
  detail?: string;
  error?: string;
  errors?: Record<string, string[]>;
}

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  const accessToken = getStoredAccessToken();

  if (accessToken && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}`;

    try {
      const body = (await response.json()) as ValidationProblemDetails;
      message = getErrorMessage(body, message);
    } catch {
      // Keep the status-based message when the API does not return JSON.
    }

    throw new ApiError(message, response.status);
  }

  return response.json() as Promise<T>;
}

function getErrorMessage(body: ValidationProblemDetails, fallback: string) {
  if (body.error) {
    return body.error;
  }

  if (body.errors) {
    const validationMessages = Object.entries(body.errors)
      .flatMap(([field, messages]) => messages.map((message) => `${field}: ${message}`))
      .join(' ');

    if (validationMessages) {
      return validationMessages;
    }
  }

  return body.detail ?? body.title ?? fallback;
}
