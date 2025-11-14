# REST API Design (Stand V2)

Die API basiert auf Minimal APIs + Carter und ist aktuell auf Authentifizierung/-autorisierung und Profilverwaltung fokussiert. Sobald neue Feature-Slices (Rooms, Matches, Stats, …) entstehen, werden sie als eigene `ICarterModule` unter `TagGame.Api/Endpoints` ergänzt.

## Versionierung & Formate
- Alle produktiven Routen hängen unter `/v1/...` (`MapV1()` in `EndpointBase`).
- Fehler werden als `application/problem+json` mit `code`-Erweiterung ausgegeben. Das Mapping steckt in `TagGame.Api/Middleware/GlobalExceptionHandler.cs`.
- Health-Endpunkte liefern `application/health+json` (IETF Draft) über den `IetfHealthResponseWriter`.

## Authentifizierung (`TagGame.Api/Endpoints/AuthModule.cs`)
| Route | Beschreibung | Besonderheiten |
| --- | --- | --- |
| `POST /v1/auth/initial` | Erstregistrierung per `DeviceId` (optional `DisplayName`/`AvatarColor`). | Legt `User` + Refresh Token an, gibt `InitialResponseDto` mit `TokenPairDto` zurück. |
| `POST /v1/auth/login` | Login über bestehende `DeviceId`. | Gibt neue Tokens aus, ohne weitere Profildaten zu ändern. |
| `POST /v1/auth/refresh` | Token-Rotation. | One-time Refresh Tokens (`RefreshToken`-Familien). Reuse → `401` mit `code=refresh_reuse`. |
| `POST /v1/auth/logout` | Refresh Token invalidieren. | Idempotent; räumt unabhängig von Servererfolg lokal auf. |

Die Implementierung verwendet `IAuthTokenService` (`TagGame.Api.Core/Features/Auth/AuthTokenService.cs`) sowie `AuthUnitOfWork`. Tests siehe `TagGame.Api.Tests/Integration/Auth`.

## Benutzerprofil (`TagGame.Api/Endpoints/UserModule.cs`)
- `GET /v1/users/me`
  - Liefert `UserProfileDto` für den aktuellen JWT-Sub.
  - Unterstützt `If-None-Match` → `304` mit aktuellem ETag (Concurrency Token aus `AuthUoW`).
- `PATCH /v1/users/me`
  - Erwartet `If-Match`; sonst `428`.
  - Validierung via FluentValidation (`TagGame.Api.Core/Validation/Users/PatchUserAccountValidator.cs`).
  - Bei Konflikt `412` + aktuelles ETag. E-Mail-Unique-Violations geben `409` zurück.

## Diagnostik (`TagGame.Api/Endpoints/HealthModule.cs`)
- `GET /health` – Liveness (kein Check, immer 200 sofern Prozess lebt).
- `GET /ready` – Readiness (führt DB-Healthchecks aus; siehe `ServiceCollection.HealthChecks`).
- `GET /ping` – leichter Ping mit Zeitstempel.

## Autorisierung & Policies
- JWT wird über `AddJwtBearer` in `TagGame.Api/Program.cs` registriert.
- Dynamische Policies (`RoomPermission:*`, `RoomRole:*`) werden zur Laufzeit im `AuthPolicyProvider` gebaut. Handler (`RoomMemberHandler`, `RoomRoleHandler`, `RoomPermissionHandler`) sitzen unter `TagGame.Api/Infrastructure/Auth/Handler` und greifen via `IGamesUoW` auf `RoomMembership` zu.
- Für HTTP-Routen gibt es den `RoomMembershipFilter` (Endpoint-Filter), für SignalR den `RoomAuthHubFilter`. Beide laden Memberships ohne Cache und speichern sie unter `HttpContext.Items["Membership"]` bzw. `HubCallerContext.Items`.
- Solange es noch keine Room-Endpunkte gibt, sind die Policies nur auf den Hub-Skeletten aktiv. Neue Endpoints sollten stets `RequireAuthorization()` und – falls raumbezogen – `AddEndpointFilter<RoomMembershipFilter>()` kombinieren. Details siehe `docs/15-Autorisierung-und-Permissions.md`.

## ETag & Optimistische Parallelität
- `EtagUtils` (`TagGame.Api.Core/Common/Http/EtagUtils.cs`) kapselt Header-Prüfungen.
- Der User-Endpunkt nutzt `GetConcurrencyToken` aus `AuthUnitOfWork`, was aktuell auf PostgreSQL `xmin`-Werte setzt.
- Best Practices: GET → `If-None-Match`, PATCH → `If-Match`. Fehlende oder ungültige Header führen zu `400`/`428` bzw. `412` bei Konflikten.

## Fehlerrichtlinien
- 400: Validierungsfehler (FluentValidation → `ValidationProblemDetails`).
- 401: ungültiges/fehlendes JWT (`auth.invalid_token`).
- 403: Policies verneint (`auth.not_member`, `auth.banned`, `auth.missing_permission`).
- 409: Konflikte (z. B. doppelte E-Mail).
- 412/428: ETag-Preconditions.
- 429: reserviert für kommende Rate-Limiter (`RateLimitExceededException`).

## Noch ausstehende Feature-Slices
- Room/Lobby/Create/Join sowie Match-/Round-Endpunkte existieren noch nicht. Anforderungen dafür stehen weiterhin in `docs/05-SignalR-Design.md`, `docs/06-Client-UX-Flow.md`, `docs/16-Statistiken.md` und `docs/17-GameChat.md`.
- Sobald eine Route konkretisiert wird, bitte `docs/04-API-Design.md` ergänzen und dabei den vorhandenen Aufbau (Tabelle mit Route/Beschreibung) beibehalten.
