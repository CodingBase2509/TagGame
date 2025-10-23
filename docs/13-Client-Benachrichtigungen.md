# Client-Benachrichtigungen (Toast/Snackbar) – Architekturleitfaden

Ziel: Entkopplte, robuste und testbare Benachrichtigungen im Client, ohne harte Abhängigkeit auf konkrete Views oder Page-Layouts.

## Ziele
- Geringe Kopplung: ViewModels kennen nur ein Publisher-Interface, keine UI-Details.
- Stabil über Navigation: Ein zentraler Host, keine dynamische Re-Anheftung pro Seite.
- Konsistente UX: Queue/Policy, Prioritäten, Theme/A11y, optionale Aktion (Snackbar).
- Sehr gut testbar: Core-Logik ohne UI, Presenter/UI separat.

## Probleme im Status quo (beobachtet)
- Service initialisiert mit konkreter `ToastView` und Cast im `AppShell` → enge Kopplung.
- Overlay hängt an `PageBase` → alle Seiten müssen `PageBase` nutzen.
- Keine Anzeige-Policy: parallele/überschreibende Toastrufe, keine Deduplizierung.
- Lokalisierung gemischt (im View), unklare Übergabe (Key vs. Plain-Text).

## Zielarchitektur
- Publisher/Presenter-Trennung
  - Client.Core: `IToastPublisher` publiziert reine `ToastRequest`-Modelle.
  - Client (UI): `ToastPresenter` subscribed auf den Publisher und steuert Anzeige/Animationen.
- Shell‑Level Host
  - Ein `ToastHost` (einziger Overlay‑View) liegt im `AppShell`-Root/Window.
  - Bleibt über Navigation bestehen; kein dynamisches Re-Anheften.
- Klare API/Optionen
  - `ToastRequest`: `Type` (Info/Success/Error), `Text` oder `ResourceKey + Args`, `DurationMs`, `Priority` (Error > Success > Info), `Position` (Top/Bottom), optionale `ActionText` + `Action` (Snackbar).
- Anzeige‑Policy
  - FIFO‑Queue, ein Toast zur Zeit; optional Deduplizierung (Zeitfenster) und Preemption durch höhere Priorität.
- Threading
  - Publisher ist threadsafe; Presenter marshalt auf den UI‑Thread.
- Lokalisierung
  - Variante A: ViewModels liefern finalen Text (Lokalisierung im VM/Core).
  - Variante B: `ResourceKey + Args` im Request, Auflösung im Presenter (einheitlich wählen!).
- Styling & A11y
  - Farben/Abstände/CornerRadius via Theme (Resources), DynamicResource/AppThemeBinding.
  - `AutomationProperties.Name`/`SemanticScreenReader.Announce(...)`; ausreichende Anzeigedauer.

## Schnittstellen (Skizze, keine Implementierung)
- Client.Core
  - `interface IToastPublisher { void Publish(ToastRequest request); }`
  - `record ToastRequest(ToastType Type, string? Text = null, string? ResourceKey = null, object[]? Args = null, int DurationMs = 3000, ToastPriority Priority = ToastPriority.Normal, ToastPosition Position = ToastPosition.Bottom, string? ActionText = null, Action? Action = null);`
- Client (UI)
  - `ToastPresenter` (Queue/Policy/Threading) + `ToastHost` (XAML/Animations) unter `AppShell`.

## Teststrategie
- ViewModels testen gegen `IToastPublisher` (Fake/Spy) → publishte Requests verifizieren.
- Presenter separat via UI‑/Visual‑Tests (Animations/Timing minimal mocken).

## Migration (theoretisch)
1) Neues Paket/Ordner in `Client.Core`: `Ui/Notifications` mit `IToastPublisher`, `ToastRequest`, `ToastType`, `ToastPriority`.
2) UI: `ToastPresenter` + `ToastHost` im `AppShell` platzieren; Presenter subscribed auf Publisher.
3) ViewModels: `IToastService` durch `IToastPublisher` ersetzen; Convenience‑Helper (Error/Success) beibehalten, intern `Publish(...)`.
4) Lokalisierung: Einheitlich entscheiden (Text vs. Key) und in allen Call Sites konsistent nutzen.
5) Entfernen/Kapseln: `Initialize(ToastView)`, `PageBase`‑Kopplung und Casts in `AppShell`.

## Alternativen
- CommunityToolkit `Toast`/`Snackbar` nutzen (schneller Start, weniger Flex, plattformnahes Look&Feel).
- „NotificationService“ als generischer Layer (Toast + Snackbar + ModalSheet) mit gemeinsamer Queue/Priorität.

