import { useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export default function Login() {
  const { login, isAuthenticated } = useAuth();
  const location = useLocation();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  /** @param {React.FormEvent} e */
  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      await login(username, password);
    } catch (err) {
      const status = err?.response?.status;
      if (status === 401) {
        setError('שם משתמש או סיסמה שגויים, או של המשתמש אינו פעיל.');
      } else {
        setError('אירעה שגיאה בהתחברות. נסו שוב.');
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="d-flex min-vh-100 align-items-center justify-content-center px-3" style={{ background: 'var(--brand-light)' }}>
      <div className="card shadow" style={{ maxWidth: 420, width: '100%' }}>
        <div className="card-header text-center">
          כניסה למערכת
        </div>
        <div className="card-body p-4">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label" htmlFor="loginUsername">שם משתמש</label>
              <input
                id="loginUsername"
                type="text"
                className="form-control"
                autoComplete="username"
                value={username}
                onChange={(ev) => setUsername(ev.target.value)}
                required
                disabled={submitting}
              />
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="loginPassword">סיסמה</label>
              <input
                id="loginPassword"
                type="password"
                className="form-control"
                autoComplete="current-password"
                value={password}
                onChange={(ev) => setPassword(ev.target.value)}
                required
                disabled={submitting}
              />
            </div>
            {error && (
              <div className="alert alert-danger py-2" role="alert">
                {error}
              </div>
            )}
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'מתחבר…' : 'התחברות'}
            </button>
          </form>
          {location?.state?.from?.pathname ? (
            <p className="small text-muted text-center mb-0 mt-3">
              נדרשת התחברות כדי להמשיך
            </p>
          ) : null}
        </div>
      </div>
    </div>
  );
}
