# Roadmap & Status

## Makro-Phasen
1. **MVP** – Auth, User-Profil, erste Room-/Match-Flows inkl. Tests/Docker.
2. **Beta** – Rejoin/Recovery, Anti-Cheat, Chat, UI-Feinschliff.
3. **v1** – Skalierung (Redis Backplane), Observability, Privacy-Controls.
4. **Monetarisierung** – AdMob + Subscription mit Server-Validation.
5. **Erweiterungen** – Teams, Power-Ups, Stats, Leaderboards.

## V2 M1 (Aktueller Sprint)
Ziel: Authentifizierung/Autorisierung + Client-Bootstrap.

### Fertig (✅)
- Auth-Stack inkl. Refresh-Rotation/Re-use Detection (`AuthModule`, `AuthTokenService`).
- Benutzerprofil GET/PATCH mit ETag-Unterstützung (`UserModule`).
- DB-Kontexte + Migrations (Games/Auth) lauffähig über Docker/Postgres.
- Dynamische Policies + Handler (`RoomPermission:*`, `RoomRole:*`, Membership/Hubs).
- Grundlegende Client-Services: `AuthService`, `IApiClient`, `AuthorizedHttpHandler`, JSON-/Resilience-Setup, Toast-Infrastruktur, Lokalisierung mit SmartFormat.
- Tests: Integration (`TagGame.Api.Tests/Integration/Auth/*`, `Users/*`, `Games/AuthZPoliciesTests.cs`) und Unit-Tests für Token-Service/Handlers/Client-Core Komponenten.

### In Arbeit (⚙️)
- Client-UI, die AuthService & Profil-Endpunkte tatsächlich konsumiert (Start/UserInit/Profile Pages sind noch leer, siehe `docs/06-Client-UX-Flow.md`).
- Games-Domain ist modelliert, aber es fehlen Feature-Services, Repositories und Endpoints für Rooms/Matches.
- Observability (Telemetry/structured logging dashboards) ist vorbereitet, aber noch nicht produktiv konfiguriert.

### Nächste Schritte
1. **Rooms/Lobby Slice**: Service + Endpoints + Hub-Implementierung (Join/Leave, Ready, Settings) basierend auf `RoomMembership`.
2. **Client Onboarding**: UI-Flows rund um `Initial`/`Login`, DeviceId-Verwaltung und Profilseite verdrahten.
3. **SignalR Feature Work**: `LobbyHub`/`GameHub` Methoden implementieren, Gruppenmanagement + Tests (`docs/05-SignalR-Design.md`).
4. **Stats/Chat Planung**: Anforderungen sind in `docs/16-Statistiken.md` und `docs/17-GameChat.md` dokumentiert – zuerst Rooms fertigstellen, dann diese Slices priorisieren.

## Backlog (Auszug, aktualisiert)
- Punkt-in-Polygon & Geofence-Validierung (Server, Tests).
- Standort-Normalisierung/Filtering & Tag-Distanzprüfungen.
- Round-State-Machine + Timer-Service im Server.
- Client: Map-Komponenten, Offline-Handling, Energie-Modus.
- Observability: zentrale Dashboards + Alerts.
- Monetarisierung & Consent-Flows (siehe `docs/09-Monetarisierung.md`).

Hinweis: Wenn ein Feature gestartet wird, bitte den jeweiligen Doc-Eintrag aktualisieren und hier kurz verlinken, sodass Roadmap + Umsetzung synchron bleiben.
