export function StatusBadge({ isAvailable }: { isAvailable: boolean }) {
  return (
    <span className={`badge-soft ${isAvailable ? 'badge-soft-success' : 'badge-soft-danger'}`}>
      <i className={`bi ${isAvailable ? 'bi-check-circle-fill' : 'bi-x-circle-fill'}`} />
      {isAvailable ? 'Available' : 'Taken'}
    </span>
  );
}
