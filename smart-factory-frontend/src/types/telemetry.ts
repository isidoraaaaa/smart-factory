export type MachineStatus = 'Normal' | 'Warning';

export interface ChocolateMachine {
  id: string;
  name: string;
  lastTemperature: number;
  status: MachineStatus;
  timestamp: string;
}

export interface TelemetryReading {
  id: string;
  temperature: number;
  status: MachineStatus;
  timestamp: string;
}