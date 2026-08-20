export interface ApiResult<T> {
  success: boolean;
  statusCode: number;
  messages: string[];
  data: T;
}

export interface DomainInfo {
  id: number;
  name: string;
  isAvailable: boolean;
  lastCheckedAt: string;
  expirationDate: string | null;
}

export interface FavoriteDomain {
  id: number;
  domainName: string;
  isAvailable: boolean;
  lastCheckedAt: string;
  expirationDate: string | null;
  createdAt: string;
}

export interface AuthCredentials {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  expiresAtUtc: string;
}
