export function ErrorAlert({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <div className="alert alert-danger d-flex align-items-center gap-2 mb-3" role="alert">
      <i className="bi bi-exclamation-triangle-fill fs-5" />
      <span>{message}</span>
    </div>
  );
}
