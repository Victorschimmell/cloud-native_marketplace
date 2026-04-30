const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export class ApiError extends Error {
  status: number;
  payload: unknown;

  constructor(status: number, payload: unknown, message?: string) {
    super(message || `API Error: ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.payload = payload;
  }
}

export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, init);

  if (!response.ok) {
    let payload: unknown = null;
    try {
      payload = await response.json();
    } catch {
      payload = await response.text();
    }

    throw new ApiError(
      response.status,
      payload,
      `Request failed with status ${response.status}`
    );
  }

  return response.json() as Promise<T>;
}
