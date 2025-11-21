# SignalR Design (Stand V2)

Die Infrastruktur für Lobby- und Game-Hubs ist angelegt, die eigentliche Spiellogik folgt noch. Dieses Dokument beschreibt den aktuellen Code und listet klar, was noch fehlt.

## Hubs & Verträge
- `LobbyHub : Hub<ILobbyClient>` (`TagGame.Api/Hubs/LobbyHub.cs`) und `GameHub : Hub<IGameClient>` (`TagGame.Api/Hubs/GameHub.cs`).
- Die Client-Schnittstellen leben unter `TagGame.Shared/Contracts`. Sie enthalten momentan nur Marker-Methoden und werden erweitert, sobald echte Events vorliegen.
- Endpunkte werden in `Program.cs` über `MapHub<LobbyHub>("/hubs/lobby")` etc. registriert.

## Autorisierung & Membership
- `[Authorize]` am Hub erzwingt gültige JWTs.
- Ressourcenbasierte Policies (`RoomPermission:*`, `RoomMember`) hängen direkt an den Hub-Methoden.
- `RoomAuthHubFilter` (`TagGame.Api/Filters/RoomAuthHubFilter.cs`) läuft vor jeder Invocation:
  1. extrahiert `roomId` aus Args oder DTOs,
  2. lädt die `RoomMembership` via `IGamesUoW`,
  3. legt sie unter `Context.Items["Membership"]` ab,
  4. kombiniert die `[Authorize]`-Attribute zu einer Policy und ruft `IAuthorizationService`.
- Fehlende Mitgliedschaft → `HubException("auth.not_member")`, Bans → `HubException("auth.banned")`, fehlende Rechte → `HubException("auth.missing_permission")`.

## Aktuelle Methoden (Stub)
| Hub | Server-Methode | Policy | Status |
| --- | --- | --- | --- |
| LobbyHub | `JoinRoom(Guid roomId)` | `RoomMember` | `NotImplementedException` – Logik für Gruppenbeitritt und Snapshot fehlt. |
| LobbyHub | `LeaveRoom(Guid roomId)` | `RoomMember` | siehe oben. |
| LobbyHub | `UpdatePlayer(Guid roomId, Guid playerId)` | `RoomPermission:ManageRoles` | Pending. |
| LobbyHub | `UpdateSettings(Guid roomId)` | `RoomPermission:EditSettings` | Pending. |
| LobbyHub | `StartGame(Guid roomId)` | `RoomPermission:StartGame` | Pending. |
| GameHub | `UpdateLocation(Guid roomId)` | `RoomMember` | Pending; hier werden später Rate-Limits & Geofence greifen. |
| GameHub | `TagPlayer(Guid roomId)` | `RoomPermission:Tag` | Pending (idempotente Tag-Kommandos). |
| GameHub | `SendChatMessage(Guid roomId)` | `RoomMember` | Placeholder für den Rundenchat (siehe `docs/17-GameChat.md`). |

## Gruppen- und Routing-Konzept (Backlog)
- Geplante Gruppen-IDs: `room:{roomId}` für Lobby-Updates, `room:{roomId}:round:{roundId}` plus Rollen-Suffix für In-Game-Routing.
- Join/Leave-Logik sowie Snapshot-Broadcasts müssen in `LobbyHub`/`GameHub` implementiert werden, sobald Room-Services in `TagGame.Api.Core` existieren.
- Ratenbegrenzungen (`UpdateLocation`, Tagging) sollen serverseitig via dedizierte Services erfolgen und werfen `RateLimitExceededException` → `429`/`HubException("rate_limited")`.

## ToDos
1. Feature-Services in `TagGame.Api.Core/Features/Rooms|Games` (JoinRoom, Ready/State, Countdown/Timer, Location/Tag-Verarbeitung).
2. Gruppenverwaltung und Snapshot-Versand im `LobbyHub`/`GameHub` inklusive Error-/Reconnect-Pfade.
3. Client-Kontrakte (`ILobbyClient`, `IGameClient`) mit echten Events befüllen und im MAUI-Client konsumieren.
4. Tests: Hub-Filter (bereits vorhanden) + echte Integrationstests, sobald Logik steht.
