import { useState, useCallback } from "react";
import type { ReactNode } from "react";
import * as authService from "../services/authService";
import type { LoginRequest, RegisterRequest } from "../types/auth";
import { AuthContext, type AuthContextValue } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await authService.login(request);
    setToken(response.token);
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await authService.register(request);
    setToken(response.token);
  }, []);

  const logout = useCallback(() => {
    setToken(null);
  }, []);

  const value: AuthContextValue = {
    token,
    isAuthenticated: token !== null,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
