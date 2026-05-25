/**
 * Persisted JWT session keys (localStorage).
 * Filename is client-only metadata; credentials must never appear in URLs.
 */

const PREFIX = 'acct_auth_';

const KEYS = {
  token: `${PREFIX}token`,
  username: `${PREFIX}username`,
  role: `${PREFIX}role`,
  expiresAtUtc: `${PREFIX}expiresAtUtc`,
};

/** @typedef {{ token: string, username: string, role: string, expiresAtUtc: string }} AuthPayload */

export function getToken() {
  return localStorage.getItem(KEYS.token);
}

/** @returns {AuthPayload | null} */
export function readStoredAuth() {
  const token = localStorage.getItem(KEYS.token);
  const username = localStorage.getItem(KEYS.username);
  const role = localStorage.getItem(KEYS.role);
  const expiresAtUtc = localStorage.getItem(KEYS.expiresAtUtc);
  if (!token || !username) return null;
  return {
    token,
    username,
    role: role ?? '',
    expiresAtUtc: expiresAtUtc ?? '',
  };
}

/** @param {AuthPayload} payload */
export function writeStoredAuth(payload) {
  localStorage.setItem(KEYS.token, payload.token);
  localStorage.setItem(KEYS.username, payload.username);
  localStorage.setItem(KEYS.role, payload.role ?? '');
  localStorage.setItem(KEYS.expiresAtUtc, payload.expiresAtUtc ?? '');
}

export function clearStoredAuth() {
  localStorage.removeItem(KEYS.token);
  localStorage.removeItem(KEYS.username);
  localStorage.removeItem(KEYS.role);
  localStorage.removeItem(KEYS.expiresAtUtc);
}
