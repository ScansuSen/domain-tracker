import { useState } from 'react';
import { ConfirmDialog } from './ConfirmDialog';
import { DomainInfoPanel } from './DomainInfoPanel';
import type { FavoriteDomain } from '../api/types';

interface Props {
  favorite: FavoriteDomain;
  onRefresh: (id: number) => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}

export function FavoriteCard({ favorite, onRefresh, onDelete }: Props) {
  const [refreshing, setRefreshing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [confirmingDelete, setConfirmingDelete] = useState(false);

  const handleRefresh = async () => {
    setRefreshing(true);
    try {
      await onRefresh(favorite.id);
    } finally {
      setRefreshing(false);
    }
  };

  const handleConfirmDelete = async () => {
    setDeleting(true);
    try {
      await onDelete(favorite.id);
    } finally {
      setDeleting(false);
      setConfirmingDelete(false);
    }
  };

  return (
    <div className="card favorite-card hover-lift shadow-soft h-100">
      <div className="card-header bg-white d-flex align-items-center justify-content-between">
        <span className="d-flex align-items-center gap-2 fw-semibold text-break">
          <i className="bi bi-bookmark-star-fill text-primary" aria-hidden="true" />
          {favorite.domainName}
        </span>
      </div>
      <div className="card-body p-0">
        <DomainInfoPanel
          domain={{
            name: favorite.domainName,
            isAvailable: favorite.isAvailable,
            lastCheckedAt: favorite.lastCheckedAt,
            expirationDate: favorite.expirationDate,
          }}
          loading={refreshing}
        />
      </div>
      <div className="card-footer bg-white d-flex justify-content-end gap-2">
        <button
          type="button"
          className="btn btn-outline-secondary btn-sm rounded-pill px-3"
          onClick={handleRefresh}
          disabled={refreshing || deleting}
        >
          <i className="bi bi-arrow-clockwise me-1" />
          Refresh
        </button>
        <button
          type="button"
          className="btn btn-outline-danger btn-sm rounded-pill px-3"
          onClick={() => setConfirmingDelete(true)}
          disabled={refreshing || deleting}
        >
          <i className="bi bi-trash me-1" />
          Delete
        </button>
      </div>

      <ConfirmDialog
        show={confirmingDelete}
        title="Remove favorite"
        message={
          <>
            Remove <strong>{favorite.domainName}</strong> from favorites?
          </>
        }
        confirmLabel="Delete"
        variant="danger"
        confirming={deleting}
        onConfirm={handleConfirmDelete}
        onCancel={() => setConfirmingDelete(false)}
      />
    </div>
  );
}
