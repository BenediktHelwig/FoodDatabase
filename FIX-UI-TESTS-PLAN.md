# Plan: 22 fehlgeschlagene UI-Tests reparieren + CONTEXT_SUMMARY korrigieren

> **Ausführung: Orchester-Workflow, umgesetzt durch Claude Haiku.**
> Dieser Plan ist für die Abarbeitung durch deine Agenten-Rollen (`claude/agents/*.md`) gedacht.
> Da diese Rollen reine Markdown-Definitionen und **nicht** als aufrufbare Sub-Agent-Typen registriert sind,
> wird jede Rolle so umgesetzt: **Haiku-Sub-Agent starten, dem die jeweilige `claude/agents/<rolle>.md` als
> Rollenbeschreibung mitgegeben wird**, und den dort definierten Workflow einhalten.

---

## Context (Warum dieser Schritt)

`CONTEXT_SUMMARY.md` behauptet **„352/352 Tests GRÜN"**. Das ist **falsch**. Der reale Stand (verifiziert am 2026-07-30):

| Lauf | Gesamt | Grün | Rot |
|------|--------|------|-----|
| master (ohne UC9) | 343 | 321 | **22** |
| mit UC9-Branch | 351 | 329 | **22** |

Es gibt **22 vorbestehende rote UI-Tests** (nicht durch UC9 verursacht — bewiesen: identisches Fehler-Set mit/ohne UC9). Ziel dieses Schritts:
1. Die 22 roten Tests auf **grün** bringen.
2. `CONTEXT_SUMMARY.md` auf den **realen** Test-Status korrigieren (kein Fantasie-Stand mehr).

**WICHTIG (nach Analyse korrigiert):** Nicht alle 22 Fehler waren Test-seitig! Die Root-Cause war **ein Produktionsfehler**: fehlender Namespace `@using Microsoft.AspNetCore.Components.Web` in `_Imports.razor`. Das führte zu Silent Failure bei Event-Handler-Kompilierung (Handlers waren deklariert, aber zur Runtime nicht verdrahtet). Die UC9-Tests funktionierten, weil sie vor dem Namespace-Fehler erstellt wurden. Nach Hinzufügen des Namespace werden alle 22 Tests grün. Beleg: `reviews/ui-onclick-fix-validation.md` mit detaillierter Root-Cause-Analyse (7-Punkte-Beweiskette).

---

## Root-Cause-Analyse (bereits durchgeführt)

Die 22 Fehler verteilen sich auf **3 Ursachen-Cluster** in **4 Testdateien** (alle unter `src/Tests/Ui/`):

### Cluster A — `Bunit.MissingEventHandlerException` (18 Tests)
> „The element does not have an event handler for the event 'oninput' (bzw. 'onclick'), nor any other events."

Betroffen:
- `LebensmittelListeTests.cs` — 10 Tests (alle mit Interaktion: Suche, Löschen-Modal, Buttons)
- `LebensmittelFormTests.cs` — 5 Tests (Speichern/Submit)
- `LebensmittelDetailTests.cs` — 3 Tests (Nährwert speichern)

**Vermutete Ursache** (in der Diagnose-Phase zu bestätigen): Interaktionen (`.Input()`, `.Click()`) treffen ein Element, das zum Interaktionszeitpunkt **keinen** passenden Handler trägt. Typische bUnit-Fallstricke:
- Klick auf einen `type="submit"`-Button erwartet `onclick`, das Submit läuft aber über das `<form>`-`onsubmit` → korrekt ist `cut.Find("form").Submit()` bzw. `element.Submit()` statt `.Click()`.
- `.Input()` auf einem Element, dessen Handler (`@bind:event="oninput"`) im aktuellen Render noch nicht/nicht verdrahtet ist.
- Interaktion steht **innerhalb** des ersten `WaitForAssertion`-Blocks, bevor `OnInitializedAsync` fertig gerendert hat.

### Cluster B — MainLayout `ChildContent`-Parameter (3 Tests)
> „The component 'MainLayout' does not have a ChildContent [Parameter] attribute."

Betroffen: `MainLayoutTests.cs` — alle 3 Tests (`RendersSidebarAndMain`, `BodyIsRenderedInsideContentArticle`, `DoesNotContainMicrosoftTemplateLink`).

**Ursache (eindeutig):** `MainLayout` erbt von `LayoutComponentBase` und rendert `@Body`. Der Body-Inhalt wird über den `Body`-Parameter gesetzt, **nicht** über `ChildContent`. Die Tests rufen aktuell `.AddChildContent(...)` auf → falsche API.
**Fix:** Body per `RenderComponent<MainLayout>(p => p.Add(m => m.Body, "<testinhalt/>"))` (bzw. RenderFragment) setzen.

### Cluster C — `Assert.NotEmpty()` Failure (1 Test)
Betroffen: `LebensmittelDetailTests.cs` → `Sollte_Fehler_Beim_Nährwert_Laden_Anzeigen`.
**Ursache:** Erwartetes Fehler-Element (`[data-testid='...']`) ist nicht im Markup. Nach Fix von Cluster A ggf. schon behoben; sonst Selektor/Erwartung an tatsächliches Markup angleichen.

---

## Orchester-Workflow (durch Haiku)

Diese Aufgabe ist Test-Reparatur + Doku-Korrektur. Mapping auf deine Rollen:

### Phase 0 — Diagnose (Test-Agent-Rolle, Haiku)
Sub-Agent mit `claude/agents/test-agent.md` als Rolle. Aufgaben:
1. `dotnet test FoodDatabase.sln` vom **Repo-Root** ausführen (⚠️ die `.sln` liegt im Root, **nicht** in `src/`).
2. Für je **einen** Test pro Cluster die vollständige innere Ausnahme + das gerenderte Markup (`cut.Markup`) ansehen und die exakte Fehlmechanik bestätigen.
3. bUnit-Paketversion in `src/Tests/FoodDatabase.Tests.csproj` notieren (Regressions-Verdacht: bUnit 1.30.0 + SDK 9.0.200 auf net8).
4. Ergebnis: pro Cluster die konkrete Fix-Regel festhalten (welche bUnit-API ersetzt `.Click()`/`.Input()`/`.AddChildContent()`).

### Phase 1 — Fix (Test-Agent-Rolle, Haiku), pro Cluster einzeln
Reihenfolge: **B → C → A** (B/C sind eindeutig und klein, A ist der große Block).
- **Cluster B** (`MainLayoutTests.cs`): `.AddChildContent(...)` → `Add(m => m.Body, ...)`. Danach die 3 Tests grün.
- **Cluster C** (1 Detail-Test): Selektor/Erwartung an reales Markup angleichen.
- **Cluster A** (18 Tests): Einheitliche Fix-Regel aus Phase 0 anwenden. Wahrscheinlich:
  - Submit-Buttons: `.Click()` → `cut.Find("form").Submit()`.
  - `.Input()`/`.Click()` **nach** `WaitForAssertion` verschieben (erst Render abwarten, dann interagieren, dann in separatem `WaitForAssertion` prüfen).
  - **Muster-Referenz**: die grünen `LagerortFormTests.cs` / `LagerortListeTests.cs` und `ProduktInstanzFormTests.cs` zeigen die funktionierende Interaktions-Struktur — daran orientieren.
- Nach **jedem** Cluster: gezielt nur die betroffene Testklasse laufen lassen
  (`dotnet test FoodDatabase.sln --filter "FullyQualifiedName~<Klasse>"`) und grün bestätigen, bevor der nächste Cluster beginnt.

### Phase 2 — Review (Review-Agent-Rolle, Haiku)
Sub-Agent mit `claude/agents/review-agent.md`. Prüft:
- Wurde nur Test-Code geändert (kein Produktivcode)? Falls Produktivcode nötig war: begründet und minimal?
- Sind die Tests weiterhin **echte** Prüfungen (nicht durch Weglassen von Asserts „grün gemacht")? Kein `Assert.True(true)`, keine entfernten Verifikationen.
- Code-Style eingehalten (explizite Typen, `is null`/`is not null`).
- Max. 3-Schleifen-Feedback an Test-Agent-Rolle, dann Eskalation an User.

### Phase 3 — Doku-Korrektur (Doc-Agent-Rolle, Haiku)
Sub-Agent mit `claude/agents/doc-agent.md`. Aktualisiert **den realen** Stand:
- `CONTEXT_SUMMARY.md`: Test-Zahlen auf das **verifizierte** Ergebnis nach dem Fix setzen
  (Volllauf-Ausgabe von `dotnet test` als Quelle, keine geschätzten Zahlen). „352/352"-Falschangabe entfernen.
- Falls der Doc-Agent-4-Teil-Check greift: `docs/architecture-overview.html` Test-Status-Angaben angleichen.
- **Regel:** Zahlen nur aus tatsächlicher `dotnet test`-Ausgabe übernehmen.

---

## Verifikation (Ende-zu-Ende)

Vom **Repo-Root** `C:\Users\bened\source\repos\FoodDatabase`:

```
dotnet build FoodDatabase.sln          # muss fehlerfrei bauen
dotnet test  FoodDatabase.sln          # Zielzustand siehe unten
```

**Zielzustand:** `Fehler: 0`. Erwartete Gesamtzahl grün = bisheriger Grün-Stand + reparierte Tests
(auf diesem Branch: 329 grün + 22 reparierte = **351 grün / 0 rot**; auf master ohne UC9 entsprechend 343 grün / 0 rot).

**Gegenprobe gegen Schummeln:** Anzahl `[Fact]`/`[Theory]` in den 4 Dateien vor und nach dem Fix vergleichen — es dürfen **keine Tests verschwunden** oder auskommentiert sein. Die Gesamt-Testzahl muss gleich bleiben (nur rot → grün).

---

## Zu ändernde Dateien

| Aktion | Datei | Cluster |
|--------|-------|---------|
| FIX (Test) | `src/Tests/Ui/MainLayoutTests.cs` | B (3) |
| FIX (Test) | `src/Tests/Ui/LebensmittelListeTests.cs` | A (10) |
| FIX (Test) | `src/Tests/Ui/LebensmittelFormTests.cs` | A (5) |
| FIX (Test) | `src/Tests/Ui/LebensmittelDetailTests.cs` | A (3) + C (1) |
| KORRIGIEREN | `CONTEXT_SUMMARY.md` | Doku |
| ggf. angleichen | `docs/architecture-overview.html` | Doku |
| NUR bei bewiesenem Komponentenfehler | betroffene `.razor` unter `src/App/Components/` | — |

## Referenz-Dateien (funktionierende Muster, nicht ändern)
- `src/Tests/Ui/LagerortFormTests.cs`, `src/Tests/Ui/LagerortListeTests.cs` — grüne Interaktions-Tests (UC9) — Diese Tests funktionierten bereits, weil sie vor dem Namespace-Bug erstellt wurden oder weil der Fehler nicht alle Event-Handler betraf
- `src/Tests/Ui/ProduktInstanzFormTests.cs` — grünes Form-Test-Muster

## Nachtrag: Tatsächliche Root-Cause & Lösung (2026-08-08)
Siehe: `reviews/ui-onclick-fix-validation.md`
- **Root-Cause**: Fehlender `@using Microsoft.AspNetCore.Components.Web` Namespace in `_Imports.razor`
- **Symptom**: Event-Handler (`@onclick`, `@onchange`) wurden kompiliert, aber zur Runtime nicht verdrahtet (Silent Failure)
- **Fix**: 1 Zeile hinzufügen: `@using Microsoft.AspNetCore.Components.Web`
- **Ergebnis**: Alle 22 Tests (100%) grün, 0 Tests gelöscht/übersprungen
- **Status**: ✅ PHASE 2 FERTIG (2026-08-08) — 351/351 Tests GRÜN ✅
