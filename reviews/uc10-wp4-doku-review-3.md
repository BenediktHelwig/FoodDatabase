# UC10 (WP4) Dokumentation Review – Versuch 3/3

**Datum**: 19.09.2026
**Phase**: Doku-Review WP4 UC10
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `feat/ui-produktinstanzen-uc10`
**Prüfgegenstand**: Commit `60ddd58`, im Gesamtbild `git diff master..HEAD -- docs/ requirements/ diagrams/`
**Maßstab**: `CLAUDE.md` (4-Teil-Check), `reviews/uc10-wp4-doku-review-1.md` (M1–M9, N1–N10), `reviews/uc10-wp4-doku-review-2.md` (M10, M11, H1–H11)

---

## VERDICT: APPROVED
## ATTEMPT: 3 von 3

**M10 behoben. M11 behoben.** Beide Korrektionen treffen genau die Stelle, die sie treffen sollten, und keine von beiden hat etwas Neues gebrochen — das ist der Punkt, an dem die letzten beiden Versuche gescheitert sind, und diesmal hält er. Der Commit umfasst exakt drei geänderte Zeilen (`1/1` im Diagramm, `2/1` in der HTML), nichts sonst wurde angefasst.

---

## Verifikation (selbst ausgeführt, nichts übernommen)

```
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 383, übersprungen: 0, gesamt: 383, Dauer: 803 ms
git status --porcelain                                          → leer (Review hat keine Datei verändert)
git diff --check master..HEAD -- docs/ requirements/ diagrams/  → leer
git show --numstat 60ddd58 → drawio 1/1, html 2/1 — genau die drei angekündigten Zeilen
```

---

## M10 — **BEHOBEN**

**Fundstelle**: `diagrams/sequence-uc10-produktinstanzen.drawio:14`

```
value="Blazor UI&#10;(/lagerbestand · /neu · /{id}/bearbeiten)"
```

Deine erste Frage — *bekommt jetzt noch irgendeine Seite einen Aufruf zugeschrieben, den sie nicht macht?* — **Nein.** Die Lifeline benennt keine Komponente mehr, nur noch Routen. Gegenprobe über alle Service-Aufrufe der drei beteiligten Seiten (`grep "Service\.[A-Za-z]*Async"`):

| Szenario | erste Nachricht von `actor-ui` | Codestelle | Route |
|---|---|---|---|
| 1 „Produktinstanz erstellen" (`msg1:72`) | `CreateAsync(...)` | `ProduktInstanzForm.razor:160` | `/lagerbestand/neu` ✓ gelistet |
| 2 „Bestand nach Lagerort filtern" (`msg9:141`) | `GetByLagerortAsync("Kühlschrank")` | `LagerbestandBearbeiten.razor:200` | `/lagerbestand` ✓ gelistet |
| 3 „Verbrauch tracken" (`msg17:210`, `msg19`) | `UpdateAsync(...)`, `GetByIdAsync(42)` | `ProduktInstanzForm.razor:168` bzw. `:119` | `/lagerbestand/{Id:int}/bearbeiten` ✓ gelistet |

Zweite Frage — *deckt die Routenliste die drei Szenarien ab?* — **Ja, und zwar in beide Richtungen**: Jede gelistete Route setzt mindestens eine Nachricht des Diagramms ab, und jede Nachricht, die von `actor-ui` ausgeht, stammt aus einer der drei gelisteten Routen. Keine überzählige, keine fehlende. `LagerbestandBearbeiten.razor` ruft von den schreibenden Methoden weiterhin ausschließlich `DeleteAsync` (`:266`) — die Zuschreibung, die M10 ausgelöst hat, ist damit vollständig aufgelöst.

Dass du die Zwischenfassung mit nur zwei Routen und der `UpdateAsync`-auf-`/neu`-Begründung abgefangen hast, war richtig: `ProduktInstanzForm.razor` trägt beide `@page`-Direktiven (`:1` und `:2`), und der Update-Pfad läuft über die zweite. Die jetzige Fassung bildet das korrekt ab.

`ProduktInstanzPage` kommt in `src/`, `docs/`, `diagrams/`, `requirements/` **nicht** mehr vor.

## M11 — **BEHOBEN**

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:462` und `:497`

Deine Frage — *benennt jede Zahl eindeutig eine Menge, und passen alle zueinander?* — **Ja.** Die Überschrift sagt jetzt selbst, welche Menge sie zählt, und liefert die Brücke zur anderen Siebzehn gleich mit. Unabhängig gegenzählt:

| Angabe | Gegenzählung gegen die Dateien | Urteil |
|---|---|---|
| „22 gelistet" (Z. 462) | 5 UC2-Namen (Z. 466-470) + 12 WP4-Namen (Z. 474-485) + 5 LebensmittelDetail-Namen (Z. 490-494) = **22** aufgezählte bUnit-Namen | ✓ |
| „17 in LagerbestandBearbeitenTests" | Datei HEAD: **17** Testmethoden | ✓ |
| „12 neu in WP4 + 5 bestehend aus UC2" | `master`: **5** Methoden, HEAD: **17** → Mengendifferenz genau die 12 gelisteten Namen | ✓ |
| „5 neu in LebensmittelDetailTests" | `master` 15 → HEAD 20; die 5 Differenznamen sind genau die 5 gelisteten | ✓ |
| `<h4>Unit-Tests (5 Fälle)</h4>` (Z. 497) | `MhdTextFormatterTests.cs`: 1 `[Theory]` × 5 `[InlineData]` (-5/-1/0/1/5) — steht jetzt **außerhalb** der bUnit-Überschrift | ✓ |
| „17 neue UI (bUnit)" (Z. 21), „17 bUnit" (Z. 505, 610), Overview `:340` | 12 + 5 = **17 neu**; die Überschrift Z. 462 rechnet dieselbe 17 vor (12 neu + 5 neu) und trennt sie sichtbar von den 17 der Datei | ✓ |
| „= 54" (Z. 505), „54/54" (Z. 610, 853) | 32 + 17 + 5 = **54** | ✓ |

Die beiden Siebzehnen existieren weiterhin — aber sie sind nicht mehr ununterscheidbar: die eine ist qualifiziert als „17 **in LagerbestandBearbeitenTests**" mit ausgeschriebener Zerlegung, die andere als „17 **neue** UI (bUnit)". Genau das war die Forderung. Die vier korrekten Zahlwerte (Z. 21, 505, 610, 853) sind laut Diff unangetastet — ich habe das nicht der Commit-Message geglaubt, sondern `git show --numstat` gegengehalten: in der HTML wurde **eine** Zeile ersetzt und **eine** eingefügt.

---

## Hat diese Korrektur etwas Neues gebrochen? — Nein (das war die eigentliche Prüffrage)

Zweimal hintereinander hat eine Nachbesserung an genau diesen zwei Stellen einen neuen Defekt erzeugt. Diesmal habe ich deshalb nicht nur die Aussage, sondern das Umfeld jeder geänderten Zeile geprüft:

| Prüfung | Ergebnis |
|---|---|
| Umfang des Commits | ✓ genau 3 Zeilen in 2 Dateien, keine dritte Datei, keine Umformatierung |
| XML-Gültigkeit beider Diagramme | ✓ parsen sauber, **39** bzw. **34** `mxCell` — identisch mit `master`, keine Zelle verloren, keine doppelte `id` |
| Zeichenkodierung der neuen Beschriftung | ✓ `·` ist U+00B7 als gültiges UTF-8, kein Mojibake (`Â·`: 0 Treffer), kein BOM, `&#10;`-Zeilenumbruch erhalten |
| Nachbarzellen im Diagramm | ✓ `msg16` (die M8-Korrektur) unverändert, kein 🟢/🟡 in der Datei, Geometrie `120×50` nicht angefasst |
| HTML-Struktur nach dem Einschub | ✓ Parser meldet **kein** offenes Tag, **kein** Mismatch; die neue `h4` sitzt korrekt als drittes Kind unter `h3 Test-Coverage (WP4)`, ohne einen Block aus seiner Ebene zu reißen |
| Kollateralschäden an Zahlen | ✓ `32/32`: 0 Treffer, `2026-06-19`: 0 Treffer, Backticks: 0 — die Ergebnisse aus Versuch 2 halten |
| Zeilenenden | ✓ keine Drift: Blob vor **und** nach dem Commit reines LF (857 → 858 Zeilen), Diagramm 272 → 272; Arbeitskopie identisch |
| `architecture-overview.html`, `use-cases.drawio` | ✓ von diesem Commit nicht berührt (letzte Änderung `1de59db` bzw. `e2f4a97`) — die Freigaben aus Versuch 1 und 2 gelten unverändert |
| Testsuite | ✓ 383/383 grün, selbst ausgeführt |

**Korrektur an meiner eigenen Angabe, zum zweiten und letzten Mal**: Ich habe die Zeilenenden in Versuch 1 als „LF" und in Versuch 2 als „CRLF" bezeichnet. Maßgeblich ist der Blob, und der ist in allen betroffenen Dateien durchgehend LF — vor wie nach jedem der drei Commits. Die CRLF-Angabe in Versuch 2 war ein Artefakt meiner Abfrage, nicht der Datei. Die Schlussfolgerung („keine Drift") war beide Male richtig und ist es auch jetzt.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Doku (XML-Doc, WHY-Kommentare) | **BESTANDEN** (unverändert seit Code-Review 2) |
| 2 | `UC10-Produktinstanzen.html` — UI-Sektion Inhalt | **BESTANDEN** (M1, M2, M3, M5) |
| 2 | — Test-Coverage | **BESTANDEN** (M4, M11) |
| 2 | — Status-Konsistenz | **BESTANDEN** (M6) |
| 2 | — Workflows | **BESTANDEN** (M7) |
| 2 | — Domain Model / Service-Interface | **nicht Gegenstand dieses Gates**, fehlerhaft → H7, Übertrag |
| 3 | `architecture-overview.html` | **BESTANDEN** |
| 4 | `requirements/use-cases.drawio` | **BESTANDEN** (M9) |
| 4 | `diagrams/sequence-uc10-produktinstanzen.drawio` | **BESTANDEN** (M8, M10) |
| 4 | `diagrams/database-schema.drawio` | **BESTANDEN** (zu Recht nicht angefasst) |

Der Konsistenz-Check aus `CLAUDE.md` ist damit in allen vier Teilen erfüllt: Es gibt keine Stelle mehr, an der der Code „fertig" sagt und die Doku „todo" — und keine, an der die Doku einen Codepfad beschreibt, den es nicht gibt.

---

## Zu H1–H11: **bleibt draußen, keiner wird nachträglich blockierend**

Du hast gefragt, ob das so bleiben kann. Ja, und zwar aus einem Grund, der nicht Bequemlichkeit ist: Keiner der elf Punkte ist durch diesen Commit entstanden oder schlechter geworden, und keiner behauptet etwas Falsches über einen Codepfad in der Schärfe von M1/M8/M10. Sie zerfallen in drei Klassen:

- **Begründungsschwäche bei richtiger Aussage** (H1, H2, H3, H6): Die beschriebene Mechanik stimmt, die mitgelieferte Erklärung trägt sie nur nicht. Ein Leser wird nicht fehlgeleitet, er bekommt bloß weniger, als er könnte.
- **Unvollständigkeit** (H4 zwei fehlende Spalten, H5 „vier" statt „drei" ungenutzte Methoden): auslassend, nicht falsch — H5 ist zudem aus dem Feature-Plan übernommen, die Wurzel liegt dort (`.agents/plans/uc10-mhd-sichten.md:14`).
- **Kosmetik und Prozess** (H8 Boxgeometrie, H9 Tippfehler, H10 Prozesslob, H11 Plan-Checkliste).

Ein Sonderfall ist **H8**: Ich hatte die Überlaufgefahr an einer 44-Zeichen-Beschriftung festgemacht. Die neue ist mit 41 Zeichen kürzer als die, die ich beanstandet hatte — der Punkt ist durch die M10-Korrektur eher besser als schlechter geworden und bleibt Kosmetik.

**H7** bleibt wie vorgeschlagen draußen und geht nach `CONTEXT_SUMMARY.md`. Dass du den letzten Versuch nicht damit belastet hast, war die richtige Entscheidung: Es sind drei vorbestehende Falschaussagen über das Datenmodell, die ich in Versuch 1 selbst durchgewinkt habe; sie in Versuch 3 zur Freigabebedingung zu machen, wäre ein verschobenes Ziel gewesen. Im Übertrag gehören sie aber **nach oben**, über N6 und N9 — es ist die Domain-Model-Sektion des zentralen Use-Cases, und sie beschreibt eine Entity, die es so nicht gibt.

### Ein neuer Hinweis, ausdrücklich kein Mangel

- **H12 · `sequence-uc10-produktinstanzen.drawio:14`**: `/neu` und `/{id}/bearbeiten` sind Kurzformen; die vollen Routen lauten `/lagerbestand/neu` und `/lagerbestand/{Id:int}/bearbeiten`. Gelesen als eigenständige Pfade existieren sie nicht — aber sie stehen hinter `/lagerbestand` in derselben Liste, die Lesart als Suffixe ist die naheliegende, und mit keiner **anderen** Route der App kollidieren sie (`/lagerorte/neu` und `/lebensmittel/neu` sind die einzigen weiteren `neu`-Routen, beide unter fremdem Präfix). Der Parameter heißt im Code `{Id:int}`, nicht `{id}`. Wer die Datei ohnehin einmal anfasst: `…/neu · …/{Id:int}/bearbeiten` wäre die präzisere Form. **Das ist kein Grund, eine vierte Runde zu drehen** — es macht keine Aussage über den Code falsch, und dafür noch einmal an dieser Zeile zu schrauben, hat in diesem Gate zweimal mehr kaputtgemacht als repariert.

---

## Fazit

Dreizehn Befunde über drei Runden, alle abgearbeitet — die letzten beiden in drei Zeilen, ohne Kollateralschaden. Die Filter-Sektion stimmt Zweig für Zweig mit `LagerbestandBearbeiten.razor:173-211`, die Feldtabellen erfassen beide Seiten vollständig, die 22 bUnit- und 29 Service-Testnamen decken sich als Menge mit den Dateien, die Sequenz-Lifeline schreibt keiner Seite mehr einen fremden Aufruf zu, und die Testzahlen benennen jede für sich eine identifizierbare Menge. Die Diagramme sind minimal-invasiv geändert, parsen sauber und haben keine Zelle verloren.

Der Grund, warum es drei Anläufe gebraucht hat, liegt nicht beim `doc-agent`: Beide Befunde aus Versuch 2 waren Folgeschäden von Korrektionen, die ich selbst verlangt hatte, und bei M11 war der Auftrag bis zum dritten Mal nicht eindeutig — es wurde nie gesagt, welche Menge die Überschrift benennen soll. Diesmal war es wörtlich vorgegeben, und es saß auf Anhieb. Die Lehre für das nächste Doku-Gate: Wenn ein Befund eine Ersatzformulierung verlangt, muss die Ersatzformulierung die Bezugsmenge mitnennen, nicht nur die Zahl.

**Freigabe erteilt.** Die Checkliste im Feature-Plan kann bei „Dokumentation freigegeben (review-agent, Modus `docs`)" abgehakt werden.

**Offene Überträge in `CONTEXT_SUMMARY.md`** (nach Dringlichkeit, keiner blockiert die PR):
1. **H7** — drei Falschaussagen in der Domain-Model-/Service-Interface-Sektion von `UC10-Produktinstanzen.html` (Z. 63-84 erfundene und fehlende Felder, `Lagerort` als FK statt String; Z. 146-156 ein `IProduktInstanzService`, das es nicht gibt und das der korrekten Liste auf Z. 690-716 derselben Seite widerspricht; Z. 613/684 „151 Zeilen" statt 224).
2. **N6** (Etikett „Service-Layer: 271" in `architecture-overview.html`), **N9** (UC3 zweideutig in `use-cases.drawio`).
3. **H1–H6, H9, H12** — Präzisierungen beim nächsten Anfassen der jeweiligen Datei.
4. **H11** — die Fortschritts-Checkliste in `.agents/plans/uc10-mhd-sichten.md` ist bis auf die erste Zeile unabgehakt, obwohl alle drei Gates durch sind. Das ist der Resume-Punkt; vor dem Löschen des Plans im Abschluss-Schritt ist es egal, für eine dazwischenfallende neue Sitzung nicht.

---

**Reviewer**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 3 von 3
**Urteil**: **APPROVED**