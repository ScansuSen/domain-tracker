import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getErrorMessage } from '../api/client';
import { useToast } from '../toast/ToastContext';
import { ErrorAlert } from '../components/ErrorAlert';

export function RegisterPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { register } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }
    if (password.length < 3) {
      setError('Password must be at least 3 characters.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const trimmedUsername = username.trim();
      await register({ username: trimmedUsername, password });
      showToast(`Account created. Welcome, ${trimmedUsername}!`, 'success');
      navigate('/', { replace: true });
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
            <p className="text-muted mb-0">Create an account to save favorite domains.</p>
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
                minLength={3}
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
                minLength={3}
              />
            </div>
            <div className="mb-3">
              <label htmlFor="confirmPassword" className="form-label">
                Confirm password
              </label>
              <input
                id="confirmPassword"
                type="password"
                className="form-control"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Creating account…' : 'Register'}
            </button>
          </form>

          <p className="text-center text-muted mt-3 mb-0">
            Already have an account? <Link to="/login">Log in</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
