# SignalR Design (Lobby & In-Game)

Wir nutzen getrennte, stark typisierte Hubs: `LobbyHub : Hub<ILobbyClient>` und `GameHub : Hub<IGameClient>` (Separation of Concerns). Gruppen laufen weiterhin pro Room.

## Verbindung & Gruppen
- URLs: `/hubs/lobby` (Lobby), `/hubs/game` (In‑Game)
- Auth: Bearer JWT verpflichtend
- Group pro RoomId: `room:{roomId}`
- Reconnect: exponential backoff, clientseitig Statuscache, serverseitig Snapshot bei Rejoin
 - Autorisierung: Connection‑Level `[Authorize]` prüft Token; ressourcenbasierte Checks pro Methode via `[Authorize(Policy=...)]` und `IHubFilter` (siehe 15-Autorisierung-und-Permissions.md)

Hinweis Typisierung
- Server → Client ist über `Hub<ILobbyClient>`/`Hub<IGameClient>` typisiert; Client → Server nutzt die Hub‑Methoden (Wrapper optional im Client).

### Rundenchat (In-Game)
- Subgruppen pro Runde und Rolle für Chat:
  - Basis: `room:{roomId}:round:{roundId}`
  - Rollen: `...:hider`, `...:seeker`
- Senden (Client → Server): `SendChatMessage({ roomId, roundId, audience, content, clientTs, idempotencyKey })`
- Empfangen (Server → Client): `ChatMessagePosted(ChatMessageDto)`
- Snapshot: `GetRecentMessages(roomId, roundId, take)` (optional, serverseitig nach Sichtbarkeit gefiltert)
- Regeln: Nur während aktiver Runde; Audience‑Routing serverseitig (kein Client‑Filter). Details: 17-GameChat.md

## Client → Server (Beispiele)
- `JoinRoom(roomId, displayName)`
- `SetReady(roomId, isReady)`
- `UpdateSettings(roomId, settings)` — Owner
- `StartGame(roomId)` — Owner
- `UpdateLocation(roomId, { lat, lng, acc, ts })` (rate-limited)
- `TagPlayer(roomId, taggedPlayerId, clientTs, idempotencyKey)`
- `LeaveRoom(roomId)`
 - `SendChatMessage(...)` (siehe oben)

## Server → Client (Beispiele)
- `LobbyState(state)` — Vollständiger Snapshot (Owner, Players, Ready-Status)
- `PlayerJoined(player)` / `PlayerLeft(playerId)`
- `SettingsUpdated(settings)`
- `GameStarted(match)` + `RoleAssigned(playerId, role)`
- `TimerTick(phase, remainingSec)`
- `LocationUpdate(playerId, location)` — nur wenn erlaubt/aggregiert
- `PlayerTagged(taggerId, taggedId, roundState)`
- `RoundEnded(summary)` / `MatchEnded(result)`
 - `ChatMessagePosted(message)` (siehe oben)

## Nachrichtenformate (Schema-Auszug)
```json
// LobbyState
{
  "roomId": "...",
  "ownerId": "...",
  "players": [ {"id":"p1","name":"Alex","ready":true} ],
  "settings": {"hideTimeSec":60,"huntTimeSec":300,"tagRadiusM":2.5},
  "state": "Lobby|Countdown|Hide|Hunt|End"
}
```

## Server-Authorität & Integrität
- Alle kritischen Regeln serverseitig prüfen (Geofence, TagRadius, Spielphase)
- Idempotency-Key für Aktionen, um Doppelverarbeitung zu vermeiden
- Rate-Limits: z. B. `UpdateLocation` ≤ 2/s/Client; Backpressure bei Überlast
 - Gruppenbeitritt nur für gültige Room‑Members; bei Ban/RoleChange ggf. proaktives Entfernen/Disconnect

## Skalierung
- Redis Backplane für Gruppen-Broadcasts über mehrere Knoten
- Presence in Redis oder DB, Heartbeats zur Bereinigung verwaister Verbindungen
