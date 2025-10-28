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
- GET `/readyz` — Readiness (DB u. a. Abhängigkeiten)
- Format: `application/health+json` (IETF Entwurf), `status: pass|warn|fail`, `checks: { name: [{ status, time, duration, description, error }] }`

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

## Optimistische Parallelität (ETag)
Um konkurrierende Änderungen sicher zu behandeln, nutzen Endpunkte für partielle Updates (z. B. Profil und Room‑Settings) ETags.

- ETag (Entity Tag): Versionskennzeichen der Ressource im HTTP‑Header `ETag: "vXYZ"`.
- GET: Client erhält das aktuelle ETag im Response‑Header. Optional kann der Client `If-None-Match: "vXYZ"` senden, der Server antwortet dann bei unverändertem Stand mit `304 Not Modified`.
- PATCH: Client sendet das zuletzt bekannte ETag per `If-Match: "vXYZ"` mit. Der Server aktualisiert nur, wenn das ETag noch passt.
  - Bei Abweichung: `412 Precondition Failed` mit ProblemDetails (z. B. `code: concurrency.etag_mismatch`) und optionalem `ETag`‑Header mit dem aktuellen Wert.
  - Ohne If‑Match (Policyabhängig): `428 Precondition Required` möglich, falls der Endpunkt ein ETag zwingend verlangt.

Empfehlungen/Implementierungshinweise
- ETag‑Quelle: Datenbank‑Concurrency‑Token (z. B. PostgreSQL `xmin` oder ein RowVersion/Timestamp Feld). Starke ETags (keine `W/`‑Präfixe) für Concurrency verwenden.
- Header‑Handling: Server setzt `ETag` in erfolgreichen GET/PATCH‑Responses; prüft bei PATCH `If-Match` vor dem Speichern.
- Endpunkte:
  - `PATCH /v1/users/me` (Profil): verlangt `If-Match` zur Absicherung paralleler Änderungen.
  - `PATCH /v1/rooms/{id}/settings` (Owner‑only): verlangt `If-Match`; bei Konflikt 412.
- ProblemDetails: konsistent mit bestehenden Fehlerkonventionen, z. B. `status: 412`, `title: "Precondition Failed"`, `code: "concurrency.etag_mismatch"`.

## Statistiken (privat)
- `GET /api/v1/stats/me` — persönliche, private Statistiken des aktuellen Nutzers (Auth erforderlich).
- `GET /api/v1/rounds/{roundId}/summary` — Runden‑Summary für Teilnehmende.

Hinweise:
- Aggregation erfolgt intern am Rundenende via Service (kein öffentlicher Write‑Endpoint).
- Details und Datenmodell: 16-Statistiken.md
