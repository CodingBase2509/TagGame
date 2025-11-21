# Daten, Sicherheit & Privacy (Stand V2)

## Bereits umgesetzt
- **Authentifizierung**: JWT Access + Refresh Rotationen via `AuthTokenService` (`TagGame.Api.Core/Features/Auth`). Tokens enthalten nur `sub` + minimale Claims. Refresh Reuse wird erkannt und mit `401` (`code=refresh_reuse`) beantwortet.
- **Transport & Speicherung**:
  - API erzwingt HTTPS im Prod, Signaturen/Validierungen konfigurieren wir in `Program.cs`.
  - Der Client nutzt `ITokenStorage` (`TagGame.Client.Core/Storage/TokenStorage.cs`) + `IAppPreferences` für lokale Secrets (SecureStorage/KeyChain via Maui Essentials).
- **Fehler-Handling**: `GlobalExceptionHandler` mappt sicherheitsrelevante Fehler auf ProblemDetails (`auth.invalid_token`, `auth.not_member`, ...). Kein Stacktrace-Leak nach außen.
- **Privacy by design**: Aktuell persistieren wir nur Auth-Daten (User, RefreshToken). Standort-, Match- oder Chat-Daten werden noch nicht erhoben.

## Offene Punkte / Backlog
- **Room-/Game-Daten**: Sobald Lobbys aktiv sind, müssen wir Datenminimierung (Polygon, Standort-Historie) und Aufbewahrungsfristen festlegen.
- **Anti-Cheat**: Dokumentierte Heuristiken (Geschwindigkeit, Accuracy, Mock-Location) sind noch nicht implementiert. Sie gehören in die zukünftigen Game-Services (`TagGame.Api.Core/Features/Games`).
- **Rate Limiting**: `RateLimitExceededException` existiert, es gibt aber noch keine produktiven Limiter (REST oder Hubs). Für Room-Endpunkte bitte Tools wie `AddRateLimiter` (fixed window pro UserId) oder serverseitige Buckets in Betracht ziehen.
- **Consent & Monetarisierung**: Ads/Subscription sind noch nicht live. Consent-Flows (UMP, Apple Tracking) sowie Entitlement-Verwaltung (`Entitlement`-Entity) müssen vor Release ergänzt werden.
- **Löschung & Export**: Es gibt noch keinen Endpoint zum Löschen eines Users bzw. zum Export der eigenen Daten. Gehört in den Auth-Bereich, inkl. Cascade auf RefreshTokens/Entitlements.

## Empfehlungen für neue Features
1. **Dateninventar anlegen** (Welche Entität speichert welche personenbezogenen Daten?).
2. **Opt-Ins dokumentieren** (z. B. Standort, Push, Analytics) und zentral in `IAppPreferences` halten.
3. **Server-Validierung** einbauen (z. B. Distanz/Geofence beim Tagging) und Ergebnisse loggen – aber ohne Roh-Standortdaten zu speichern, wenn es nicht nötig ist.
4. **Security-Reviews** im PR: neue Endpoints nur mit Policies + ProblemDetails.

Dieses Dokument sollte mit jedem sicherheitsrelevanten Feature-Commit aktualisiert werden.
