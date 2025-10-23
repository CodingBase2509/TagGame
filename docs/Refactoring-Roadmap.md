# Refactoring- und Verbesserungs-Roadmap

Stand: 2025-10-22

Dieses Dokument bündelt geplante Refactorings, Optimierungen und Qualitätsmaßnahmen für das TagGame‑Projekt. Es ist als lebende Roadmap gedacht.

## Cross‑Cutting
- Einheitliche Build‑Standards via `Directory.Build.props/targets` — gleiche Paketversionen, `TreatWarningsAsErrors` für Kernprojekte.
- Starke Analyzers (+ `dotnet format`, `.editorconfig`) — Stil und Fehler früh automatisiert erkennen.
- OpenTelemetry (Logs/Metrics/Traces) + Korrelations‑IDs — schnellere Ursachenanalyse und Monitoring.
- Konsistentes Fehler‑/Resultat‑Modell (z. B. `Result<T>`) — weniger If/Else, klarere Fehlercodes.
- `System.Text.Json` Source‑Generator (`JsonSerializerContext`) — schneller, trimming‑freundlich, weniger Reflection.
- Internationalisierung: `ILocalizer` + Markup‑Extension, Live‑Sprachwechsel, Pluralisierung; Pseudo‑Lokalisierung im Dev (siehe 14-Lokalisierung.md)

## TagGame.Api
- Endpoints in Feature‑Gruppen + Endpoint Filters — klare Struktur, DRY für Validierung/Auth.
- FluentValidation + ProblemDetails — saubere 400‑Antworten mit Details.
- Globales Fehler‑/Exception‑Handling in `Middleware/` — konsistente Fehlerkarten, keine Leaks.
- API Versioning + sauberes OpenAPI — sichere Weiterentwicklung, gute Doku.
- Resilienz/Performance: Rate Limiting, Response Caching/ETag, Gzip, Health Checks — stabiler und schneller.
- DI‑Komposition: `services.AddCore(configuration)` aus TagGameApi.Core; API bleibt dünn (Transport/Policies/Docs).

## TagGameApi.Core
- Feature‑Services pro Bereich (Game, Player, Rooms) anstelle striktem CQRS — einfacher Einstieg, gute Testbarkeit.
- EF Core: DbContext‑Pooling, Repositories, Konfigurationen; Migrations unter `Migrations/` im Core.
- Abstraktionen (Ports): Repositories, Clock/IdGenerator, externe Integrationen (optional) — saubere Trennung.
- Validierung (FluentValidation) + Mapping (Mapster/manuell) — Domain ↔ DTOs/Contracts.
- Optionale Outbox/Domain‑Events bei Bedarf — erst aktivieren, wenn Integrationen es erfordern.
 - Autorisierung: Bitflags‑Permissions + Membership‑Modell; dynamischer `IAuthorizationPolicyProvider` (z. B. `RoomPermission:*`), Endpoint‑Filter zum Membership‑Laden, kurze Caches; Details: 15-Autorisierung-und-Permissions.md

## TagGame.Client.Core
- MVVM mit `CommunityToolkit.Mvvm` (ObservableObject, AsyncRelayCommand) — weniger Boilerplate, sehr testbar.
- Services: getypte `HttpClientFactory` + Polly (Retry/Timeout/CircuitBreaker) — robuste Netzlogik.
- Klare Domänen‑ vs. DTO‑Modelle + Mapper — UI bleibt dünn, keine Leaks.
- Einheitliches State‑Pattern (Loading/Content/Error) — vorhersehbare UI‑Zustände.
- Hohe Testabdeckung für Services/ViewModels (Fakes, Clock/Time abstrahieren) — schnelle, stabile Tests.
- Benachrichtigungen: Einführung `IToastPublisher` (Core) + `ToastPresenter/ToastHost` (UI) mit Queue/Priorität; Ablösen der PageBase‑Kopplung. Details: 13-Client-Benachrichtigungen.md

## TagGame.Client (MAUI)
- Shell‑Navigation + zentrale Navigations‑Abstraktion — weniger Fragilität, Deep Links einfacher.
- Performance: AOT + Trimming, Bild‑Optimierung, Virtualisierung großer Listen — flüssige UI.
- UX: Dark Mode, leere Zustände, Skeletons, konsistente Fehler‑Toasts/Dialoge — bessere Wahrnehmung.
- Accessibility + Lokalisierung (Resx) — breitere Nutzbarkeit, Internationalisierung.
- Offline‑Strategie (Cache/LocalStorage) + Konnektivitäts‑Hinweise — nutzbar bei schlechter Verbindung.

## TagGame.Shared
- DTOs als `record` + versionsfähige DTO‑Pakete — Erweiterbarkeit ohne Brüche.
- Streng typisierte IDs/Enums statt Strings — weniger Fehler durch Magic‑Strings.
- Zeit/Zeitzonen: durchgehend `DateTimeOffset` (UTC) — korrekte Zeiten überall.
- Gemeinsame Json‑Kontexte (`JsonSerializable`) hier zentral — Client/API teilen Serialisierung.

## Tests
- Unit: Services/ViewModels isoliert, Data‑Builder/Fixtures — schnelle, zielgerichtete Checks.
- Integration API: `WebApplicationFactory` + Testcontainers (Postgres) — realistische End‑zu‑Ende‑Pfade.
- Contract‑Tests gegen OpenAPI (generierter Client) — keine Schema‑Drift zwischen Client/API.
- MAUI UI‑Smoke‑Tests für Kernflows — Schutz vor Regressionen.
- Coverage‑Gate für Kernprojekte — Fokus auf das Wesentliche.

## Build & CI/CD
- GitHub Actions: Restore/Build/Test, Coverage‑Upload, Artefakte — zuverlässige PR‑Checks.
- Docker‑Images für API, Migrations‑Check im CI, `docker compose` Smoke‑Test — deploy‑fähig.
- Mobile: PR‑Builds (Compile‑Only), Release‑Pipelines mit Signierung — reproduzierbare App‑Releases.
- Dependabot/Renovate + Security‑Scans (Container/Deps) — aktuelles, sicheres System.
- Pre‑commit Hooks (format, analyzers, schnelle Tests) — weniger rote Builds.
