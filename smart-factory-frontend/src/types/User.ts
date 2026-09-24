export type UserType = "Operator" | "Admin";

export interface User {
  id: string;
  name: string;
  lastname: string;
  username: string;
  email: string;
  userType: UserType;
}
