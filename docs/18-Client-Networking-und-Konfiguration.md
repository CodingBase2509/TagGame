# Client Networking & Konfiguration

Dieses Dokument beschreibt die clientseitige HTTP/SignalR‑Resilienz, JSON‑Konventionen und die Konfiguration.

## JSON‑Konventionen
- Beide Seiten (API/Client) verwenden konsistente System.Text.Json‑Optionen:
  - camelCase Propertynamen
  - Enums als Strings (camelCase)
  - Null‑Werte werden nicht geschrieben
  - TimeSpan: konstantes Format ("c"); beim Lesen zusätzlich Millisekunden zulässig
- Server: `AddSharedJsonOptions()` (Minimal APIs + MVC)
- Client: `IJsonOptionsProvider` nutzt `TagGame.Shared.Json.JsonDefaults`

## Typed HTTP‑Client
- `IApiClient` (Client.Core): JSON‑GET/POST/PATCH/DELETE mit Shared‑Options
- Fehlerbehandlung: `ProblemDetailsHandler` mappt `application/problem+json` auf `ApiProblemException`

## Resilience (Retry/Timeout)
- Optionen: `NetworkResilienceOptions` (Client.Core)
  - HTTP: MaxRetries, BaseDelayMs, MaxDelayMs, TotalRequestTimeoutMs, RetryOn429
  - Hub: ReconnectDelaysMs, MaxReconnectWindowMs
- Konfigurator: `HttpResilienceConfigurator` (Retry + Total‑Timeout)
- Reihenfolge der Handler (Request → Response):
  - ProblemDetails → Authorized → Resilience → Sockets

## SignalR Reconnect
- Policy: `HubRetryPolicyFactory` liefert eine feste Delay‑Sequenz (optional begrenzt durch Fenster)
- Verwendung: `WithAutomaticReconnect(policy)`

## Konfiguration (appsettings)
- Der MAUI‑Client lädt `Resources/Raw/appsettings.json` (Prod) und `appsettings.Development.json` (Debug)
- Schlüssel:
  - `Api:BaseUrl` / `Api:BaseAddress`
  - `Networking:Http` / `Networking:Hub`
- Emulator‑Adressen:
  - Android: `http://10.0.2.2:<port>`
  - iOS Simulator: `http://127.0.0.1:<port>`

## App‑Einstellungen (Preferences)
- `IAppPreferences` publiziert Theme/Language/Benachrichtigungen als Snapshot + Event
- Persistenz: MAUI Preferences (Client/Infrastructure)

