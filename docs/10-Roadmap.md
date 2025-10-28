# Roadmap & Backlog

## Phasen
1) MVP
   - Account/DisplayName, Token
   - Room erstellen/beitreten, Settings, Boundaries
   - Lobby Presence (Ready), Countdown, einfache Runde (Hide/Hunt), Tag-Ereignis
   - Basis-Tests, Docker Compose, Telemetrie light

2) Beta
   - Rejoin/Recovery, Offline-Toleranz
   - Anti-Cheat-Heuristiken, Rate-Limits
   - UI-Feinschliff, Onboarding
   - Erste Lasttests, Bugfixes
   - In-Game Chat (Rundenchat, `@all/@hider/@seeker`)

3) v1
   - Stabilität, Lokalisierung, Accessibility
   - Skalierung mit Redis Backplane
   - Privacy-Controls & Data Export/Delete

4) Monetarisierung
   - AdMob Integration (Interstitial/Rewarded)
   - Subscription + Server-side Receipt Validation
   - Remote Config & A/B-Tests

5) Erweiterungen
   - Team-Modi, Power-Ups, Challenges
   - Persistente, private Statistiken (MVP) & Runden‑Summary
   - Leaderboards (später)

## Backlog (Auszug)
- Punkt-in-Polygon Implementierung + Tests
- Standort-Normalisierung/Filter (Accuracy, Smoothing)
- Round-State-Machine im Server inkl. Timer
 - Rundenchat: SignalR Subgruppen (Hider/Seeker), Snapshot, Rate-Limits
- Client: Map-Komponenten, Energiesparmodus
- SignalR: Snapshot + Deltas, Idempotency für `TagPlayer`
- Observability-Dashboards (Grafana/Seq/ELK)

## v2 M1 — Umsetzung (Plan)
- Ziel: Authentifizierung und Autorisierung produktionsreif — JWT Access + rotierende Refresh Tokens, Ressourcen‑basierte Policies, Membership‑Kontext. Client kann anonym bootstrappen, Tokens speichern/refreshen und Profil pflegen.

- Sequenz A: AuthN (Server‑Fundament)
  - #23 User + RefreshToken entities & migrations (Verifikation, ggf. Deltas)
  - #24 JWT access + refresh rotation (TokenService, Signing/Validation, DI)
  - #25 Auth endpoints: /v1/users, /v1/auth/refresh, /v1/auth/logout
  - #55 JWT refresh reuse detection + rotation tests (Familien‑Sperre bei Reuse)
  - Tests: Unit (TokenService), Integration (Create → Refresh → Logout, Fehlerfälle)

- Sequenz B: AuthZ (Policies/Membership)
  - #26 Bitflags permissions enum + role masks
  - #27 RoomMembership entity + repository (EF + Uniqueness)
  - #28 Dynamic policy provider: RoomPermission:*
  - #29 Membership endpoint filter (HTTP) — lädt Membership in HttpContext
  - #30 SignalR auth guards for room methods (IAuthorizationService/IHubFilter)
  - Tests: Policy‑Erzwingung (200/403), Filter (fehlende Membership → 403)

- Sequenz C: Client Auth‑Flow
  - #54 Client.Core: AuthService + TokenStorage + AuthorizedHttpHandler (Attach/Refresh on 401)
  - #92 HttpClientFactory + typed clients wiring (nutzt #54/#80/#91)
  - #67 Client UI: SetupProfilePage (First‑Run Modal)
  - #64 API: Update profile (PATCH /v1/users/me, ETag/If‑Match)
  - #65 Client UI: Profile screen (DisplayName/Avatar) — nutzt #64

- Sequenz D: Docs & Security
  - Aktualisieren: 04‑API‑Design (ETag/If‑Match für PATCH, bereits ergänzt), Auth‑Flows (Create/Refresh/Logout), Fehlerkonventionen (412)
  - Kurze Developer‑Notes zu Token‑Rotation/Reuse‑Detection und ProblemDetails‑Mappung im Client

- PR‑Strategie
  - Kleine, fokussierte PRs (1–3 Issues), Beschreibung mit „Closes #…“.
  - Reihenfolge: A → B → C, Docs kontinuierlich (D). Client‑PRs können parallel nach Abschluss A laufen.
  - CI prüft Build/Tests/Format; Merge schließt Issues automatisch.

- Definition of Done (M1)
  - Server: JWT Access/Refresh mit Rotation + Reuse‑Detection; Endpoints funktionieren inkl. ProblemDetails bei Fehlern; Policies/Filter/Hubs erzwingen AuthZ korrekt.
  - Client: Anonymer Bootstrap, Tokens gespeichert + Auto‑Refresh bei 401, AuthorizedHttpHandler aktiv; SetupProfile + Profil‑Update funktionieren (ETag/412 abgedeckt).
  - Tests grün (Unit/Integration); OpenAPI beschreibt Auth‑Endpoints/Scopes; Developer‑Docs aktualisiert.
