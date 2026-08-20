import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';

export type ToastVariant = 'success' | 'danger' | 'warning' | 'info';

interface ToastItem {
  id: number;
  message: string;
  variant: ToastVariant;
}

interface ToastContextValue {
  showToast: (message: string, variant?: ToastVariant) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

const AUTO_DISMISS_MS = 4500;

const VARIANT_META: Record<ToastVariant, { icon: string; label: string }> = {
  success: { icon: 'bi-check-circle-fill', label: 'Success' },
  danger: { icon: 'bi-exclamation-triangle-fill', label: 'Error' },
  warning: { icon: 'bi-exclamation-circle-fill', label: 'Warning' },
  info: { icon: 'bi-info-circle-fill', label: 'Info' },
};

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const nextId = useRef(0);

  const dismissToast = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const showToast = useCallback(
    (message: string, variant: ToastVariant = 'success') => {
      const id = nextId.current++;
      setToasts((prev) => [...prev, { id, message, variant }]);
      window.setTimeout(() => dismissToast(id), AUTO_DISMISS_MS);
    },
    [dismissToast],
  );

  const value = useMemo<ToastContextValue>(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-viewport position-fixed top-0 end-0 p-3" style={{ zIndex: 1090 }}>
        {toasts.map((toast) => {
          const meta = VARIANT_META[toast.variant];
          return (
            <div
              key={toast.id}
              className={`toast app-toast app-toast-${toast.variant} show align-items-center border-0 mb-2`}
              role="alert"
              aria-live="assertive"
              aria-atomic="true"
            >
              <div className="d-flex">
                <div className="toast-body d-flex align-items-center gap-2">
                  <i className={`bi ${meta.icon}`} aria-hidden="true" />
                  <span>{toast.message}</span>
                </div>
                <button
                  type="button"
                  className="btn-close btn-close-white me-2 m-auto"
                  aria-label="Close"
                  onClick={() => dismissToast(toast.id)}
                />
              </div>
            </div>
          );
        })}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within a ToastProvider');
  return ctx;
}
