# Smart Chocolate Factory — Telemetry Dashboard

![CI](https://github.com/isidoraaaaa/smart-factory/actions/workflows/ci.yml/badge.svg)

A full-stack Proof of Concept application for real-time monitoring of a chocolate factory's production line. A background service simulates sensor data for multiple machines at once, readings are persisted to a database, and users (Operator/Admin) track machine status live through a React dashboard, with full authentication and role-based access.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Running the Project](#running-the-project)
- [Testing](#testing)
- [CI/CD](#cicd)
- [Known Limitations](#known-limitations)

## Architecture

The backend follows **Clean (Layered) Architecture**, split into four independent projects:

```
        API
       ╱  │  ╲
      ╱   │   ╲
Application │ Infrastructure
      ╲   │   ╱
       ╲  │  ╱
        Domain
```

- **Domain** — entities (`ChocolateMachine`, `TelemetryReading`, `AppUser`), enums (`MachineStatus`, `UserType`); depends on nothing
- **Application** — business logic (services: `AuthService`, `MachineService`, `UserService`), interfaces (repositories, hasher, token generator), DTOs
- **Infrastructure** — data access implementation (EF Core, SQL Server), SignalR hub, JWT generation, BCrypt password hashing
- **API** — controllers, JWT middleware, `Program.cs` configuration

The frontend (React + TypeScript + Vite) is organized by resource type:

```
src/
├── components/    # UI components (LoginForm, Dashboard, MachinesList, UsersList, Navbar...)
├── services/      # fetch calls to the backend API
├── types/         # TypeScript interfaces/types
├── context/        # AuthContext — holds the JWT token across the app
```

Backend and frontend live in the same repository (monorepo), in separate folders (`SmartFactoryBackend/`, `smart-factory-frontend/`).

## Tech Stack

**Backend**
- .NET 8, ASP.NET Core Web API
- Entity Framework Core + SQL Server
- SignalR (real-time WebSocket communication)
- JWT authentication (manual implementation — `System.IdentityModel.Tokens.Jwt`), BCrypt password hashing
- xUnit + Moq (unit tests for the Application layer)

**Frontend**
- React 18 + TypeScript, Vite
- React Router (navigation)
- `@microsoft/signalr` client
- `jwt-decode` (reading the role out of the token)

**DevOps**
- Git + GitHub (monorepo)
- GitHub Actions (CI: backend build + test, frontend build)

## Features

- **Real-time telemetry** — a background service generates a temperature reading for every active machine every 30 seconds, broadcasts it live over SignalR, and automatically picks up newly added machines
- **Persistent history** — every reading is saved to the database; the dashboard loads the last N readings on page load, then keeps updating live
- **Authentication** — registration and login with JWT tokens; the token is kept in React state (not in `localStorage`)
- **Role-based authorization**:
  - **Operator** (default role on registration) — can view machines and their history
  - **Admin** (set manually in the database) — can additionally add/delete machines, and view/delete users (except themselves)
- **Navigation** — sidebar with icons (Machines / Users — visible to Admins only / Logout)

## Running the Project

### Prerequisites

- .NET 8 SDK
- Node.js 18+ (LTS recommended)
- SQL Server (LocalDB or Express)

### Backend

```bash
cd SmartFactoryBackend
dotnet restore
```

Set the connection string in `SmartFactoryBackend.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SmartFactoryDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Apply migrations:

```bash
dotnet ef database update --project SmartFactoryBackend.Infrastructure --startup-project SmartFactoryBackend.API
```

Run the API:

```bash
dotnet run --project SmartFactoryBackend.API
```

The backend is available at `https://localhost:7279` (Swagger at `/swagger`).

### Frontend

```bash
cd smart-factory-frontend
npm install
npm run dev
```

The frontend is available at `http://localhost:5173`.

### Setting up an Admin user

All newly registered users get the **Operator** role by default. To promote a user to Admin, run this directly against the database:

```sql
UPDATE Users SET UserType = 1 WHERE Username = 'your_username';
```

(After the change, the user needs to log in again to get a fresh token with the updated role.)

## Testing

Unit tests cover the Application layer (`AuthService`, `MachineService`, `UserService`), using mocked repositories (Moq) — no real database required.

```bash
cd SmartFactoryBackend
dotnet test
```

## CI/CD

A GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push/PR to the `master` branch:

- **Backend Build & Test** — restore, build, run unit tests
- **Frontend Build** — install dependencies, production build

## Known Limitations

This is a PoC/portfolio project, with a few intentional simplifications:

- The JWT signing key is stored directly in `appsettings.json` (for local development simplicity) — in production it should live in an environment variable or a secrets manager (e.g. Azure Key Vault)
- The JWT token is kept in React state rather than an `httpOnly` cookie — refreshing the page currently logs the user out
- There's no email verification or "forgot password" flow
- The Admin role can only be assigned manually in the database; there's no UI for promoting users
