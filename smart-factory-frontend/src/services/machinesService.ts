import type { ChocolateMachine } from "../types/telemetry";

const API_BASE_URL = "https://localhost:7279/api";

export async function getMachines(
  token: string | null,
): Promise<ChocolateMachine[]> {
  const response = await fetch(`${API_BASE_URL}/machines`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!response.ok) {
    throw new Error("Failed to fetch machines");
  }
  return response.json();
}

export async function getMachine(
  token: string | null,
  id: string,
): Promise<ChocolateMachine> {
  const response = await fetch(`${API_BASE_URL}/machines/${id}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!response.ok) {
    throw new Error(`${response.status} Failed to fetch machine with id ${id}`);
  }
  return response.json();
}

export async function addMachine(token: string | null, machineName: string) {
  const response = await fetch(`${API_BASE_URL}/machines`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ name: machineName }),
  });

  if (!response.ok) {
    throw new Error("Failed to add machine.");
  }
  return response.json();
}

export async function deleteMachine(token: string | null, machineId: string) {
  const response = await fetch(`${API_BASE_URL}/machines/${machineId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    throw new Error("Failed to delete machine.");
  }
}

export async function getMachineHistory(
  token: string | null,
  machineId: string,
  take: number,
) {
  const response = await fetch(
    `${API_BASE_URL}/machines/${machineId}/history?take=${take}`,
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  );

  if (!response.ok) {
    throw new Error("Failed to load machine history.");
  }

  return response.json();
}
