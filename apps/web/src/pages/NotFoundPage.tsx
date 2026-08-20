import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="d-flex flex-column align-items-center justify-content-center text-center py-5">
      <span className="brand-mark mb-3" style={{ width: '3.5rem', height: '3.5rem', fontSize: '1.75rem' }}>
        <i className="bi bi-signpost-split" />
      </span>
      <h3 className="fw-bold">Page not found</h3>
      <p className="text-muted">The page you are looking for does not exist.</p>
      <Link to="/" className="btn btn-primary rounded-pill px-4">
        Back to Home
      </Link>
    </div>
  );
}
