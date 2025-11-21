# Autorisierung & Permissions (Stand V2)

## Ziele
- Ressourcenbasierte Prüfungen auf Basis der Raum-Mitgliedschaft (`RoomMembership`).
- Policies werden deklarativ per `[Authorize(Policy = "RoomPermission:StartGame")]` o. Ä. angebracht.
- Der gleiche Satz Anforderungen gilt für REST und SignalR.

## Datenmodell
- `RoomMembership` (`TagGame.Shared/Domain/Games/RoomMembership.cs`) enthält `UserId`, `RoomId`, `RoomRole`, `PlayerType`, `PermissionsMask` (`RoomPermission` Flags), `IsBanned`, `JoinedAt`.
- Rollen + Flags leben unter `TagGame.Shared/Domain/Games/Enums`.
- `PermissionExtensions` (`TagGame.Api.Core/Common/Security/PermissionExtensions.cs`) liefert `Includes(...)`, `IncludesAny(...)`, `Effective(...)`.

## Policy Provider & Requirements
- `AuthPolicyProvider` (`TagGame.Api/Infrastructure/Auth/AuthPolicyProvider.cs`) erkennt folgende Namensräume:
  - `RoomMember` → `RoomMemberRequirement`
  - `RoomRole:<Role>` → `RoomRoleRequirement(Role)`
  - `RoomPermission:<Permission>` → `RoomMemberRequirement` + `RoomPermissionRequirement(Permission)`
- Handler liegen unter `TagGame.Api/Infrastructure/Auth/Handler/` und unterstützen sowohl `HttpContext` als auch `HubInvocationContext`.

## HTTP Flow
1. Endpoint fügt `RequireAuthorization()` hinzu und – falls eine `roomId` vorkommt – den `RoomMembershipFilter` (`TagGame.Api/Filters/RoomMembershipFilter.cs`).
2. Filter liest `roomId` aus Route (Keys: `roomId`, `id`, `room`), `userId` aus Claims und lädt Membership via `IGamesUoW`.
3. Fehlende/ungültige Daten → ProblemDetails (`room_id_missing`, `auth.invalid_token`, `auth.not_member`, `auth.banned`).
4. Erfolgreiche Membership wird in `HttpContext.Items["Membership"]` gespeichert, sodass Handler nicht erneut auf die DB gehen müssen.
5. Handler führen Flag-/Rollenprüfungen durch und rufen `context.Succeed(...)` bei Erfolg.

## SignalR Flow
- `[Authorize]` am Hub sorgt für JWT-Validierung.
- `RoomAuthHubFilter` lädt `roomId` aus Arguments oder DTOs, liest Membership, legt sie unter `Context.Items` ab und kombiniert alle `[Authorize]`-Attribute (Klasse + Methode) zu einer Policy.
- Bei Verstößen wirft der Filter `HubException` mit `auth.not_member`, `auth.banned`, `auth.missing_permission`.

## Fehler-Codes
| Code | Bedeutung | Quelle |
| --- | --- | --- |
| `auth.invalid_token` | JWT fehlt/ungültig | Filter/Handlers |
| `room_id_missing` | `roomId` konnte nicht bestimmt werden | `RoomMembershipFilter` |
| `auth.not_member` | Keine Membership gefunden | Filter/HubFilter |
| `auth.banned` | `RoomMembership.IsBanned == true` | Filter/HubFilter |
| `auth.missing_permission` | Flag oder Rolle fehlt | Filter (`HubException`), Handler (`403`) |

## Tests
- Unit: `TagGame.Api.Tests/Unit/Auth/RoomAuthorizationHandlersTests.cs` – prüft `RoomMember/RoomRole/RoomPermission`.
- Integration: `TagGame.Api.Tests/Integration/Games/AuthZPoliciesTests.cs` – Minimal-Endpoints mit `RoomPermission:*` + Filter → erwartet 200/403.

## Backlog / Ergänzungen
- Kein Cache: Memberships werden pro Request/Invocation aus der DB gelesen (der Filter legt sie lediglich in `Items` ab). Falls nötig, kann später ein Memory/Redis-Cache ergänzt werden.
- Policy-Namespacing für weitere Ressourcen (z. B. Stats) kann über neue Prefixe erfolgen; `AuthPolicyPrefix` enthält bereits `RoomPermission` und `RoomRole`.
- Wenn Endpoints DTOs statt Route-Param nutzen, sicherstellen, dass `RoomMembershipFilter` die `roomId` aus `RouteValues` erhält (ggf. `MapGroup("rooms/{roomId}")`).
- Für Hubs müssen Gruppen-IDs und Join/Leave-Logik noch implementiert werden; Policies funktionieren bereits (siehe `docs/05-SignalR-Design.md`).
