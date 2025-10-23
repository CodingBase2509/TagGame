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

