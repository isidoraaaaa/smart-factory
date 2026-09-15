import { Users, Factory, LogOutIcon } from "lucide-react";
import { NavLink } from "react-router-dom";
import "./Navbar.css";
import { useAuth } from "../../context/useAuth";

export function Navbar() {
  const { logout, role } = useAuth();
  const isAdmin = role === "Admin";

  return (
    <nav className="sidebar">
      <NavLink
        to="/"
        end
        className={({ isActive }) => `nav-item${isActive ? " active" : ""}`}
        data-tooltip="Machines"
      >
        <Factory size={22} />
      </NavLink>

      {isAdmin && (
        <NavLink
          to="/users"
          className={({ isActive }) => `nav-item${isActive ? " active" : ""}`}
          data-tooltip="Users"
        >
          <Users size={22} />
        </NavLink>
      )}

      <button className="nav-item" onClick={logout} data-tooltip="Logout">
        <LogOutIcon size={22} />
      </button>
    </nav>
  );
}
