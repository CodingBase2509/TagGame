# REST API Design (Setup/Metadaten)

Basierend auf Minimal APIs. Auth per JWT (anonymes Bootstrap + optionaler Accountname). Responses mit ProblemDetails bei Fehlern.

## Auth & Nutzer
- POST `/users` — Nutzerkonto/Profil anlegen
  - Request: `{ "displayName": "Alex" }`
  - Response: `{ "userId": "...", "token": "jwt..." }`

- POST `/tokens/refresh` — Token erneuern
  - Request: `{ "refreshToken": "..." }`
  - Response: `{ "token": "jwt..." }`

## Lobbys / Räume
- POST `/rooms` — Raum erstellen
  - Body: `{ "settings": { "hideTimeSec": 60, "huntTimeSec": 300, "tagRadiusM": 2.5 }, "boundaries": { "polygon": [[lat,lng], ...] } }`
  - Response: `{ "roomId": "...", "code": "ABCD" }`

- GET `/rooms/code/{code}` — Raum via Code finden
  - Response: `{ "roomId": "...", "ownerId": "...", "players": [...], "settings": {...} }`

- POST `/rooms/{roomId}/join` — Raum beitreten
  - Body: `{ "rolePreference": "auto|seeker|hider" }`
  - Response: `{ "membershipId": "..." }`

- POST `/rooms/{roomId}/leave`
- PATCH `/rooms/{roomId}/settings` — nur Owner

## Diagnostics
- GET `/healthz` — Liveness
- GET `/readyz` — Readiness

## Autorisierung (Policies)
- Endpoints deklarieren Policies statt Rollen:
  - Beispiel: `[Authorize(Policy = "RoomPermission:StartGame")]` für Raumstart.
- Membership‑Kontext (User × Room) wird vor der Autorisierung geladen (Endpoint‑Filter/Middleware) und kurz gecacht.
- Fehlerkonventionen: 403 mit ProblemDetails `code` wie `auth.not_member`, `auth.missing_permission`.
- Details: 15-Autorisierung-und-Permissions.md

## Fehlerkonventionen
- 400 Validierungsfehler (FluentValidation → ProblemDetails)
- 401/403 für Auth/Autorisierung
- 409 für Konflikte (z. B. Raum voll)
- 429 Throttling
