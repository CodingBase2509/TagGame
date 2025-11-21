# Statistiken & Rundenabschluss (Backlog)

Es existieren noch keine Entities, Services oder Endpoints für Statistiken. Dieses Dokument dient als Platzhalter für das geplante Feature und fasst die Anforderungen zusammen.

## Zielbild
- Private Spielerstatistiken (`GET /v1/stats/me`).
- Runden-Summary für Teilnehmende (`GET /v1/rounds/{roundId}/summary`).
- Aggregation wird serverseitig ausgelöst, wenn eine Runde endet – kein öffentlicher Write.

## Offene Arbeitspakete
1. **Domain & Persistenz** – Tabellen `UserStats`, `RoundStats`, `RoundPlayerStats` im `games`-Schema mit Idempotenz (Unique pro RoundId) und Cleanup-Strategien.
2. **Service-Layer** – `StatsService` in `TagGame.Api.Core/Features/Stats` verarbeitet Game-Events, Aggregationslogik + Tests.
3. **API** – Carter-Module `StatsModule`/`RoundsModule` mit Policies (nur Owner/Teilnehmende) und ProblemDetails.
4. **Client** – ViewModels + Screens für Profil-Kacheln und Rundenabschluss, inkl. Toast-Hinweis bei neuen Bestwerten.
5. **Tests** – Unit (Aggregation, Streaks), Integration (AuthZ 200/403), Client-VMs (Mapping/Formatting).

Bitte erst anpacken, wenn Rooms/Matches produktiv laufen und relevante Daten existieren.
