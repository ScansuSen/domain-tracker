import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { favoritesApi } from '../api/favoritesApi';
import { getErrorMessage, isSessionExpired } from '../api/client';
import { FavoriteCard } from '../components/FavoriteCard';
import { ErrorAlert } from '../components/ErrorAlert';
import { useToast } from '../toast/ToastContext';
import type { FavoriteDomain } from '../api/types';

export function FavoritesPage() {
  const [favorites, setFavorites] = useState<FavoriteDomain[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToast();

  useEffect(() => {
    let cancelled = false;
    favoritesApi
      .list()
      .then((data) => {
        if (!cancelled) setFavorites(data);
      })
      .catch((err) => {
        if (!cancelled && !isSessionExpired(err)) setError(getErrorMessage(err));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const handleRefresh = async (id: number) => {
    try {
      const updated = await favoritesApi.refresh(id);
      setFavorites((prev) => prev.map((f) => (f.id === id ? updated : f)));
      showToast(`${updated.domainName} was refreshed.`, 'success');
    } catch (err) {
      if (!isSessionExpired(err)) showToast(getErrorMessage(err), 'danger');
    }
  };

  const handleDelete = async (id: number) => {
    const target = favorites.find((f) => f.id === id);
    try {
      await favoritesApi.remove(id);
      setFavorites((prev) => prev.filter((f) => f.id !== id));
      showToast(`${target?.domainName ?? 'Domain'} removed from favorites.`, 'success');
    } catch (err) {
      if (!isSessionExpired(err)) showToast(getErrorMessage(err), 'danger');
    }
  };

  if (loading) {
    return (
      <div className="d-flex align-items-center gap-2 text-muted">
        <div className="spinner-border spinner-border-sm" role="status" aria-hidden="true" />
        Loading favorites…
      </div>
    );
  }

  return (
    <div>
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h5 className="mb-0 fw-bold">
          <i className="bi bi-star-fill text-primary me-2" />
          Your favorites
        </h5>
        {favorites.length > 0 && (
          <span className="text-muted small">
            {favorites.length} domain{favorites.length === 1 ? '' : 's'}
          </span>
        )}
      </div>

      <ErrorAlert message={error} />

      {favorites.length === 0 ? (
        <div className="text-center text-muted py-5">
          <i className="bi bi-star fs-1 d-block mb-2 opacity-50" />
          No favorites yet.{' '}
          <Link to="/">Search for a domain</Link> and add it to your list.
        </div>
      ) : (
        <div className="row row-cols-1 row-cols-md-2 g-3">
          {favorites.map((favorite) => (
            <div className="col" key={favorite.id}>
              <FavoriteCard favorite={favorite} onRefresh={handleRefresh} onDelete={handleDelete} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
