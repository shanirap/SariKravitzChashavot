import { Routes, Route, Link, Navigate, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth/AuthContext';
import Login from './pages/Login';
import Employers from './pages/Employers';
import EmployerDetails from './pages/EmployerDetails';
import EmployeeDetails from './pages/EmployeeDetails';
import AddEmployee from './pages/AddEmployee';
import ImportEmployees from './pages/ImportEmployees';
import ImportEmployers from './pages/ImportEmployers';
import EmployerInstitutionSymbols from './pages/EmployerInstitutionSymbols';
import EmployerActions from './pages/EmployerActions';

/**
 * Full authenticated shell: navbar, logout, nested routes matched against the same browser location.
 */
function ProtectedShell() {
  const { isAuthenticated, auth, logout } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return (
      <Navigate to="/login" replace state={{ from: location }} />
    );
  }

  return (
    <>
      <nav className="navbar navbar-expand-lg navbar-dark navbar-dark-custom">
        <div className="container-fluid px-4">
          <Link className="navbar-brand d-flex align-items-center gap-2" to="/">
            <span style={{ background: '#fff', borderRadius: 12, padding: '8px 20px', display: 'inline-flex', alignItems: 'center' }}>
              <img
                src="/logo.png"
                alt="שרי קרביץ"
                style={{ height: 60, objectFit: 'contain' }}
              />
            </span>
          </Link>
          <div className="navbar-nav ms-auto align-items-center gap-2 flex-row">
            <span className="text-white-50 small d-none d-md-inline">
              {auth?.username}
              {auth?.role ? ` · ${auth.role}` : ''}
            </span>
            <Link className="nav-link" to="/">
              <i className="bi bi-building me-1"></i>מעסיקים
            </Link>
            <button
              type="button"
              className="btn btn-sm btn-outline-light"
              onClick={logout}
            >
              התנתק
            </button>
          </div>
        </div>
      </nav>

      <div className="main-container">
        <Routes>
          <Route path="/" element={<Employers />} />
          <Route path="/employers/:id" element={<EmployerDetails />} />
          <Route path="/employers/:id/actions" element={<EmployerActions />} />
          <Route
            path="/employers/:id/institution-symbols"
            element={<EmployerInstitutionSymbols />}
          />
          <Route
            path="/employees/:employeeId/:employerId"
            element={<EmployeeDetails />}
          />
          <Route
            path="/employers/:employerId/add-employee"
            element={<AddEmployee />}
          />
          <Route
            path="/employers/:employerId/import-employees"
            element={<ImportEmployees />}
          />
          <Route path="/import/employees" element={<ImportEmployees />} />
          <Route path="/import/employers" element={<ImportEmployers />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="*" element={<ProtectedShell />} />
    </Routes>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
