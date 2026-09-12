import { Routes, Route, Navigate } from "react-router-dom";
import { LoginForm } from "./components/Login/LoginForm";
import { Dashboard } from "./components/Dashboard/Dashboard";
import { useAuth } from "./context/useAuth";
import { MachinesList } from "./components/MachinesList/MachinesList";

function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  return (
    <Routes>
      <Route path="/" element={<MachinesList />} />
      <Route path="/machines/:machineId" element={<Dashboard />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
