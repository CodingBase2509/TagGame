# Teststrategie (Stand V2)

## Tooling
- xUnit + FluentAssertions + Moq in allen Testprojekten.
- API-Integrations-Tests laufen mit `WebApplicationFactory<Program>` + Testcontainers/Postgres (`IntegrationTestBase`).
- `dotnet test` oder `dotnet test --collect:"XPlat Code Coverage"` deckt die gesamte Lösung ab.

## Aktueller Fokus
### TagGame.Api.Tests
- **Unit**
  - `Auth/AuthTokenServiceTests.cs`, `Auth/AuthTokenServiceReuseTests.cs` – Token-Rotation, Reuse-Detection, DateTimeEdgecases.
  - `Auth/RoomAuthorizationHandlersTests.cs` – `RoomMember/RoomRole/RoomPermission` Handler verifizieren verschiedene Masks/Ban-Szenarien.
  - `Persistence/*` – `EfDbRepository`, `AuthUnitOfWork` etc. gegen InMemory-DbContext.
- **Integration**
  - `Auth/AuthEndpointsTests.cs` – kompletter Initial/Login/Refresh/Logout-Fluss inkl. Fehlerszenarien.
  - `Users/UserProfile*Tests.cs` – GET/PATCH `users/me` inkl. ETag/412/428/409.
  - `Games/AuthZPoliciesTests.cs` – Minimal-Routen mit Policies, prüft `200/403`, Bans und fehlende Memberships mit echtem `GamesDbContext`.

### TagGame.Client.Tests
- **Unit**
  - `Auth/AuthServiceTests.cs` – DeviceId-Register/Login, Refresh, Logout.
  - `Http/ApiClientTests.cs`, `Http/ProblemDetailsHandlerTests.cs` – JSON-Handling & Fehlerabbildung.
  - `Storage/TokenStorageTests.cs` – SecureStorage-Fakes, JSON-Ser/Deser.
  - `Security/*` – Token-Helpers/options.
- (Noch) keine Integration/UI-Tests; wird relevant, sobald reale ViewModels/Pages Daten laden.

## Lücken & Backlog
- SignalR-Tests (Join/Leave, Gruppenrouting) fehlen, da Hubs noch keinen Code besitzen (`docs/05-SignalR-Design.md`).
- Domain-/Game-Services müssen künftig Unit-Tests für Geofence/State-Machine bekommen.
- Client-ViewModels brauchen MVVM-Tests, sobald echte Datenflüsse implementiert sind.
- Lokalisierungs-Lints (fehlende Keys, Pseudo-Loc) sind geplant, aber noch nicht automatisiert.

Beim Anlegen neuer Features bitte Tests auf der Ebene platzieren, auf der die Logik liegt (Client.Core oder Api.Core) und die bestehenden Module als Vorbild nutzen.
