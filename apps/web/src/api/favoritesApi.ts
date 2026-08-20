import { apiClient } from './client';
import type { ApiResult, FavoriteDomain } from './types';

export const favoritesApi = {
  async list(): Promise<FavoriteDomain[]> {
    const { data } = await apiClient.get<ApiResult<FavoriteDomain[]>>('/favorites');
    return data.data;
  },
  async add(domainName: string): Promise<FavoriteDomain> {
    const { data } = await apiClient.post<ApiResult<FavoriteDomain>>('/favorites', { domainName });
    return data.data;
  },
  async remove(id: number): Promise<void> {
    await apiClient.delete<ApiResult<null>>(`/favorites/${id}`);
  },
  async refresh(id: number): Promise<FavoriteDomain> {
    const { data } = await apiClient.post<ApiResult<FavoriteDomain>>(`/favorites/${id}/refresh`);
    return data.data;
  },
};
