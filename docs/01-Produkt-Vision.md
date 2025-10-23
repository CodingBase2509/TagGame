# Produkt-Vision und Ziele

## Kurzbeschreibung
TagGame ist eine mobile App mit Backend für ein ortsbasiertes "Verstecken & Fangen"-Spiel. Spieler:innen erstellen Lobbys, treten bei, definieren ein Spielgebiet und spielen rundenbasiert als Sucher:in oder Verstecker:in. Die App kombiniert eine REST API für Setup/Metadaten mit SignalR für Lobby- und In-Game-Echtzeit.

## Zielgruppe
- Freundesgruppen, Klassen, Jugendgruppen, Events (Indoor/Outdoor)
- Casual Gaming unterwegs mit minimaler Einstiegsbarriere

## Wertversprechen
- Schneller Start: Lobby teilen, beitreten, losspielen
- Faires, server-autoritatives Gameplay mit Geofencing
- Reibungsloser Echtzeit-Abgleich (Presence, Countdown, Rollenwechsel)

## Erfolgsmetriken (Beispiele)
- Tägliche aktive Nutzer (DAU) und Retention (D1/D7)
- Durchschnittliche Runden/Session, Match-Abbruchrate < 5%
- Durchschnittliche Lobbygröße, Beitrittszeit < 5s
- Battery drain < 6% pro 15 Minuten aktiver Runde (mittlere Geräteklasse)
- Monetarisierung: ARPDAU und A/B-Test-Ergebnisse für Ads/Subscription

## Leitprinzipien
- Mobile-first, Offline-tolerant, Realtime-first
- Server-authoritative Game-State; Clients sind View/Controller
- Datenschutz-by-Design (Standortdaten sind sensibel; Minimierung + Transparenz)
- Iterativ erweiterbar: Start als modularer Monolith, skalierbar mit Backplane

