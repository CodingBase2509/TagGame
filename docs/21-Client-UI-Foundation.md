# Client UI Foundation (Stand V2)

> Ergänzt [AGENTS.md](../AGENTS.md) für alle Arbeiten am MAUI-Client.

## Ordnerüberblick
```
TagGame.Client/
├── App.xaml / App.xaml.cs          # Ressourcen-Merge + Shell Bootstrap
├── AppShell.xaml(.cs)              # Routen-Registrierung, keine Logik
├── Infrastructure/
│   ├── Localization/               # Resx-Katalog + Initializer
│   └── Notifications/              # ToastPublisher, ToastPresenter
├── Resources/
│   ├── Fonts/, Images/, Localization/
│   └── Styles/
│       ├── Colors.xaml
│       ├── Styles.xaml
│       └── CustomStyles.xaml
├── Ui/
│   ├── Components/Toasts/
│   ├── Extensions/LocExtension.cs
│   ├── Services/ToastHostService.cs
│   └── Views/ (Start, Lobby, Game, Settings, PageBase)
└── MauiProgram.cs                  # DI Setup (Client + Client.Core)
```
`TagGame.Client.Core/` enthält ViewModels, Services, Navigation, Notifications, Localization-Interfaces, usw.

## Navigation
- Routen-Konstanten: `TagGame.Client.Core/Navigation/Routes.cs`.
- Registrierung: `AppShell.RegisterRoutes()` ruft `Routing.RegisterRoute(Routes.X, typeof(Page))` für alle Pages auf. Neue Views dort hinzufügen.
- Pages erben von `PageBase` (`Ui/Views/PageBase.cs`). Dadurch hängen ToastHost + Shared Overlays automatisch an jeder Seite. Im XAML einfach `<base:PageBase ...>` als Root verwenden.
- ViewModels sollten über ein `INavigationService` arbeiten (Interface in `Client.Core/Navigation`). Direkte Shell-Aufrufe im UI-Code vermeiden.

## Ressourcen & Theme
- Farben/Brushes in `Resources/Styles/Colors.xaml`; globale Control-Styles in `Styles.xaml`; projektspezifische Komponenten in `CustomStyles.xaml`.
- Beim Hinzufügen neuer UI-Bausteine bevorzugt `CustomStyles.xaml` nutzen (z. B. weitere Buttons, Badges, Chips).
- Fonts sind in `MauiProgram.AddMauiApp()` registriert (`ManropeRegular`, `ManropeSemiBold`, ...). Nutze die Aliasse in Styles/Views statt Font-Dateinamen.

## Lokalisierung
- Schnittstelle `ILocalizer` & `ILocalizationCatalog` liegen im Client.Core (`Localization/`).
- Resx-Dateien: `Resources/Localization/App.resx`, `App.de.resx` (weitere Sprachen hier ergänzen).
- Markup-Extension `{loc:Loc Key=..., Args='...'}` unter `Ui/Extensions/LocExtension.cs`. Sie reagiert automatisch auf Kulturwechsel.
- `LocalizationInitializer` (Infrastructure) liest `IAppPreferences` und ruft `ILocalizer.SetCultureAsync`. Bitte beim App-Start aufrufen (`MauiProgram`/`App`).

## Toasts & Benachrichtigungen
- Core-Interface: `TagGame.Client.Core/Notifications/IToastPublisher` + `ToastRequest`.
- UI-Layer: `Infrastructure/Notifications/ToastPublisher.cs` (bridged), `ToastPresenter`, `ToastHost`, `ToastHostService`.
- Styles & Layouts unter `Resources/Styles/CustomStyles.xaml` und `Ui/Components/Toasts/*.xaml`.
- Toaster erscheinen automatisch, weil `PageBase` den globalen Host injiziert.
- Für neue Toast-Typen zuerst `ToastRequest` erweitern, dann Presenter/Host/Styles anfassen.

## PR-Checkliste (UI)
- [ ] Richtigen Layer gewählt (ViewModel in Client.Core, UI-Kram in Client).
- [ ] Neue Ressourcen in `Colors.xaml`/`CustomStyles.xaml` hinterlegt, keine Inline-Hexwerte.
- [ ] Routes aktualisiert und Page in `AppShell.RegisterRoutes()` registriert.
- [ ] Lokalisierungs-Keys (`Resources/Localization`) + Doku (`docs/14-Lokalisierung.md`) gepflegt.
- [ ] Toast-Verhalten bei Fehlern/Erfolgen abgestimmt (`IToastPublisher`).
- [ ] Screenshots/GIFs an PR angehängt, wenn visuelle Änderungen sichtbar sind.
