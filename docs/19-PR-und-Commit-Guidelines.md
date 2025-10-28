# PR- und Commit-Guidelines

Diese Richtlinien beschreiben, wie Commits benannt werden und wie Pull Requests erstellt und beschrieben werden.

## Commits
- Stil: kurz, imperativ (z. B. „add“, „update“, „fix“)
- Optionaler Scope: `api:`, `client:`, `client-core:`, `infra:`, `docs:` usw.
- Verweise auf Issues/PRs: am Ende „... (#123)“ oder „Closes #123“ im PR (Beschreibung)
- Beispiele:
  - `feat(client-core): add AuthService + token storage (#54)`
  - `fix(api): refresh rotation edge-case (#24)`
  - `docs: update getting started`

## Pull Requests
- Größe: kleine, fokussierte PRs (1–3 Issues)
- Beschreibung: besteht aus zusammengefassten Commit-Messages der zugehörigen Commits (eine Zeile pro Commit, neueste oben)
- Struktur (empfohlen):
  - Kurz-Titel („M1: auth endpoints + token rotation“)
  - Beschreibung (Commit-Zusammenfassung aus dem Branch)
  - „Closes #…“ für Issues, die der PR abschließt
  - Labels/Milestones setzen (z. B. „v2 M1 — AuthN/AuthZ“)
  - Screenshots/GIFs bei UI-Änderungen
- Checkliste vor Merge:
  - CI grün (Build/Tests/Format)
  - Rebase/mergeable gegen master
  - Changelog/Docs (falls erforderlich) aktualisiert

## Konvention „PR-Beschreibung = Commit-Zusammenfassung“
- Beim Erstellen eines PRs wird die Beschreibung mit den Commit-Messages aus dem PR-Branch gegenüber `master` befüllt (neueste zuerst).
- Beispiel (gekürzt):
  - `ea9fbb7 add centralized problem detail handling - addition to #92`
  - `133f2ce impl http basics - #92`
  - `7fc6752 Impl retry and connectivity policy - #80`

## Hinweise
- Im Zweifel lieber einen PR aufteilen als zu groß werden lassen.
- Commit-Messages sind die Quelle für die PR-Beschreibung — prägnant und aussagekräftig formulieren.

