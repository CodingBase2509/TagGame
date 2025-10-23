# Monetarisierung: Ads & Subscription

## Ziele
- Nicht-invasives, faires Modell: Ads außerhalb intensiver Spielphasen
- Subscription schaltet Ads aus und bietet optionale Komfortfeatures

## Google Ads (AdMob)
- Platzierung:
  - Interstitial: zwischen Runden/Match-Ende
  - Rewarded: freiwillig für Hinweis (z. B. Richtungsping für Sucher)
  - Banner: nur im Lobby/Room-Screen (optional, zurückhaltend)
- Steuerung:
  - Remote Config (z. B. App Config via REST): Frequenz, Caps, Aktivierung/Deaktivierung
  - A/B-Tests: Placement/Timing
- Compliance:
  - EEA Consent (UMP SDK), Non-Personalized Ads fallback
  - COPPA/Altersabfrage je nach Zielregion

## Subscription/Premium
- Features:
  - Entfernt Ads
  - Komfort: Erweiterte Statistiken, Custom-Avatare, Private Rooms+, größere Raumgrößen
- Technik:
  - Store Billing (iOS/Android) → Server-seitige Quittungsprüfung
  - Server verwaltet Entitlements, Token an Client ("premium": true)
  - Grace Period & Offline-Cache des Status

## Robustheit
- Ad-Load-Fehler: UI fällt elegant zurück (kein Blockieren von Flows)
- Entitlements bei Reconnect validieren; Caching zur Minimierung von Latenz

## KPIs & Experimente
- ARPDAU, Fill-Rate, eCPM, Time-to-Ad
- Retention vs. Ad-Intensität, A/B-Test-Auswertungen

