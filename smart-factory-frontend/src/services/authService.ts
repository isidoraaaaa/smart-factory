// src/services/authService.ts
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
} from "../types/auth";

const API_BASE_URL = "https://localhost:7279/api";

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Invalid username or password.");
  }

  return response.json();
}

export async function register(
  request: RegisterRequest,
): Promise<AuthResponse> {
  const response = await fetch(`${API_BASE_URL}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Username is already taken.");
  }

  return response.json();
}
