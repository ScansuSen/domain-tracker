import { formatDate } from '../utils/formatDate';
import { StatusBadge } from './StatusBadge';

export interface DisplayDomain {
  name: string;
  isAvailable: boolean;
  lastCheckedAt: string;
  expirationDate: string | null;
}

interface Props {
  domain: DisplayDomain | null;
  loading?: boolean;
}

export function DomainInfoPanel({ domain, loading }: Props) {
  if (loading) {
    return (
      <div className="domain-info-panel d-flex align-items-center justify-content-center text-muted">
        <div className="spinner-border me-2" role="status" aria-hidden="true" />
        Checking…
      </div>
    );
  }

  if (!domain) {
    return (
      <div className="domain-info-panel d-flex flex-column align-items-center justify-content-center text-muted gap-2">
        <i className="bi bi-globe2 fs-1 opacity-50" />
        <span className="fs-5">Information</span>
      </div>
    );
  }

  return (
    <div className="domain-info-panel d-flex flex-column gap-3 p-4">
      <div className="d-flex flex-wrap align-items-center justify-content-between gap-2">
        <div className="d-flex align-items-center gap-2 text-break">
          <i className="bi bi-globe2 text-primary fs-4" aria-hidden="true" />
          <h4 className="mb-0 text-break">{domain.name}</h4>
        </div>
        <StatusBadge isAvailable={domain.isAvailable} />
      </div>
      <div className="row g-2">
        <div className="col-sm-6">
          <div className="stat-tile h-100">
            <div className="stat-label">Last checked</div>
            <div className="stat-value">{formatDate(domain.lastCheckedAt)}</div>
          </div>
        </div>
        <div className="col-sm-6">
          <div className="stat-tile h-100">
            <div className="stat-label">Expires</div>
            <div className="stat-value">
              {domain.isAvailable ? '—' : formatDate(domain.expirationDate)}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
