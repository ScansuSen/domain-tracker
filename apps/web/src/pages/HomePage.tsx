import { useState } from 'react';
import type { FormEvent } from 'react';
import { domainApi } from '../api/domainApi';
import { favoritesApi } from '../api/favoritesApi';
import { getErrorMessage, isSessionExpired } from '../api/client';
import { useToast } from '../toast/ToastContext';
import { DomainInfoPanel } from '../components/DomainInfoPanel';
import { ErrorAlert } from '../components/ErrorAlert';
import type { DomainInfo } from '../api/types';

const DOMAIN_PATTERN = /^([a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$/;

export function HomePage() {
  const [query, setQuery] = useState('');
  const [result, setResult] = useState<DomainInfo | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [adding, setAdding] = useState(false);
  const [added, setAdded] = useState(false);

  const { showToast } = useToast();

  const handleSearch = async (event: FormEvent) => {
    event.preventDefault();
    const name = query.trim().toLowerCase();

    if (!DOMAIN_PATTERN.test(name)) {
      setError('Please enter a valid domain name (e.g. example.com).');
      return;
    }

    setLoading(true);
    setError(null);
    setAdded(false);
    try {
      const domain = await domainApi.check(name);
      setResult(domain);
    } catch (err) {
      setResult(null);
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleAddFavorite = async () => {
    if (!result) return;
    setAdding(true);
    setError(null);
    try {
      await favoritesApi.add(result.name);
      setAdded(true);
      showToast(`${result.name} added to favorites.`, 'success');
    } catch (err) {
      if (!isSessionExpired(err)) showToast(getErrorMessage(err), 'danger');
    } finally {
      setAdding(false);
    }
  };

  return (
    <div className="mx-auto" style={{ maxWidth: 720 }}>
      <div className="text-center mb-4">
        <h2 className="fw-bold mb-1">Check a domain</h2>
        <p className="text-muted mb-0">Look up availability, WHOIS/RDAP details, and save it for later.</p>
      </div>

      <form
        className="card shadow-soft border-0 p-2 p-sm-3 mb-4"
        style={{ borderRadius: 'var(--bs-border-radius-lg)' }}
        onSubmit={handleSearch}
      >
        <div className="input-group input-group-lg">
          <span className="input-group-text bg-white border-0">
            <i className="bi bi-search text-muted" />
          </span>
          <input
            type="text"
            className="form-control border-0"
            placeholder="example.com"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            aria-label="Domain name"
            autoFocus
          />
          <button type="submit" className="btn btn-primary px-4" disabled={loading}>
            {loading ? (
              <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true" />
            ) : (
              'Search'
            )}
          </button>
        </div>
      </form>

      <ErrorAlert message={error} />

      <DomainInfoPanel domain={result} loading={loading} />

      {result && (
        <div className="d-flex justify-content-end mt-3">
          <button
            type="button"
            className="btn btn-outline-primary rounded-pill px-4"
            onClick={handleAddFavorite}
            disabled={adding || added}
          >
            <i className={`bi ${added ? 'bi-star-fill' : 'bi-star'} me-1`} />
            {added ? 'Added to favorites' : adding ? 'Adding…' : 'Add Favorite'}
          </button>
        </div>
      )}
    </div>
  );
}
