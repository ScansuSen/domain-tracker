import { apiClient } from './client';
import type { ApiResult, AuthCredentials, AuthResponse } from './types';

export const authApi = {
  async register(credentials: AuthCredentials): Promise<AuthResponse> {
    const { data } = await apiClient.post<ApiResult<AuthResponse>>('/auth/register', credentials);
    return data.data;
  },
  async login(credentials: AuthCredentials): Promise<AuthResponse> {
    const { data } = await apiClient.post<ApiResult<AuthResponse>>('/auth/login', credentials);
    return data.data;
  },
};
