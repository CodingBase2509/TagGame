# Autorisierung & Permissions (AuthZ) – Leitfaden

Ziel: Ressourcenbasierte, performante und testbare Autorisierung für Räume (Rooms) mit feingranularen Rechten – passend zu TagGameApi.Core und ohne striktes CQRS.

## Kernideen
- Rechte sind kontextuell: sie hängen an der Room‑Membership (User × Room), nicht am globalen User.
- Policy‑basiert über Requirements; Endpoints/Hubs deklarieren Policies, Handler prüfen dynamisch.
- Tokens bleiben schlank (UserId, wenige UX‑Claims). Autorisierung nutzt DB/Cache, um Stale‑Claims zu vermeiden.

## Datenmodell
- `User`: globale Identität/Profil.
- `RoomMembership`: `UserId`, `RoomId`, `Role` (Owner/Moderator/Player), `PermissionsMask` (Bitflags), `IsBanned`, `JoinedAt`.
- Rollen → Basis‑Maske; optional Grant/Deny für Feinsteuerung.

## Bitflags (Permissions)
- `[Flags]`‑Enum, Werte als Potenzen von 2: `1, 2, 4, 8, 16, …`.
- Beispiel (Auszug):
  - `StartGame = 1`
  - `EditSettings = 2`
  - `KickPlayer = 4`
  - `Invite = 8`
  - `Tag = 16`
- Speicherung: `PermissionsMask` als `int`/`long` in `RoomMembership`.
- Prüfung (effizient): `(mask & Permission.StartGame) != 0` (statt `HasFlag`).
- Rolle → Eff. Maske: `Effective = (RoleMask | GrantedMask) & ~DeniedMask` (Grant/Deny optional).

## Policies & Requirements
- Requirements (Beispiele):
  - `RoomMemberRequirement`: Membership existiert, `!IsBanned`.
  - `RoomRoleRequirement(Owner|Moderator|Player)`.
  - `RoomPermissionRequirement(Permission needed)`: `(membership.PermissionsMask & needed) != 0`.
  - `PremiumRequirement`: Entitlement prüfen (DB/Cache; Claim nur kosmetisch).
- Policy‑Komposition: Policies bestehen aus Requirements (UND‑verknüpft).
  - `CanStartGame` = `RoomMember` + `RoomPermissionRequirement(StartGame)`.
  - `CanEditSettings` = `RoomMember` + `RoomPermissionRequirement(EditSettings)`.

## Dynamischer Policy‑Provider
- Eigener `IAuthorizationPolicyProvider` erzeugt Policies zur Laufzeit aus Namen:
  - `"RoomPermission:StartGame"` → `RoomMemberRequirement` + `RoomPermissionRequirement(StartGame)`.
  - `"RoomRole:Owner"` → `RoomRoleRequirement(Owner)`.
- Vorteil: Deklarativer Stil am Endpoint/Hub ohne große Registrierungslisten.

## HTTP‑Flow (Minimal APIs)
1) AuthN: JwtBearer validiert das Token.
2) Membership‑Lader (Endpoint‑Filter/Middleware vor Authorization):
   - Extrahiert `roomId` aus Route (oder Body/Query, falls nötig).
   - Lädt Membership (Cache→Repo), cacht 30–120 s pro `(userId, roomId)`.
   - Legt die Membership in `HttpContext.Items["Membership"]` ab.
3) Authorization: `IAuthorizationService.AuthorizeAsync(User, resource, requirement)`
   - `resource` enthält mind. `roomId`, idealerweise die geladene `membership`.
   - Requirements prüfen Rolle/Flags; bei Erfolg kann der Handler auf `Items["Membership"]` zugreifen.

## SignalR‑Flow
- `[Authorize]` am Hub prüft nur Token (Connection‑Level).
- Methoden sind mit Policies annotiert, z. B. `RoomMember`, `RoomPermission:StartGame`.
- Ein zentraler `IHubFilter` lädt aus Methodenargumenten die `roomId`, lädt die Membership, legt sie in `Context.Items["Membership"]` ab und kombiniert die `[Authorize]`‑Daten zu einer Policy (`IAuthorizationPolicyProvider`). Die Prüfung erfolgt über `IAuthorizationService.AuthorizeAsync(User, invocationContext, policy)`.
- Die bestehenden `AuthorizationHandler` unterstützen neben `HttpContext` auch `HubInvocationContext` (lesen Membership aus `Context.Items` oder DB).
- Wichtig: `roomId` ist erst zur Methodenzeit bekannt; deshalb „alles in Middleware“ hier nicht ausreichend.

## Caching & Invalidierung
- Membership‑Cache pro `(userId, roomId)` mit kurzer TTL (30–120 s).
- Änderungen (Kick/Ban/RoleChange) → Cache‑Invalidation; bei Scale via Redis Pub/Sub.
- Hubs: Bei kritischen Änderungen optional proaktives `Group.Remove`/Disconnect.

## Fehlerkonventionen
- 401: `auth.invalid_token`, `auth.expired`.
- 403: `auth.not_member`, `auth.banned`, `auth.missing_permission`, `subscription.required`.
- Antworten als ProblemDetails mit `code` und optional `remediation`.

## Tests
- Unit: Policy‑Handler mit Fake‑Memberships (Maske trifft/nicht, Banned, verschiedene Rollen).
- Integration (HTTP): Routen mit Policies (200/403), Membership‑Lader greift korrekt auf Route.
- SignalR: Join/Leave, Gruppen, verbotene Methoden (Guard/403/Abbruch), Rate‑Limits.

## Platzierung: Middleware vs. Requirements
- Authentifizierung: zentral JwtBearer‑Middleware.
- HTTP‑Autorisierung: gut zentralisierbar; Membership‑Lader als Endpoint‑Filter/Middleware vor `AuthorizationMiddleware`.
- SignalR: Connection‑Authorize nur Token; ressourcenbasierte Prüfungen in Hub‑Methoden/`IHubFilter`.
