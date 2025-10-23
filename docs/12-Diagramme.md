# Diagramme (Abläufe & Struktur)

## Architektur (Übersicht)
```mermaid
graph LR
  A[MAUI App] -->|REST| B[ASP.NET Core API]
  A -->|SignalR| C[SignalR Hubs]
  B --> D[(PostgreSQL)]
  C -. scale .- E[(Redis Backplane)]
```

## Sequence: Nutzer registriert sich & erstellt einen Raum
```mermaid
sequenceDiagram
  participant App
  participant API
  App->>API: POST /users {displayName}
  API-->>App: 200 {userId, token}
  App->>API: POST /rooms {settings, boundaries}
  API-->>App: 200 {roomId, code}
```

## Sequence: Lobby beitreten & Ready
```mermaid
sequenceDiagram
  participant App
  participant Hub as LobbyHub
  App->>Hub: Connect (JWT)
  App->>Hub: JoinRoom(roomId)
  Hub-->>App: LobbyState(...)
  App->>Hub: SetReady(true)
  Hub-->>App: LobbyState(players.ready)
```

## Sequence: Spielstart & Phasen
```mermaid
sequenceDiagram
  participant Owner
  participant Hub as LobbyHub
  Owner->>Hub: StartGame(roomId)
  Hub-->>Owner: GameStarted, RoleAssigned
  Hub-->>Others: GameStarted, RoleAssigned
  loop Timer
    Hub-->>All: TimerTick(phase, remaining)
  end
  Note over Hub: Server prüft Regeln (Geofence, Radius)
```

## Sequence: Tag-Ereignis
```mermaid
sequenceDiagram
  participant Seeker
  participant Hub as LobbyHub
  Seeker->>Hub: TagPlayer(taggedId, key)
  Hub-->>Seeker: PlayerTagged(...)
  Hub-->>Others: PlayerTagged(...)
```

