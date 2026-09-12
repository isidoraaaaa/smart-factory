import { createContext } from "react";
import type { LoginRequest, RegisterRequest } from "../types/auth";

export interface AuthContextValue {
  token: string | null;
  isAuthenticated: boolean;
  role: string | null;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
}
export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
import { jwtDecode } from "jwt-decode";
import type { DecodedToken } from "../types/auth";

// unutar AuthProvider-a, ili kao zasebna funkcija

export function getRoleFromToken(token: string | null): string | null {
  if (!token) return null;
  try {
    const decoded = jwtDecode<DecodedToken>(token);
    return decoded[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];
  } catch {
    return null;
  }
}
