# UI‑Lokalisierung – Architekturleitfaden

Ziel: Einheitliche, robuste und testbare Lokalisierung (i18n) für die MAUI‑UI — unabhängig von konkreten Implementierungen.

## Ziele
- Einheitliche Übersetzungen in XAML und Code.
- Laufzeit‑Sprachwechsel ohne App‑Neustart (Live‑Updates).
- Korrekte Pluralisierung/Gender/Formatierung je Kultur.
- Gute Testbarkeit und reibungsloser Übersetzungs‑Workflow.

## Ressourcen‑Struktur
- Pro Feature/Screen eigene Ressourcen:
  - `Resources/Localization/App.resx` (global)
  - `Resources/Localization/StartPage.resx`, `LobbyPage.resx`, …
- Key‑Konvention: `bereich.unterbereich.sinn` (kontextreich; z. B. `lobby.players.count`).
- Neutrale Fallback‑Sprache (z. B. `en`) via `NeutralResourcesLanguage`.
- Kommentare in Resx als Translator‑Hinweise (Kontext, Variablenbedeutung).

## Zugriff in XAML und Code
- XAML: Markup‑Extension mit Dynamic‑Binding, z. B. `{loc Key=greeting, Page=StartPage}` → reagiert auf Sprachwechsel.
- Code: `ILocalizer`
  - Indexer: `localizer["key"]`
  - Formatierung: `localizer.Format("key", args)`
  - Ereignis: `CultureChanged` (für Live‑Update‑Mechanismen)
- Keine String‑Konkatenation; stattdessen formatierbare Keys.

## Kultur & Laufzeit‑Wechsel
- Quelle: System‑Sprache als Default; App‑Override in Settings (persistiert).
- Setze `CultureInfo.CurrentUICulture` und `CultureInfo.CurrentCulture`.
- `LocalizationManager` verwaltet aktuelle Kultur und publiziert `CultureChanged`.
- UI aktualisiert sich über DynamicResource/INPC in der Markup‑Extension.

## Pluralisierung, Gender, Kontext
- Plural: ICU‑ähnliche Regeln oder Library (z. B. SmartFormat) hinter `ILocalizer`.
- Gender/Anrede: eigene Keys oder Platzhalter mit Regeln; klare Translator‑Hinweise.
- Kontextvarianten: Suffixe (`.title`, `.button`, `.hint`) statt Mehrzwecktexte.

## Assets, RTL, Accessibility
- Keine Texte in Bildern; sprachneutrale Icons.
- RTL‑Support: `FlowDirection` kulturabhängig; spiegeln von Layout/Icons, wenn nötig.
- A11y: `AutomationProperties`/`SemanticScreenReader` ebenfalls lokalisiert.
- Lange/kurze Texte: Wrapping/Auto‑Fit; Layouts nicht hart begrenzen.

## Fehler & Server‑Integration
- Server liefert stabile Fehlercodes; Client mappt Code → Ressourcenschlüssel.
- Lokalisierte Fehlermeldungen im UI; Rohtexte nur als Fallback/Log.

## Build & Packaging
- Satelliten‑Assemblies pro Sprache (Android/iOS wählen automatisch).
- Trimming‑freundlich: zentrale Resource‑Typen; `NeutralResourcesLanguage` setzen.
- Optional: Export/Import XLIFF für externe Übersetzungstools.

## Teststrategie
- Pseudo‑Lokalisierung im Dev (Textverlängerung, diakritische Zeichen) für Layout‑Checks.
- Unit‑Checks: Keys vorhanden, keine Duplikate, Pluralfälle (0/1/N) korrekt.
- UI‑Smoke‑Tests in mindestens zwei Sprachen (kurz/lang, z. B. en/de).
- Lint in CI: fehlende/unbenutzte Keys detektieren.

## API‑Skizze (theoretisch)
- `interface ILocalizer { string this[string key] { get; } string Format(string key, params object[] args); event EventHandler CultureChanged; }`
- Markup‑Extension: `{Loc Key=..., Args={Binding ...}}` mit Live‑Update.

## Einführung (schrittweise)
1) Keys pro Screen inventarisieren und in Resx migrieren.
2) Markup‑Extension einführen; XAML schrittweise umstellen.
3) Codezugriffe auf `ILocalizer` migrieren; Formatierung statt String‑Konkatenation.
4) Pluralisierung/Gender zunächst für häufige Flows (Spieleranzahl, Timer, Fehler) aktivieren.
5) Pseudo‑Loc prüfen, Layoutfixes; weitere Sprachen ausrollen.

