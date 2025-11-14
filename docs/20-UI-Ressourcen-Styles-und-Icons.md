# UI-Ressourcen: Farben, Styles & Icons

## Struktur
```
TagGame.Client/
├── App.xaml                      # merged dictionaries
└── Resources/
    ├── Styles/
    │   ├── Colors.xaml           # Farbpalette + Brushes
    │   ├── Styles.xaml           # Baseline Styles für MAUI-Controls
    │   └── CustomStyles.xaml     # Projekt-spezifische Komponenten (Cards, Toasts, Buttons)
    ├── Images/*.svg              # UI-Icons
    ├── Fonts/                    # Manrope-Varianten
    └── Localization/             # App.resx etc. (siehe docs/14)
```

`App.xaml` merged aktuell `Colors.xaml`, `Styles.xaml` und `CustomStyles.xaml`. Wenn neue Dictionaries hinzukommen (z. B. Plattform-Overrides), dort ergänzen.

## Farben & Brushes (`Resources/Styles/Colors.xaml`)
- Enthält sämtliche Hex-Werte (Primary, Background, Text, Status, Role Accents) plus passende `SolidColorBrush`-Einträge.
- Light/Dark erfolgt über `AppThemeBinding` direkt in den Brush-Definitionen.
- Neue Farben bitte hier hinzufügen (inkl. Brush) und über `StaticResource` verwenden – keine Inline-Hex-Werte in Views.

## Baseline Styles (`Resources/Styles/Styles.xaml`)
- Deckt Standard-MAUI-Controls ab (Page, Button, Entry, Label, Border, ActivityIndicator, etc.).
- Anpassungen hier wirken global. Für Feature-spezifische Varianten lieber `CustomStyles.xaml` nutzen, damit Template-Updates einfacher bleiben.

## Custom Styles (`Resources/Styles/CustomStyles.xaml`)
- Enthält Komponenten wie `Card`, `ElevatedCard`, `FabButton`, `Toast*`, `PopupCard`, Chat-Bubbles und Label-Varianten.
- Nutzt die Brushes aus `Colors.xaml`. Wenn neue UI-Bausteine nötig sind (z. B. Badge, Pill), bitte in dieser Datei ergänzen.
- Beispielverwendung:
```xaml
<Border Style="{StaticResource Card}">
  <StackLayout>
    <Label Style="{StaticResource PlayerNameLabel}" Text="Alex" />
    <Label Style="{StaticResource PlayerRoleLabel}" Text="Hider" />
  </StackLayout>
</Border>
```

## Icons & Assets
- UI-Icons liegen unter `Resources/Images` (snake_case, SVG). Einbindung via `<Image Source="qr_code.svg" />`.
- App-Icon & Splash sind im `.csproj` referenziert (`MauiIcon`, `MauiSplashScreen`). Bei Änderungen SVG ersetzen und ggf. Größen anpassen.
- Fonts werden in `MauiProgram.cs` registriert (Manrope Regular/SemiBold/etc.). Styles greifen über Aliasse wie `ManropeRegular`.

## Do's & Don'ts
- ✅ Farben immer aus `Colors.xaml`/Brushes beziehen.
- ✅ Neues Styling → `CustomStyles.xaml` oder ein eigenes Dictionary anlegen, nicht in Views duplizieren.
- ✅ Bei Dark/Light-abhängigen Werten konsequent `AppThemeBinding` verwenden.
- ❌ Keine Hex-Werte oder Fonts direkt in XAML-Views definieren.
- ❌ Template-Dateien löschen – `Styles.xaml` liefert weiterhin sinnvolle Defaults, auch wenn CustomStyles existieren.

## Tests/Checks
- Schneller UI-Check: Light/Dark im Emulator wechseln und Toasts/Karten/Buttons prüfen.
- Build (Android/iOS) sicherstellen, nachdem neue Assets eingefügt wurden (`dotnet build TagGame.Client/TagGame.Client.csproj -f net9.0-android`).
