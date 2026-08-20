import axios, { AxiosError } from 'axios';
import type { ApiResult } from './types';
import { tokenStorage, UNAUTHORIZED_EVENT } from './tokenStorage';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.getToken();
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

const AUTH_ENDPOINTS = ['/auth/login', '/auth/register'];

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiResult<unknown>>) => {
    const isAuthEndpoint = AUTH_ENDPOINTS.includes(error.config?.url ?? '');
    if (error.response?.status === 401 && !isAuthEndpoint) {
      tokenStorage.clear();
      window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    }
    return Promise.reject(error);
  },
);

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError<ApiResult<unknown>>(error)) {
    const messages = error.response?.data?.messages;
    if (messages && messages.length > 0) return messages.join(' ');
    if (error.message) return error.message;
  }
  if (error instanceof Error) return error.message;
  return 'Something went wrong. Please try again.';
}

export function isSessionExpired(error: unknown): boolean {
  return axios.isAxiosError<ApiResult<unknown>>(error) && error.response?.status === 401;
}
