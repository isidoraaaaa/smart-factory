import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

// Tipovi koji odgovaraju backend Domain modelima
type MachineStatus = "Normal" | "Warning";

interface ChocolateMachine {
  id: string;
  name: string;
  lastTemperature: number;
  status: MachineStatus;
  timestamp: string;
}

const HUB_URL = "https://localhost:7279/hubs/telemetry";
const MAX_LOG_ENTRIES = 5;

function App() {
  const [isConnected, setIsConnected] = useState(false);
  const [machine, setMachine] = useState<ChocolateMachine | null>(null);
  const [history, setHistory] = useState<ChocolateMachine[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.on("ReceiveTelemetry", (data: ChocolateMachine) => {
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
  }, []);

  const isWarning = machine?.status === "Warning";

  return (
    <div className="app-container">
      {/* Indikator konekcije */}
      <div className="connection-indicator">
        <span
          className={`status-dot ${isConnected ? "connected" : "disconnected"}`}
        />
        <span>{isConnected ? "Connected" : "Disconnected"}</span>
      </div>

      {/* Glavni Dashboard panel */}
      <div className={`dashboard-panel ${isWarning ? "warning" : "normal"}`}>
        {machine ? (
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

      {/* Istorijski Log */}
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
              <tr key={`${entry.timestamp}-${index}`}>
                <td>{new Date(entry.timestamp).toLocaleTimeString()}</td>
                <td>{entry.lastTemperature.toFixed(1)}°C</td>
                <td className={`machine-status ${entry.status}`}>
                  {entry.status}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default App;
