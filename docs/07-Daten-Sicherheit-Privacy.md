# Daten, Sicherheit & Privacy

## Datenschutzprinzipien
- Datenminimierung: Speichere nur notwendige Standortdaten und nur so lange wie nötig
- Transparenz: Klare Erklärungen, Opt-in/Opt-out Optionen (Analytics, Ads Consent)
- Löschkonzepte: Benutzer kann Profil/Verlauf löschen; Standort-Event-Historie kurzlebig

## Rechtliches
- GDPR/DSGVO: Einwilligungen für Standort, Ads-Personalisierung (EEA via UMP)
- Altersgrenzen/Elternhinweise je nach Region; keine Daten an Dritte ohne Einwilligung

## Auth & Transport
- JWT (kurzlebig, Refresh-Token), `Authorization: Bearer ...`
- SignalR Hubs nur mit Token; Claims-Validierung serverseitig
- Speicherung auf Gerät: Secure Storage/Keychain/Keystore

## Anti-Cheat / Integrität
- Geschwindigkeits-Heuristik (z. B. > 10 m/s → verdächtig)
- Schlecht-genaue Messungen ggf. nicht werten (Accuracy > Schwellwert)
- Mindestintervall zwischen Tags; Distanzprüfung serverseitig gegen letzte gültige Positionen
- Optional: Mock-Location-Erkennung (Android), Jailbreak/Root-Hinweise

## Ratenbegrenzung & DoS-Schutz
- Per-IP und per-User Rate Limits (REST, Hub-Methoden)
- Backpressure & Drop-Strategie für `UpdateLocation`

## Observability
- Strukturierte Logs (Korrelation per RoomId/PlayerId), PII-reduziert
- Metriken: Hub-Verb., Broadcast-Latenz, Fehlerquoten, Rate-Limit-Hits

