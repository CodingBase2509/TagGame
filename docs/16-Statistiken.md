# Spielstatistiken (privat) & Rundenabschluss

Ziel: Spieler nachhaltig motivieren durch persönliche, private Statistiken sowie eine kompakte Runden‑Summary direkt nach dem Spielende. Öffentliches Leaderboard ist bewusst „später“ und nicht Teil des MVP.

## Scope & Nicht‑Ziele
- Scope (MVP):
  - Persistente, persönliche Statistiken pro Nutzer (kumulative Zähler, letzte Aktivität).
  - Runden‑Summary: kompakte Auswertung für alle Teilnehmenden am Rundenende.
- Nicht‑Ziele (später):
  - Öffentliche Leaderboards (global/raumbezogen).
  - Achievements/Badges, Seasons/ELO.

## Ereignisfluss & Aggregation
- Auslöser: „Runde beendet“ (oder „Spiel beendet“) im Game‑Flow.
- Aggregation: `StatsService` verarbeitet das Ereignis, berechnet inkrementelle Kennzahlen und persistiert diese idempotent.
- Idempotenz: pro `RoundId`/`GameId` nur einmal zählen (Unique‑Index auf Verarbeitungsmarke).

## Datenmodell (Vorschlag)
- UserStats (kumulativ, pro Nutzer):
  - `UserId`, `RoundsPlayed`, `RoundsWon`, `HiderCount`, `SeekerCount`, `TagsGiven`, `TagsReceived`, `AvgSurvivalMs`, `WinStreak`, `LastPlayedAt`.
- RoundStats (Summary pro Runde):
  - `RoundId`, `GameId`, `DurationMs`, `WinningSide`.
- RoundPlayerStat (Teilnehmerdaten pro Runde):
  - `RoundId`, `UserId`, `Role`, `Survived`, `TagsGiven`, `TagsReceived`.

DTOs (Auszug):
- `StatsMySummaryDto`: oberes Kachel‑Set für Profil/„Meine Stats“.
- `RoundSummaryDto`: Dauer, Gewinnerseite, pro Spieler Rolle/Survival/Tags.

## API‑Design (MVP)
- `GET /api/v1/stats/me` — persönliche, private Statistiken des aktuellen Nutzers.
- `GET /api/v1/rounds/{roundId}/summary` — Runden‑Summary für Teilnehmende.

Intern (keine öffentlichen Writes):
- Ereignis‑Hook/Service am Rundenende triggert Aggregation; kein separater öffentlicher Endpoint nötig.

Autorisierung/Privacy:
- Eigene Stats: immer sichtbar (eingeloggt).
- Runden‑Summary: sichtbar für Teilnehmende; später ggf. Policy für Besitzer/Moderation.

## Client‑Integration (MAUI)
- Rundenabschluss‑Screen: zeigt `RoundSummaryDto` (z. B. Gewinner, eigene Leistung, kleine Highlights/Toasts „Neues PB!“).
- „Profil/Meine Stats“‑Ansicht: lädt `GET /stats/me` und zeigt Kacheln/Trend.
- Benachrichtigungen: via `IToastPublisher` dezente Hinweise auf neue Bestwerte (siehe 13-Client-Benachrichtigungen.md).

## Platzierung im Repo
- API Endpoints: `TagGame.Api/Endpoints/Stats/*` (Route Group `stats`), `rounds/{id}/summary` ggf. unter `Rounds`.
- Service/Logik: `TagGame.Api.Core/Features/Stats/StatsService.cs` (+ optional Reader für komplexere Abfragen).
- Abstraktionen/Repos: `TagGame.Api.Core/Abstractions/Stats/*` bzw. `Persistence/EfCore/Repositories/Stats/*`.
- EF Core: `TagGame.Api.Core/Persistence/EfCore/Configurations` + `Migrations`.
- DTOs: `TagGame.Shared/DTOs/Stats/*`.
- Client‑VMs: `TagGame.Client.Core/Ui/ViewModels/Stats/*`.

## Teststrategie (Kurz)
- Unit: Aggregation im `StatsService` (Idempotenz, Kantenfälle 0/1/N, Streaks).
- Integration: Endpoints `GET /stats/me` (AuthZ 200/401), `GET /rounds/{id}/summary` (nur Teilnehmende 200, andere 403).
- Client: VM‑Tests für Mapping/Formatierung; Pseudo‑Lokalisierung (siehe 14-Lokalisierung.md) für Texte.

## Ausblick (später)
- Leaderboards (gesamt, 7/30 Tage, pro Room) mit Opt‑in/Privacy‑Kontrollen.
- Achievements/Badges, Saison‑Resets, optionales Rating/ELO.
