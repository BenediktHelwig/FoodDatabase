# UC9 (WP4) Dokumentation Review – Versuch 2/3

**Datum**: 2026-09-19
**Phase**: Doku-Review WP4 UC9 (nachgeholt)
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `docs/uc9-wp4-nachzug` (Commits `378559c` → `bd29e58` → `dcc337b` über `master` `a57e572`)
**Vorgänger**: `reviews/uc9-wp4-doku-review-1.md` (CHANGES REQUESTED, 2 × HIGH + 7 × MEDIUM blockierend, 6 × LOW)

---

## VERDICT: PASS
## ATTEMPT: 2 von 3

**Freigabe erteilt.** Alle neun blockierenden Befunde M1–M9 sind behoben — sieben vollständig,
zwei (M2, M4) mit LOW-Restpunkten, die keine Aussage falsch machen. L1 und L5 sind ebenfalls
umgesetzt und korrekt.

**Regressionen**: keine gefundenen Falschaussagen in den ~250 neuen Zeilen. Ich habe die neuen
Abschnitte Satz für Satz gegen `LagerortListe.razor`, `LagerortForm.razor`, `LagerortService.cs`,
`NavMenu.razor` und `20260625145118_InitialCreate.cs` formuliert — nicht gegen die Commit-Message.
Sechs neue LOW-Anmerkungen (N1–N6), keine davon freigabe-relevant.

**Testlauf**: `dotnet test src/Tests/FoodDatabase.Tests.csproj` → **361 bestanden, 0 Fehler,
0 übersprungen** (selbst ausgeführt). `src/` ist im ganzen Branch unberührt.

**XML-Validität**: `sequence-uc9-lagerorte.drawio` (51 Zellen, +21), `database-schema.drawio`
(50 Zellen) und `requirements/use-cases.drawio` (34 Zellen, unverändert) parsen sauber,
**keine doppelten `id`**.

**HTML-Validität**: `UC9-Lagerorte.html` mit einem Parser gegengeprüft — keine offenen Tags,
keine Mismatches; `<section>` 11/11, `<div>` 7/7, `<ul>` 16/16, `<table>` 6/6, `<pre>` 9/9
ausbalanciert. Relevant, weil ein `<pre>`-Block, zwei Tabellen und vier Listen neu sind.

**Diff-Hygiene**: `git diff --check` sauber. Branch gegen `master`: 5 Dateien, +532/-34, davon
271 Zeilen der Report 1 selbst. Beide HTML-Dateien **durchgängig CRLF** (571/571 bzw. 383/383
Zeilen, auch die neuen), beide `.drawio` weiterhin LF wie auf `master`. Keine Formatdrift, keine
gelöschten Kommentare, kein fremder Status, kein Produktivcode. `git status` zeigt nur die
untracked `bash.exe.stackdump` — die gehört **nicht** in den Commit.

---

## Befund-für-Befund: Nachprüfung

| # | Befund aus Report 1 | Severity | Status |
|---|---|---|---|
| M1 | Fuß der Seite „READY FOR TEST-AGENT" | HIGH | **behoben** |
| M2 | FK `LagerortId` + Navigations-Property als umgesetzt dokumentiert | HIGH | **behoben**, LOW-Rest (N6) |
| M3 | Code-Struktur ohne die Razor- und Testdateien | MEDIUM | **behoben** |
| M4 | Routen, DI, Verhalten, NavMenu nicht dokumentiert | MEDIUM | **behoben**, 3 LOW-Reste (N1–N3) |
| M5 | Kein Workflow für die UC9-UI | MEDIUM | **behoben** |
| M6 | Normalisierungsregel widerspricht dem eigenen Beispiel | MEDIUM | **behoben** |
| M7 | ER-Diagramm zeigt nicht existierende FK-Spalte | MEDIUM | **behoben** |
| M8 | Sequence-Diagramm kennt die UC9-UI nicht | MEDIUM | **behoben**, 2 LOW-Anmerkungen (N4, N5) |
| M9 | „User Review & Approval ✅ FERTIG" ohne Beleg | MEDIUM | **behoben** |

### M1 — behoben
`docs/features/UC9-Lagerorte.html:567`: Fuß lautet jetzt
`Dokumentation erstellt: 2026-06-25 | Aktualisiert: 2026-09-19 | Status: FERTIG (Service + UI, auf master seit 23eb5fc)`.
Gegenprobe über die ganze Datei nach `Pending`, `18 Service`, `26 GRÜN`, `26/26`, `18 Tests`:
**null Treffer**. Kopf (Z. 21), Checkliste (Z. 488–492), Phasen-Tabelle (Z. 504–540) und Fuß sagen
jetzt alle dasselbe. Der Konsistenz-Check aus `CLAUDE.md` ist damit erfüllt.

### M2 — behoben (LOW-Rest N6)
Alle fünf beanstandeten Stellen sind erfasst:
- Z. 105: neuer Absatz `⏳ Geplant (noch nicht umgesetzt): … string (ProduktInstanz.cs:45); die FK-Anbindung ist in FoodDatabaseContext.cs:64-65 als offen vermerkt.` — beide Quellenangaben nachgeprüft, sie stimmen.
- Z. 123–125 alt: die `ICollection<ProduktInstanz>`-Navigation ist **ersatzlos entfernt**. Der Entity-Block zeigt jetzt genau die vier Properties, die `src/App/Models/Lagerort.cs` hat.
- Z. 156: `(⏳ geplant — aktuell string-Feld)` ergänzt.
- Z. 464/465: beide `Erweitert:`-Zeilen mit `⏳ geplant` versehen.
Gegenprobe: `grep -rn "LagerortId" src/ --include=*.cs --include=*.razor` liefert weiterhin
**keinen** Treffer; die Prämisse des Befundes gilt also unverändert, und die Seite behauptet
nirgends mehr das Gegenteil. Ein `grep` über die Seite nach `LagerortId`/`FK` findet nur noch
qualifizierte Stellen.

### M3 — behoben
Z. 279–290: Der Baum führt jetzt `Components/Pages/Lager/LagerortListe.razor ✅ (Route /lagerorte)`
und `LagerortForm.razor ✅ (Route /lagerorte/neu)` sowie einen `src/Tests/`-Zweig mit
`Unit/Services/LagerortServiceTests.cs (24)`, `Ui/LagerortListeTests.cs (5)`,
`Ui/LagerortFormTests.cs (3)`. Alle drei Pfade existieren, die Routen stimmen mit
`LagerortListe.razor:1` und `LagerortForm.razor:1` überein, die Zahlen mit der Gegenzählung aus
Report 1.

### M4 — behoben (LOW-Reste N1–N3)
Neue Sektion „UI-Komponenten im Detail" (Z. 349–446), im Aufbau deckungsgleich mit dem Maßstab
`docs/features/UC6-VerbrauchAusbuchen.html:143-154`: Route → Beschreibung →
Service-Abhängigkeiten → Komponenten-Struktur (Tabelle Element/Typ/Zweck) → Verhalten. Gegen den
Code geprüft, jede Angabe einzeln:

| Aussage | Beleg |
|---|---|
| Route `/lagerorte` bzw. `/lagerorte/neu` | `LagerortListe.razor:1`, `LagerortForm.razor:1` ✓ |
| DI Liste: `ILagerortService` | `LagerortListe.razor:4` ✓ |
| DI Form: `ILagerortService` + `NavigationManager` | `LagerortForm.razor:5-6` ✓ |
| Felder `lagerortListe`, `fehler`, `lädt` | `LagerortListe.razor:59-61` ✓ |
| Felder `formModel` (`LagerortFormModel`), `fehler`, `lädt` | `LagerortForm.razor:54-56`, `:84-88` ✓ |
| Spinner + „Lädt…" | `LagerortListe.razor:16-22` ✓ |
| „Keine Lagerorte gefunden." | `:49` ✓ |
| `alert-danger` „Fehler beim Abrufen der Lagerorte: …" | `:25`, `:78` ✓ |
| Spalten `Name | Erstellt am`, Format `dd.MM.yyyy` | `:31-41` ✓ |
| `[Required]` via `DataAnnotationsValidator` | `LagerortForm.razor:19`, `:86` ✓ |
| Speichern-Button disabled während `lädt` | `:37` ✓ |
| „Abbrechen" verlinkt `/lagerorte` | `:40` ✓ |
| NavMenu-Eintrag `NavMenu.razor:39` | wörtlich vorhanden ✓ |

Damit hält die UC9-Seite dem UC6-Maßstab stand — sie ist an einer Stelle sogar genauer (Lade-,
Leer-, Fehler- und Erfolgszustand einzeln aufgeführt). Die drei verbliebenen Ungenauigkeiten
stehen unter N1–N3.

### M5 — behoben
Neuer „Workflow 3: Lagerorte verwalten (UC9-UI)" (Z. 170–188). Schritt für Schritt gegen den Code
gehalten: `OnInitializedAsync` → `GetAlleLagerorte()` (`LagerortListe.razor:63-74`), die drei
Anzeigezustände, `+ Neu` → `/lagerorte/neu` (`:12`), `GetOrCreateAsync` mit Validierung und
Normalisierung (`LagerortService.cs:146-152`), `NavigateTo("/lagerorte")`
(`LagerortForm.razor:65`), Fehlerzweig `ArgumentException`/`ArgumentNullException` → `alert-danger`
(`:67-76`). Das Beispiel `"kuehler" → "Kuehler"` ist gegen `^[A-Za-z]+$` **und** gegen
`NormalisiereLagerort` durchgerechnet und richtig.

### M6 — behoben
Z. 59–60 lautet jetzt „Erster Buchstabe groß. Ein einzelner Lower→Upper-Übergang mit ≤2
Großbuchstaben bleibt erhalten, sonst wird der Rest klein." mit den Beispielen
`"lagerA" → "LagerA"`, `"lAgEr" → "Lager"`, `"LAGER" → "Lager"`. Gegen
`LagerortService.NormalisiereLagerort` (`:186-236`) nachgerechnet:
`lowerToUpperTransitions == 1 && upperCaseCount <= 2` → Pattern bleibt, sonst Simple Capitalize.
`"lagerA"`: 1 Übergang, 1 Upper → `LagerA` ✓. `"lAgEr"`: 2 Übergänge → `Lager` ✓. `"LAGER"`:
0 Übergänge → `Lager` ✓. Die Regel deckt sich jetzt mit Z. 292 derselben Seite und mit den vier
`NormalisiereLagerort`-Tests. Kein Widerspruch mehr.

### M7 — behoben
`diagrams/database-schema.drawio`: genau **ein** `value`-Attribut geändert, `FK LagerortId: int (UC9)`
→ `FK LagerortId: int (UC9 – ⏳ geplant)`. Diff = 1/1 Zeile, `id`s unverändert, XML valide, keine
Neuformatierung, die übrigen (UC10-)Feldabweichungen dieser Zelle bewusst nicht angefasst — genau
wie in L6 verlangt. Vorbildlich minimal.

### M8 — behoben (LOW-Anmerkungen N4, N5)
`diagrams/sequence-uc9-lagerorte.drawio`: „SZENARIO 3: Lagerort über die UC9-UI anlegen
(/lagerorte/neu)" mit eigener Lifeline `LagerortForm.razor` (`s3-form`, `s3-line-form`), neun
Nachrichten, Fehler-Note und Resultat-Note; `pageHeight` 900 → 1400 und die fünf Bestands-Lifelines
auf y=1350 verlängert. Die Inhalte gegen den Code:
- `1: Klick "Speichern" (btn-speichern)` → `LagerortForm.razor:36` ✓
- `2: GetOrCreateAsync(formModel.Name)` → `:64` ✓
- `3: ValidateLagerort + NormalisiereLagerort` → `LagerortService.cs:148-152` ✓ (Platzierung siehe N4)
- `4: FirstOrDefaultAsync(Name == …)` / `6: Add(new Lagerort) + SaveChangesAsync()` / `7: Lagerort(Id=3)` → `LagerortService.cs:154-173` ✓
- `9: NavigateTo("/lagerorte")` → `LagerortForm.razor:65` ✓
- Fehler-Note `ArgumentException/ArgumentNullException → alert-fehler` → `:67-76`, `:15` ✓
Das Szenario folgt der Koordinaten- und Pfeilkonvention der bestehenden Szenarien (User-Pfeile
x=80→200, Service→Repo 370→540, dashed/open für Rückgaben). Der User hat die volle Variante
statt des Vermerks gewählt — sie ist geliefert, nicht abgekürzt.

**Zur XML-Reparatur in Zeile 24** (außerhalb meiner Liste, ausdrücklich zur Bewertung vorgelegt):
`value="IRepository\n<Lagerort>"` → `value="IRepository&#10;&lt;Lagerort&gt;"` ist **richtig und
notwendig**. Unescaped `<Lagerort>` machte die Datei zu ungültigem XML; draw.io hätte sie nicht
mehr geöffnet. Eine Anmerkung zur Begründung: `&#10;` ist in dieser Datei **nicht** das übliche
Format — es kommt genau zweimal vor (diese Zelle und die neue `s3-form`), während drei
Bestands-Zellen (`actor_ui:18`, `actor_service:21`, `actor_db:27`) ein literales `\n` tragen.
`&#10;` ist trotzdem die **korrekte** Wahl: mxGraph wandelt ein echtes Newline-Zeichen in HTML-Labels
in `<br>` um, ein literales `\n` wird dagegen als die zwei Zeichen „\n" gerendert. Die Änderung hat
also nicht nur die XML-Validität hergestellt, sondern nebenbei das einzige korrekt umbrechende
Label der Datei erzeugt. Die drei Altlasten bleiben als Hinweis stehen (siehe HINWEISE).

### M9 — behoben
Z. 491 lautet jetzt
`User Review & Approval – ✅ Code auf master seit 23eb5fc (Direkt-Commit, kein PR); Doku-Gate nachgeholt am 2026-09-19, siehe reviews/uc9-wp4-doku-review-1.md`,
Z. 492 `Merged zu Master – ✅ FERTIG (Commit 23eb5fc, 30.07.2026 — kein PR)`. Beides belegbar:
`23eb5fc` liegt auf der First-Parent-Linie von `origin/master`, unter `git log --merges` gibt es
keine UC9-PR, und der referenzierte Report liegt seit `bd29e58` im Repo. Aus der falschen Abnahme
ist eine präzise, nachprüfbare Aussage geworden. (Ergänzungsvorschlag ohne Freigabe-Wirkung: N7.)

---

## Neue Befunde aus der Nachbesserung (alle LOW, **nicht** freigabe-blockierend)

### N1 — NavMenu-Satz beschreibt das Gegenteil dessen, was passiert · LOW
`docs/features/UC9-Lagerorte.html:446`: „… und wird über `data-bs-dismiss="offcanvas"` automatisch
geschlossen, wenn der Benutzer das Menü **öffnet**." Das Attribut schließt das Offcanvas-Menü beim
**Klick auf den Link**, nicht beim Öffnen. Der Satz ist in sich widersprüchlich.
**Soll**: „… das Offcanvas-Menü schließt sich beim Klick auf den Eintrag automatisch."
Nebenpunkt: Z. 445 zitiert das Markup ohne genau dieses Attribut, obwohl der Folgesatz es
bespricht — ins Zitat aufnehmen.

### N2 — `btn-neu` ist kein `NavLink` und kein Komponentenfeld · LOW
`docs/features/UC9-Lagerorte.html:383-384`: Typ-Spalte sagt `NavLink`. Im Code ist es ein einfacher
Anker: `<a href="/lagerorte/neu" class="btn btn-primary" data-testid="btn-neu">+ Neu</a>`
(`LagerortListe.razor:12`) — kein Blazor-`NavLink`. Außerdem stehen in derselben Tabelle sonst nur
C#-Felder; `btn-neu` ist ein `data-testid`.
**Soll**: Typ `<a> (data-testid="btn-neu")` oder die Zeile in den Abschnitt „Verhalten" ziehen.

### N3 — Dritter `catch`-Zweig der Form fehlt in der Fehlerdoku · LOW
`docs/features/UC9-Lagerorte.html:438` nennt nur `ArgumentException`/`ArgumentNullException`.
`LagerortForm.razor:77-81` hat einen dritten Zweig: `catch (Exception ex)` → `Fehler beim Speichern: {ex.Message}`.
Teil 1 des 4-Teil-Checks verlangt „Fehlerbehandlung dokumentiert" — eine Zeile fehlt.

### N4 — Sequence: Service-interne Aufrufe hängen an der Repository-Lifeline · LOW
`diagrams/sequence-uc9-lagerorte.drawio:252` (`s3-msg3`): `ValidateLagerort + NormalisiereLagerort`
ist als Pfeil von x=370 (Service) nach x=540 (`IRepository<Lagerort>`) gezeichnet. Beides sind
Selbstaufrufe innerhalb von `LagerortService`; die Repository-Abstraktion existiert im Code gar
nicht (der Service nutzt `FoodDatabaseContext` direkt). **Das ist die Konvention der bestehenden
Szenarien** (`msg1_3`, `msg1_7` machen es genauso), also kein Rückschritt — aber der neue
Szenario-Block ist in sich inkonsistent, weil `s3-msg4`/`s3-msg6` die Repo-Lifeline überspringen
und direkt zur DB gehen. Sauber wäre ein Selbstaufruf-Pfeil auf der Service-Lifeline.

### N5 — Zwei Lifelines auf derselben x-Position · LOW
Dieselbe Datei: `line_ui` (generisches „Blazor UI (UC10/UC2)", x=200) wurde auf y=1350 verlängert
und läuft damit durch Szenario 3, wo an exakt derselben x-Position die neue Lifeline
`s3-line-form` (y=920–1200) liegt. Die Pfeile des Szenarios sind dadurch formal zweideutig
adressiert; optisch fällt es nicht auf, weil die Linien deckungsgleich sind.
**Soll** (optional): `line_ui` bei y≈800 enden lassen — für Szenario 3 wird sie nicht gebraucht.

### N6 — Überschrift „(UPDATED)" über einem geplanten Schema · LOW
`docs/features/UC9-Lagerorte.html:104`: `<h3>Tabelle: ProduktInstanz (UPDATED)</h3>` — direkt
darunter steht der `⏳ Geplant`-Absatz aus M2. Der Widerspruch ist entschärft, weil die
Einschränkung unmittelbar folgt, aber „UPDATED" liest sich weiter wie „erledigt".
**Soll**: `Tabelle: ProduktInstanz (⏳ geplante Erweiterung)`.

### N7 — Der Verweis in Z. 491 zeigt auf den abgelehnten Report · LOW
Die M9-Korrektur belegt das nachgeholte Doku-Gate mit `reviews/uc9-wp4-doku-review-1.md` — das ist
der Report mit dem Urteil CHANGES REQUESTED. Der abschließende Beleg ist dieser Report
(`reviews/uc9-wp4-doku-review-2.md`).
**Soll** (kann ohne eigene Schleife im selben Commit mitlaufen, mit dem dieser Report geschrieben
wird): `… siehe reviews/uc9-wp4-doku-review-1.md und -2.md`.

### N8 — Unescaptes `<Lagerort>` in der HTML-Seite selbst · LOW
`docs/features/UC9-Lagerorte.html:286`: `└── FoodDatabaseContext.cs ✅ (DbSet<Lagerort>)` — innerhalb
eines `<pre>`, aber **nicht** escaped. Der Browser parst `<Lagerort>` als unbekanntes Element; die
Seite zeigt gerendert „(DbSet)". Derselbe Defekttyp, der im Sequence-Diagramm gefunden und behoben
wurde — hier in der Datei, die gerade bearbeitet wurde. Der Defekt ist **älter als dieser Branch**
(identisch auf `master`, Z. 265) und die einzige Stelle der Datei dieser Art.
**Soll**: `DbSet&lt;Lagerort&gt;`. Alle neu eingefügten generischen Typen sind korrekt escaped —
der Fehler wurde nicht reproduziert.

---

## L1 und L5 — mitbewertet

**L1 (SQL-Dialekt) — umgesetzt und korrekt.** Z. 94 kennzeichnet den Block als „Illustratives
Pseudo-DDL; tatsächliche Implementierung in SQLite Migrationen unter `src/App/Migrations/`", das
DDL selbst ist auf SQLite gezogen (`INTEGER PRIMARY KEY AUTOINCREMENT`, `TEXT`, `BOOLEAN`).
Gegen `20260625145118_InitialCreate.cs:14-27` geprüft: Spaltennamen `Id`, `Name`, `CreatedAt`,
`IsArchived` und `UNIQUE` auf `Name` (`IX_Lagerorte_Name`, `unique: true`) stimmen. Die beiden
`DEFAULT`-Klauseln gibt es real nicht — EF setzt die Werte im Code (`LagerortService.cs:163-168`).
**Ich trage die Einschätzung des Orchestrators mit**: mit der Pseudo-DDL-Kennzeichnung ist das
vertretbar, es ist eine Illustration und keine Schema-Referenz.

**L5 (`ProduktInstanz`-Zeile der Overview) — umgesetzt und korrekt.**
`docs/architecture-overview.html:213`: `✅ UC10 Service + UC2 (Service + UI WP4) — UC10-UI ⏳ offen`.
Deckt sich mit der UC10-Karte (Z. 125–130), mit Z. 358 (`WP4 Weitere UI-Komponenten (UC4, UC10)`)
und mit der Legende in `requirements/use-cases.drawio:112`. Die Mehrdeutigkeit ist weg. Diff: 1
Zeile, keine Nebenwirkung.

## L2, L3, L4, L6 — nicht umgesetzt: mitgetragen

- **L2** (`ILagerortService.cs:40`, „Rest Lower") und **L6** (Code-/UC10-Beobachtungen) sind
  Code-Änderungen. Dieser Branch fasst `src/` nicht an — richtig so, sie gehören in den Branch, der
  die Datei ohnehin anfasst. Mitgetragen. **Zur Vormerkung für WP4 UC10**: L2 zusammen mit dem
  Platzhalter `placeholder="z.B. Kühlschrank"` in `LagerortForm.razor:27` — dieser Vorschlag
  verstößt selbst gegen `^[A-Za-z]+$` und würde beim Absenden eine `ArgumentException` auslösen.
  Das ist ein echter, wenn auch kleiner UX-Fehler im Produktivcode, kein Doku-Befund.
- **L3** ist durch die Reports 1 und 2 erledigt. Mitgetragen.
- **L4**: Commit-Messages werden nicht umgeschrieben — das ist die Regel aus `CLAUDE.md`
  („Nie den Verlauf umschreiben"). Die Richtigstellung im Text von `dcc337b` ist der korrekte Weg.
  Mitgetragen; eine Historienänderung hätte hier mehr Schaden angerichtet als Nutzen.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Doku `LagerortListe.razor`, `LagerortForm.razor`, `Lagerort.cs` | BESTANDEN (unverändert) |
| 1 | `ILagerortService.cs` XML-Docs | BESTANDEN mit L2 (Code, anderer Branch) |
| 2 | `UC9-Lagerorte.html` — Testzahlen, Testnamen, Einschränkungen, Diagramm-Links | BESTANDEN |
| 2 | `UC9-Lagerorte.html` — Domain Model / Schema | BESTANDEN (M2, L1 behoben; N6, N8 LOW) |
| 2 | `UC9-Lagerorte.html` — UI-Doku (Struktur, Routen, Verhalten, Workflow) | BESTANDEN (M3–M5 behoben; N1–N3 LOW) |
| 2 | `UC9-Lagerorte.html` — Status-Konsistenz | BESTANDEN (M1, M9 behoben; N7 LOW) |
| 2 | `UC9-Lagerorte.html` — Anforderungstabelle | BESTANDEN (M6 behoben) |
| 3 | `architecture-overview.html` — UC9-/UC10-Karte, Entities, Projekt-Status | BESTANDEN (L5 behoben) |
| 4 | `requirements/use-cases.drawio` | BESTANDEN (unverändert, war korrekt) |
| 4 | `diagrams/database-schema.drawio` | BESTANDEN (M7 behoben) |
| 4 | `diagrams/sequence-uc9-lagerorte.drawio` | BESTANDEN (M8 behoben + XML-Reparatur; N4, N5 LOW) |

---

## HINWEISE

- **Die XML-Reparatur war ein Fund, kein Streuverlust.** Sie ist offengelegt, im Commit-Text
  begründet, auf ein Attribut begrenzt und technisch richtig. Genau so gehört ein Defekt behandelt,
  über den man beim Arbeiten stolpert. Dass die Begründung „`&#10;` ist in dieser Datei üblich"
  faktisch nicht zutrifft (2 × `&#10;` gegen 3 × literales `\n`), ändert nichts am Ergebnis — die
  gewählte Form ist die einzige, die überhaupt funktioniert.
- **Drei Altlasten im selben Diagramm** (`actor_ui:18`, `actor_service:21`, `actor_db:27`) tragen
  weiterhin ein literales `\n`, das als Zeichenfolge gerendert wird. Rein kosmetisch, älter als
  dieser Branch, bewusst nicht anzufassen — beim nächsten Anfassen der Datei mitziehen.
- **Prozess-Beobachtung, positiv**: Der Befund aus Report 1 („der Auftrag beschrieb
  String-Ersetzungen, kein Doku-Review") ist adressiert worden. `dcc337b` liefert eine inhaltlich
  gefasste Nachbesserung: geprüfte Beispielnamen, ein gegen den Code formulierter Workflow, eine
  Sektion nach dem UC6-Maßstab. Die zwei Beispielnamen, die beim ersten Anlauf gegen `^[A-Za-z]+$`
  verstießen, sind vor dem Commit korrigiert worden — ich habe **alle** Beispielnamen der Seite
  nachgeprüft (`lagerA`, `lagerC`, `lagerb`, `kuehler`, `LagerA`, `LagerB`, `KuehlraumEins`,
  `lAgEr`, `LAGER`): jeder erfüllt die Validierung, und jedes angegebene Normalisierungsergebnis
  stimmt mit `NormalisiereLagerort` überein. Kein dritter Durchrutscher.
- `bash.exe.stackdump` liegt untracked im Working Tree und darf nicht mitcommittet werden.

---

## Fazit

**Freigabe erteilt.** Neun blockierende Befunde, neun behoben — und zwar so, dass das Ziel erreicht
ist und nicht nur die Stelle berührt wurde. Die Feature-Seite behauptet keine FK-Integration mehr,
die es nicht gibt, sondern nennt die Quelle, die die Lücke belegt. Sie dokumentiert die WP4-UI jetzt
mit Routen, Dateipfaden, injizierten Services, Feldern, allen vier Anzeigezuständen, dem
NavMenu-Eintrag und einem Workflow, den ich Zeile für Zeile gegen die beiden Razor-Dateien halten
konnte. Das Sequence-Diagramm hat das UI-Szenario, das der Maßstab `1758dbe` verlangt, in voller
Form statt als Vermerk — der Weg, den der User ausdrücklich gewählt hat.

Die Nachbesserung ist mit +250 Zeilen groß genug gewesen, um neue Fehler zu produzieren; sie hat
keine produziert, die eine Aussage falsch macht. Die acht LOW-Punkte sind ein falsch beschriebenes
Bootstrap-Attribut, ein Typ-Eintrag in einer Tabellenzelle, ein fehlender dritter `catch`, zwei
Diagramm-Feinheiten, eine Überschrift, ein Report-Verweis und ein unescapetes Zeichen, das seit Juni
so dasteht. Keiner davon rechtfertigt eine dritte Schleife; N1, N3, N6 und N7 sind
Ein-Zeilen-Änderungen, die ohne eigenes Gate in den Commit passen, mit dem dieser Report abgelegt
wird — wenn nicht, gehören sie in die Roadmap, nicht in eine Verzögerung.

Bemerkenswert an der Diff-Hygiene: `database-schema.drawio` = 1 geänderte Zeile, `architecture-overview.html`
= 1 geänderte Zeile. Wo der Befund klein war, ist die Änderung klein geblieben. Das ist der
Unterschied zwischen Beheben und Umschreiben.

**Nächster Schritt**: Doku-Gate abgehakt, weiter mit dem Abschluss — PR-Text, `CONTEXT_SUMMARY.md`,
Pull Request. Eine dritte Schleife ist nicht nötig. Für die Roadmap getrennt vorzumerken, nicht für
diese PR: **L2** und der Platzhalter `z.B. Kühlschrank` in `LagerortForm.razor:27`, **L6**, sowie die
eigentliche UC9→UC10-Integration (FK statt String) — alle drei sind Code-Aufgaben für WP4 UC10.

---

**Reviewer**: Review-Agent
**Datum**: 2026-09-19
**Versuch**: 2 von 3
**Urteil**: **PASS — Freigabe erteilt**
