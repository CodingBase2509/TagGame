# Client-Benachrichtigungen (Stand V2)

Die Toast-Infrastruktur ist vollständig implementiert. Dieser Leitfaden fasst zusammen, wie sie funktioniert und wie neue Features sie nutzen sollten.

## Architektur
```
ViewModel/Service --(IToastPublisher)--> ToastPublisher (Client) --(event)--> ToastPresenter --(ToastHost)--> UI
```
- **Publisher API** (`TagGame.Client.Core/Notifications/IToastPublisher.cs`)
  - Asynchrones `PublishAsync(ToastRequest request)`. Helper-Extensions für Info/Success/Error (`IToastPublisher.Extensions.cs`).
  - `ToastRequest` (`ToastRequest.cs`) enthält `ToastType`, `Message`, `DurationMs`, `IsLocalized`, `ToastPriority`.
- **UI Bridge** (`TagGame.Client/Infrastructure/Notifications/ToastPublisher.cs`)
  - Implementiert sowohl `IToastPublisher` (DI in Client.Core) als auch `IToastSender` (Event für Presenter).
- **Presenter + Host**
  - `ToastPresenter` (`Client/Infrastructure/Notifications/ToastPresenter.cs`) hält genau einen `ToastHost`.
  - `ToastHost` (`Client/Ui/Components/Toasts/ToastHost.xaml(.cs)`) ist das Overlay mit Queue-/Badge-Logik.
  - `ToastHostService` (`Client/Ui/Services/ToastHostService.cs`) enthält Textauflösung via `ILocalizer`, Animationshilfen und Timer (`WaitDurationAsync`).
- **PageBase** (`Client/Ui/Views/PageBase.cs`) injiziert `ToastPresenter` und fügt den globalen Host jeder Page hinzu, inkl. Safe-Area & Overlay.

## Verhalten
- FIFO-Queue (`ToastHost`) mit Priorisierung: `ToastPriority.High/Critical` springen nach oben; Badge zeigt Anzahl wartender Toasters.
- Ein Toast läuft aktiv durch (AnimateIn → Wait → AnimateOut). Nachfolgende Einträge pausieren, bis sie oben sind.
- Lokalisation: `ToastRequest.IsLocalized=true` bedeutet, dass `Message` ein Ressourcen-Key ist (`ILocalizer.GetString`). Für rohe Texte `IsLocalized=false` setzen.
- Styling: `Resources/Styles/CustomStyles.xaml` definiert `ToastInfo/Success/Error/Warning` Styles; Icons liegen unter `Resources/Images/toast_*.svg`.

## Verwendung in ViewModels/Services
```csharp
await _toasts.PublishSuccessAsync("lobby.joined", durationMs: 4000);
await _toasts.PublishErrorAsync("network.offline", isLocalized: true);
```
- Keine UI-spezifischen Klassen aus ViewModels ansprechen. `ToastRequest` reicht vollständig.
- Für Handlungsaktionen (Undo etc.) könnte `ToastRequest` erweitert werden (ActionText/Handler). Aktuell nicht nötig – bitte bei Bedarf zuerst Interface erweitern.

## Tests
- ViewModels testen gegen `IToastPublisher` mit einem Fake/Spy (siehe `TagGame.Client.Tests/Unit/Notifications` sobald vorhanden).
- Presenter/Host werden derzeit nicht unit-getestet. UI-Smoke-Tests oder visueller Check (StartPage Buttons) reichen.

## Offene Verbesserungen
- Deduplizierung innerhalb eines Zeitfensters
- Persistenz über Navigationswechsel? (Der Host hängt an PageBase, bleibt also stabil. Für Shell-Modals muss ggf. `ToastHost` repositioniert werden.)

Bei Änderungen am Toast-System immer beide Schichten (Client.Core + Client) im Blick behalten und `PageBase`-Integration prüfen.
