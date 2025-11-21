# Tag Game (v2)

Mobile hide & seek for groups, powered by an ASP.NET Core backend and a .NET MAUI client. The current milestone (v2) focuses on authentication/authorization, a modern client foundation, and a solid testing/documentation setup that makes future feature slices (Rooms, Matches, Chat, Stats) easy to add.

## Highlights
- **Modern stack** – .NET 9, Minimal APIs + Carter, SignalR hubs, EF Core with PostgreSQL, MAUI client (Android/iOS) that shares models via `TagGame.Shared`.
- **Auth & Profiles** – Anonymous bootstrap via device ID, refresh-token rotation with reuse detection, user profile GET/PATCH guarded by ETags.
- **Resource-based authorization** – Dynamic policies like `RoomPermission:StartGame`, reusable endpoint/hub filters that load `RoomMembership` once and expose it to handlers.
- **Client foundation** – ViewModel-friendly services in `TagGame.Client.Core`, typed HTTP client with resilience, localization via SmartFormat, and a queue-based toast system.
- **Tests & docs** – Integration tests with Testcontainers, focused unit tests for token/authorization logic, and a docs folder that captures architecture, roadmap, and feature backlogs.

## Repository Layout
| Project | Purpose |
| --- | --- |
| `TagGame.Api` | ASP.NET Core host (Minimal APIs, SignalR hubs, middleware, Carter modules). |
| `TagGame.Api.Core` | Feature services, abstractions, EF Core contexts/migrations, validation/mapping. |
| `TagGame.Client` | .NET MAUI app (Views, Components, Toast presenter/host, platform resources). |
| `TagGame.Client.Core` | UI-agnostic ViewModels, services (HTTP/auth/storage), localization, navigation, notifications. |
| `TagGame.Shared` | Shared domain models, DTOs, enums, JSON defaults, hub contracts. |
| `TagGame.Api.Tests` / `TagGame.Client.Tests` | Unit + integration tests for server and client code. |
| `docs/` | Living documentation (architecture, auth, roadmap, backlog slices, guidelines). |

## Tech Stack
- **Backend:** ASP.NET Core, Minimal APIs + Carter, SignalR, EF Core (Npgsql), FluentValidation, Serilog, OpenTelemetry-ready.
- **Client:** .NET MAUI, MVVM via `CommunityToolkit.Mvvm` (planned), SmartFormat localization, typed `HttpClientFactory` with resilience handlers.
- **Database:** PostgreSQL (Docker), separate schemas for `auth` and `games` contexts.
- **Testing:** xUnit, FluentAssertions, Moq, Testcontainers for Postgres.

## Quick Start
1. **Clone & restore**
   ```bash
   git clone https://github.com/CodingBase2509/TagGame.git
   cd TagGame
   dotnet restore
   ```
2. **Start infra (Postgres + pgAdmin)**
   ```bash
   docker compose up -d
   ```
   `.env` contains default credentials (`pgUser`, `pgPassword`, `pgDb`).
3. **Build everything**
   ```bash
   dotnet build TagGame.sln -c Debug
   ```
4. **Run the API**
   ```bash
   dotnet run --project TagGame.Api
   ```
   Endpoints: `/health`, `/ready`, `/ping`, `/v1/auth/*`, `/v1/users/me`.
5. **(Optional) Build the MAUI client**
   ```bash
   dotnet build TagGame.Client/TagGame.Client.csproj -f net9.0-android -c Debug
   # Use -t:Run with a connected emulator/device
   ```

> Full setup details (connection strings, CORS, client configuration) live in `docs/00-Getting-Started-v2.md`.

## Database & EF Core
- Contexts: `AuthDbContext` (schema `auth`) and `GamesDbContext` (schema `games`).
- Design-time factories live in `TagGame.Api.Core/Persistence/Contexts/DesignTime*Factory.cs`.
- Add migrations:
  ```bash
  # Games schema
  dotnet ef migrations add <Name> -c GamesDbContext -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj -o Persistence/Migrations/Games
  # Auth schema
  dotnet ef migrations add <Name> -c AuthDbContext  -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj -o Persistence/Migrations/Auth
  ```
- Apply migrations:
  ```bash
  dotnet ef database update -c GamesDbContext -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj
  dotnet ef database update -c AuthDbContext  -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj
  ```
- Helpful commands: `dotnet ef migrations list ...`, `dotnet ef migrations remove ...`, keep `dotnet-ef` updated (`dotnet tool update --global dotnet-ef`).

## Running Tests
```bash
dotnet test --collect:"XPlat Code Coverage"
```
- API integration tests (`TagGame.Api.Tests/Integration/*`) spin up Testcontainers/Postgres automatically.
- Client unit tests cover AuthService, HTTP handlers, storage and will grow with upcoming ViewModel work.

## Key Documentation
| Topic | Location |
| --- | --- |
| Getting started / local setup | `docs/00-Getting-Started-v2.md` |
| Architecture overview | `docs/02-System-Architektur.md` |
| Domain models | `docs/03-Domain-Modell.md` |
| API design | `docs/04-API-Design.md` |
| SignalR & real-time plan | `docs/05-SignalR-Design.md` |
| Client UX foundations | `docs/06-Client-UX-Flow.md`, `docs/21-Client-UI-Foundation.md` |
| Security & privacy | `docs/07-Daten-Sicherheit-Privacy.md` |
| Authorization & permissions | `docs/15-Autorisierung-und-Permissions.md` |
| Backlog slices (Stats, Chat, etc.) | `docs/16-Statistiken.md`, `docs/17-GameChat.md` |
| Contribution guidelines | `docs/19-PR-und-Commit-Guidelines.md` |

## Contributing
- Follow the structure in `AGENTS.md` and the PR/commit rules in `docs/19-PR-und-Commit-Guidelines.md` (short, imperative commits; PR description = commit summary).
- Keep docs in sync with code—every new feature slice should update the relevant document.
- Prefer small, focused PRs with passing tests and screenshots for UI changes.

Happy tagging! 🏃‍♀️
