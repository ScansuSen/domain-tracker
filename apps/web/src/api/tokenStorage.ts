import type { AuthResponse } from './types';

const SESSION_KEY = 'domain-tracker.session';

interface StoredSession {
  token: string;
  username: string;
  expiresAtUtc: string;
}

export const tokenStorage = {
  getSession(): StoredSession | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    try {
      const session = JSON.parse(raw) as StoredSession;
      if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
        localStorage.removeItem(SESSION_KEY);
        return null;
      }
      return session;
    } catch {
      return null;
    }
  },
  getToken(): string | null {
    return tokenStorage.getSession()?.token ?? null;
  },
  set(auth: AuthResponse): void {
    const session: StoredSession = auth;
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  },
  clear(): void {
    localStorage.removeItem(SESSION_KEY);
  },
};

export const UNAUTHORIZED_EVENT = 'domain-tracker:unauthorized';
