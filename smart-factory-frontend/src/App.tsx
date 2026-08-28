import { LoginForm } from "./components/login/LoginForm";
import { Dashboard } from "./components/dashboard/Dashboard";
import { useAuth } from "./context/useAuth";

function App() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginForm />;
  }

  return <Dashboard />;
}

export default App;
