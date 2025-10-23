# System-Architektur (High-Level)

## Überblick
Monolithische .NET 9 Lösung mit klaren Modulen: REST (Setup, Accounts, Lobbys), SignalR (Lobby/In-Game), EF Core (Persistenz). Start als Single-Node; bei Bedarf horizontal skalieren mit Redis-Backplane für SignalR.

```mermaid
graph LR
  subgraph Client (MAUI)
    UI[Views/Components]
    VM[ViewModels (Client.Core)]
    Cli[REST + SignalR Clients]
    UI --> VM --> Cli
  end

  subgraph API (ASP.NET Core)
    REST[Minimal API Endpoints]
    HUBS[SignalR Hubs]
    CORE[TagGameApi.Core\n(Feature-Services + EF Core)]
    DOM[Shared Domain]
    DB[(PostgreSQL)]
    Cache[(Redis Backplane)]
    REST --> CORE --> DB
    HUBS --> CORE
    CORE --> DOM
    HUBS -. optional .-> Cache
  end

  Cli <--> REST
  Cli <--> HUBS
```

## Projektstruktur (Mapping)
- `TagGame.Api` — REST Endpoints (`Endpoints/`), SignalR Hubs, Middleware, Auth; DI-Komposition (`services.AddCore(...)`).
- `TagGame.Api.Core` — Feature-Services (einfacher Service‑Stil), Abstraktionen (Repos/System), EF Core (DbContext, Repositories, `Migrations/`), Validierung/Mapping.
- `TagGame.Client` — MAUI UI (`Ui/Views`, `Ui/Components`).
- `TagGame.Client.Core` — UI‑agnostische ViewModels, API‑Client/Services, State, Offline‑Storage.
- `TagGame.Shared` — Domain‑Modelle, DTOs, Konstanten.
- Tests — `TagGame.Api.Tests`, `TagGame.Client.Tests`.

## Layering & Verantwortlichkeiten
- Domain (Shared): Zustände, Regeln, Invarianten (z. B. Spiel‑States, Boundary‑Checks).
- Core (Api.Core): Feature‑Services je Bereich (z. B. Game, Player, Rooms), Orchestrierung, Transaktionen, Persistenz via Repositories/EF Core. Keine strikte CQRS/CQS, komplexere Reads optional über Reader/Queries.
- Transport (Endpoints/Hubs): AuthZ/AuthN, DTOs, Fehlerformate (ProblemDetails), Throttling/Rate‑Limiting; ruft Feature‑Services direkt auf.
- Client.Core: State‑Management, ViewModels, API‑Zugriff, Offline‑Robustheit; Benachrichtigungen via `IToastPublisher` (siehe 13-Client-Benachrichtigungen.md); Lokalisierung via `ILocalizer` + Live‑Updates (siehe 14-Lokalisierung.md).

## Sicherheit & AuthZ
- AuthN: JWT Access + Refresh Tokens (kurzlebig/rotierend).
- AuthZ: Ressourcenbasiert über Room‑Membership + Bitflags; Policies/Requirements (dynamische Policy‑Namen wie `RoomPermission:StartGame`).
- Details: 15-Autorisierung-und-Permissions.md

## Skalierung & Verfügbarkeit
- Phase 1: Single API Instanz + PostgreSQL
- Phase 2: Mehrere API-Replikas, SignalR Redis Backplane, Sticky Sessions optional
- Phase 3: Read-Replica DB, CDN für statische Inhalte, Rate Limiting pro Benutzer/Lobby

## Persistenz
- PostgreSQL via EF Core (GameRoom, Player, Membership, Match, Round, TagEvents)
- Kurzlebige Daten (Echtzeit, Presence) im Arbeitsspeicher; bei Scale via Redis teilen

## Sicherheit
- JWT-basierte Authentifizierung, Hubs nur mit validem Token
- Input-Validierung, globale Fehlerbehandlung, strukturierte Logs
