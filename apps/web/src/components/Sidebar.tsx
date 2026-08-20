import { NavLink } from 'react-router-dom';

interface Props {
  open: boolean;
  onNavigate: () => void;
}

export function Sidebar({ open, onNavigate }: Props) {
  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `sidebar-link ${isActive ? 'active' : ''}`;

  return (
    <>
      {open && <div className="sidebar-backdrop d-md-none" onClick={onNavigate} />}
      <nav className={`app-sidebar d-flex flex-column gap-1 p-3 ${open ? 'open' : ''}`}>
        <div className="sidebar-section-label mb-2">Menu</div>
        <NavLink to="/" end className={linkClass} onClick={onNavigate}>
          <i className="bi bi-house-door-fill" />
          Home
        </NavLink>
        <NavLink to="/favorites" className={linkClass} onClick={onNavigate}>
          <i className="bi bi-star-fill" />
          Favorites
        </NavLink>
      </nav>
    </>
  );
}
