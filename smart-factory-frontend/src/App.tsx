import { Routes, Route, Navigate } from "react-router-dom";
import { LoginForm } from "./components/Login/LoginForm";
import { Dashboard } from "./components/Dashboard/Dashboard";
import { useAuth } from "./context/useAuth";
import { MachinesList } from "./components/MachinesList/MachinesList";
import { UsersList } from "./components/UsersList/UsersList";
import { UserDetails } from "./components/UserDetails/UserDetails";
import { Navbar } from "./components/Navbar/Navbar";
import "./App.css";

function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  return (
    <div className="app-layout">
      <Navbar />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<MachinesList />} />
          <Route path="/machines/:machineId" element={<Dashboard />} />
          <Route path="/users" element={<UsersList />} />
          <Route path="/users/:userId" element={<UserDetails />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
