import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api';
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from './authStorage';

const AuthContext = createContext(null);

function isJwtExpired(expiresAtUtc) {
  if (!expiresAtUtc || typeof expiresAtUtc !== 'string') return false;
  const t = new Date(expiresAtUtc).getTime();
  if (!Number.isFinite(t)) return false;
  // Small clock skew cushion (milliseconds)
  return t <= Date.now() + 5_000;
}

export function AuthProvider({ children }) {
  const navigate = useNavigate();
  const [auth, setAuth] = useState(() => readStoredAuth());

  useEffect(() => {
    const stored = readStoredAuth();
    if (!stored) {
      setAuth(null);
      return;
    }
    if (isJwtExpired(stored.expiresAtUtc)) {
      clearStoredAuth();
      setAuth(null);
      return;
    }
    setAuth(stored);
  }, []);

  const logout = useCallback(() => {
    clearStoredAuth();
    setAuth(null);
    navigate('/login', { replace: true });
  }, [navigate]);

  /**
   * @param {string} username
   * @param {string} password
   */
  const login = useCallback(
    async (username, password) => {
      const trimmedUser = username?.trim?.() ?? '';
      const trimmedPass = password ?? '';
      const { data } = await api.post('/auth/login', {
        username: trimmedUser,
        password: trimmedPass,
      });

      const payload = {
        token: data.token,
        username: data.username ?? trimmedUser,
        role: data.role ?? '',
        expiresAtUtc: data.expiresAtUtc != null ? String(data.expiresAtUtc) : '',
      };
      writeStoredAuth(payload);
      setAuth(payload);
      navigate('/', { replace: true });
    },
    [navigate],
  );

  const isAuthenticated = !!(
    auth?.token &&
    !isJwtExpired(auth.expiresAtUtc)
  );

  const value = useMemo(
    () => ({
      auth,
      isAuthenticated,
      login,
      logout,
    }),
    [auth, isAuthenticated, login, logout],
  );

  return (
    <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx)
    throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
