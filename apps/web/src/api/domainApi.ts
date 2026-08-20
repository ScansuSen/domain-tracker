import { apiClient } from './client';
import type { ApiResult, DomainInfo } from './types';

export const domainApi = {
  async check(name: string): Promise<DomainInfo> {
    const { data } = await apiClient.get<ApiResult<DomainInfo>>('/domains/check', {
      params: { name },
    });
    return data.data;
  },
};
