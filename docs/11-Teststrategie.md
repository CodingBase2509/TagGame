# Teststrategie

## Ebenen
- Unit-Tests: Domain-Regeln (Geofence, Distanz/Tag, State-Machine)
- Integrations-Tests: REST + EF Core (Testcontainers/Postgres), Validierungsfehler
- SignalR-Tests: In-Memory/TestServer – Join/Leave, Broadcast, Reconnect, Idempotenz
- UI-Tests (leichtgewichtig): ViewModels (Client.Core)
- Benachrichtigungen: ViewModels publizieren über `IToastPublisher` (Fake/Spy prüfen Requests); Presenter/Host separat mit UI-Tests (Timing/Animation minimal mocken)
- Lokalisierung: Pseudo‑Lokalisierung für Layout‑Robustheit; Unit‑Checks auf fehlende/duplizierte Keys; Plural‑Cases (0/1/N) testen
 - Autorisierung: Unit‑Tests für Policy‑Handler (versch. Rollen/Masks/Ban); Integrationstests für `[Authorize(Policy=...)]` (200/403); SignalR‑Methoden‑Guards
- Optional Lasttests: k6/Locust-Skripte gegen Staging (ohne echte Standortdaten)

## Tooling
- xUnit, FluentAssertions, Moq
- Testcontainers für DB, ggf. Redis
- Code Coverage Berichte

## Leitlinien
- Tests nahe der Logik (Client.Core für VM/Clients)
- Eindeutige, reproduzierbare Seeds/Testdaten
- Kein Flaky: Timeouts/Timer testbar kapseln (Abstraktion/Clock)
