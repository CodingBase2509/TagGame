# Getting Started (v2) – Lokales Setup

Ziel: API und Basis-Umgebung lokal starten, sodass neue Contributors direkt arbeiten können.

## Voraussetzungen
- .NET SDK 9
- Docker Desktop (inkl. Docker Compose)
- Git
- Optional für Client-Build: Android/iOS SDKs (MAUI). In CI wird der MAUI‑Build bewusst ausgelassen.

## Repository klonen
```bash
git clone https://github.com/CodingBase2509/TagGame.git
cd TagGame
```

## Datenbank per Docker starten
- `.env` enthält Standard‑Werte für Postgres (User/Pass/DB). Bei Bedarf anpassen.
- Starten:
```bash
docker compose up -d
```
- Dienste:
  - Postgres unter `localhost:5432`
  - pgAdmin unter `http://localhost:5050` (Login: admin@admin.com / root)

## API konfigurieren und starten
- Connection String (Dev) ist in `TagGame.Api/appsettings.Development.json` hinterlegt:
  - `Host=localhost;Port=5432;Database=TagGame;Username=taggame;Password=SecurePassword;Include Error Detail=true`
- Alternativ via ENV setzen: `ConnectionStrings__DefaultConnection="..."`.

### Restore & Build
```bash
dotnet restore
dotnet build TagGame.sln -c Debug
```

### Datenbank-Migrationen anwenden (optional, falls noch nicht geschehen)
```bash
# Games
dotnet ef database update -c GamesDbContext -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj
# Auth
dotnet ef database update -c AuthDbContext  -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj
```
Hinweis: Falls die EF‑Tools fehlen: `dotnet tool update --global dotnet-ef`.

### API starten
```bash
dotnet run --project TagGame.Api
```
- Health/Liveness: `GET /health`
- Readiness (inkl. DB‑Checks): `GET /ready`
- Ping: `GET /ping`

## CORS (Dev)
- Standardmäßig erlaubt die API Loopback‑Origins (alle Ports), nützlich für lokale Tools.
- Um Origins explizit zu setzen, `Cors:AllowedOrigins` in `appsettings.Development.json` (oder User‑Secrets/ENV) als `string[]` konfigurieren.
- In Development wird die Policy automatisch aktiviert; siehe `TagGame.Api/Extensions/CorsExtensions.cs`.

## Client (optional)
- MAUI‑Client wird lokal nicht standardmäßig in CI gebaut.
- Android‑Build Beispiel:
```bash
dotnet build TagGame.Client/TagGame.Client.csproj -f net9.0-android -c Debug
# Mit -t:Run auf einem angeschlossenen Gerät/Emulator starten
```

### Client-Konfiguration (appsettings)
- Der MAUI‑Client lädt `Resources/Raw/appsettings.json` sowie im Debug `appsettings.Development.json` aus dem App‑Paket.
- Wichtige Schlüssel:
  - `Api:BaseUrl` / `Api:BaseAddress` — Basis‑Adresse der API (http im Dev; https in Prod)
  - `Networking:Http` — Retry/Timeout‑Einstellungen für den typed HttpClient
  - `Networking:Hub` — Reconnect‑Delays für SignalR
- Emulator‑Hinweise:
  - Android: Host‑Loopback ist `http://10.0.2.2:<port>`
  - iOS Simulator: `http://127.0.0.1:<port>` (oder `http://localhost:<port>`)

### Client‑Networking & JSON
- Typed Client: `IApiClient` in Client.Core kapselt JSON‑GET/POST/PATCH/DELETE
- Resilience: Retry + Total‑Timeout über `NetworkResilienceOptions` (#80)
- Fehler: `application/problem+json` wird zu `ApiProblemException` gemappt (inkl. traceId, Validation‑Errors)
- JSON Defaults: Gemeinsame Optionen (camelCase, Enums als Strings, Null‑Omit, TimeSpan‑Converter) auf Server und Client (#91/#93)

## Tests
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Nützliche Befehle
- Komplettstack (nur DB + pgAdmin in Compose aktiv): `docker compose up -d`
- Formatprüfung lokal wie in CI: `dotnet tool install -g dotnet-format && dotnet format --verify-no-changes`

## Troubleshooting
- Port belegt: Passen Sie Ports in `docker-compose.yml` an oder stoppen Sie konkurrierende Prozesse.
- EF‑Migration schlägt fehl: Prüfen Sie Connection String/DB‑Erreichbarkeit und stellen Sie sicher, dass der DB‑Container läuft (`docker ps`).
