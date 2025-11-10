# UI‑Ressourcen: Styles, Farben und Icons

Kurzleitfaden für die Pflege und Nutzung von UI‑Ressourcen im MAUI‑Client.

## Struktur
- Styles/Colors: `TagGame.Client/Resources/Styles`
  - App‑Merge: `TagGame.Client/App.xaml:7`
  - Aktiv genutzt: `CustomColors.xaml`, `CustomStyles.xaml`
  - Template‑Baseline (nicht mehr aktiv gemerged): `Colors.xaml`, `Styles.xaml`
- Icons (UI): `TagGame.Client/Resources/Images/*.svg`
- App‑Icon: `TagGame.Client/Resources/AppIcon/appicon.svg` (+ Android rund: `appicon_round.svg`)
- Splash: `TagGame.Client/Resources/Splash/splash_round.svg`
- Fonts: `TagGame.Client/Resources/Fonts` (via `Manrope*`‑Aliasse registriert)

## Styles & Farben
- Merged in `App.xaml` als Resource Dictionaries:
  - `TagGame.Client/App.xaml:7`: `Resources/Styles/CustomColors.xaml`
  - `TagGame.Client/App.xaml:8`: `Resources/Styles/CustomStyles.xaml`
- Theme‑Binding: Light/Dark wird per `AppThemeBinding` gewählt. Beispiel für Hintergründe:
  ```xaml
  <ContentPage BackgroundColor="{AppThemeBinding Light={StaticResource ColorBackgroundLight}, Dark={StaticResource ColorBackgroundDark}}" />
  ```
- Häufige Styles (Auszug, siehe Datei für Details):
  - Labels: `BaseLabel`, `TitleLabel`, `SubtitleLabel`
  - Buttons: `PrimaryButton`, `SecondaryButton`, `DangerButton`, `FabButton`
  - Container: `CardBorder`, `ToastBorder`, `PopupCard`, `OverlayPanel`
  - Eingaben: implizite Styles für `Entry`, `Editor`, `Slider`, `Switch`, `ProgressBar`
- Neue Farben anlegen:
  1) In `CustomColors.xaml` je eine Light/Dark‑`Color` definieren (ggf. zusätzlich passende `SolidColorBrush` Schlüssel).
  2) In `CustomStyles.xaml` per `StaticResource` und `AppThemeBinding` verwenden.
- Fonts: In `Builder.Ressources.cs` als Aliasse registriert (z. B. `ManropeRegular`, `ManropeSemiBold`). Beispiel:
  ```xaml
  <Label Style="{StaticResource TitleLabel}" Text="Überschrift" />
  <Button Style="{StaticResource PrimaryButton}" Text="Aktion" />
  ```

## Icons (UI, App‑Icon, Splash)
- UI‑Icons: SVGs unter `Resources/Images`. Nutzung in XAML:
  ```xaml
  <Image Source="send.svg" HeightRequest="20" />
  ```
  Hinweise:
  - Benennung: kleinbuchstaben_snake_case (`send.svg`, `qr_code.svg`).
  - SVG möglichst einfach (einfarbig/monochrom), gute Kontraste auf Light/Dark.
- App‑Icon: In der CSPROJ referenziert
  - `TagGame.Client/TagGame.Client.csproj:38` — Basis‑Icon (`MauiIcon appicon.svg`)
  - `TagGame.Client/TagGame.Client.csproj:40` — Android rund (`appicon_round.svg`)
- Splash‑Screen: `TagGame.Client/TagGame.Client.csproj:45` (`MauiSplashScreen` mit `BaseSize` und Hintergrund‑`Color`).

## Do’s & Don’ts
- Do: Neue/angepasste Farbschlüssel in `CustomColors.xaml` pflegen – nicht mehr die Template‑Dateien ändern.
- Do: Theme‑Handling konsequent per `AppThemeBinding` umsetzen.
- Do: Konsistente Namen (snake_case) für Icons; SVG bevorzugen.
- Don’t: Farbcodes inline in Views streuen; stattdessen Ressourcen nutzen.

## Tests/Checks
- Schneller visueller Check: Light/Dark auf Gerät/Emulator umschalten, Buttons/Labels/Cards gegen Hintergründe prüfen.
- Android/iOS‑Build: `dotnet build TagGame.Client/TagGame.Client.csproj -f net9.0-android` (bzw. `net9.0-ios`).

