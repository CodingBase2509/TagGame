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

## Implementierung (Stand V2)

Komponenten und Pfade:
- Interface (Core): `TagGame.Client.Core/Localization/ILocalizer.cs`
  - `GetString(key)` und `GetFormat(key, params object[] args)`
  - `SetCultureAsync(CultureInfo)` + `CultureChanged` für Live‑Updates
- Katalog (Client): `TagGame.Client/Infrastructure/Localization/ResxCatalog.cs`
  - Liest `.resx` über `ResourceManager` (z. B. `App.resx`, `App.de.resx`)
- Localizer (Client): `TagGame.Client/Infrastructure/Localization/Localizer.cs`
  - Nutzt SmartFormat (`SmartFormat`, `SmartFormat.Extensions`) für Plural/Choose/Conditional
- Markup‑Extension (Client): `TagGame.Client/Ui/Extensions/LocExtension.cs`
  - XAML: `{loc Key=...}` bzw. `{loc Key=..., Args='arg1|arg2|...'}`
  - `Args` ist ein einzelner String; wird mit `|` getrennt und typisiert (Zahl/Bool → automatisch erkannt)
- Initializer (Client):
  - `TagGame.Client/Infrastructure/Localization/LanguageMap.cs` (Enum → CultureInfo)
  - `TagGame.Client/Infrastructure/Localization/LocalizationInitializer.cs` (setzt Kultur beim Start und bei `IAppPreferences.PreferencesChanged`)
- DI (Client): `TagGame.Client/Infrastructure/DependencyInjection.cs`
  - Registriert `ILocalizationCatalog` und `ILocalizer`

Ressourcen & Build:
- Resx‑Dateien: `TagGame.Client/Resources/Localization/App.resx`, `App.de.resx`
- CSProj enthält die `EmbeddedResource`‑Einträge und Designer‑Generierung (`ResXFileCodeGenerator`).

## SmartFormat – Muster

- Plural (empfohlen):
  - en: `lobby.players.count = "{0:plural:zero|no players|one|{0} player|other|{0} players}"`
  - de: `lobby.players.count = "{0:plural:one|{0} Spieler|other|{0} Spieler}"`
  - Aufruf im Code: `localizer.GetFormat("lobby.players.count", count)`
  - XAML mit Markup: `<Label Text="{loc Key=lobby.players.count, Args='3'}" />`

- Auswahl (Choose):
  - en: `profile.greeting = "{0:choose(male|female|other):Welcome, Mr {1}|Welcome, Ms {1}|Welcome, {1}}"`
  - XAML: `<Label Text="{loc Key=profile.greeting, Args='male|John'}" />`

Hinweise:
- Platzhalter sind 0‑basiert (`{0}`, `{1}`, ...). Bei Plural ist `{0}` typischerweise die Anzahl.
- Literal‑Klammern in Texten werden doppelt geschrieben (`{{` bzw. `}}`).

## XAML‑Nutzung (LocExtension)

Namespace deklarieren (oben im XAML):
`xmlns:loc="clr-namespace:TagGame.Client.Ui.Extensions"`

Beispiele:
- Statisch: `<Label Text="{loc Key=app.title}" />`
- Plural Zahl: `<Label Text="{loc Key=lobby.players.count, Args='5'}" />`
- Mehrere Argumente: `<Label Text="{loc Key=profile.greeting, Args='male|John'}" />`

Args‑Parsing:
- `Args` ist ein einzelner String; Trennzeichen ist immer `|`.
- Tokens werden zu `int/long/decimal/double/bool` geparst, sonst `string`.

## Laufzeit‑Sprachwechsel

- Präferenzen: `IAppPreferences` hält `Language` und publiziert `PreferencesChanged`.
- Initialisierung: `LocalizationInitializer.InitializeAsync()` beim App‑Start aufrufen; setzt die Kultur und reagiert auf künftige Änderungen.
- Manuell (z. B. Settings‑Seite): `await IAppPreferences.ChangeLanguageAsync(Language.German)` reicht — der Initializer synchronisiert die Kultur und UI aktualisiert sich über `CultureChanged`.

## Tests – Leitlinien

- Localizer:
  - Fallback: unbekannter Key → `"[key]"`
  - `GetFormat` mit Plural/Choose liefert sprachspezifische Varianten
  - `SetCultureAsync` feuert `CultureChanged`
- Markup‑Extension:
  - Args‑Parsing ("3" → Zahl; "male|John" → zwei Tokens)
  - Für UI‑Tests reicht ein kleiner Smoke‑Test in zwei Sprachen (en/de)

