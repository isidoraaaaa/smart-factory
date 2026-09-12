import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import type { ChocolateMachine } from "../../types/telemetry";
import "./MachinesList.css";

const API_BASE_URL = "https://localhost:7279/api";

export function MachinesList() {
  const { token, role, logout } = useAuth();
  const navigate = useNavigate();

  const [machines, setMachines] = useState<ChocolateMachine[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [newMachineName, setNewMachineName] = useState("");
  const [error, setError] = useState<string | null>(null);

  const isAdmin = role === "Admin";

  const fetchMachines = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/machines`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data: ChocolateMachine[] = await response.json();
      setMachines(data);
    } catch (err) {
      console.error("Failed to load machines:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchMachines();
  }, [token]);

  const handleAddMachine = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!newMachineName.trim()) {
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/machines`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ name: newMachineName }),
      });

      if (!response.ok) {
        throw new Error("Failed to add machine.");
      }

      setNewMachineName("");
      await fetchMachines();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    }
  };

  const handleDeleteMachine = async (id: string) => {
    try {
      const response = await fetch(`${API_BASE_URL}/machines/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!response.ok) {
        throw new Error("Failed to delete machine.");
      }

      await fetchMachines();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    }
  };

  return (
    <div className="machines-list-container">
      <div className="machines-list-header">
        <h1>Machines</h1>
        <button className="logout-button" onClick={logout}>
          Logout
        </button>
      </div>

      {isAdmin && (
        <form className="add-machine-form" onSubmit={handleAddMachine}>
          <input
            type="text"
            placeholder="New machine name"
            value={newMachineName}
            onChange={(e) => setNewMachineName(e.target.value)}
          />
          <button type="submit">Add Machine</button>
        </form>
      )}

      {error && <p className="machines-list-error">{error}</p>}

      {isLoading ? (
        <p>Loading machines...</p>
      ) : machines.length === 0 ? (
        <p>No machines were found.</p>
      ) : (
        <ul className="machines-list">
          {machines.map((machine) => (
            <li key={machine.id} className="machine-item">
              <div
                className="machine-item-info"
                onClick={() => navigate(`/machines/${machine.id}`)}
              >
                <span className="machine-item-name">{machine.name}</span>
                <span
                  className={`machine-item-status status-${machine.status.toLowerCase()}`}
                >
                  {machine.status}
                </span>
              </div>

              {isAdmin && (
                <button
                  className="delete-machine-button"
                  onClick={() => handleDeleteMachine(machine.id)}
                >
                  Delete
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
