# In-Game Chat (Backlog)

Der Game-Chat wurde noch nicht implementiert. `GameHub.SendChatMessage` ist lediglich ein Platzhalter mit Policy-Attributen. Dieses Dokument beschreibt weiterhin die Anforderungen, damit der spätere Slice konsistent bleibt.

## Anforderungen
- Chat ist nur während aktiver Runden verfügbar (`Hide`/`Hunt`).
- Zielgruppen: `All`, `Hider`, `Seeker` (Server filtert, Clients vertrauen nicht auf lokale Checks).
- Snapshot für Rejoiner: letzte N Nachrichten (z. B. 50) je `roomId+roundId`.
- Limits: Rate-Limiting pro Spieler (z. B. 5 Nachrichten / 5 Sekunden) + Längenbegrenzung.

## Server TODO
1. Feature-Service `ChatService` unter `TagGame.Api.Core/Features/Chat` (Validierung, Audience-Routing, optionale Persistenz/Redis TTL).
2. DTOs in `TagGame.Shared/DTOs/Chat` (z. B. `ChatMessageDto`).
3. Hub-Methoden in `TagGame.Api/Hubs/GameHub.cs` implementieren (inkl. Gruppenmanagement, Idempotency-Key).
4. Policies erweitern (`RoomPermission:Chat`), falls feineres Rechtemodell notwendig ist.
5. Tests (Unit + Hub-Integration), inkl. Rate-Limit/ban-Szenarien.

## Client TODO
- UI-Komponenten (Composer, Message-List, Audience-Picker) unter `TagGame.Client/Ui/Views/Game`.
- ViewModels + Services im Client.Core (`ChatViewModel`, `IChatService`), inklusive Offline-Recovery (Fetch Snapshot nach Reconnect).
- Lokalisierte Fehlermeldungen (`docs/14-Lokalisierung.md`) und Toasts bei Rate-Limits (`docs/13-Client-Benachrichtigungen.md`).

Bitte dieses Dokument aktualisieren, sobald die Implementierung startet.
