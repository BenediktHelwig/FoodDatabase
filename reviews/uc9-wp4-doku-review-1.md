# UC9 (WP4) Dokumentation Review – Versuch 1/3

**Datum**: 2026-09-19
**Phase**: Doku-Review WP4 UC9 (nachgeholt)
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `docs/uc9-wp4-nachzug` (Commit `378559c` über `master` `a57e572`)

---

## VERDICT: CHANGES REQUESTED
## ATTEMPT: 1 von 3

**Befunde**: 2 x HIGH · 7 x MEDIUM (alle freigabe-blockierend) · 6 x LOW (nicht blockierend)
**Testlauf**: `dotnet test src/Tests/FoodDatabase.Tests.csproj` → **361 bestanden, 0 Fehler** (selbst ausgeführt)
**Gegenzählung**: `LagerortServiceTests.cs` = **24** `[Fact]` / 0 `[Theory]`, `LagerortListeTests.cs` = **5**, `LagerortFormTests.cs` = **3** → 24 + 8 = **32**. Die korrigierten Zahlen stimmen.
**Diff-Hygiene**: `git diff --check` sauber, `--numstat` = 2/2 + 10/10 = **12 Zeilen**, beide Dateien weiterhin CRLF. Keine Formatdrift.

**Auftrag A (Commit `378559c`) ist inhaltlich fast vollständig in Ordnung** — ein Befund (M9).
**Auftrag B (nachgeholtes WP4-Gate) fällt durch.** Die UC9-Feature-Seite beschreibt eine Datenbank-Integration, die es im Code nicht gibt, und dokumentiert die UI-Schicht, um die es in WP4 geht, nur in zwei Tabellenzeilen — ohne Routen, ohne Workflow, ohne Dateipfade. Genau die Lücke, die entsteht, wenn ein Gate übersprungen wird.

---

## Auftrag A — Prüfung von Commit `378559c`

### A.1 Testzahlen — korrekt und vollständig

| Stelle (UC9-Lagerorte.html) | Neu | Gegenprobe |
|---|---|---|
| Z. 181 `Tests (24 Service + 8 UI = 32 GRÜN)` | OK | 24 + 5 + 3 |
| Z. 183 `LagerortService Tests (24 Tests)` | OK | 24 `[Fact]` |
| Z. 235 `Service Tests: 24` | OK | — |
| Z. 238/239 `Total 32` / `32/32 GRÜN` | OK | — |
| Z. 358 `24 Tests geschrieben (Commit 0df33ea)` | OK | `git show 0df33ea:.../LagerortServiceTests.cs` → 24 `[Fact]` |
| Z. 386 `0df33ea (24 Tests)` | OK | dito |
| Z. 399 `FERTIG (32/32 Tests GRÜN)` | OK | — |

Alle acht Stellen sind erwischt. Konsistenz-Gegenprobe über die **ganze** Datei nach `18`, `26`, „Pending": **null Treffer**.

### A.2 Nichts Richtiges kaputt gemacht

- Z. 359 und Z. 393 `108/108 Tests grün` stehen unverändert — korrekt, das ist der damalige Projekt-Gesamtstand zum Commit `ea3957b`, nicht die UC9-Zahl.
- Die Testnamen-Liste Z. 186–209 ist unangetastet; ich habe sie gegen die 24 Methodennamen abgeglichen: 3 × `GetAlleLagerorte`, 4 × `GetAutoComplete`, 6 × `ValidateLagerort`, 4 × `NormalisiereLagerort`, 5 × `GetOrCreateAsync`, 2 × SQL-Injection = 24. Deckungsgleich.
- Die 5 + 3 UI-Testnamen (Z. 216–229) stimmen **wörtlich** mit den Methodennamen in `LagerortListeTests.cs` und `LagerortFormTests.cs` überein.

### A.3 UC10-Karte (`architecture-overview.html:125–130`) — sachlich richtig

Neu: `Service (32 Tests) ✅ fertig + UI (WP4 offen)` / Badge `✅ Service Complete (32/32 Tests)`.
Abgeglichen gegen Z. 339 (`UC10: … 32 Tests`), Z. 358 (`⏳ WP4 Weitere UI-Komponenten (UC4, UC10)`) und die Legende in `requirements/use-cases.drawio:112` (`UC3/UC10: Service only`) — **konsistent**. Der Zusatz „und Lagerort" ist durch `ProduktInstanz.Lagerort` gedeckt. Keine Beanstandung.

### A.4 Merge-Status — ein Befund

Z. 365 `Merged zu Master – ✅ FERTIG (Commit 23eb5fc)` ist belegt: `23eb5fc` (30.07.2026) liegt auf der First-Parent-Linie von `origin/master`.
Z. 364 `User Review & Approval – ✅ FERTIG` ist **nicht** belegt → **M9**.

---

## Auftrag B — nachgeholtes Doku-Review der UC9-UI (4-Teil-Check)

### Teil 1 — Code-Dokumentation: bestanden, mit einer Ungenauigkeit

`LagerortListe.razor` und `LagerortForm.razor` tragen beide einen `<summary>`-Block, der Zweck, Route-Kontext und Service-Aufruf nennt. `Lagerort.cs` dokumentiert alle vier Properties inklusive des Soft-Delete-Grundes. DI-Registrierung `builder.Services.AddScoped<ILagerortService, LagerortService>()` (`Program.cs:39`) vorhanden — die Seiten sind zur Laufzeit auflösbar.
Eine Ungenauigkeit in `ILagerortService.cs:40` → **L2**.

### Teil 2 — Feature-HTML `docs/features/UC9-Lagerorte.html`

| Kriterium (CLAUDE.md) | Befund |
|---|---|
| Status-Badge | Kopf Z. 21 `✅ FERTIG (Service + UI)` — korrekt, **aber** der Fuß Z. 440 sagt das Gegenteil → **M1** |
| Domain Model: Entities + Felder | FEHLER: dokumentiert eine Navigations-Property und eine FK-Spalte, die es nicht gibt → **M2** |
| Test-Coverage: alle Testfälle gelistet | OK — 24 + 5 + 3, Namen deckungsgleich |
| Workflow: Geschäftslogik erklärt | FEHLER: kein einziger Workflow der UC9-**UI** → **M5** |
| UI-Komponenten beschrieben | UNVOLLSTÄNDIG: zwei Tabellenzeilen, keine Routen, kein NavMenu → **M4**; Code-Struktur ohne die Razor-Dateien → **M3** |
| Diagramm-Links | Sektion Z. 427–437 nennt ER-, Sequence- und Use-Case-Diagramm. Vorhanden und korrekt benannt (nur `<code>`, kein `<a href>` — projektüblich, kein Mangel) |
| Innere Widerspruchsfreiheit | FEHLER bei der Normalisierungsregel Z. 59 → **M6** |

Positiv und ausdrücklich verifiziert: Die dokumentierten **Einschränkungen** (Z. 244–248: kein Update, kein Delete, keine Detail-Seite) sind gegen `ILagerortService.cs` geprüft und **korrekt** — das Interface hat genau 5 Methoden (`GetAlleLagerorte`, `GetLagerorteMitAutoComplete`, `ValidateLagerort`, `NormalisiereLagerort`, `GetOrCreateAsync`), kein Update, kein Delete. Die Angabe „ILagerortService.cs ✅ (5 Methoden)" (Z. 261) stimmt.

### Teil 3 — `docs/architecture-overview.html`

| Stelle | Inhalt | Urteil |
|---|---|---|
| Z. 181–186 UC9-Karte | `Service (24 Tests) + UI (8 bUnit-Tests) … Badge 32/32` | OK, selbst nachgezählt |
| Z. 241–244 Entity `Lagerort` | `✅ UC9` | OK |
| Z. 337 | `UC9: Lagerorte (24 Service-Tests …)` | OK |
| Z. 338 | `WP4 UC9: Lagerorte UI (8 bUnit-Tests + LagerortListe.razor + LagerortForm.razor …)` | OK |
| Z. 356 | `WP4 UC9: Lagerorte UI – FERTIG! (8 bUnit-Tests + Liste + Form)` | OK |
| Z. 340 | `GESAMT: 361/361 (271 Service / 90 UI)` | OK — `dotnet test` liefert 361 |
| Z. 211–214 Entity `ProduktInstanz` | `✅ UC10 + UC2 (Service + UI WP4)` | mehrdeutig im Licht der neuen UC10-Karte → **L5** |

Teil 3 ist für UC9 **durchgängig konsistent**. Keine blockierenden Befunde.

### Teil 4 — Diagramme

- **`requirements/use-cases.drawio`**: UC9-Ellipse grün, Legende `UC9: 24 Service + 8 UI`, `UC3/UC10: Service only` — gegengezählt, **korrekt**. Keine Beanstandung.
- **`diagrams/sequence-uc9-lagerorte.drawio`**: bildet nur die Service-Szenarien mit einer generischen Lifeline `Blazor UI (UC10/UC2)` ab. Die UC9-eigenen UI-Abläufe fehlen → **M8**. Zusätzlich zeigt Schritt 13 `CreateProduktInstanz(…, lagerortId=3, …)` einen Aufruf, den es nicht gibt (Teil von **M2**).
- **`diagrams/database-schema.drawio`**: `ProduktInstanz` trägt `FK LagerortId: int (UC9)` — existiert im Code nicht → **M7**.

---

## Kernbefund: die LagerortId-Integration ist Dokumentation ohne Code

Selbst verifiziert, mehrfach:

- `grep -rn "LagerortId" src/` über `*.cs` und `*.razor` → **kein einziger Treffer**.
- `src/App/Models/ProduktInstanz.cs:45`: `public string Lagerort { get; set; }` — ein freier String, keine Relation.
- `src/App/Data/FoodDatabaseContext.cs:64-65`, wörtlich:
  `// ProduktInstanz → Lagerort Integration kommt in UC9-Dev-Phase`
  `// Momentan wird Lagerort als string Feld in ProduktInstanz gespeichert`
- `src/App/Migrations/FoodDatabaseContextModelSnapshot.cs:141`: `b.Property<string>("Lagerort")`. Die Tabelle `Lagerorte` existiert (`InitialCreate`), eine FK-Spalte nicht.
- `ILagerortService` wird in der gesamten App **nur** von `LagerortListe.razor` und `LagerortForm.razor` injiziert — keine Verwendung in `ProduktInstanzService` oder `LagerbestandService`. `ILagerbestandService` kennt Lagerorte nur als `string` (`GetBestaendePorLagerortAsync(string lagerort)`).

Die UC9-Doku beschreibt an vier Stellen das Gegenteil. Das ist kein historischer Restbefund, sondern der heutige Stand der Seite — und der Grund, warum M2 HIGH ist: Ein Leser, der die Seite (Badge „FERTIG") als Wahrheit nimmt, baut auf einer Relation auf, die nicht existiert.

---

## MÄNGEL (freigabe-blockierend)

### M1 — Fuß der Feature-Seite sagt „READY FOR TEST-AGENT" · HIGH
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeile **440**
**Falsch**: `Dokumentation erstellt: 2026-06-25 | Status: SPEZIFIZIERT & READY FOR TEST-AGENT`
Dieselbe Seite sagt in Z. 21 `✅ FERTIG (Service + UI)`, in Z. 365 „Merged zu Master", und die Overview führt UC9 als fertig.
**Konsequenz**: Exakt der Fehlertyp aus dem Konsistenz-Check in `CLAUDE.md` („Code sagt fertig, aber irgendeine Stelle sagt todo → FEHLER"). Derselbe Befundtyp wie Mangel 3 im UC6-Review — hier im Fuß, und deshalb von der Zahlen-Korrektur in `378559c` nicht erfasst worden.
**Soll**: Z. 440 ersetzen durch
`Dokumentation erstellt: 2026-06-25 | Aktualisiert: 2026-09-19 | Status: FERTIG (Service + UI, auf master seit 23eb5fc)`.

### M2 — FK `LagerortId` und Navigations-Property sind dokumentiert, existieren aber nicht · HIGH
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeilen **103–110**, **124–125**, **156**, **337**, **338**
**Falsch**:
- Z. 105–109: `ALTER TABLE ProduktInstanzen ADD COLUMN LagerortId INT NULLABLE` + `FK_ProduktInstanz_Lagerort … ON DELETE SET NULL`
- Z. 124–125: `public ICollection<ProduktInstanz> ProduktInstanzen { get; set; } = new List<ProduktInstanz>();` im Entity-Block — `src/App/Models/Lagerort.cs` hat **vier** Properties (`Id`, `Name`, `CreatedAt`, `IsArchived`) und keine Navigation
- Z. 156: `ProduktInstanz wird mit LagerortId=3 erstellt`
- Z. 337: `Erweitert: UC10: Produktinstanzen mit MHD (LagerortId FK)`
- Z. 338: `Erweitert: UC2: Lagerbestand (AddToBestand mit Lagerort-Input)` — `ILagerbestandService` ruft `ILagerortService` nirgends auf

**Konsequenz**: Teil 2 verlangt „Domain Model: Alle Entities + Felder dokumentiert". Dokumentiert ist ein Feld, das es nicht gibt, und eine Schema-Migration, die nie geschrieben wurde. Belegt durch `FoodDatabaseContext.cs:64-65`, das die Integration selbst als **ausstehend** kommentiert.
**Soll**: Die Aussagen als *geplant* kennzeichnen statt als umgesetzt —
- Über Z. 103 eine Zeile setzen: `<p><strong>⏳ Geplant (noch nicht umgesetzt):</strong> ProduktInstanz speichert den Lagerort aktuell als <code>string</code> (<code>ProduktInstanz.cs:45</code>); die FK-Anbindung ist in <code>FoodDatabaseContext.cs:64-65</code> als offen vermerkt.</p>`
- Z. 124–125 ersatzlos aus dem Entity-Block entfernen (die Klasse hat die Property nicht).
- Z. 156: `ProduktInstanz wird mit LagerortId=3 erstellt` → `ProduktInstanz wird mit LagerortId=3 erstellt (⏳ geplant — aktuell string-Feld)`.
- Z. 337: `(LagerortId FK)` → `(LagerortId FK ⏳ geplant)`; Z. 338: `(AddToBestand mit Lagerort-Input ⏳ geplant)`.

### M3 — Code-Struktur führt die UI-Dateien nicht · MEDIUM
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeilen **255–266**
**Falsch**: Der Baum unter „💻 Implementierung (FERTIG ✅)" listet `Models/`, `Services/`, `Data/` — die beiden Razor-Komponenten, um die es in WP4 geht, fehlen, ebenso die Testdateien.
**Konsequenz**: Wer die WP4-Arbeit sucht, findet auf der Feature-Seite keinen Pfad dorthin. Der Abschnitt behauptet Vollständigkeit („FERTIG") für eine Struktur, in der die Hälfte des Features fehlt.
**Soll**: Den Baum ergänzen um
```
└── Components/Pages/Lager/
    ├── LagerortListe.razor ✅ (Route /lagerorte)
    └── LagerortForm.razor  ✅ (Route /lagerorte/neu)
```
und darunter die Testdateien nennen: `src/Tests/Unit/Services/LagerortServiceTests.cs` (24), `src/Tests/Ui/LagerortListeTests.cs` (5), `src/Tests/Ui/LagerortFormTests.cs` (3).

### M4 — Routen, DI und NavMenu-Eintrag nirgends dokumentiert · MEDIUM
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeilen **302–319**
**Falsch**: Die UI-Komponenten-Tabelle gibt je einen Satz Beschreibung. Es fehlen: Route `/lagerorte` (`LagerortListe.razor:1`), Route `/lagerorte/neu` (`LagerortForm.razor:1`), die injizierten Abhängigkeiten (`ILagerortService`, bei der Form zusätzlich `NavigationManager`), das Verhalten (Lade-Zustand, Leer-Zustand „Keine Lagerorte gefunden.", `alert-danger` bei Service-Exception, Redirect auf `/lagerorte` nach dem Speichern, `[Required]`-Validierung am `LagerortFormModel`) und der Navigationseintrag `<NavLink href="/lagerorte">Lagerorte</NavLink>` (`NavMenu.razor:39`).
**Konsequenz**: Das Projekt hat für genau das einen Maßstab: `docs/features/UC6-VerbrauchAusbuchen.html:143-154` dokumentiert Route, DI, Komponentenfelder und den NavMenu-Eintrag; die UC6-Seite hat dafür eine eigene Sektion. UC9 bleibt weit dahinter — und das ist die Doku zu der Phase, die dieses Gate abnehmen soll.
**Soll**: Nach der Tabelle Z. 319 eine Sektion „UI-Komponenten im Detail" nach dem Muster von `UC6-VerbrauchAusbuchen.html:143-154` einfügen, je Komponente mit: Route, injizierte Services, Felder (`lagerortListe`, `fehler`, `lädt` bzw. `formModel`, `lädt`, `fehler`), Happy Path, Fehlerpfad, Test-Verweis. Plus eine Zeile zum NavMenu-Eintrag (`NavMenu.razor:39`).

### M5 — Kein Workflow für die UC9-UI · MEDIUM
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeilen **142–178**
**Falsch**: Beide Workflows beschreiben das Einbetten von Lagerorten in UC10 und UC2 — also Abläufe, die es (siehe M2) so nicht gibt. Der Ablauf, der **tatsächlich** implementiert ist, kommt nicht vor.
**Konsequenz**: Teil 2 verlangt „Workflow Sektion: Geschäftslogik erklärt". Für die gelieferte UI ist sie leer.
**Soll**: Einen „Workflow 3: Lagerorte verwalten (UC9-UI)" ergänzen, gegen den Code formuliert:
```
1. Benutzer klickt im Menü auf "Lagerorte" (NavMenu → /lagerorte)
2. LagerortListe ruft in OnInitializedAsync ILagerortService.GetAlleLagerorte()
3. Treffer → Tabelle (Name, Erstellt am, Format dd.MM.yyyy)
   Leer → Hinweis "Keine Lagerorte gefunden."
   Exception → alert-danger "Fehler beim Abrufen der Lagerorte: …"
4. Klick "+ Neu" → /lagerorte/neu
5. Eingabe Name → [Required] + Service-Validierung (nur A-Z, a-z)
6. Speichern → GetOrCreateAsync(name): normalisiert, legt an oder gibt Bestehendes zurück
7. Erfolg → NavigationManager.NavigateTo("/lagerorte")
   ArgumentException/ArgumentNullException → alert-danger "Eingabefehler: …"
```

### M6 — Normalisierungsregel widerspricht dem eigenen Beispiel · MEDIUM
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeilen **59–60**
**Falsch**: Die Regel lautet „Single-Word-Input: Capitalize (1. Buchstabe groß, **Rest klein**)", das Beispiel in derselben Zeile ist `"lagerA" → "LagerA"` — der Rest bleibt dort gerade **nicht** klein. Z. 292 derselben Seite beschreibt das tatsächliche Verhalten korrekt („Smart Capitalization: Erkenne Lower-to-Upper Übergänge, Behalte Pattern bei 1 Übergang & ≤2 Upper"). Gegen die Tests geprüft: `NormalisiereLagerort_LowercaseWithCapitals_ShouldCapitalize` erwartet `"LagerA"`, `NormalisiereLagerort_MixedCase_ShouldCapitalize` erwartet für `"lAgEr"` → `"Lager"`.
**Konsequenz**: Die Anforderungstabelle — die Stelle, an der ein Leser die Regel nachschlägt — beschreibt ein anderes Verhalten als der Code und als die Implementierungstabelle weiter unten.
**Soll**: Z. 59 auf die tatsächliche Regel ziehen: `Single-Word-Input: Erster Buchstabe groß. Ein einzelner Lower→Upper-Übergang mit ≤2 Großbuchstaben bleibt erhalten, sonst wird der Rest klein.` Beispiele Z. 60 um `"lAgEr" → "Lager"` ergänzen.

### M7 — ER-Diagramm zeigt eine FK-Spalte, die es nicht gibt · MEDIUM
**Datei**: `diagrams/database-schema.drawio`, Zelle `ProduktInstanz (ZENTRAL!)`, Attributliste
**Falsch**: Die Feldliste enthält `FK LagerortId: int (UC9)`. Im Code existiert die Spalte nicht (siehe Kernbefund).
**Konsequenz**: Teil 4 gegen Teil 2 und gegen den Code — derselbe Widerspruch wie M2, nur im Diagramm.
**Soll**: **Nur** dieses eine `value`-Attribut ändern: `FK LagerortId: int (UC9)` → `FK LagerortId: int (UC9 – ⏳ geplant)`. Gezielte String-Ersetzung mit `Edit`, kein XML-Werkzeug zum Speichern; danach XML-Validität und eindeutige `id` prüfen. Die übrigen Feldabweichungen dieser Zelle (`Einheit`, `Barcode`, `MHD`, `KaufDatum` gegen die realen Namen `MindestbestandMenge`, `Verfallsdatum`, `Einkaufsdatum`, `ErstelltAm`) betreffen UC10 und gehören **nicht** in diesen Branch — siehe L6.

### M8 — Sequence-Diagramm kennt die UC9-UI nicht · MEDIUM
**Datei**: `diagrams/sequence-uc9-lagerorte.drawio`
**Falsch**: Einzige UI-Lifeline ist `Blazor UI (UC10/UC2)`. Die beiden UC9-Seiten kommen nicht vor; Szenario 1 endet mit `CreateProduktInstanz(…, lagerortId=3, …)` — ein Aufruf, den es nicht gibt.
**Konsequenz**: Teil 4 verlangt „Sequence-Diagramme: neue Workflows". Der WP4-Workflow ist neu und fehlt. Bei UC6 wurde genau das mit `1758dbe` nachgezogen (`VerbrauchListe.razor` + `/verbrauchen` als eigene Lifeline) — dieser Maßstab gilt hier ebenso.
**Soll**: Ein „SZENARIO 3: Lagerort über die UC9-UI anlegen" ergänzen, mit Lifelines `Benutzer` → `LagerortForm.razor (/lagerorte/neu)` → `LagerortService` → `SQLite`, den Pfeilen `Klick "Speichern" (btn-speichern)` → `GetOrCreateAsync(name)` → `Validate + Normalisiere` → `INSERT/SELECT` → `NavigateTo("/lagerorte")`, plus dem Fehlerrückweg `ArgumentException → alert-fehler`. Vorgehen wie bei M7: gezielte `Edit`-Einfügungen, neue `id`s eindeutig, danach XML prüfen.
*Alternative, falls der Diagramm-Eingriff bewusst vermieden werden soll*: In der Diagramm-Sektion der Feature-Seite (Z. 427–437) ausdrücklich vermerken, dass `sequence-uc9-lagerorte.drawio` nur die Service-Szenarien abdeckt und die UI-Ebene offen ist. Eine der beiden Varianten muss umgesetzt werden — ein stillschweigendes Weglassen nicht.

### M9 — „User Review & Approval ✅ FERTIG" ohne Beleg · MEDIUM
**Datei**: `docs/features/UC9-Lagerorte.html`, Zeile **364** (neu in `378559c`)
**Falsch**: Die Zeile behauptet ein erfolgtes User-Review. `23eb5fc` liegt auf der First-Parent-Linie von `origin/master`, wurde also **direkt** committet — es gibt keine zugehörige PR (`git log --merges`: PR #1–#9, keine für UC9) und unter `reviews/` keinen UC9-WP4-Report. Dass dieses Gate nie stattfand, ist der Anlass dieses Branches; die Zeile behauptet nun das Gegenteil.
**Konsequenz**: Aus einer ehrlichen Lücke („Pending") wird eine falsche Abnahme. Das ist schlechter als der Ausgangszustand.
**Soll**: Z. 364 auf den belegbaren Stand ziehen, z. B.
`<input type="checkbox" checked> User Review & Approval – ✅ Code auf master seit 23eb5fc (Direkt-Commit, kein PR); Doku-Gate nachgeholt am 2026-09-19, siehe reviews/uc9-wp4-doku-review-1.md`.
Z. 365 kann bleiben, gewinnt aber durch die Präzisierung `(Direkt-Commit 23eb5fc, 30.07.2026 — kein PR)`.

---

## Befunde ohne Freigabe-Wirkung

### L1 — SQL-Dialekt passt nicht zum Projekt · LOW
`docs/features/UC9-Lagerorte.html:95-100` nutzt `INT PRIMARY KEY AUTO_INCREMENT`, `DEFAULT GETDATE()`, `BIT` — MySQL- bzw. SQL-Server-Syntax in einem SQLite-/EF-Core-Projekt, dessen Schema aus `src/App/Migrations/` stammt.
**Soll**: Als illustratives Pseudo-DDL kennzeichnen oder durch die reale Migration ersetzen.

### L2 — XML-Doc beschreibt die Normalisierung falsch · LOW
`src/App/Services/Interfaces/ILagerortService.cs:40`: `<returns>Der normalisierte Name (First Letter Upper, Rest Lower).</returns>` — für `"lagerA" → "LagerA"` trifft „Rest Lower" nicht zu (gleiche Ursache wie M6). Code-Datei, in diesem Branch nicht zu ändern; beim nächsten Anfassen mitziehen.

### L3 — Kein Validierungsreport für UC9-WP4 · LOW
Unter `reviews/` liegen `uc9-doku-review-1.md` und `uc9-test-review-1.md`, beide vom 25.06.2026 und ausschließlich zur Service-Phase (bestätigt: kein Treffer auf `razor`/UI/WP4). UC6 hat zusätzlich `uc6-wp4-documentation-validation.md`. Mit dem vorliegenden Report ist das Gate dokumentiert; ein eigener Validierungsreport ist nicht zwingend, die Lücke aber festzuhalten.

### L4 — Begründung in der Commit-Message stimmt nicht · LOW
`378559c` schreibt: „The badge class stays `done` because the page defines no other." `docs/css/shared.css:309` definiert `.status-badge.in-progress`, `:314` `.status-badge.todo`. Das Beibehalten von `done` war durch den Feature-Plan gedeckt („Badge bleibt, wird aber sprachlich auf den Service bezogen") und ist konsistent mit der UC3-Karte (Z. 140–143, ebenfalls Service-only mit `done`) — **keine Änderung nötig**, nur die Begründung war falsch.

### L5 — `ProduktInstanz`-Zeile der Overview wird durch die neue UC10-Karte mehrdeutig · LOW
`docs/architecture-overview.html:213`: `✅ UC10 + UC2 (Service + UI WP4)`. Gemeint ist die UC2-UI; neben der neuen Karte („UI (WP4 offen)") liest es sich wie eine fertige UC10-UI.
**Soll** (optional): `✅ UC10 Service + UC2 (Service + UI WP4) — UC10-UI ⏳ offen`.

### L6 — Beobachtungen außerhalb des Scopes (Code / UC10) · LOW
- `LagerortListe.razor:54-58`: Der `<summary>`-Block beschreibt die **Komponente**, steht aber am Feld `lagerortListe`. Inhaltlich richtig, formal am falschen Symbol.
- `LagerortForm.razor:58-82`: `lädt` wird im Erfolgsfall nicht zurückgesetzt — folgenlos, weil direkt navigiert wird; die drei `catch`-Blöcke setzen es korrekt.
- `diagrams/database-schema.drawio`, Zelle `ProduktInstanz`: `Einheit`, `Barcode`, `MHD`, `KaufDatum` entsprechen nicht `ProduktInstanz.cs` (`MindestbestandMenge`, `Verfallsdatum`, `Einkaufsdatum`, `ErstelltAm`). **UC10-Gebiet** — gehört in das UC10-Gate, nicht hierher.
- Die eigentliche UC9→UC10-Integration (FK statt String) ist eine offene **Code**-Aufgabe. Sie taucht in `CONTEXT_SUMMARY.md` als Aufgabe nicht auf und sollte beim anstehenden WP4-UC10 mitentschieden werden.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Doku `LagerortListe.razor`, `LagerortForm.razor`, `Lagerort.cs` | BESTANDEN |
| 1 | `ILagerortService.cs` XML-Docs | BESTANDEN mit L2 |
| 2 | `UC9-Lagerorte.html` — Testzahlen, Testnamen, Einschränkungen, Diagramm-Links | BESTANDEN (Auftrag A verifiziert) |
| 2 | `UC9-Lagerorte.html` — Domain Model / Schema | **M2 (HIGH)** |
| 2 | `UC9-Lagerorte.html` — UI-Doku (Struktur, Routen, Workflow) | **M3, M4, M5** |
| 2 | `UC9-Lagerorte.html` — Status-Konsistenz | **M1 (HIGH), M9** |
| 2 | `UC9-Lagerorte.html` — Anforderungstabelle | **M6** |
| 3 | `architecture-overview.html` — UC9-Karte, Z. 337/338/356, Gesamtzahl 361 | BESTANDEN |
| 3 | `architecture-overview.html` — UC10-Karte (Auftrag A) | BESTANDEN |
| 4 | `requirements/use-cases.drawio` | BESTANDEN |
| 4 | `diagrams/database-schema.drawio` | **M7** |
| 4 | `diagrams/sequence-uc9-lagerorte.drawio` | **M8** |

---

## Fazit

**Freigabe nicht erteilt.** 2 HIGH, 7 MEDIUM blockierend; 6 LOW ohne Freigabe-Wirkung. Kein Befund berührt den Produktivcode — die 361 Tests bleiben grün und sind von allen Korrektionen unabhängig.

Auftrag A ist gute Arbeit: Alle acht Zahlen stimmen, keine Stelle übersehen, nichts Richtiges mitgerissen, 12 Zeilen Diff ohne Formatdrift, CRLF erhalten, die UC10-Karte deckt sich mit der Legende in `use-cases.drawio`. Der einzige Befund daraus ist M9 — eine Checkbox, die aus „Pending" ein „FERTIG" gemacht hat, für das es keinen Beleg gibt.

Auftrag B zeigt, was das übersprungene Gate gekostet hat, und es ist mehr als die Testzahl. Die Feature-Seite dokumentiert zu einer Phase, die reine UI-Arbeit war, **keine Route, keinen Workflow, keinen Dateipfad** — während die UC6-Seite genau das vorbildlich tut. Und sie beschreibt seit Juni eine FK-Beziehung zwischen `ProduktInstanz` und `Lagerort`, die nie gebaut wurde; `FoodDatabaseContext.cs:64-65` sagt das selbst, die Doku und das ER-Diagramm sagen das Gegenteil. Über ein Jahr Projektlaufzeit hat niemand widersprochen, weil niemand hinsehen musste.

Ein Muster ist dabei festzuhalten, nicht als Vorwurf an den `doc-agent`: `378559c` hat genau die acht Zahlen und zwei Zeilen korrigiert, die im Auftrag standen — nicht mehr. Der Auftrag beschrieb String-Ersetzungen, kein Doku-Review. Für Versuch 2 sollte der Auftrag deshalb **inhaltlich** formuliert sein: „Die UC9-Feature-Seite dokumentiert die WP4-UI vollständig (Routen, Verhalten, Workflow) und behauptet keine Integration, die es nicht gibt" — statt einer Liste von Zeilennummern.

**Nächster Schritt**: M1–M9 an den `doc-agent`, Versuch 2/3. Jeder Befund nennt Datei, Zeile und Soll-Zustand und ist ohne Rückfrage abarbeitbar. Für M7 und M8 gilt die Auflage aus dem Feature-Plan: gezielte `Edit`-Ersetzungen, kein Neuschreiben über XML-Werkzeuge, danach XML-Validität prüfen.

---

**Reviewer**: Review-Agent
**Datum**: 2026-09-19
**Versuch**: 1 von 3
**Urteil**: **CHANGES REQUESTED** (Freigabe nicht erteilt)
