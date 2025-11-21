# Domain-Modell (Stand V2)

Der fachliche Kern lebt im Shared-Projekt (`TagGame.Shared`). Die Klassen werden sowohl von der API als auch vom Client referenziert und bilden die Grundlage für EF-Core-Konfigurationen in `TagGame.Api.Core`. Dieses Dokument beschreibt nur das, was heute tatsächlich vorhanden ist.

## Auth-Bereich (`TagGame.Shared/Domain/Auth`)
- `User` (`TagGame.Shared/Domain/Auth/User.cs`)
  - Felder: `Id`, optionale Profilfelder (`DisplayName`, `AvatarColor`, `Email`, `DeviceId`), `Flags`, `CreatedAt`, `LastSeenAt`.
  - Verwendet durch `AuthDbContext` (`TagGame.Api.Core/Persistence/Contexts/AuthDbContext.cs`) und die Endpoints in `TagGame.Api/Endpoints/AuthModule.cs` sowie `UserModule.cs`.
- `RefreshToken` (`TagGame.Shared/Domain/Auth/RefreshToken.cs`)
  - Speichert rotierende Refresh-Tokens (Hash + `FamilyId`, `ReplacedById`, `RevokedAt`).
  - Wird vom `AuthTokenService` (`TagGame.Api.Core/Features/Auth/AuthTokenService.cs`) verwaltet und in Integrationstests unter `TagGame.Api.Tests/Integration/Auth` abgedeckt.
- `Entitlement` (`TagGame.Shared/Domain/Auth/Entitlement.cs`)
  - Platzhalter für zukünftige Premium-Features (Typ/Quelle/ValidFrom/ValidUntil). Noch nicht angebunden.

## Spielbereiche (`TagGame.Shared/Domain/Games`)
- `GameRoom` (`.../Games/GameRoom.cs`) – Aggregate Root mit `Id`, `Name`, `AccessCode`, `OwnerUserId`, `GameState`, `RoomSettings`, optionalen `Boundaries` (`GeoPolygon`) und `CreatedAt`.
- `RoomSettings` (`.../Games/RoomSettings.cs`) – Hide/Hunt-Dauern und `TagRadiusM`. Standardwerte (60s/300s/2.5m) werden sowohl in Tests als auch beim Seed genutzt.
- `RoomMembership` (`.../Games/RoomMembership.cs`) – Surrogate `Id`, `UserId`, `RoomId`, `PlayerType`, `RoomRole`, `PermissionsMask` (`RoomPermission` Flags), `IsBanned`, `JoinedAt`.
  - Direkt genutzt von den Authorization-Handlern/Filtern (`TagGame.Api/Filters/RoomMembershipFilter.cs`, `TagGame.Api/Filters/RoomAuthHubFilter.cs`) und von `PermissionExtensions` (`TagGame.Api.Core/Common/Security/PermissionExtensions.cs`).
- `Match`, `Round`, `TagEvent` (`.../Games/Match.cs`, `.../Round.cs`, `.../TagEvent.cs`) – Datenstrukturen für Mehr-Runden-Spiele und Tag-Ereignisse. Persistenzklassen sind vorhanden, aber noch nicht durch Endpoints oder Services befüllt.

## Enums & Flags (`TagGame.Shared/Domain/Games/Enums`)
- `RoomPermission` – `[Flags]` (z. B. `EditSettings`, `StartGame`, `ManageRoles`, `Tag`). Wird über dynamische Policies (`RoomPermission:*`) genutzt, siehe `docs/15-Autorisierung-und-Permissions.md`.
- `RoomRole`, `PlayerType`, `GameState`, `MatchStatus`, `RoundPhase` – liefern Typsicherheit für Rollen und Statusmaschinen.

## Persistenz-Mapping
- `AuthDbContext` & `GamesDbContext` liegen unter `TagGame.Api.Core/Persistence/Contexts`. Jede Domäne hat eigene `Configurations/` und `Migrations/`-Ordner.
- Repositories/Unit-of-Work finden sich unter `TagGame.Api.Core/Persistence/Repositories` (z. B. `AuthUnitOfWork`, `GamesUnitOfWork`).
- Die API injiziert beide Kontexte in `TagGame.Api.Core/DependencyInjection.cs` und entscheidet anhand der Entität, welches UoW eingesetzt wird (`EfDbRepository`).

## Praktische Nutzung
- HTTP-Endpunkte für Benutzerprofile greifen ausschließlich auf den Auth-Bereich zu (`UserModule`).
- SignalR/HTTP-Autorisierung lädt `RoomMembership` pro Request/Invocation, steckt sie in `HttpContext.Items["Membership"]` bzw. `HubCallerContext.Items` und wertet sie in den Handlern aus.
- Clientseitige Modelle in `TagGame.Client.Core` (z. B. `Lobby`- und `Game`-ViewModels) referenzieren derzeit nur `RoomSettings`/`RoomPermission` – weiterer Ausbau folgt mit den kommenden Features.

## Offene Regeln (Backlog)
Die ursprünglichen Invarianten (ein Spieler pro aktiver Lobby, Geofence-Validierungen, serverseitige Tag-Prüfungen) sind noch nicht implementiert. Sobald die zugehörigen Services in `TagGame.Api.Core/Features/*` landen, sollten sie hier ergänzt werden. Bis dahin gilt: Domain-Klassen enthalten nur Struktur, keine Validierungslogik.
