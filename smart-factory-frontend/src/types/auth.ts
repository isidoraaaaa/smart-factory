export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  lastname: string;
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
}
