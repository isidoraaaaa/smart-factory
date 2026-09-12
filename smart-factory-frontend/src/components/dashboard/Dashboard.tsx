import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import "./Dashboard.css";
import type { ChocolateMachine, TelemetryReading } from "../../types/telemetry";
import { useAuth } from "../../context/useAuth";
import { useNavigate, useParams } from "react-router-dom";

const API_BASE_URL = "https://localhost:7279/api";
const HUB_URL = "https://localhost:7279/hubs/telemetry";
const MAX_LOG_ENTRIES = 5;

export function Dashboard() {
  const { machineId } = useParams<{ machineId: string }>();
  const navigate = useNavigate();
  const { token, logout } = useAuth();

  const [isConnected, setIsConnected] = useState(false);
  const [machine, setMachine] = useState<ChocolateMachine | null>(null);
  const [history, setHistory] = useState<ChocolateMachine[]>([]);
  const [isLoadingHistory, setIsLoadingHistory] = useState(true);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!machineId) return;

    const loadInitialData = async () => {
      try {
        const machinesResponse = await fetch(`${API_BASE_URL}/machines`, {
          headers: { Authorization: `Bearer ${token}` },
        });

        if (!machinesResponse.ok) {
          throw new Error("Failed to load machines");
        }
        const machines: ChocolateMachine[] = await machinesResponse.json();

        if (machines.length === 0) {
          setIsLoadingHistory(false);
          return;
        }

        const currentMachine = machines.find((m) => m.id === machineId);

        if (!currentMachine) {
          navigate("/");
          return;
        }

        setMachine(currentMachine);

        const historyResponse = await fetch(
          `${API_BASE_URL}/machines/${machineId}/history?take=${MAX_LOG_ENTRIES}`,
          { headers: { Authorization: `Bearer ${token}` } },
        );
        const readings: TelemetryReading[] = await historyResponse.json();

        // Mapiraj TelemetryReading u ChocolateMachine oblik da odgovara history state-u
        const mappedHistory: ChocolateMachine[] = readings.map((r) => ({
          id: r.id,
          name: currentMachine.name,
          lastTemperature: r.temperature,
          status: r.status,
          timestamp: r.timestamp,
        }));

        setHistory(mappedHistory);
      } catch (err) {
        console.error("Failed to load initial history:", err);
      } finally {
        setIsLoadingHistory(false);
      }
    };

    loadInitialData();
  }, [token, navigate, machineId]);

  useEffect(() => {
    if (isLoadingHistory || !machineId) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token ?? "",
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.on("ReceiveTelemetry", (data: ChocolateMachine) => {
      if (data.id !== machineId) return;
      setMachine(data);
      setHistory((prev) => [data, ...prev].slice(0, MAX_LOG_ENTRIES));
    });

    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));
    connection.onclose(() => setIsConnected(false));

    connection
      .start()
      .then(() => setIsConnected(true))
      .catch((err) => {
        console.error("SignalR connection error:", err);
        setIsConnected(false);
      });

    return () => {
      connection.stop();
    };
  }, [isLoadingHistory, token, machineId]);

  const isWarning = machine?.status === "Warning";

  return (
    <div className="app-container">
      <div className="dashboard-header">
        <button className="back-button" onClick={() => navigate("/")}>
          ← Back to Machines
        </button>
        <div className="connection-indicator">
          <span
            className={`status-dot ${isConnected ? "connected" : "disconnected"}`}
          />
          <span>{isConnected ? "Connected" : "Disconnected"}</span>
        </div>
        <button className="logout-button" onClick={logout}>
          Logout
        </button>
      </div>

      <div className={`dashboard-panel ${isWarning ? "warning" : "normal"}`}>
        {isLoadingHistory ? (
          <p>Loading...</p>
        ) : machine ? (
          <>
            <h1 className="machine-name">{machine.name}</h1>
            <p className="machine-temperature">
              {machine.lastTemperature.toFixed(1)}°C
            </p>
            <p className="machine-status">{machine.status}</p>
          </>
        ) : (
          <p>Waiting for telemetry data...</p>
        )}
      </div>

      <div className="history-log">
        <h2>Recent Readings</h2>
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>Temperature</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {history.map((entry, index) => (
              <tr
                key={`${entry.timestamp}-${index}`}
                className={
                  entry.status === "Warning" ? "row-warning" : "row-normal"
                }
              >
                <td>{new Date(entry.timestamp).toLocaleTimeString()}</td>
                <td>{entry.lastTemperature.toFixed(1)}°C</td>
                <td>{entry.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
