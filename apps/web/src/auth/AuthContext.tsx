import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { authApi } from '../api/authApi';
import { tokenStorage, UNAUTHORIZED_EVENT } from '../api/tokenStorage';
import type { AuthCredentials } from '../api/types';
import { useToast } from '../toast/ToastContext';

interface AuthContextValue {
  username: string | null;
  isAuthenticated: boolean;
  login: (credentials: AuthCredentials) => Promise<void>;
  register: (credentials: AuthCredentials) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [username, setUsername] = useState<string | null>(() => tokenStorage.getSession()?.username ?? null);
  const { showToast } = useToast();

  useEffect(() => {
    const handleUnauthorized = () => {
      setUsername(null);
      showToast('Your session has expired. Please log in again.', 'warning');
    };
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
  }, [showToast]);

  const login = useCallback(async (credentials: AuthCredentials) => {
    const response = await authApi.login(credentials);
    tokenStorage.set(response);
    setUsername(response.username);
  }, []);

  const register = useCallback(async (credentials: AuthCredentials) => {
    const response = await authApi.register(credentials);
    tokenStorage.set(response);
    setUsername(response.username);
  }, []);

  const logout = useCallback(() => {
    tokenStorage.clear();
    setUsername(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ username, isAuthenticated: username !== null, login, register, logout }),
    [username, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
