# Tag Game (v2)

Mobile Hide & Seek with a .NET 9 backend and .NET MAUI client.

Project layout (v2)
- API: `TagGame.Api` (Minimal APIs, DI composition)
- Core: `TagGame.Api.Core` (feature services, EF Core, validators/mapping)
- Client: `TagGame.Client` (MAUI UI)
- Client Core: `TagGame.Client.Core` (ViewModels/Services)
- Shared: `TagGame.Shared` (domain/DTOs)

Build
- `dotnet restore`
- `dotnet build TagGame.sln -c Debug`

Run API
- `dotnet run --project TagGame.Api`

Docs
- See `docs/02-System-Architektur.md` for architecture, and `docs/15-Autorisierung-und-Permissions.md` for AuthZ.
