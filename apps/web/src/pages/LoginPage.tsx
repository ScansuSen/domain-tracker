import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getErrorMessage } from '../api/client';
import { useToast } from '../toast/ToastContext';
import { ErrorAlert } from '../components/ErrorAlert';

interface LocationState {
  from?: { pathname: string };
}

export function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { login } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as LocationState | null)?.from?.pathname ?? '/';

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const trimmedUsername = username.trim();
      await login({ username: trimmedUsername, password });
      showToast(`Welcome back, ${trimmedUsername}!`, 'success');
      navigate(from, { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-screen d-flex align-items-center justify-content-center">
      <div className="card auth-card shadow-lg" style={{ width: 380 }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column align-items-center text-center mb-3">
            <span className="brand-mark mb-2" style={{ width: '3rem', height: '3rem', fontSize: '1.5rem' }}>
              <i className="bi bi-globe-americas" />
            </span>
            <h4 className="card-title mb-1">Domain Tracker</h4>
            <p className="text-muted mb-0">Log in to manage your favorite domains.</p>
          </div>

          <ErrorAlert message={error} />

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="username" className="form-label">
                Username
              </label>
              <input
                id="username"
                type="text"
                className="form-control"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                autoFocus
              />
            </div>
            <div className="mb-3">
              <label htmlFor="password" className="form-label">
                Password
              </label>
              <input
                id="password"
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Logging in…' : 'Log in'}
            </button>
          </form>

          <p className="text-center text-muted mt-3 mb-0">
            No account yet? <Link to="/register">Register</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
