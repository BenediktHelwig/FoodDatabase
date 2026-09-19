# UC10 (WP4) Dokumentation Review – Versuch 2/3

**Datum**: 19.09.2026
**Phase**: Doku-Review WP4 UC10
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `feat/ui-produktinstanzen-uc10`
**Prüfgegenstand**: Commit `e2f4a97`, im Gesamtbild `git diff master..HEAD -- docs/ requirements/ diagrams/`
**Maßstab**: `CLAUDE.md` (4-Teil-Check), `reviews/uc10-wp4-doku-review-1.md` (M1–M9, N1–N10), `docs/features/UC6-VerbrauchAusbuchen.html:143-154` und `:243-280`, `.agents/plans/uc10-mhd-sichten.md`

---

## VERDICT: CHANGES REQUESTED
## ATTEMPT: 2 von 3

**M1–M9 sind sachlich behoben**, ebenso N1–N5, N7 (teilweise) und N8. Die Arbeit ist deutlich besser als in Versuch 1: die Filter-Logik stimmt jetzt Zeile für Zeile mit dem Code überein, die Testnamen sind gegen die Dateien belegbar, der UC6-Maßstab ist erfüllt. Was blockiert, sind **zwei Befunde, die diese Nachbesserung selbst erzeugt hat** — beide in genau den zwei Punkten, die zweimal Thema waren: die Diagramm-Lifeline (M8) und eine Testzahl (M4/N7). Beide sind Ein-Zeilen-Korrektionen.

---

## Verifikation (selbst ausgeführt, nichts übernommen)

```
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 383, übersprungen: 0, gesamt: 383, Dauer: 895 ms
git status --porcelain                                  → leer (Review hat keine Datei verändert)
git diff --check master..HEAD -- docs/ requirements/ diagrams/  → leer
```

**Testzahlen zum dritten Mal gegengezählt, unabhängig:**

| Angabe auf der Seite | Gegenzählung | Urteil |
|---|---|---|
| 32 Service (Z. 21, 504, 609) | `ProduktInstanzServiceTests.cs`: 28 `[Fact]` + 1 `[Theory]` × 4 `[InlineData]` = **32**; 29 benannte Methoden = T1–T29 | ✓ |
| T1–T29 namentlich | Mengenvergleich Doku ↔ Datei: **0** Namen nur im Code, **0** nur in der Doku | ✓ |
| 12 neue + 5 bestehende in `LagerbestandBearbeitenTests.cs` (Z. 463) | `master`: genau die 5 genannten UC2-Tests; HEAD: 17 Methoden → 12 neu. Alle 17 Namen zeichengleich | ✓ |
| 5 neue in `LebensmittelDetailTests.cs` (Z. 488) | `master` 15 → HEAD 20; die 5 `+`-Namen im Diff sind genau die 5 gelisteten | ✓ |
| 5 Unit (`MhdTextFormatterTests.cs`, Z. 497-499) | 1 `[Theory]` × 5 `[InlineData]` (-5/-1/0/1/5), Name und Fallbeschreibung korrekt | ✓ |
| 17 neue bUnit / 54 gesamt (Z. 21, 504, 609, 852) | 12 + 5 = 17; 32 + 17 + 5 = 54; deckt sich mit `architecture-overview.html:340` | ✓ |
| **„17 Tests insgesamt" (Z. 462)** | unter dieser Überschrift stehen **22** bUnit-Namen **plus** der Unit-Test-Block | ✗ **siehe M11** |

**Validität**: `UC10-Produktinstanzen.html` (857 Z.) mit HTML-Parser gegengeprüft — **kein** offenes Tag, **kein** Mismatch, **keine** verschluckte spitze Klammer. Backticks: **0**. `2026-06-19`: **0** Treffer. `32/32`: **0** Treffer. Beide Diagramme parsen als XML (40 bzw. 35 Zellen), **keine** doppelten `id`. `ProduktInstanzPage` kommt in `src/`, `docs/`, `diagrams/`, `requirements/` **nicht mehr** vor (nur noch in historischen `reviews/`-Dateien und in der Commit-Message — korrekt).

**Zeilenenden / Formatdrift**: Alle drei Dateien sind im Blob **CRLF, vor und nach dem Commit** (HTML 555/555 → 857/857 CR-Zeilen; alle 327 hinzugefügten Zeilen CRLF; beide Diagramme unverändert). Keine Drift. — *Korrektur meiner eigenen Angabe aus Versuch 1*: Ich hatte die HTML-Datei dort als „durchgehend LF" bezeichnet; das war die durch `core.autocrlf` konvertierte Sicht und falsch benannt. Die Schlussfolgerung (keine Drift) hielt und hält.

**Gegen den Code formuliert, nicht gegen die Commit-Message**: `LagerbestandBearbeiten.razor` (275 Z.), `LebensmittelDetail.razor`, `ProduktInstanzForm.razor`, `MhdTextFormatter.cs`, `ProduktInstanzService.cs` (224 Z.), `IProduktInstanzService.cs`, `ProduktInstanz.cs`, `LagerortKonstanten.cs`, `LagerortService.cs:75`, alle drei Testdateien, `find src/App/Components/Pages -name "*.razor"` + alle `@page`-Routen.

---

## Alte Befunde — einzeln

| Befund | Status | Beleg |
|---|---|---|
| **M1** Filter-Ablauf | **behoben** | Z. 276-289 nennt alle drei Zweige mit dem tatsächlich gerufenen Service, in der Reihenfolge des Codes (`:173-211`): expired → `GetVerfallenenAsync` → `< Today` → Ordinal → `OrderBy`; „Alle"+Lagerort → `GetByLagerortAsync` **ohne** In-Memory-Filter → `OrderBy`; Vorgabe → `GetNachVerfallsdatumSortiertAsync` ohne Nachbearbeitung. Kein Widerspruch mehr zu `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf`. Der Workflow Z. 297-315 sagt dasselbe. Drei sprachliche Reste → H1–H3 |
| **M2** Datumsgrenze | **behoben** | Z. 291-294: Überschrift sagt jetzt „ist … unterschiedlich", Service `≤`, UI `<`, WHY-Satz erhalten. Deckt sich mit `ProduktInstanzService.cs:146`, `LagerbestandBearbeiten.razor:76/181`, `LebensmittelDetail.razor:230` |
| **M3** Ordinal-Semantik | **behoben** | Z. 458: „case-sensitiv (StringComparison.Ordinal in der UI-Filterung; `==`-Operator im Service)" + nutzersichtbare Folge. Beides gegen `:188` und `ProduktInstanzService.cs:117` geprüft. Der bewachende Test steht als eigener Eintrag Z. 484 |
| **M4** Test-Coverage-Liste | **behoben** | Alle 22 bUnit-Namen + der Unit-Test namentlich, alle drei Dateipfade genannt und korrekt. Namen gegen die Dateien geprüft, keine Abweichung. Einzig die Überschrift darüber zählt falsch → **M11** |
| **M5** UC6-Maßstab | **behoben** | Element/Typ/Zweck-Tabelle für **beide** Seiten (Z. 226-272, 349-390) — gegen `:150-157` bzw. `:270-277` abgeglichen, **alle** Felder erfasst, Typen korrekt; nummerierte Nutzer-Workflows (Z. 298-315, 401-408); Edge-Case-Listen (Z. 317-328, 423-427) inkl. Fehlerzustand von `/lagerbestand` und dem Punkt, dass die Filterleiste im Fehlerfall sichtbar bleibt (`:19` vs. `:49` ✓). Damit hält die Seite dem Maßstab `UC6:143-154` **und** `:243-280` stand |
| **M6** Stand 19.06. | **behoben** | Alle vier Stellen (Z. 504, 609, 633, 852) gezogen, Doc Version 2.1, Datum 2026-09-19; Gegenprobe auf `32/32` und `2026-06-19` leer |
| **M7** Workflow 2 | **behoben** | Z. 115-129 beschreibt jetzt die gebaute Seite (zwei Statuswerte, vier Lagerorte, rote Zeile, MHD-Text) und kennzeichnet Gruppierung/Ampel/Kategorie-Filter ausdrücklich als UC7-Zielbild. Eine Spalte fehlt in der Aufzählung → H4 |
| **M8** Sequenz-Diagramm | **teilweise behoben → M10** | Variante (b) ist umgesetzt: `msg16` nennt die zwei echten Zustände ✓, kein 🟢/🟡 mehr in der Datei ✓, die Feature-Seite trägt den Ebenen-Vermerk (Z. 662) ✓. **Aber**: der neue Lifeline-Name erzeugt zwei neue Falschaussagen → **M10** |
| **M9** Legende | **behoben** | `use-cases.drawio:112`: `(UC3)` ist aus der weißen Kategorie raus, der `uc3`-Knoten wurde korrekt **nicht** angefasst, der UC10-Eintrag ist unverändert richtig |

### N-Befunde aus Versuch 1

- **N1 Singularformen** — **behoben** (Z. 334, 336; deckt sich mit `MhdTextFormatter.cs:17/25`).
- **N2 dritter Leerzustand** — **behoben** (Z. 425 „Keine Packungen vorhanden." zeichengleich mit `:257`). Dass Fehler und Leermeldung einander ausschließen (`else if (string.IsNullOrEmpty(fehlerPackungen))`), steht weiterhin nicht da — bewusst als Rest hingenommen, kein Befund.
- **N3 Beschriftung** — **behoben** (alle fünf Fundstellen „Nur abgelaufene").
- **N4 Kausalität / Terminzusage** — **behoben** (Z. 457: „Grund 1 … Grund 2 …" sauber getrennt; „nicht terminiertes Vorhaben").
- **N5 Gegenrichtung der Doppelung** — **behoben, und diesmal mit der richtigen Begründung** (Z. 457: Umlaute gegen `^[A-Za-z]+$`; gegen `LagerortService.cs:75` geprüft — die einzige Regex dieser Art im Produktivcode). Die selbstwiderlegende Erstfassung ist weg.
- **N7 Zweideutigkeit „17"** — **halb behoben**: Kopf (Z. 21) und Overview (`:340`) sagen jetzt eindeutig „17 neue"; Z. 462 führt die Zweideutigkeit neu ein → **M11**.
- **N8 Platzhalter T30–T32** — **behoben** (Z. 564-568: T26–T29 plus der Hinweis, dass T26 als vier zählt; 29 Namen + 3 = 32 ✓).
- **N6 (Etikett „Service-Layer: 271") und N9 (UC3 zweideutig)** — **ich trage mit, dass beide offen bleiben**. Beide sind älter als dieser Branch, keiner von beiden ist durch diesen Commit schlechter geworden, und N9 ist die Wurzel von M9, nicht dessen Rest. `CONTEXT_SUMMARY.md` ist der richtige Ort.

---

## NEUE MÄNGEL

### M10 — MEDIUM · Die Lifeline heißt jetzt konkret — und ist damit in zwei von drei Szenarien falsch

**Fundstelle**: `diagrams/sequence-uc10-produktinstanzen.drawio:14`

```
value="Blazor UI&#10;(LagerbestandBearbeiten.razor, /lagerbestand)"
```

Die Datei hat **eine einzige** UI-Lifeline (`actor-ui`, `line-ui:38`), die alle drei Szenarien bedient:

| Szenario | erste Nachricht von `actor-ui` | wer ruft das im Code wirklich? |
|---|---|---|
| 1 „Produktinstanz erstellen" (`label-create:67`) | `msg1:72` **`CreateAsync(...)`** | `ProduktInstanzForm.razor:160` — Routen `/lagerbestand/neu` und `/lagerbestand/{Id:int}/bearbeiten` |
| 2 „Bestand nach Lagerort filtern" (`:136`) | `msg9:141` `GetByLagerortAsync("Kühlschrank")` | `LagerbestandBearbeiten.razor:200` ✓ |
| 3 „Verbrauch tracken" (`:205`) | `msg17:210` **`UpdateAsync(...)`** | `ProduktInstanzForm.razor:168` |

Gegenprobe (`grep -rn "ProduktInstanzService\.\(CreateAsync\|UpdateAsync\|DeleteAsync\)" src/App --include=*.razor`): `LagerbestandBearbeiten.razor` ruft von den schreibenden Methoden **ausschließlich** `DeleteAsync` (`:266`). `CreateAsync` und `UpdateAsync` kommen dort nicht vor.

Vor dem Commit zeigte der Name auf eine Komponente, die es nicht gibt — vakuum-falsch. Jetzt zeigt er auf eine reale Komponente und behauptet über sie zwei Aufrufe, die sie nicht macht. Das ist dieselbe Defektsorte wie M8 und M1: Die Doku beschreibt einen Codepfad, den es nicht gibt. Dass es die Korrektur von M8 selbst erzeugt hat, macht es nicht kleiner — es ist die Stelle, an der jetzt zum zweiten Mal hingesehen wurde.

**Soll** (eine Attribut-Ersetzung, gezielte `Edit`, kein XML-Werkzeug, danach Parser-Gegenprobe):

```
value="Blazor UI&#10;(/lagerbestand · /lagerbestand/neu)"
```

oder ausgeschrieben `LagerbestandBearbeiten.razor (Szenario 2) / ProduktInstanzForm.razor (Szenario 1+3)`. Beides trägt; entscheidend ist nur, dass keine Seite mehr einen Aufruf zugeschrieben bekommt, den sie nicht macht. Wer die längere Form wählt, prüft die Box-Geometrie (`120×50`, `fontSize=10`) kurz gegen — siehe H8.

Wenn die Feature-Seite (Z. 662) ohnehin angefasst wird: ein Halbsatz, dass Szenario 1 und 3 zur Formularseite gehören, wäre der saubere Abschluss von Variante (b). Nicht verlangt.

### M11 — MEDIUM · Die Überschrift zählt 17, darunter stehen 22 bUnit-Tests und ein Unit-Test

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:462` — `<h4>UI-Tests (bUnit) — 17 Tests insgesamt</h4>`

Diese `h4` ist die einzige Überschrift der Sektion; ihr untergeordnet stehen **drei** Blöcke:

| Block | Zeile | Inhalt |
|---|---|---|
| `LagerbestandBearbeitenTests.cs` | 463-486 | 5 bestehende + 12 neue = **17 bUnit** |
| `LebensmittelDetailTests.cs` | 488-495 | **5 bUnit** (neu) |
| `MhdTextFormatterTests.cs` | 497-500 | **1 Unit-Test** (5 Fälle) — **kein bUnit-Test** |

Damit ist „17 Tests insgesamt" unter jeder Lesart falsch:

- Als Summe der Sektion gelesen: es sind **22** bUnit-Tests gelistet, nicht 17.
- Als „die 17 aus der ersten Datei" gelesen: dann bezeichnet „17" hier eine **andere Menge** als das „17 neue UI (bUnit)" im Kopf (Z. 21), in der h2 (Z. 504), in der Status-Tabelle (Z. 609) und in `architecture-overview.html:340` — dort sind es 12 aus der einen plus 5 aus der anderen Datei. Zwei verschiedene Siebzehnen auf derselben Seite.
- Zusätzlich steht der Unit-Test unter einer Überschrift „UI-Tests (bUnit)", obwohl die Seite ihn an vier anderen Stellen ausdrücklich getrennt zählt.

Das ist der Konsistenz-Check aus `CLAUDE.md` in Reinform, und es ist **derselbe Zähler, der schon zweimal korrigiert wurde** (N7 in Versuch 1; die 22-unter-17-Fassung, die der Orchestrator vor der Übergabe abgefangen hat). Die Trennung 12 + 5 innerhalb der ersten Datei ist richtig gemacht — nur die Klammer darüber ist nicht mitgezogen worden. **Das wertet ich nicht als Agenten-Versagen, sondern als Auftragsproblem**: Es wurde dreimal an derselben Zahl geschraubt, ohne dass einmal gesagt wurde, welche Menge die Überschrift benennen soll.

**Soll** (zwei Zeilen): Überschrift auf `UI-Tests (bUnit) — 22 gelistet: 17 in LagerbestandBearbeitenTests (12 neu + 5 bestehend aus UC2), 5 neu in LebensmittelDetailTests` (oder auf „17 neue bUnit-Tests", dann aber der 5er-Block aus der ersten Datei sichtbar ausgeklammert). Und für `MhdTextFormatterTests.cs` eine eigene `h4` „Unit-Tests (5 Fälle)", damit er nicht unter „bUnit" steht.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Doku (XML-Doc, WHY-Kommentare) | **BESTANDEN** (unverändert seit Code-Review 2) |
| 2 | `UC10-Produktinstanzen.html` — UI-Sektion Inhalt | **BESTANDEN** (M1, M2, M3, M5 behoben) |
| 2 | — Test-Coverage | **NICHT BESTANDEN** (M11; Inhalt korrekt, Überschrift falsch) |
| 2 | — Status-Konsistenz | **BESTANDEN** (M6) |
| 2 | — Workflows | **BESTANDEN** (M7) |
| 2 | — Domain Model / Service-Interface | **nicht Gegenstand dieses Gates**, aber fehlerhaft → H7, Übertrag |
| 3 | `architecture-overview.html` | **BESTANDEN** (unverändert seit Versuch 1) |
| 4 | `requirements/use-cases.drawio` | **BESTANDEN** (M9) |
| 4 | `diagrams/sequence-uc10-produktinstanzen.drawio` | **NICHT BESTANDEN** (M10) |
| 4 | `diagrams/database-schema.drawio` | **BESTANDEN** (zu Recht nicht angefasst) |

---

## HINWEISE — kein Freigabe-Hindernis

- **H1 · Z. 278** überschreibt Zweig 1 mit „(ohne Lagerort)" und führt drei Zeilen später den Lagerort-Nachfilter darin auf. Die Bedingung im Code ist allein `statusFilter == expired` (`:173`). Klammer streichen.
- **H2 · Z. 280**: „In-Memory, **weil nur dieser Pfad in-memory nachfiltert**" ist eine Tautologie. Der belegbare Grund, den ich in Versuch 1 genannt hatte, fehlt weiterhin: `GetVerfallenenAsync` nimmt **keinen** Lagerort-Parameter (`IProduktInstanzService.cs`), deshalb muss dieser Pfad im Speicher einengen, während „Alle"+Lagerort direkt `GetByLagerortAsync` rufen kann.
- **H3 · Z. 282**: „OrderBy — **Nachfilter erfordert Sortierung**" verknüpft falsch. Ein `Where` zerstört keine Ordnung; sortiert werden muss, weil `GetVerfallenenAsync` (`ProduktInstanzService.cs:143-147`) unsortiert liefert — genau die Begründung, die Zweig 2 (Z. 286) korrekt trägt.
- **H4 · Spaltenlisten unvollständig**: Z. 120 und Z. 313 nennen fünf Spalten; die Tabelle hat sieben `<th>` (`:60-66`) — „Einkauft" (`Einkaufsdatum`) und „Aktionen" fehlen. Die Packungstabelle (Z. 406, 412-417) ist dagegen vollständig.
- **H5 · Z. 208** „die **vier** ungenutzten Service-Methoden": Auf `master` war `GetNachVerfallsdatumSortiertAsync` bereits angeschlossen; ungenutzt waren drei — die vierte tatsächlich ungenutzte, `GetTagesBisVerfallAsync`, wird von WP4 bewusst **nicht** angeschlossen (N+1, Z. 331). Die Formulierung stammt aus `.agents/plans/uc10-mhd-sichten.md:14` und steht seit `1de59db` auf der Seite; die Gegenprobe des Plans (`:188`) listet selbst nur drei Methoden. Beim nächsten Anfassen auf „drei" ziehen.
- **H6 · Z. 668**: Die neue Kurzfassung von Szenario 3 („UpdateAsync → Validierung → Repository.UpdateAsync → DB UPDATE") lässt den `GetByIdAsync`-Lesezugriff weg, den sowohl der Code (`ProduktInstanzService.cs:177`) als auch das zusammengefasste Diagramm (`msg19`) haben — die alte Fassung hatte ihn. Kein Fehler, aber eine Verschlechterung im Detail.
- **H7 · Selbstkorrektur, und ein Übertrag in derselben Größenordnung wie N6/N9**: Ich habe in Versuch 1 „Domain Model / Service-Interface: BESTANDEN (unverändert, war korrekt)" geschrieben. Das war falsch, ich habe die Blöcke nicht gegen den Code gehalten. Tatsächlich:
  - Z. 70-84 nennt Felder, die `ProduktInstanz.cs` nicht hat (`MHD`, `LagerortId: int? (FK)`, `Barcode`, `KaufDatum`, `Notiz`) und lässt die vorhandenen aus (`Verfallsdatum`, `Einkaufsdatum`, `Lagerort: string`, `MindestbestandMenge`, `ErstelltAm`). Die Entity-Tabelle Z. 63-66 führt `Lagerort` als Foreign Key (0..1) — es ist ein String.
  - Z. 146-156 zeigt ein `IProduktInstanzService`, das es nicht gibt (`CreateProduktInstanzAsync`, `GetProduktInstanzByIdAsync`, `GetAllProduktInstanzenAsync`, `FindByBarcodeAsync`, `GetByLagerortAsync(int)`), und widerspricht damit der korrekten Liste auf derselben Seite (Z. 690-716).
  - Z. 613 und 684: „ProduktInstanzService.cs (151 Zeilen)" — die Datei hat 224.

  **Ich blockiere dafür nicht.** Alle drei Stellen sind unverändert aus `master` (die Hunks dieses Commits berühren sie nicht), älter als dieser Branch, und ich habe sie in Versuch 1 ausdrücklich durchgewinkt — in Versuch 2 daraus ein Hindernis zu machen, wäre ein verschobenes Ziel. Sie gehören zu N6/N9 in die `CONTEXT_SUMMARY.md`, und zwar mit höherer Priorität als jene beiden: Das sind drei falsche Aussagen über das Datenmodell auf der Seite des zentralen Use-Cases. Wenn du willst, dass sie in Versuch 3 mitlaufen, weil die Datei ohnehin offen ist, ist das vertretbar — dann bitte ausdrücklich als Zusatzauftrag formulieren, nicht als Mangel dieses Gates.
- **H8 · Kosmetik**: Die neue Lifeline-Beschriftung (44 Zeichen) sitzt in einer `120×50`-Box mit `fontSize=10`; bei `whiteSpace=wrap` läuft sie voraussichtlich über den Rahmen. Wer M10 anfasst, kann die Geometrie gleich mit anpassen.
- **H9 · Tippfehler und ein Satzabbruch**: Z. 293 „Filtertabbelle"; Z. 309 endet mitten im Satz („… auf die aus GetVerfallenenAsync"); Z. 429 „Hilfklasse"; Z. 225 Überschrift „Komponenten-Feld" im Singular über einer Acht-Zeilen-Tabelle.
- **H10 · Prozess, positiv**: Die zwei Befunde, die du vor der Übergabe selbst abgefangen hast, waren beide echte Fehler und beide richtig diagnostiziert — insbesondere die selbstwiderlegende Umlaut-Begründung, die ich in Versuch 1 nur als Lücke (N5) und nicht als Falle benannt hatte. Die Korrektur trägt jetzt (`LagerortService.cs:75` gegengeprüft). Dass die Testliste danach immer noch eine falsche Klammer hat (M11), spricht nicht gegen die Kontrolle, sondern dafür, den Auftrag beim dritten Anlauf präzise zu formulieren.
- **H11 · Plan-Checkliste**: In `.agents/plans/uc10-mhd-sichten.md` ist außer der ersten Zeile **keine** Fortschritts-Zeile abgehakt, obwohl Implementierung, Tests und beide Vorgänger-Gates durch sind. Das ist der Resume-Punkt für eine neue Sitzung — nicht mein Prüfgegenstand, aber es fällt beim Lesen auf.

---

## Diff-Hygiene

| Prüfung | Ergebnis |
|---|---|
| Diagramm-Diff auf die drei Zeilen begrenzt | ✓ `sequence-…drawio` genau 2 geänderte Zeilen (`actor-ui`, `msg16`), `use-cases.drawio` genau 1 (Legende). Keine Umformatierung, keine Geometrie-Änderung, kein Neuschreiben durch ein XML-Werkzeug |
| XML-Gültigkeit nach der Änderung | ✓ 40 bzw. 35 Zellen, keine doppelten `id`; `&#34;` in `msg16` korrekt escaped |
| Zeilenenden | ✓ CRLF vor und nach dem Commit, in allen drei Dateien |
| Whitespace-Fehler (`git diff --check`) | ✓ leer |
| Unbeabsichtigte Dateien im Commit | ✓ keine — nur die drei erwarteten |
| Arbeitsverzeichnis nach dem Review | ✓ `git status --porcelain` leer, ich habe nichts verändert |

---

## Fazit

Neun von neun Befunden sind inhaltlich abgearbeitet, und zwar nicht oberflächlich: Die Filter-Sektion stimmt jetzt Zweig für Zweig mit `LagerbestandBearbeiten.razor:173-211` überein, sie widerspricht keinem Test mehr, die Feldtabellen erfassen **alle** Komponentenfelder beider Seiten mit korrekten Typen, die 22 bUnit-Namen und die 29 Service-Testnamen habe ich als Mengen gegen die Dateien verglichen — keine einzige Abweichung. Der UC6-Maßstab ist in beiden geforderten Teilen erfüllt. Die Diagramme sind minimal-invasiv geändert und parsen sauber. Das ist deutlich mehr als ein Abarbeiten der Liste.

Blockiert wird an zwei Stellen, und beide liegen ausgerechnet in den Punkten, die schon zweimal Thema waren. **M10**: Die M8-Korrektur hat die erfundene Lifeline `ProduktInstanzPage` durch eine reale Komponente ersetzt, die zwei der drei Szenarien nachweislich nicht ausführt — `CreateAsync` und `UpdateAsync` liegen in `ProduktInstanzForm.razor`. Eine falsche Aussage über den Code, neu entstanden, eine Attribut-Ersetzung weit. **M11**: Die Testzahl ist zum dritten Mal nicht rund — die Trennung 12 + 5 ist richtig gemacht, die Überschrift darüber nicht mitgezogen, und „17" bezeichnet auf derselben Seite jetzt zwei verschiedene Mengen. Hier liegt das Problem beim Auftrag, nicht beim Agenten: Es wurde nie festgelegt, welche Menge diese Überschrift benennen soll. Für Versuch 3 bitte genau das vorgeben.

Nicht blockiert habe ich bei drei pre-existing Falschaussagen über das Datenmodell (H7), die ich in Versuch 1 selbst durchgewinkt habe. Sie sind gravierender als N6 und N9, aber sie sind älter als dieser Branch, und ein nachgeschobenes Hindernis in Versuch 2 wäre ein verschobenes Ziel. Sie gehören in die `CONTEXT_SUMMARY.md` — oder, wenn du es so willst, als ausdrücklich benannter Zusatzauftrag in Versuch 3.

**Nächster Schritt**: M10 und M11 an den `doc-agent`, Versuch 3/3 — und dem User sagen, dass es der letzte Versuch ist. Beide Befunde nennen Datei, Zeile und die konkrete Ersatzformulierung; zusammen sind sie unter zehn geänderten Zeilen. H1–H4, H6, H9 können mitlaufen, wenn die Dateien ohnehin offen sind, sind aber kein Teil des Gates. Für M10 gilt die Diagramm-Regel des Feature-Plans: gezielte `Edit`-Ersetzung, kein XML-Werkzeug, danach Parser-Gegenprobe.

Die Checkliste im Feature-Plan bleibt bei „Dokumentation freigegeben (review-agent, Modus `docs`)" **offen**.

---

**Reviewer**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 2 von 3
**Urteil**: **CHANGES REQUESTED**
