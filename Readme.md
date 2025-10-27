# Tag Game (v2)

Mobile Hide & Seek with a .NET 9 backend and .NET MAUI client.

Project layout (v2)
- API: `TagGame.Api` (Minimal APIs, DI composition)
- Core: `TagGame.Api.Core` (feature services, EF Core, validators/mapping)
- Client: `TagGame.Client` (MAUI UI)
- Client Core: `TagGame.Client.Core` (ViewModels/Services)
- Shared: `TagGame.Shared` (domain/DTOs)

Getting Started
- Siehe `docs/00-Getting-Started-v2.md` für lokales Setup (Docker DB, API starten, CORS/EF‑Hinweise).

Build
- `dotnet restore`
- `dotnet build TagGame.sln -c Debug`

Run API
- `dotnet run --project TagGame.Api`

Docs
- See `docs/02-System-Architektur.md` for architecture, and `docs/15-Autorisierung-und-Permissions.md` for AuthZ.

EF Core Migrations & Datenbank
- Kontexte und Schemas
  - AuthDbContext → Schema `auth`
  - GamesDbContext → Schema `games`
  - Design‑Time Factories vorhanden: `TagGame.Api.Core/Persistence/Contexts/DesignTimeAuthFactory.cs`, `TagGame.Api.Core/Persistence/Contexts/DesignTimeGamesFactory.cs`

- Migration anlegen (je Kontext)
  - Games: `dotnet ef migrations add $MigrationName -c GamesDbContext -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj -o Persistence/Migrations/Games`
  - Auth:  `dotnet ef migrations add $MigrationName -c AuthDbContext  -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj -o Persistence/Migrations/Auth`

- Migration anwenden
  - Games: `dotnet ef database update -c GamesDbContext -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj`
  - Auth:  `dotnet ef database update -c AuthDbContext  -p TagGame.Api.Core/TagGame.Api.Core.csproj -s TagGame.Api/TagGame.Api.csproj`

- Nützliche Befehle
  - Migrations auflisten: `dotnet ef migrations list -c GamesDbContext ...` bzw. `-c AuthDbContext ...`
  - Letzte Migration entfernen (nicht angewendet!): `dotnet ef migrations remove -c GamesDbContext ...` bzw. `-c AuthDbContext ...`

- Connection String
  - Dev: in `TagGame.Api/appsettings.Development.json` unter `ConnectionStrings:DefaultConnection`
  - Alternativ per ENV beim EF‑Aufruf setzen:
    - `ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=TagGame;Username=taggame;Password=SecurePassword;Include Error Detail=true" dotnet ef ...`

- EF Tools Version
  - Um Tool/Runtime‑Mismatch‑Warnungen zu vermeiden: `dotnet tool update --global dotnet-ef`
