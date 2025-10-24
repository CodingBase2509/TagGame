# In‑Game Chat (Rundenchat)

Ziel: Leichter, rollenbasierter Chat innerhalb einer laufenden Runde. Spieler können Nachrichten an verschiedene Zielgruppen senden: `@all` (alle), `@hider` (nur Hider), `@seeker` (nur Seeker). Nicht Bestandteil des MVP, aber zeitnah danach.

## Scope & Nicht‑Ziele
- Scope (erste Iteration):
  - Chat ist nur während einer aktiven Runde nutzbar (Hide/Hunt).
  - Zielgruppen: `@all`, `@hider`, `@seeker`.
  - Letzte N Nachrichten (z. B. 50–200) für (Re)Joiner der laufenden Runde verfügbar.
  - Rate‑Limiting und Längenbegrenzung pro Nachricht.
- Nicht‑Ziele (später):
  - Threads, Reactions, Anhänge, Mentions einzelner User, Moderations‑UI.
  - Persistenz über lange Zeiträume (Privacy/Scope beachten).

## Transport & Routing (SignalR)
- Hub: Erweiterung des bestehenden Game‑Hubs oder separater `ChatHub` (Empfehlung: Teil des Game‑Hubs, eigene Methodennamen).
- Gruppenstrategie:
  - Basisgruppe pro Raum und Runde: `room:{roomId}:round:{roundId}`
  - Rollen‑Subgruppen: `...:hider`, `...:seeker`
  - Server weist Verbindungen beim Rundenstart/Role‑Assign den Subgruppen zu; bei Leave/Ban/Role‑Change entfernen/aktualisieren.
- Methoden (Client → Server):
  - `SendChatMessage({ roomId, roundId, audience, content, clientTs, idempotencyKey })`
    - `audience` ∈ `All|Hider|Seeker`
    - Validierungen: AuthZ (Room‑Membership), Runde aktiv, Rate‑Limit, `content` Länge, gesittete Zeichen (z. B. keine Steuerzeichen)
- Events (Server → Client):
  - `ChatMessagePosted(ChatMessageDto)` an passende Gruppe(n)
  - Optional: `RecentChat(ChatMessageDto[])` als Antwort auf Snapshot/Fetch
- Snapshot/Fetched History:
  - Methode: `GetRecentMessages(roomId, roundId, take = 50)` (Server filtert nach Sichtbarkeit des Callers)

## Sicherheits-/Autorisierungsregeln
- Verbindung: `[Authorize]` auf Hub; pro Methode ressourcenbasierte Checks (siehe 15-Autorisierung-und-Permissions.md).
- Senden: nur aktive Room‑Members, Runde muss im State Hide/Hunt sein; Optionale Policy `RoomPermission:Chat`.
- Sichtbarkeit: Server sendet basierend auf `audience` in die jeweiligen Gruppen (`All → base`, `Hider → :hider`, `Seeker → :seeker`). Kein Client‑seitiges Filtern zur Sicherheit.
- Missbrauchsschutz: Rate‑Limit z. B. 5 Nachrichten/5 Sekunden pro User; Längenlimit z. B. 280 Zeichen; Logging + Metriken.

## Datenmodell & Persistenz
- DTO (über die Leitung): `ChatMessageDto`
  - Felder: `id`, `roomId`, `roundId`, `senderId`, `senderName`, `roleAtSend`, `audience`, `content`, `sentAtUtc`
- Persistenz‑Optionen:
  - A) Ephemer (empfohlen für Start): In‑Memory oder Redis‑List je `room:round` mit TTL (z. B. 24h) für Rejoin‑Snapshot; Multi‑Node‑Betrieb → Redis.
  - B) DB‑Tabelle `ChatMessage` (Schema `games`): langlebiger, dafür Privacy/Retention beachten; Cleanup‑Job + Indexe.
- Idempotenz: `idempotencyKey` pro Sender + Round, Unique‑Index zur Doppelvermeidung.

## Server‑Platzierung (Repo)
- Hub/Methoden: `TagGame.Api/Hubs/GameHub.cs` (oder `ChatHub.cs`), Registrierung in Startup.
- Feature‑Logik: `TagGame.Api.Core/Features/Chat/ChatService.cs` (Rate‑Limits, Validation, Routing, Optional Persistenz/Cache).
- Abstraktionen/Repos: `TagGame.Api.Core/Abstractions/Chat/*` + `Persistence/EfCore/Repositories/Chat/*` (falls DB).
- DTOs: `TagGame.Shared/DTOs/Chat/*`.

## Client (MAUI)
- UI: Overlay/Panel während Hide/Hunt; Composer mit Audience‑Picker (`@all/@hider/@seeker`, Default: `@all`).
- Anzeige: Badge/Icon für Audience; Autoscroll; Diskrete Töne/Vibration optional.
- Offline/Recovery: Bei Reconnect `GetRecentMessages` abrufen; Duplikate via `id` filtern.

## Tests
- Unit: `ChatService` — Audience‑Routing, Rate‑Limits, Idempotenz, Längen‑/Inhaltsvalidierung.
- Integration: Hub‑Methoden — 200/403 je nachdem, ob Membership besteht; Empfang nur in korrekten Subgruppen.
- Last: Broadcast‑Pfad (All vs. Hider/Seeker) unter Backplane (Redis) evaluieren.

## Observability & Betrieb
- Logs: Sendeversuche (abgedrosselt), Drops wegen Limits/Policy, Fehler.
- Metriken: Nachrichten/s, Rate‑Limit‑Hits, Audience‑Verteilung.
- Retention: bei Variante A TTL in Redis; bei Variante B Cleanup‑Job (z. B. 24–72h).
