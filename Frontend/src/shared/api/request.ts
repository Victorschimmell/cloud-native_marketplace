import { getStoredAccessToken } from '../../features/auth/authStorage';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export class ApiError extends Error {
  public readonly status: number;
  public readonly payload: unknown;

  constructor(message: string, status: number, payload: unknown = null) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.payload = payload;
  }
}

interface ValidationProblemDetails {
  title?: string;
  detail?: string;
  error?: string;
  message?: string;
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
    const payload = await readErrorPayload(response);

    if (isValidationProblemDetails(payload)) {
      message = getErrorMessage(payload, message);
    } else if (typeof payload === 'string' && payload) {
      message = payload;
    }

    throw new ApiError(message, response.status, payload);
  }

  return response.json() as Promise<T>;
}

async function readErrorPayload(response: Response): Promise<unknown> {
  const text = await response.text();

  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

function isValidationProblemDetails(payload: unknown): payload is ValidationProblemDetails {
  return Boolean(payload) && typeof payload === 'object';
}

function getErrorMessage(body: ValidationProblemDetails, fallback: string) {
  if (body.error) {
    return body.error;
  }

  if (body.message) {
    return body.message;
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
