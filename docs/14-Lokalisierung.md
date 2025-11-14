# Lokalisierung (Stand V2)

Die Laufzeit-Lokalisierung ist bereits verdrahtet. Dieses Dokument erklärt die Komponenten und wie neue Texte/Sprachen ergänzt werden.

## Komponenten
- `ILocalizer` (`TagGame.Client.Core/Localization/ILocalizer.cs`) – API mit `GetString`, `GetFormat`, `SetCultureAsync`, `CultureChanged`.
- `Localizer` (`TagGame.Client.Core/Localization/Localizer.cs`) – nutzt `SmartFormat` mit `PluralLocalizationFormatter`, `ChooseFormatter`, `ConditionalFormatter`, `ListFormatter`, `TimeFormatter`.
- `ILocalizationCatalog` (`Client.Core/Localization/ILocalizationCatalog.cs`) + `ResxCatalog` (`TagGame.Client/Infrastructure/Localization/ResxCatalog.cs`) – liest eingebettete `.resx` Dateien.
- Ressourcen: `TagGame.Client/Resources/Localization/App.resx` + `App.de.resx` (weitere Sprachen analog). Designer-Dateien werden automatisch generiert.
- `LocalizationInitializer` (`Client/Infrastructure/Localization/LocalizationInitializer.cs`) – setzt Kultur anhand `IAppPreferences` und reagiert auf Änderungen (Event `PreferencesChanged`).
- `LocExtension` (`TagGame.Client/Ui/Extensions/LocExtension.cs`) – XAML-Markup `{loc Key=..., Args='...'}` mit Live-Updates.

## Verwendung
### In ViewModels/Services
```csharp
public sealed class LobbyViewModel
{
    private readonly ILocalizer _loc;
    public LobbyViewModel(ILocalizer loc) => _loc = loc;

    public string Title => _loc.GetString("lobby.title");
    public string PlayersLabel(int count) => _loc.GetFormat("lobby.players.count", count);
}
```

### In XAML
```xaml
<ContentPage xmlns:loc="clr-namespace:TagGame.Client.Ui.Extensions">
  <Label Text="{loc:Loc Key=lobby.title}" />
  <Label Text="{loc:Loc Key=lobby.players.count, Args='5'}" />
</ContentPage>
```
- `Args` string wird per `|` getrennt (`"male|John"`). Parser versucht `int/long/decimal/double/bool`, sonst `string`.
- `LocExtension` subscribt auf `ILocalizer.CultureChanged` und aktualisiert Bindings automatisch.

## Kulturwechsel
1. `LocalizationInitializer.InitializeAsync()` beim App-Start aufrufen (siehe `MauiProgram.cs`).
2. `IAppPreferences` hält `Language` + `PreferencesChanged` (`TagGame.Client.Core/Services/IAppPreferences`).
3. Auf `CultureChanged` können UI-Komponenten reagieren (ToastHost tut das bereits).

## Konventionen
- Schlüssel bleiben sprachneutral & beschreibend: `area.section.label` (z. B. `lobby.players.count`).
- Pluralisierung via SmartFormat `plural:` Pattern. Beispiel `App.resx`:
  - `lobby.players.count = {0:plural:zero|no players|one|{0} player|other|{0} players}`
  - `App.de.resx = {0:plural:one|{0} Spieler|other|{0} Spieler}`
- Hinweise für Übersetzer (Kommentare in .resx) hinzufügen, wenn Kontext nicht offensichtlich ist.

## Tests & Hygiene
- Unit-Tests für `Localizer` (`TagGame.Client.Tests/Unit/Localization` sobald vorhanden) sollen sicherstellen: fehlender Key → `[key]`, Plural/Choose funktionieren, `CultureChanged` feuert.
- Pseudo-Lokalisierung (z. B. `fr-zz`) kann durch einen zusätzlichen `.resx` Eintrag simuliert werden; in der Dev-App einfach `preferences.SetLanguage(Language.Pseudo)` aufrufen.
- Beim Hinzufügen neuer Keys sowohl `App.resx` als auch `App.de.resx` (oder andere Sprachen) anpassen und `docs/21-Client-UI-Foundation.md` aktualisieren, falls UI-Textbausteine betroffen sind.

## Offene Aufgaben
- Weitere Sprachen hinzufügen (mindestens Englisch/Deutsch finalisieren, Pseudo-Loc optional).
- Lint auf fehlende/ungenutzte Keys in CI (z. B. via `ResXManager` oder eigener Analyzer).
- Einheitliche Fehlermappings (Server `code` → Ressourcen-Key) zentralisieren, sobald mehr ProblemDetails aus der API genutzt werden.
