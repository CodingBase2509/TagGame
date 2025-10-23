# Repository Guidelines (Agent)

Diese Datei richtet sich an Agenten, die im Repo arbeiten. Sie spiegelt die Zielarchitektur der Docs wider und beschreibt, wo neuer Code landen soll und welche Konventionen zu beachten sind.

## Project Structure & Module Organization
- `TagGame.Api` — ASP.NET Core API. Endpoints in `Endpoints/`, SignalR Hubs, Middleware/Filters, AuthN/AuthZ‑Setup, DI‑Komposition (`services.AddCore(...)`).
- `TagGameApi.Core` — Server‑Kern: Feature‑Services (statt striktem CQRS), Abstraktionen (`Abstractions/` für Persistence/System), EF Core (`Persistence/EfCore/` inkl. `TagGameDbContext`, `Configurations/`, `Repositories/`, `Migrations/`), Validierung (`Validation/`), Mapping (`Mapping/`).
- `TagGame.Client` — .NET MAUI App. UI in `Ui/Views` und `Ui/Components`, Assets unter `Resources/`, Plattformcode `Platforms/Android|iOS`. Toast‑Anzeige (Presenter/Host) liegt hier.
- `TagGame.Client.Core` — UI‑agnostische ViewModels, Services (Abstraktionen/Implementierungen für HTTP/Storage), State‑Management, Lokalisierung (`ILocalizer`), Benachrichtigungen (`IToastPublisher`). Keine `Microsoft.Maui.*`‑Abhängigkeiten.
- `TagGame.Shared` — Domain‑Modelle, DTOs, Konstanten.
- Tests — `TagGame.Api.Tests`, `TagGame.Client.Tests` (Unit + Integration).

Hinweis: Existieren noch V1‑Artefakte (z. B. `TagGame.Client.Testables`), neue Arbeiten am V2‑Layout ausrichten (Client.Core statt Testables) und die Docs aktuell halten.

## Build, Test, and Development Commands
- Restore/build: `dotnet restore` · `dotnet build TagGame.sln -c Debug`
- Run API: `dotnet run --project TagGame.Api`
- Full stack via Docker: `docker compose up --build` (erfordert `.env` mit `pgUser`, `pgPassword`, `pgDb`).
- Build Client: `dotnet build TagGame.Client/TagGame.Client.csproj -f net9.0-android` (oder `net9.0-ios`). Mit `-t:Run` auf Gerät/Emulator starten.
- Tests: `dotnet test` bzw. `dotnet test --collect:"XPlat Code Coverage"`

## Coding Style & Naming Conventions
- C# 13; `Nullable` + implicit usings aktiviert. 4‑Space Indent; Klammern neue Zeile; `var` für offensichtliche Typen.
- PascalCase für Typen/Methoden; camelCase für Locals/Parameter; `_camelCase` für private Felder. Async‑Methoden enden auf `Async`.
- Place new code by area:
  - API Endpoints/Hubs → `TagGame.Api/Endpoints`
  - Validatoren/Mapping → `TagGameApi.Core/Features/<Feature>/(Validation|Mapping)`
  - Feature‑Services → `TagGameApi.Core/Features/<Feature>/<Feature>Service.cs`
  - Abstraktionen (Repos/System) → `TagGameApi.Core/Abstractions/*`
  - EF Core (DbContext/Repos/Migrations) → `TagGameApi.Core/Persistence/EfCore/*`
  - DTOs → `TagGame.Shared/DTOs/<Area>`
  - ViewModels → `TagGame.Client.Core/Ui/ViewModels`

## Architecture Conventions
- Vertical Slice je Feature; einfache Service‑Klassen statt striktem CQRS. Komplexe Reads optional als Reader.
- AuthZ: Ressourcenbasiert (Room‑Membership + Bitflags). Dynamische Policies (`RoomPermission:*`), Membership‑Lader als Endpoint‑Filter. Siehe `docs/15-Autorisierung-und-Permissions.md`.
- Lokalisierung: `ILocalizer` (+ Markup‑Extension) mit Live‑Sprachwechsel. Siehe `docs/14-Lokalisierung.md`.
- Benachrichtigungen: `IToastPublisher` (Core) + Presenter/Host im Client; Queue/Priorität. Siehe `docs/13-Client-Benachrichtigungen.md`.
- Observability: Serilog + OpenTelemetry; ProblemDetails für Fehler; Rate Limiting; API Versioning.

## Testing Guidelines
- Frameworks: xUnit, FluentAssertions, Moq; Integration mit Testcontainers (Docker erforderlich).
- Tests benennen `<Target>Tests.cs`; Ordner `Unit/` und `Integration/`.
- Fokus: Logik in `Client.Core` halten (VMs/Services testbar), API‑Integration (WebApplicationFactory + Postgres), AuthZ‑Policies (200/403), Lokalisierung (Pseudo‑Loc, Pluralfälle), Benachrichtigungen (Publisher‑Fakes).

## Commit & Pull Request Guidelines
- Commits: kurz, imperativ (z. B. „add“, „update“, „fix“), optionaler Scope (`api:`, `client:`). Verweise auf Issues/PRs: „... (#123)“.
- PRs: klare Beschreibung, verlinkte Issues, aktualisierte Tests, Screenshots/GIFs für UI‑Änderungen.

## Security & Configuration Tips
- Verbindungen: `ConnectionStrings__DefaultConnection` (ENV) oder via `docker compose` und `.env`.
- AuthN: Kurzlebige JWTs + Refresh‑Rotation (One‑Time Use); keine Secrets commiten (ENV/User‑Secrets).

## Agent‑Specific Notes
- Änderungen minimal halten und an obige Struktur/Docs ausrichten. Neue Features folgen den Konventionen (Features‑Ordner, Services, Abstraktionen, Tests).
- Keine tiefgreifende Umstrukturierung ohne explizite Anweisung. Bei Abweichungen vom V2‑Layout Rücksprache halten bzw. in den Docs festhalten.

