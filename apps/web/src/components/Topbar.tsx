import { Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../toast/ToastContext';

interface Props {
  title: string;
  onToggleSidebar: () => void;
}

export function Topbar({ title, onToggleSidebar }: Props) {
  const { username, isAuthenticated, logout } = useAuth();
  const { showToast } = useToast();

  const handleLogout = () => {
    logout();
    showToast('You have been logged out.', 'info');
  };

  return (
    <header className="app-topbar d-flex align-items-center justify-content-between px-3 px-md-4">
      <div className="d-flex align-items-center gap-2 gap-md-3">
        <button
          type="button"
          className="btn btn-link text-dark p-0 d-md-none"
          aria-label="Toggle navigation"
          onClick={onToggleSidebar}
        >
          <i className="bi bi-list fs-2" />
        </button>
        <span className="brand-mark d-none d-sm-inline-flex">
          <i className="bi bi-globe-americas" />
        </span>
        <div className="d-flex flex-column lh-sm">
          <span className="fw-bold">Domain Tracker</span>
          <span className="text-muted small d-none d-sm-inline">{title}</span>
        </div>
      </div>

      {isAuthenticated ? (
        <div className="d-flex align-items-center gap-2 gap-md-3">
          <span className="d-none d-sm-flex align-items-center gap-2 text-muted">
            <span className="avatar-circle">{(username ?? '?').charAt(0).toUpperCase()}</span>
            {username}
          </span>
          <button type="button" className="btn btn-sm btn-outline-secondary rounded-pill" onClick={handleLogout}>
            <i className="bi bi-box-arrow-right me-1" />
            Log out
          </button>
        </div>
      ) : (
        <Link to="/login" className="btn btn-sm btn-primary rounded-pill px-3">
          Log in
        </Link>
      )}
    </header>
  );
}
