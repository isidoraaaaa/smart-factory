import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import type { ChocolateMachine } from "../../types/telemetry";
import "./MachinesList.css";
import {
  addMachine,
  deleteMachine,
  getMachines,
} from "../../services/machinesService";
import * as signalR from "@microsoft/signalr";

const HUB_URL = "https://localhost:7279/hubs/telemetry";

export function MachinesList() {
  const { token, role } = useAuth();
  const navigate = useNavigate();

  const [machines, setMachines] = useState<ChocolateMachine[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [newMachineName, setNewMachineName] = useState("");
  const [error, setError] = useState<string | null>(null);

  const isAdmin = role === "Admin";

  const fetchMachines = useCallback(async () => {
    try {
      const data: ChocolateMachine[] = await getMachines(token);
      setMachines(data);
    } catch (err) {
      console.error("Failed to load machines:", err);
    }
  }, [token]);

  useEffect(() => {
    const loadMachines = async () => {
      try {
        await fetchMachines();
      } finally {
        setIsLoading(false);
      }
    };

    loadMachines();
  }, [fetchMachines]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token ?? "",
      })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveTelemetry", (data: ChocolateMachine) => {
      setMachines((prev) =>
        prev.map((m) => (m.id === data.id ? { ...m, ...data } : m)),
      );
    });

    connection
      .start()
      .catch((err) => console.error("SignalR connection error:", err));

    return () => {
      connection.stop();
    };
  }, [token]);

  const handleAddMachine = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      if (!newMachineName.trim()) {
        throw new Error("Machine name cannot be empty.");
      }
      await addMachine(token, newMachineName);
      setNewMachineName("");
      await fetchMachines();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    }
  };

  const handleDeleteMachine = async (id: string) => {
    try {
      await deleteMachine(token, id);
      await fetchMachines();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    }
  };

  return (
    <div className="machines-list-container">
      <div className="machines-list-header">
        <h1>Machines</h1>
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
        <p className="machines-list-info">Loading machines...</p>
      ) : machines.length === 0 ? (
        <p className="machines-list-info">No machines were found.</p>
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
