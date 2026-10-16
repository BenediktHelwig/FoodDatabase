# UC10 (WP4) Dokumentation Review – Versuch 1/3

**Datum**: 19.09.2026
**Phase**: Doku-Review WP4 UC10
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `feat/ui-produktinstanzen-uc10`
**Prüfgegenstand**: Commit `1de59db`, im Gesamtbild `git diff master..HEAD`
**Maßstab**: `CLAUDE.md` (4-Teil-Check), `.agents/plans/uc10-mhd-sichten.md` (Schritt 4), `docs/features/UC6-VerbrauchAusbuchen.html:143-154` und die Test-Coverage-Sektion derselben Seite (Z. 243-280), eigene Vormerkungen aus `reviews/uc10-wp4-code-review-2.md` (L5) und `reviews/uc10-wp4-test-review-2.md` (HINWEIS „Bringschuld beim docs-Gate")

---

## VERDICT: CHANGES REQUESTED
## ATTEMPT: 1 von 3

Ein HIGH, acht MEDIUM. Die Seite hat den UI-Teil zum ersten Mal überhaupt — das ist substanzielle Arbeit und die richtige Lücke. Aber sie beschreibt an einer Stelle einen Codepfad, den es nicht gibt, sie lässt die beiden Punkte offen, die ich mir aus den Vorreviews selbst vorgemerkt hatte (Ordinal-Semantik), sie erfüllt den UC6-Maßstab nur zur Hälfte, und vier Stellen derselben Datei sagen weiterhin „32 Tests, Stand 2026-06-19", während der Kopf „32 + 17 + 5" sagt. Genau das ist der Konsistenz-Check aus `CLAUDE.md`.

---

## Verifikation (selbst ausgeführt, nichts übernommen)

```
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 383, übersprungen: 0, gesamt: 383, Dauer: 772 ms
git status --porcelain                       → leer (Review hat keine Datei verändert)
git diff --check master..HEAD -- docs/ requirements/   → leer
```

**Testzahlen gegengezählt** — ich habe dir nicht geglaubt, und das Ergebnis deckt sich trotzdem:

| Angabe | Gegenzählung | Urteil |
|---|---|---|
| 32 Service | `ProduktInstanzServiceTests.cs`: 28 `[Fact]` + 1 `[Theory]` × 4 `[InlineData]` = **32** | ✓ |
| 17 neue bUnit | `LagerbestandBearbeitenTests.cs` master 5 → jetzt 17 (+12), `LebensmittelDetailTests.cs` master 15 → jetzt 20 (+5) = **17** | ✓ |
| 5 neue Unit | `MhdTextFormatterTests.cs`: 1 `[Theory]` × 5 `[InlineData]` = **5** | ✓ |
| Projektsumme 383 | `Ui` 107 + `Unit` 274 (256 Fact + 18 InlineData) + `Integration` 2 = **383** | ✓ |
| Aufschlüsselung 271 + 107 + 5 | arithmetisch korrekt; die „271 Service-Layer" enthalten allerdings 2 Integrationstests (siehe N6) | ✓ mit Anmerkung |

**Validität**: `UC10-Produktinstanzen.html` (667 Z.) und `architecture-overview.html` mit einem Parser gegengeprüft — **kein** offenes Tag, **kein** Mismatch, **keine** als Tag verschluckte spitze Klammer. `requirements/use-cases.drawio` (34 Zellen) und `diagrams/sequence-uc10-produktinstanzen.drawio` (39 Zellen) parsen als XML, keine doppelten `id`. Backticks in der HTML-Datei: **0**.

**Zeilenenden**: `UC10-Produktinstanzen.html` ist im Working Tree durchgehend LF — **und war es auf `master` schon**. Keine Formatdrift, die 120 neuen Zeilen fügen sich ein. `architecture-overview.html` und `use-cases.drawio` bleiben CRLF wie auf `master`.

**Was ich gegen den Code formuliert habe, nicht gegen die Commit-Message**: `LagerbestandBearbeiten.razor` (275 Z.), `LebensmittelDetail.razor` (Packungssektion Z. 203-259 und `:314-326`), `MhdTextFormatter.cs`, `ProduktInstanzService.cs:89-222`, `LagerortKonstanten.cs`, `NavMenu.razor`, das Verzeichnis `src/App/Components/Pages/`.

---

## MÄNGEL

### M1 — HIGH · Der dokumentierte Filter-Ablauf beschreibt einen Codepfad, den es nicht gibt

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:224-229`

```
1. Status-Filter: „Alle" → GetNachVerfallsdatumSortiertAsync()
                  „Nur abgelaufen" → GetVerfallenenAsync()
2. Nachfilter: Wenn Lagerort gewählt → In-memory Where() auf Service-Ergebnis
```

Der Code hat **drei** Zweige, nicht zwei (`LagerbestandBearbeiten.razor:173-211`):

| Zustand | Tatsächlicher Ablauf |
|---|---|
| Status = abgelaufen | `GetVerfallenenAsync()` → `Where(< Today)` → optional **In-memory** `string.Equals(…, Ordinal)` → `OrderBy` |
| Status = Alle **+ Lagerort gewählt** | **`GetByLagerortAsync(lagerort)`** — ein eigener Service-Aufruf, **kein** In-Memory-Nachfilter → `OrderBy` |
| Status = Alle, kein Lagerort | `GetNachVerfallsdatumSortiertAsync()`, bereits sortiert, kein Nachsortieren |

Die Doku behauptet für den zweiten Fall, die Seite lade alles und filtere im Speicher. Sie tut das Gegenteil, und das ist keine Feinheit: `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` (`LagerbestandBearbeitenTests.cs:263`, mit `Verify(…, Times.Once)`) prüft **genau diesen Service-Aufruf**. Die Doku widerspricht damit einem grünen Test — und die Sektion nennt `GetByLagerortAsync` zwei Absätze weiter oben selbst unter den Service-Abhängigkeiten, ohne dass der Ablauf sie je aufruft. Dieselbe Defektsorte, die bei UC9 als M2 HIGH lief: die Seite beschreibt eine Implementierung, die es nicht gibt.

**Soll**: Die drei Zweige einzeln aufführen, mit dem jeweils wirklich aufgerufenen Service. Dazu der Satz, warum nur der Expired-Pfad in-memory nachfiltert (weil `GetVerfallenenAsync` keinen Lagerort-Parameter hat) und warum zwei der drei Zweige nachsortieren müssen (weil `GetVerfallenenAsync` und `GetByLagerortAsync` unsortiert liefern — ein reines `Where`, siehe `ProduktInstanzService.cs:115-118` und `:143-147`, während nur `GetNachVerfallsdatumSortiertAsync` sortiert).

### M2 — MEDIUM · „Datumsgrenze überall gleich" — und darunter steht, dass sie es nicht ist

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:231-235`

```
⚠️ WICHTIG: Datumsgrenze überall gleich:
   - Service: GetVerfallenenAsync() liefert (Verfallsdatum ≤ heute)
   - UI: Nachfilter auf (Verfallsdatum < heute)
   - Färbung: Auch (Verfallsdatum < heute)
```

Die Überschrift behauptet Gleichheit, die Aufzählung zeigt den Unterschied. Beide Einzelangaben stimmen (`ProduktInstanzService.cs:146` `<= referenzdatum`; `LagerbestandBearbeiten.razor:76`, `:181`; `LebensmittelDetail.razor:230` jeweils `< DateTime.Today`) — der Rahmensatz macht daraus trotzdem eine falsche Aussage. Ein Leser, der nur die fette Zeile liest, nimmt das Gegenteil dessen mit, was der Abschnitt erklären will. Das ist die **nicht-offensichtliche Verhaltensweise Nr. 1** aus deinem Prüfschwerpunkt: Der Inhalt ist da, die Verpackung sagt das Falsche.

**Soll**: Überschrift auf die tatsächliche Aussage ziehen, z. B. „Die UI-Grenze ist enger als die Service-Grenze — und innerhalb der UI überall dieselbe:". Der Begründungssatz („Mindestens haltbar bis" bedeutet, dass die Packung am MHD-Tag noch gut ist) ist richtig und kann bleiben; er ist der WHY-Kommentar, den die Doku schuldet, und er deckt sich mit `LagerbestandBearbeiten.razor:178-179`.

### M3 — MEDIUM · Die Ordinal-Semantik fehlt — die zweite Bringschuld dieses Gates

**Fundstelle**: fehlt auf der ganzen Seite (`grep` über `Ordinal`, `case-sensitiv`, `Groß-/Klein` → 0 Treffer)

Ich hatte mir das in `reviews/uc10-wp4-test-review-2.md` (HINWEIS, letzter Punkt) selbst vorgemerkt: Der Lagerortvergleich ist in **beiden** Pfaden case-sensitiv — `string.Equals(…, StringComparison.Ordinal)` (`LagerbestandBearbeiten.razor:188`) und `p.Lagerort == lagerort` (`ProduktInstanzService.cs:117`). `ExpiredFilter_IsCaseSensitiveForLagerort` (`LagerbestandBearbeitenTests.cs:715-782`) hält diese Entscheidung fest und wird rot, sobald jemand sie zurücknimmt.

Die Folge ist nutzersichtbar: Ein Datensatz mit `"kühlschrank"` in der Spalte fällt aus dem Filter „Kühlschrank" heraus, ohne dass irgendetwas darauf hinweist. Steht das nirgends, weichen Test und Doku in genau dem Punkt voneinander ab, an dem zwei Code-Reviews gehangen haben (M4 aus Code-Review 1/2).

**Soll**: In den Abschnitt „⚠️ Wichtige Einschränkung: Lagerort-Doppelung" (Z. 283 ff.) einen Satz: Der Lagerortvergleich ist **ordinal und case-sensitiv**, in der UI wie im Service; abweichend geschriebene Bestandsdaten erscheinen nicht im Filter. Mit Verweis auf den bewachenden Test.

### M4 — MEDIUM · Die Test-Coverage-Sektion listet keinen einzigen Testfall

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:308-309` — zwei Zeilen, ein Satz:

> „Beide Razor-Seiten sind mit bUnit-Tests abgedeckt. Szenarien umfassen: Filterleiste-Verhalten, Lagerort-Selektionen, MHD-Text-Darstellung, Lade-/Fehler-/Leerzustände, Packungslisten-Rendering."

`CLAUDE.md`, Teil 2 des 4-Teil-Checks, verlangt wörtlich „Test-Coverage Sektion: **Alle Test-Cases gelistet**". Der Maßstab steht auf derselben Ebene im Repo: `UC6-VerbrauchAusbuchen.html:243-280` listet alle zehn UI-Tests einzeln, nennt die Testdatei mit Pfad und schließt mit einer Zusammenfassung. Dieselbe Seite listet zwei Sektionen höher die 32 Service-Tests als T1–T32 — die neuen 22 Tests werden also nach einem anderen, deutlich schwächeren Maßstab behandelt als die alten 32 auf derselben Seite.

Zusätzlich fehlen **alle drei Testdateipfade**: `src/Tests/Ui/LagerbestandBearbeitenTests.cs`, `src/Tests/Ui/LebensmittelDetailTests.cs`, `src/Tests/Unit/MhdTextFormatterTests.cs` kommen auf der Seite nicht vor (das ist der UC9-Befund M3 in klein).

**Soll**: Zwei Checklisten nach dem Vorbild `UC6:262-277` — „UI-Tests (bUnit)" mit den 12 neuen Tests aus `LagerbestandBearbeitenTests` und den 5 neuen aus `LebensmittelDetailTests`, „Unit-Tests" mit den 5 Fällen aus `MhdTextFormatterTests`. Je Block die Testdatei mit Pfad und Anzahl. Die Namen sind abzählbar vorhanden, nichts muss geschätzt werden.

### M5 — MEDIUM · Der UC6-Maßstab ist zur Hälfte erfüllt: keine Komponenten-Struktur, kein Nutzer-Workflow, keine Edge Cases

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:200-311` gegen `UC6-VerbrauchAusbuchen.html:143-154` ff.

| UC6-Bestandteil | UC10 |
|---|---|
| Route | ✓ beide Seiten, gegen `@page` geprüft |
| Beschreibung | ✓ |
| Service-Abhängigkeiten (DI) | ✓ vollständig, gegen `@inject` geprüft |
| **Komponenten-Struktur (Tabelle Element / Typ / Zweck)** | **fehlt** |
| **Workflow aus Nutzersicht (Happy Path)** | **fehlt** |
| **Fehlerfall als Ablauf** | nur für `LebensmittelDetail` als Fließtext; für `/lagerbestand` gar nicht |
| **Edge Cases** | **fehlt** |

Kein einziges Komponentenfeld ist dokumentiert: `bestaende`, `lebensmittelMap`, `statusFilter`, `lagerortFilter`, `fehler`, `lädt`, `bestätigungLöschen` (`LagerbestandBearbeiten.razor:150-157`) und `packungen`, `fehlerPackungen` (`LebensmittelDetail.razor:275`) kommen auf der Seite nicht vor. Die Commit-Message behauptet „component fields" seien dokumentiert — sie sind es nicht; das ist der einzige Punkt, an dem die Commit-Message mehr verspricht als die Datei hält. Bei UC9 war exakt dieser Zuschnitt der Befund M4, und er wurde dort mit der Element/Typ/Zweck-Tabelle geschlossen.

Ebenfalls nicht dokumentiert, obwohl es die Filterleiste erst bedienbar macht: der Fehlerzustand von `/lagerbestand` (`catch` → `alert-danger`, `:222-225`) und dass die Filterleiste im Fehlerfall sichtbar bleibt, der Nutzer also aus dem Filter wieder herauskommt (`:19` gegen `:49` — der Punkt, den M3 des Code-Reviews erzwungen hat).

**Soll**: Eine Element/Typ/Zweck-Tabelle je Seite und ein nummerierter Workflow „Filtern auf `/lagerbestand`" nach dem Vorbild `UC6:187-200`, plus eine kurze Edge-Case-Liste (beide Leerzustände, Fehlerzustand, Kombination beider Filter).

### M6 — MEDIUM · Vier Stellen derselben Datei stehen noch auf dem Stand vom 19.06.2026

**Fundstellen** in `docs/features/UC10-Produktinstanzen.html`:

| Zeile | Text | Widerspruch zu |
|---|---|---|
| 313 | `<h2>✅ Tests (32 Unit Tests - 100% PASSING ✅)</h2>` | Kopf Z. 21: „32 Service + 17 UI (bUnit) + 5 Unit Tests" |
| 420 | `<td>✅ 32/32 PASSING (100%)</td>` (Zeile **Tests** der Implementation-Status-Tabelle) | ebenda |
| 447 | `✅ docs/features/UC10-Produktinstanzen.html (aktualisiert 2026-06-19)` | die Datei wurde heute geändert |
| 662 | `Status: ✅ COMPLETE (32/32 Tests Passing) \| Doc Version: 2.0 \| Updated: 2026-06-19` | Kopf Z. 21 |

Der Fuß ist die Stelle, an der bei UC9 der Befund M1 saß, und er ist dieselbe Falle: Der Kopf wurde gepflegt, der Fuß nicht, und beide sprechen über dieselbe Sache. Die Zeile 313 ist verteidigbar, wenn man sie als Überschrift der reinen Service-Test-Sektion liest — dann sollte sie das aber sagen („32 Service-Unit-Tests"), sonst liest sich die Seite als „das Projekt hat 32 Tests für UC10", was seit heute falsch ist.

**Soll**: Alle vier Stellen auf den gezählten Stand ziehen (32 Service + 17 bUnit + 5 Unit = 54 für UC10), Datum auf 2026-09-19, `Doc Version` hochziehen. Danach die Gegenprobe, die bei UC9 gefordert war: `grep` über die Seite nach `32/32` und `2026-06-19` muss leer sein bzw. nur noch qualifizierte Treffer liefern.

### M7 — MEDIUM · „Workflow 2: Bestand überblicken" beschreibt eine Oberfläche, die es nicht gibt

**Fundstelle**: `docs/features/UC10-Produktinstanzen.html:115-124`

```
2. Sieht ALLE ProduktInstanzen gruppiert:
   - Nach LebensmittelKatalog
   - Mit MHD-Status (🟢 OK, 🟡 Bald verfallen, 🔴 Verfallen)
3. Kann filtern nach:
   - Lagerort
   - MHD-Status
   - Produkt-Kategorie
```

Gegen `LagerbestandBearbeiten.razor` gehalten: Die Tabelle ist **nicht gruppiert** (flache Zeilenliste, `:70-103`). Es gibt **keine dreistufige Ampel** — nur `table-danger` für abgelaufen plus die Textspalte (`:76-77`, `:87`); das gelbe „bald verfallen" ist `VerfallsdatumStatus` aus UC7 und laut Feature-Plan („Ausdrücklich nicht Teil von UC10") ausdrücklich **nicht** gebaut. Einen **Kategorie-Filter** gibt es nicht. Der Statusfilter hat genau zwei Werte.

Solange die Seite keinen UI-Teil hatte, war dieser Absatz eine Skizze. Jetzt steht er zwölf Bildschirmzeilen über einer Sektion, die dieselbe Seite korrekt beschreibt — die Datei widerspricht sich selbst, und der Leser hat keinen Anhaltspunkt, welche der beiden Beschreibungen die gebaute ist. Das ist der Konsistenz-Check aus `CLAUDE.md` in seiner Umkehrung: nicht „irgendwo steht noch todo", sondern „irgendwo steht mehr als fertig".

**Soll**: Workflow 2 auf das ziehen, was `/lagerbestand` tut (Statusfilter Alle/Nur abgelaufene, Lagerortfilter aus den vier Konstanten, Spalte „Tage bis MHD", rote Zeile bei abgelaufen, zwei Leerzustände) — oder den Absatz ausdrücklich als Zielbild für UC7 kennzeichnen. Beides geht; stehenlassen geht nicht.

### M8 — MEDIUM · Sequence-Diagramm: die Begründung trägt nicht, und zwar aus einem anderen Grund als bei UC9

**Fundstelle**: `diagrams/sequence-uc10-produktinstanzen.drawio:135-203` („SCENARIO 2: Bestand nach Lagerort filtern") sowie `docs/features/UC10-Produktinstanzen.html:469-479`

Du hast mir die Bewertung ausdrücklich vorgelegt, also im Detail: Die Begründung des `doc-agent` — Szenario 2 decke den Filter-Ablauf auf Service-Ebene ab — stimmt für die **Service**-Seite. `msg9`–`msg15` bilden `GetByLagerortAsync` sauber ab, und dass UI-Details in die HTML-Doku gehören, ist eine vertretbare Linie.

Nur hat Szenario 2 eine **UI-Lifeline**, und die macht zwei Aussagen über die Oberfläche, die seit heute nachweislich falsch sind:

1. `actor-ui` (Z. 14) heißt **„Blazor UI (ProduktInstanzPage)"**. Eine Komponente dieses Namens existiert nicht und hat nie existiert — `find src/App/Components/Pages -name "*.razor"` kennt zehn Dateien, keine davon heißt so. Die Seite, die den Lagerortfilter wirklich ausführt, heißt `LagerbestandBearbeiten.razor` und liegt unter `/lagerbestand`. Solange es gar keine UI gab, war das ein Platzhalter; jetzt zeigt das Diagramm mit Namen auf etwas Nichtexistentes, während das Reale danebenliegt.
2. `msg16` (Z. 199) lautet „8: Bestand anzeigen mit MHD-Status (🟢 OK, 🟡 Bald weg, 🔴 Verfallen)" — dieselbe Drei-Status-Ampel wie in M7, die die gebaute Seite nicht hat und die laut Plan UC7 gehört.

Das ist also **nicht** der UC9-Fall „das UI-Szenario fehlt", sondern der schärfere: Das UI-Szenario ist da und beschreibt die falsche UI. Teil 4 des 4-Teil-Checks („Sequence-Diagramme: neue Workflows, aktualisierte Beschreibungen") ist damit nicht erfüllt. Erschwerend: Die Diagramm-Sektion der Feature-Seite (Z. 469-479) referenziert Szenario 2 als „Nach Lagerort filtern", ohne zu sagen, dass die dortige UI-Ebene nicht die WP4-UI ist — die billige Variante ist also ebenfalls nicht eingelöst.

**Zwei gangbare Wege, beide tragen** (wie bei UC9 halte ich beide offen; die Wahl liegt beim User):

- **(a) Volle Variante, Maßstab `1758dbe`**: Ein „SZENARIO 4: MHD-Sichten über die WP4-UI" mit eigener Lifeline `LagerbestandBearbeiten.razor` — Auswahl „Nur abgelaufene" → `GetVerfallenenAsync()` → In-Memory-Einengung `< Today` → optionaler Ordinal-Nachfilter → `OrderBy` → Tabelle mit „Tage bis MHD"; dazu die Korrektur von `actor-ui` und `msg16`. Das ist der Weg, den der User bei UC9 gewählt hat.
- **(b) Billige Variante**: Kein neues Szenario. Stattdessen `actor-ui` auf `Blazor UI (LagerbestandBearbeiten.razor, /lagerbestand)` korrigieren, `msg16` auf die zwei tatsächlichen Zustände ziehen, und in der Diagramm-Sektion der Feature-Seite (Z. 469-479) ausdrücklich vermerken, dass das Sequenzdiagramm die Service-Ebene abbildet und die WP4-UI-Abläufe in der UI-Sektion dieser Seite stehen. Vorbild für die Formulierung: `UC6-VerbrauchAusbuchen.html:281-284`, wo der Diagramm-Link genau so eine Einordnung trägt.

**Was nicht geht**: die beiden Falschaussagen unangetastet lassen. Egal welcher Weg — `actor-ui` und `msg16` sind zu korrigieren.

**Diagramm-Regel beachten** (Feature-Plan Z. 153-154): gezielte `Edit`-Ersetzungen, kein Neuschreiben über XML-Werkzeuge, danach Parser-Gegenprobe. Die Datei parst aktuell sauber (39 Zellen, keine doppelten `id`) — dieser Zustand ist zu halten.

### M9 — MEDIUM · Die neue Legende in `use-cases.drawio` widerspricht dem Knoten, der zwei Bildschirme darüber liegt

**Fundstelle**: `requirements/use-cases.drawio:112` (Legende), gegen `:65` (`uc3`) derselben Datei und `docs/architecture-overview.html:141-144`

Neu steht dort: `🟡 Weiß = Service only, UI TODO (UC3)`. In derselben Datei trägt der Knoten `uc3` den Wert **„✅ Nährwerte verwalten (pro 100g/ml)"** und die Füllfarbe `#d4edda` — **grün**, nicht weiß. Die UC3-Karte der Architecture-Overview sagt „✅ Fertig (gemergt)". Die Legende erklärt also eine Farbkategorie, in der kein einziger Knoten liegt, und weist ihr ausgerechnet einen grünen zu.

Der UC10-Teil der Änderung ist richtig und war die Plan-Vorgabe (UC10 raus aus „Service only", rein in die grüne Aufzählung) — nachgeprüft: `uc10` ist grün mit ✅, die Angabe „UC10: 32 Service + WP4 UI" deckt sich mit meiner Zählung. Der Fehler ist nur der mitgezogene UC3-Vermerk.

**Soll**: `(UC3)` aus der gelb/weißen Kategorie streichen — die Kategorie ist dann leer und kann als Legendeneintrag bleiben oder entfallen. **Nicht** den `uc3`-Knoten umfärben; welcher UC3 hier gemeint ist, ist eine ältere Unklarheit und gehört nicht in diesen Commit (siehe N9).

---

## Was geprüft wurde und stimmt

Damit die Mängelliste nicht den Eindruck erweckt, die Sektion sei durchweg schief — ich habe jede Aussage der neuen UI-Sektion einzeln gegen den Code gehalten:

| Aussage | Beleg |
|---|---|
| Route `/lagerbestand` | `LagerbestandBearbeiten.razor:1` ✓ |
| Route `/lebensmittel/{Id:int}` | `LebensmittelDetail.razor:1` ✓ |
| `MhdTextFormatter.cs` in `src/App/Models/`, Namespace `FoodDatabase.App.Models`, `static string GetMhdText(int)` | Datei Z. 1, 6, 12 ✓ |
| DI `/lagerbestand`: die drei genannten Service-Methoden | `:4`, `:176`, `:200`, `:210` ✓ |
| DI `LebensmittelDetail`: `GetByLebensmittelAsync` neu injiziert | `:6`, `:317` ✓ |
| Filter-Dropdown aus `LagerortKonstanten.AlleWerte`, nicht hartcodiert | `:33` ✓ (Plan-Vorgabe eingelöst) |
| Leerzustände wörtlich „Kein Lagerbestand vorhanden." / „Keine Treffer für diesen Filter." | `:248`, `:250` — zeichengleich ✓ |
| **„Tage bis MHD" im UI gerechnet, nicht per `GetTagesBisVerfallAsync`, Begründung N+1** | `:80` bzw. `LebensmittelDetail.razor:234`; `GetTagesBisVerfallAsync` macht je Aufruf ein `GetByIdAsync` (`ProduktInstanzService.cs:216`) ✓ — **die dritte nicht-offensichtliche Verhaltensweise aus deinem Prüfschwerpunkt, korrekt und mit dem richtigen WHY dokumentiert** |
| Packungsliste getrennt geladen, Fehler verdeckt das Lebensmittel nicht | `LebensmittelDetail.razor:314-326`, eigenes Feld `fehlerPackungen` ✓ |
| Spalten der Packungstabelle, Sortierung, `dd.MM.yyyy`, Link `/lagerbestand/{id}/bearbeiten` | `:218-250`, `:319-321` ✓ — vollständig und in der richtigen Reihenfolge |
| Färbung `Verfallsdatum < DateTime.Today` | `:230` ✓ |
| Lagerort-Tabelle: vier Konstanten, UC9-Regex `^[A-Za-z]+$`, `ILagerortService` von `ProduktInstanz` ungenutzt | `LagerortKonstanten.cs`, `ProduktInstanzService.cs:112`, Grep über `src/` ✓ |
| Overview: „WP4 Weitere UI-Komponenten (UC4, UC5, UC7, UC8)" | gegengeprüft am Verzeichnis `src/App/Components/Pages/` — für diese vier existiert **keine** Razor-Seite; die Änderung von „(UC4, UC10)" auf diese vier ist **korrekt und eine echte Verbesserung** ✓ |
| Overview: UC10-Karte, Entity-Zeile `ProduktInstanz`, Abschlussliste, Gesamtsumme 383 | alle vier Stellen konsistent, Zahlen gegengezählt ✓ |

Die **Lagerort-Einschränkung** (deine Bringschuld aus Code-Review 2, L5) ist damit **im Kern erfüllt**: Die Doppelung ist benannt, tabellarisch gegenübergestellt, die Folge für den Nutzer ist ausgesprochen. Sie ist **nicht falsch dargestellt** — das war deine ausdrückliche Sorge, und sie hat sich nicht bestätigt. Was fehlt, ist die Ordinal-Aussage (M3) und eine saubere Kausalität (N4). Die Bringschuld ist also zu drei Vierteln eingelöst.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Doku (`MhdTextFormatter.cs` XML-Doc, WHY-Kommentare in beiden Razor-Seiten) | **BESTANDEN** — unverändert seit Code-Review 2, dort geprüft |
| 2 | `UC10-Produktinstanzen.html` — UI-Sektion Inhalt | **NICHT BESTANDEN** (M1, M2, M3, M5) |
| 2 | `UC10-Produktinstanzen.html` — Test-Coverage | **NICHT BESTANDEN** (M4) |
| 2 | `UC10-Produktinstanzen.html` — Status-Konsistenz | **NICHT BESTANDEN** (M6) |
| 2 | `UC10-Produktinstanzen.html` — Workflows | **NICHT BESTANDEN** (M7) |
| 2 | `UC10-Produktinstanzen.html` — Domain Model / Service-Interface / Validierungen | BESTANDEN (unverändert, war korrekt) |
| 3 | `architecture-overview.html` — UC-Karte, Entity-Status, Projekt-Status | **BESTANDEN** — alle drei geforderten Stellen gepflegt, Zahlen gegengezählt |
| 4 | `requirements/use-cases.drawio` | **NICHT BESTANDEN** (M9) — UC10-Teil richtig, UC3-Vermerk falsch |
| 4 | `diagrams/sequence-uc10-produktinstanzen.drawio` | **NICHT BESTANDEN** (M8) |
| 4 | `diagrams/database-schema.drawio` | **BESTANDEN** — zu Recht nicht angefasst, UC10 ändert kein Schema |

---

## HINWEISE — kein Freigabe-Hindernis, gehören aber nicht unter den Tisch

- **N1 · Singularformen fehlen.** Z. 248-250 nennt „N Tage verbleibend" / „vor N Tagen abgelaufen". `MhdTextFormatter.cs:17`, `:25` hat zwei Sonderfälle: „1 Tag verbleibend" und „vor 1 Tag abgelaufen" — beide von je einem `[InlineData]` gedeckt. Zwei Wörter.
- **N2 · Der dritte Leerzustand fehlt.** Die Leerzustands-Liste (Z. 238-242) gilt nur für `/lagerbestand`. `LebensmittelDetail.razor:254-258` hat einen eigenen: „Keine Packungen vorhanden." — und die Bedingung `else if (string.IsNullOrEmpty(fehlerPackungen))`, die verhindert, dass Fehler und Leermeldung gleichzeitig stehen (das war L2 aus Code-Review 1). Beides gehört in den `LebensmittelDetail`-Abschnitt.
- **N3 · Beschriftung uneinheitlich.** Z. 225 schreibt „Nur abgelaufen", Z. 214 und die UI selbst (`:26`) „Nur abgelaufene".
- **N4 · Kausalität in Z. 306 verrutscht.** „… kann es nicht im UC10-Filter verwenden (wird von `ProduktInstanzService.CreateAsync` abgelehnt)." Zwei wahre Tatsachen, falsch verknüpft: Der Filter zeigt den Wert nicht, **weil** das Dropdown aus `LagerortKonstanten.AlleWerte` gefüllt wird; dass `CreateAsync` ihn zusätzlich ablehnt, ist ein zweiter, eigener Riegel. Getrennt hinschreiben. — Ebenfalls in Z. 306: „wird in einer späteren Phase zusammengeführt" sagt eine Terminzusage zu, die es nicht gibt; der Plan führt die Zusammenführung unter „Ausdrücklich nicht Teil von UC10" als eigenes Vorhaben mit Service-Änderung, Migration und TDD-Pflicht. „Eine Zusammenführung ist nicht terminiert" wäre die belegbare Formulierung.
- **N5 · Die Doppelung ist nur in eine Richtung beschrieben.** Die Seite sagt, dass ein UC9-Lagerort in UC10 nicht ankommt. Die Gegenrichtung fehlt: „Kühlschrank" und „Tiefkühler" lassen sich unter `/lagerorte` **gar nicht anlegen**, weil der Umlaut gegen `^[A-Za-z]+$` verstößt. Die Commit-Message sagt das, die Seite nicht — und es ist der Satz, der die Disjunktheit erst vollständig macht.
- **N6 · „Service-Layer: 271 Tests"** (`architecture-overview.html:341`) enthält die 2 Integrationstests aus `src/Tests/Integration`. Die Summe stimmt (271 + 107 + 5 = 383), das Etikett ist ungenau — und es ist **älter als dieser Branch** (`master` führte 271 + 90 = 361 nach derselben Logik). Beim nächsten Anfassen mitziehen, nicht dafür eine Schleife.
- **N7 · „17 UI (bUnit)" ist zweideutig.** Gemeint sind 17 **neue** Tests; zufällig hat `LagerbestandBearbeitenTests.cs` jetzt auch genau 17. Die beiden von UC10 berührten Testdateien enthalten zusammen 37. „17 neue bUnit-Tests" wäre eindeutig.
- **N8 · Drei Platzhalter in der alten Testliste.** `:376-378` führt „T30: (Additional parameterized test)", „T31: (Additional edge case)", „T32: (Additional edge case)". Älter als dieser Branch, die Gesamtzahl 32 stimmt trotzdem — aber wenn die Sektion für M6 ohnehin angefasst wird, sind das drei nachschlagbare Namen aus `ProduktInstanzServiceTests.cs`.
- **N9 · UC3 heißt im Repo zwei verschiedene Dinge.** `use-cases.drawio:30` und `architecture-overview.html:141` kennen UC3 als „Nährwerte" (fertig), `architecture-overview.html:359` als „Lagerbestand Export UI – BLOCKIERT". Das ist deutlich älter als UC10 und **nicht in diesem Gate zu klären** — es ist nur die Wurzel von M9 und gehört in `CONTEXT_SUMMARY.md`, damit der nächste, der die Legende anfasst, nicht dieselbe Falle trifft.
- **N10 · Prozess, positiv.** Die beiden Dinge, die du vor der Übergabe selbst abgefangen hast (Platzhalter `N UI Tests`, neun Markdown-Backticks in HTML), sind sauber weg — 0 Backticks, keine Platzhalterzahl. Und die Testzahlen sind **gezählt, nicht abgeschrieben**: Ich habe alle vier Angaben unabhängig nachgerechnet und keine Abweichung gefunden. Das ist genau die Lehre aus dem UC9-Branch, und sie ist hier angekommen.

---

## Fazit

Die Seite hat zum ersten Mal einen UI-Teil, und der Kern davon ist richtig: Routen, injizierte Services, Dateipfade, beide Leerzustandstexte wörtlich, die Fehlertrennung der Packungsliste, die N+1-Begründung für die Tages-Rechnung im UI — das habe ich Zeile für Zeile gegen die Razor-Dateien gehalten und nicht gegen die Commit-Message, und es trägt. Die Lagerort-Einschränkung, um die zwei Code-Reviews gekreist sind, ist benannt, tabellarisch gegenübergestellt und **nicht falsch dargestellt**. Die Overview ist in allen drei geforderten Sektionen gepflegt, die Zahlen stimmen nachgerechnet, und die Korrektur der WP4-Restliste auf „UC4, UC5, UC7, UC8" ist mehr, als verlangt war, und stimmt.

Was blockiert, ist nichts davon, sondern vier Sorten von Lücke. **Erstens** eine falsche Aussage über den Code: Der dokumentierte Filter-Ablauf kennt den Zweig nicht, der `GetByLagerortAsync` aufruft, und widerspricht damit einem grünen Test (M1). **Zweitens** die Hälfte des UC6-Maßstabs, der im Plan namentlich als Maßstab steht — keine Komponentenfelder, kein Nutzer-Workflow, keine Edge Cases, und eine Test-Coverage-Sektion, die statt 22 Testnamen einen Satz enthält, während dieselbe Seite die 32 Service-Tests einzeln auflistet (M4, M5). **Drittens** vier Stellen, die noch den Stand vom Juni behaupten, plus ein Workflow-Absatz, der eine Ampel und einen Kategorie-Filter beschreibt, die es nicht gibt — die Seite widerspricht sich damit selbst (M6, M7). **Viertens** Teil 4: Das Sequenzdiagramm zeigt eine Lifeline namens `ProduktInstanzPage`, die es nicht gibt, und verspricht dieselbe Drei-Status-Ampel; die Legende des Use-Case-Diagramms steckt einen grünen Knoten in die weiße Kategorie (M8, M9).

Zur Frage, die du mir ausdrücklich vorgelegt hast: **Die Begründung des `doc-agent` trägt nicht** — aber aus einem anderen Grund als bei UC9. Dort fehlte das UI-Szenario. Hier ist es da und beschreibt die falsche UI. Beide Auswege aus dem UC9-Review stehen weiterhin offen (volle Variante nach `1758dbe` oder Vermerk auf der Feature-Seite), und ich nenne beide, weil beide tragen — nur die Korrektur von `actor-ui` und `msg16` ist in beiden Fällen fällig. Dass du die Bewertung nicht vorweggenommen hast, war richtig; hätte ich sie von dir übernommen, hätte ich die zwei Falschaussagen im Diagramm nicht gefunden.

Kein Befund ist eine Geschmacksfrage, alle neun nennen Datei, Zeile und Soll-Zustand und sind ohne Rückfrage abarbeitbar. Der Umfang ist groß, aber die Arbeit ist es nicht: M1, M2, M3, M6, M9 und der Pflichtteil von M8 sind zusammen unter dreißig Zeilen; M4, M5 und M7 sind Schreibarbeit gegen vorhandene, abzählbare Quellen — keine davon verlangt eine Entscheidung, die noch nicht gefallen ist.

**Nächster Schritt**: M1–M9 an den `doc-agent`, Versuch 2/3. Für M8 die Entscheidung (a) oder (b) vorher mit dem User klären oder sie ihm mit deiner Empfehlung vorlegen — ich empfehle **(b)**, weil UC10 im Gegensatz zu UC9 keinen neuen Service-Workflow eingeführt hat und die Feature-Seite den UI-Ablauf nach Behebung von M1 und M5 ohnehin vollständig trägt; ein viertes Szenario würde dieselbe Aussage ein zweites Mal pflegebedürftig machen. Für M8 und M9 gilt die Diagramm-Regel aus dem Feature-Plan: gezielte `Edit`-Ersetzungen, kein XML-Werkzeug, danach Parser-Gegenprobe. Die Überträge aus den beiden Vorgänger-Gates (N2–N5 aus Code-Review 2; M3-Rest, N1, L3, L4 aus Test-Review 2) bleiben wie vereinbart offen und sind hier nicht bewertet; N9 gehört in dieselbe Liste.

Die Checkliste im Feature-Plan bleibt bei „Dokumentation freigegeben (review-agent, Modus `docs`)" **offen**.

---

**Reviewer**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 1 von 3
**Urteil**: **CHANGES REQUESTED**
