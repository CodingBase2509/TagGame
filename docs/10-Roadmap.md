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
   - Persistente Statistiken & Leaderboards

## Backlog (Auszug)
- Punkt-in-Polygon Implementierung + Tests
- Standort-Normalisierung/Filter (Accuracy, Smoothing)
- Round-State-Machine im Server inkl. Timer
- Client: Map-Komponenten, Energiesparmodus
- SignalR: Snapshot + Deltas, Idempotency für `TagPlayer`
- Observability-Dashboards (Grafana/Seq/ELK)

## v2 M0 — Umsetzung (Plan)
- Ziel: Lauffähiges Skelett mit echter DB, sauberer API-Basis und CI.

- Sequenz A: Persistence + Container (Startpunkt)
  - #43 EF Core: Npgsql + DbContext + DI + design-time factory
  - #44 EF Core: initial migration + apply to dev DB
  - #21 Docker Compose: API + Postgres
  - #47 Dockerfile for API + .dockerignore
  - #49 Basic health endpoints with DB check

- Sequenz B: API Platform Baseline
  - #19 ProblemDetails + error pipeline (RFC7807)
  - #20 OpenAPI/Swagger baseline (Bearer, IDs, Tags)
  - #46 API versioning: /api/v1 + route groups
  - #45 CORS policy for MAUI dev
  - #48 CI: dotnet format verification

- Sequenz C: Abschluss M0
  - #50 Developer docs: Getting started v2
  - Schließen per PR: #42 Centralized NuGet, #18 Directory.Build.props + analyzers, #22 CI build/test/coverage

- PR-Strategie
  - Kleine, fokussierte PRs (1–3 Issues), Beschreibung mit „Closes #…“.
  - CI prüft Build/Tests/Format; Merge schließt Issues automatisch.

- Definition of Done (M0)
  - API startet lokal und im Container, verbindet zu Postgres.
  - Health-/Readiness-Endpunkte vorhanden (DB‑Check).
  - Einheitliches Fehlermodell (ProblemDetails), Dokumentation via Swagger.
  - Versionierte Routen (/api/v1) und CORS für MAUI‑Dev.
  - CI grün (Restore/Build/Test/Coverage/Format‑Check).
