# UC10 Test-Phase Review (WP4 „MHD-Sichten für Produktinstanzen", UI) — Versuch 1/3

**Datum**: 19.09.2026
**Reviewer**: Review-Agent
**Phase**: Test-Review WP4 UC10
**Versuch**: 1/3
**Prüfgegenstand**: Commit `f9d3b8a`, Branch `feat/ui-produktinstanzen-uc10`
**Maßstab**: Feature-Plan `.agents/plans/uc10-mhd-sichten.md` (Schritt 3), `reviews/uc10-wp4-code-review-1.md` und `-2.md` (M2, M4 und die Notiz „Für den `test-agent`"), `CLAUDE.md` (Code-Style)
**Urteil**: ❌ **CHANGES REQUESTED** (3 Mängel, alle im Testcode; der Produktivcode bleibt freigegeben)

---

## 📊 Verifikation (selbst ausgeführt)

```
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 377, gesamt: 377, Dauer: 806 ms
git show f9d3b8a --stat   → 2 Dateien, 825/-0 — rein additiv, keine bestehende Zeile verändert
git status --porcelain    → leer (Review hat nichts verändert)
```

377/377 grün, wie gemeldet. **Grün ist hier der Soll-Zustand** (UI-Ausnahme von TDD, `UI-PHASE-PLAN.md:144`) und in keinem Fall ein Befund. Die Prüfung galt deshalb ausschließlich der Frage, ob diese 16 Tests *rot* würden, wenn jemand das Verhalten kaputt macht — nicht, ob sie heute durchlaufen.

**Was gut ist und zuerst gesagt gehört**: Die beiden Tests, an denen dieses Feature hängt, sind in der Substanz richtig gebaut.

- `FilteredList_IsSortedByVerfallsdatum` (`LagerbestandBearbeitenTests.cs:362`) füttert den Mock **tatsächlich** unsortiert (30/10/20 Tage, IDs 3/1/2) und prüft die gerenderte Zeilenreihenfolge. Streicht jemand `.OrderBy(p => p.Verfallsdatum)` in `LagerbestandBearbeiten.razor:203-205`, kommt die Mock-Reihenfolge 3-1-2 durch und der Test bricht. Das ist ein echter Regressionswächter, kein Selbstbeweis. Dasselbe gilt für `Sollte_Packungsliste_Nach_Verfallsdatum_Sortieren` (`LebensmittelDetailTests.cs:772`) gegen `LebensmittelDetail.razor:319-321`.
- `TodayExpiredItem_DoesNotAppear_InExpiredFilter` (`:310`) trifft die Sache genau: Der Mock liefert aus `GetVerfallenenAsync` eine Packung mit `Verfallsdatum = DateTime.Today` — also exakt das, was der Service mit seinem `<=` zurückgibt — und der Test erwartet **null** Zeilen plus „Keine Treffer für diesen Filter". Entfällt der Nachfilter `< DateTime.Today` (`razor:180-182`), erscheint die Zeile und der Test wird rot. Die Entscheidung aus M2 ist damit festgenagelt, und der `test-agent` hat die Warnung aus Code-Review 2 korrekt umgesetzt statt gegen den alten Stand zu schreiben.

Der dritte Prüfschwerpunkt fällt dagegen aus, und daran hängt der erste Mangel.

---

## MÄNGEL

### M1 — HIGH · `LagerortFilter_IsCaseSensitive` prüft keine Case-Sensitivität

**Fundstelle**: `src/Tests/Ui/LagerbestandBearbeitenTests.cs:706-755`

Der Test setzt `GetByLagerortAsync("Kühlschrank")` auf, wählt im Dropdown „Kühlschrank" und erwartet eine Zeile. Damit läuft er durch den **Nicht-Expired-Zweig** (`LagerbestandBearbeiten.razor:197-206`) — und dort führt die UI überhaupt keinen Stringvergleich durch, sie reicht den Wert an den gemockten Service durch. Der einzige case-sensitive Code der Seite ist `string.Equals(p.Lagerort, lagerortFilter, StringComparison.Ordinal)` in `razor:188`, und der liegt im Expired-Zweig, den dieser Test nie betritt.

**Konsequenz**: Ändert jemand `StringComparison.Ordinal` in `OrdinalIgnoreCase` — also genau die zweite Variante, zwischen denen M4 entschieden hat —, bleiben alle 377 Tests grün. Die Ordinal-Semantik ist ungedeckt, während ein Test danebensteht, dessen Name behauptet, sie sei gedeckt. Das ist schlechter als gar kein Test: Der Nächste, der die Zeile anfasst, sieht den Namen und hält die Entscheidung für abgesichert.

Hinzu kommt: Der Test ist inhaltlich ein **Duplikat** von `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` (`:260-307`) — gleicher Mock-Wert, gleiche Interaktion, gleiche Assertion, nur mit dem `.Change()` in ein `WaitForAssertion` gewickelt.

**Korrektur**: Test über den Expired-Pfad führen, weil nur dort verglichen wird.

```
GetVerfallenenAsync → [ {Id=1, Lagerort="Kühlschrank", Verfallsdatum = Today.AddDays(-2)},
                        {Id=2, Lagerort="kühlschrank", Verfallsdatum = Today.AddDays(-1)} ]
Status = "expired", Lagerort = "Kühlschrank"
Erwartung: genau eine Zeile, und zwar zeile-1; zeile-2 kommt nicht vor.
```

Beide Packungen überleben den Datumsfilter, die Trennung entsteht allein am Vergleich. Mit `OrdinalIgnoreCase` stünden zwei Zeilen da → rot. Das ist der Beweis, den der Name verspricht.

---

### M2 — MEDIUM · Die Sortierung im Expired-Pfad ist von keinem Test gedeckt

**Fundstelle**: fehlender Test zu `src/App/Components/Pages/Lager/LagerbestandBearbeiten.razor:193-195`

Die Seite sortiert an **drei** Stellen nach: im Expired-Zweig (`:193-195`), im Lagerort-Zweig (`:203-205`) und implizit über den Service im ungefilterten Zweig. `FilteredList_IsSortedByVerfallsdatum` deckt den Lagerort-Zweig ab — der Expired-Zweig bleibt offen:

| Test | Zeilen im Expired-Pfad | sortierrelevant? |
|---|---|---|
| `StatusFilter_Ruft_GetVerfallenenAsync_Auf` | 1 | nein |
| `TodayExpiredItem_DoesNotAppear_InExpiredFilter` | 0 | nein |
| `CombinedFilters_WorksCorrectly` | 1 (nach Nachfilter) | nein |
| `EmptyState_WithFilter_ShowsDifferentMessage` | 0 | nein |

**Konsequenz**: Streicht jemand `.OrderBy(...)` in `razor:193-195`, bleiben 377 Tests grün — und die Ansicht „Nur abgelaufene" verliert die FIFO-Reihenfolge, wegen der die Seite existiert. `GetVerfallenenAsync` ist ein reines `Where` (Plan, Zeile 85-88, ausdrücklich als Fallstrick markiert). Der Plan verlangt in Schritt 3 „**gefilterte Liste ist nach Verfallsdatum sortiert**" — das ist zur Hälfte eingelöst.

**Korrektur**: Entweder `StatusFilter_Ruft_GetVerfallenenAsync_Auf` auf drei unsortierte abgelaufene Packungen erweitern (z. B. IDs 3/1/2 mit -1/-30/-15 Tagen) und die Zeilenreihenfolge prüfen, oder einen eigenen `ExpiredFilter_IsSortedByVerfallsdatum` nach dem Muster von `:362` anlegen. Ein Test, nicht mehr.

---

### M3 — MEDIUM · Code-Style: 56 hinzugefügte Zeilen mit `var` statt explizitem Typ

**Fundstellen**: `LagerbestandBearbeitenTests.cs` in allen elf neuen Tests, u. a. `:213`, `:226`, `:231`, `:237`, `:247`, `:253`, `:263`, `:281`, `:313`, `:365`, `:403`, `:438`, `:483`, `:526`, `:569`, `:612`, `:640`, `:709` — dazu `LebensmittelDetailTests.cs:764` und `:841` (`var rows = cut.FindAll(...)`).

```
git show f9d3b8a | grep "^+" | grep -cE "\bvar\b"   → 58
```

Es sind durchweg `new`-Ausdrücke und Aufrufe mit festem Rückgabetyp (`List<ProduktInstanz>`, `Mock<IProduktInstanzService>`, `IElement`, `IRefreshableElementCollection<IElement>`) — kein Fall der `var`-Ausnahme für Typ-Variabilität. `CLAUDE.md` führt explizite Typen als verbindlichen Standard für **alle** Code-Commits, und das Review-Kriterium Nr. 1 stuft es als kritisch ein.

**Warum das hier zählt und nicht Geschmack ist**: Genau dieser Punkt war **M6 in Code-Review 1** und hat den `dev-agent` in diesem Feature einen Nachbesserungslauf gekostet; in Code-Review 2 habe ich ihn mit der Gegenprobe „keine hinzugefügte Zeile enthält `var`" freigegeben. Den `test-agent` im selben Feature an einem weicheren Maßstab zu messen, wäre willkürlich. Bemerkenswert ist zudem, dass die fünf neuen Tests in `LebensmittelDetailTests.cs` den Standard bis auf die zwei `var rows` **einhalten** (`List<ProduktInstanz> packungen = new List<ProduktInstanz>`, `Mock<ILebensmittelService> lebensmittelMock = new()`) — die Regel ist dem Agenten also bekannt, sie wurde nur in der zweiten Datei nicht angewandt.

**Korrektur**: Nur die **hinzugefügten** Zeilen umstellen. Die fünf Bestandstests in `LagerbestandBearbeitenTests.cs:18-207` bleiben unangetastet — sie sind unveränderter Code und nicht Gegenstand dieses Commits; ein Reformat würde den Diff aufblähen, ohne etwas abzusichern.

---

## Kleine Befunde (LOW — kein Freigabe-Hindernis, gern im selben Durchgang)

- **L1 · Namen behaupten „ruft auf", ohne es zu verifizieren.** `StatusFilter_Ruft_GetVerfallenenAsync_Auf` (`:210`) und `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` (`:260`) enthalten kein `Verify`. Der Aufruf ist indirekt belegt (der jeweils andere Setup liefert leer, die Zeile kann nur aus dem erwarteten Aufruf stammen) — das trägt, ist aber schwächer als der Name. Ein `instanzMock.Verify(s => s.GetVerfallenenAsync(It.IsAny<DateTime?>()), Times.Once)` macht den Namen wahr und fängt zusätzlich einen doppelten Serviceaufruf pro Filterwechsel ab, den heute nichts bemerken würde.
- **L2 · Die Heute-Grenze der Zeilenfärbung ist unbewacht.** `MhdColumn_ShowsTodayExpiring` (`:566`) prüft den Text „Heute ablaufend", aber nicht, dass die Zeile **kein** `table-danger` trägt. Kippt `razor:76` von `<` auf `<=`, bleibt alles grün — obwohl das genau die Grenze ist, die M2 über alle drei Stellen vereinheitlicht hat. Eine Zeile: `Assert.DoesNotContain("table-danger", cut.Find("[data-testid='zeile-1']").ClassList)`.
- **L3 · Der Zeileninhalt der Packungsliste wird kaum geprüft.** `Sollte_Packungsliste_Mit_Produktinstanzen_Rendern` (`:705`) assertet Zeilenzahl und zwei Lagerort-Strings. Menge, Verfallsdatum, die Spalte „Tage bis MHD" und der vom Plan geforderte Link auf `/lagerbestand/{id}/bearbeiten` (`razor:243-247`) sind in **keinem** Test der Detailseite abgedeckt — verschwindet die MHD-Zelle oder bricht das href, merkt es niemand. Eine Zeile exemplarisch vollständig prüfen genügt.
- **L4 · Das Lagerort-Dropdown wird nicht gegen `LagerortKonstanten.AlleWerte` geprüft.** Der Plan verlangt in Schritt 1 ausdrücklich „**nicht** hartcodieren — die Konstante iterieren". Kein Test würde bemerken, wenn jemand vier `<option>`-Literale einträgt oder die Konstante um einen fünften Wert wächst, ohne dass das Dropdown folgt. `Assert.Equal(LagerortKonstanten.AlleWerte.Length + 1, cut.Find("[data-testid='select-lagerort-filter']").QuerySelectorAll("option").Length)` bindet beides aneinander.
- **L5 · Zehn neue Analyzer-Warnungen.** `Assert.True(cut.Markup.Contains(...))` statt `Assert.Contains(...)`: neu bei `LagerbestandBearbeitenTests.cs:518, 561, 604, 700, 701` und `LebensmittelDetailTests.cs:766, 767, 887, 890, 977` (xUnit2009, `Assert.False` → `Assert.DoesNotContain` bei `:701`). Die Bestandstests machen es genauso, der Analyzer meckert also nicht erst seit heute — neu hinzugefügter Code sollte aber keine Warnungen mehr erzeugen, und der praktische Preis ist real: Bei einem Fehlschlag liest man „Expected: True, Actual: False" statt des fehlenden Textes samt Markup. Mechanische Ersetzung.
- **L6 · Act im Assert-Block.** `LagerbestandBearbeitenTests.cs:691-695` und `:742-746` führen `Find(...).Change(...)` **innerhalb** von `WaitForAssertion` aus. Heute harmlos, weil der Block nicht wirft; sobald er es doch tut, wird die Interaktion bis zum Timeout wiederholt ausgelöst, und AAA ist nicht mehr lesbar. Die neun anderen neuen Tests machen es richtig — die zwei angleichen.
- **L7 · Zwei deutsche Namen in einer englischen Datei.** `StatusFilter_Ruft_GetVerfallenenAsync_Auf` und `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` stehen zwischen fünf englischen Bestands- und neun englischen Neutests. In `LebensmittelDetailTests.cs` ist es umgekehrt gelöst, dort fügen sich die neuen `Sollte_…`-Namen sauber ein — dort stimmt es also. Die beiden Ausreißer angleichen, dann ist jede Datei in sich konsistent.
- **L8 · `WithoutFilter_CallsGetNachVerfallsdatumSortiertAsync` (`:435`) ist inhaltlich ein Duplikat** von `RendersTable_WithAllProduktInstanzen` (`:19`): dieselben Mocks, dieselbe Aussage, nur mit einer statt zwei Zeilen. Mit einem `Verify` aus L1 bekäme er eine eigene Aussage; ohne ist er Ballast.

---

## HINWEISE

- **H1 · Zum Umfang — die Begründung des `test-agent` trägt nicht, der Befund blockiert trotzdem nicht.**
  Ich habe das unabhängig bewertet, wie gewünscht. 548 Zeilen für 11 Tests entstehen nicht aus Klarheit, sondern aus einem 25-zeiligen Arrange-Block, der in jedem Test zeichengleich wiederholt wird (zwei Mocks, zwei Setups, zwei `AddSingleton`). Das ist kein „langer, klarer Test", das ist Copy-Paste — und der Beweis dafür steht **in diesem Commit selbst**: Weil die Detailseite eine dritte Abhängigkeit bekam, mussten in `LebensmittelDetailTests.cs` **13 bestehende Tests** um denselben fünfzeiligen `produktInstanzMock`-Block ergänzt werden (`:53-57`, `:97-101`, `:139-143`, `:183-187`, …). Genau die Nachpflege an N Stellen, die der `test-agent` für unproblematisch hielt, ist bei der ersten Gelegenheit eingetreten und macht rund 100 der 277 neuen Zeilen dieser Datei aus. Ein privates `SetupServices(...)` je Testklasse hätte das auf eine Stelle reduziert, ohne einen einzigen Test unleserlicher zu machen.
  **Trotzdem kein Mangel**: Ein Test, der ausführlicher ist als nötig, blockiert nach meinem eigenen Maßstab nicht, und die Bestandstests beider Dateien geben dieses Muster vor — der Commit fällt nicht aus dem Rahmen, er verlängert ihn. Empfehlung für das nächste Anfassen dieser Dateien, gern optional schon bei der Nachbesserung zu M1–M3, aber ausdrücklich **nicht** als Auflage.
- **H2 · `MhdTextFormatter` hat keinen eigenen Unit-Test.** Die Klasse ist seit Code-Review 1 die einzige Quelle für beide Seiten und hat fünf Zweige; drei davon werden über `MhdColumn_ShowsFutureDays/PastDays/TodayExpiring` indirekt gerendert, die beiden **Singular-Zweige** („vor 1 Tag abgelaufen", „1 Tag verbleibend", `MhdTextFormatter.cs:18` und `:26`) sind ungedeckt. Ein achtzeiliges `[Theory]` mit fünf `[InlineData]` deckt alles ab, läuft ohne bUnit-Rendering und wäre schneller und schärfer als die drei Rendertests. Kein Mangel — die Klasse gehört formal zur Code-Phase, und die Anzeige ist ja abgedeckt —, aber die billigste Coverage-Verbesserung, die hier zu haben ist.
- **H3 · Der Fehlerpfad von `/lagerbestand` ist völlig ungetestet.** `razor:51` (`alert-fehler`) und der `catch` in `AlleAbrufen()` haben weder vor noch nach diesem Commit einen Test. Das ist eine Altlast der UC2-Phase und nicht durch UC10 entstanden — die Detailseite macht es mit `Sollte_Fehler_Anzeigen_Wenn_Lebensmittel_Nicht_Geladen` vorbildlich. Für einen späteren Durchgang vormerken, nicht hier.
- **H4 · Das Zurücksetzen eines Filters ist nicht gedeckt.** Kein Test wechselt von „Nur abgelaufene" zurück auf „Alle". Der Pfad ist trivial (`OnStatusFilterChanged` → `AlleAbrufen`), aber es ist die häufigste Nutzerbewegung nach dem Filtern.
- **H5 · Kein Befund am Produktivcode.** Die Tests haben nichts aufgedeckt, was den freigegebenen Code in Frage stellt. N2–N5 aus Code-Review 2 bleiben wie vereinbart offen und waren hier nicht Gegenstand. Die Freigabe der Implementierung steht unverändert.

---

## Checkliste Test-Phase Review

- [x] Tests laufen: 377/377 grün, selbst ausgeführt (grün ist hier korrekt — UI-Ausnahme)
- [x] Diff rein additiv, kein bestehender Test verändert oder entschärft
- [x] Zentraler Fallstrick „Sortierung nach Filter": **Lagerort-Pfad gedeckt** (Mock nachweislich unsortiert) — **Expired-Pfad offen** → M2
- [x] M2-Entscheidung aus Code-Review 1/2 (Heute-Grenze `<` statt `<=`) korrekt festgenagelt
- [ ] **M4-Entscheidung (Ordinal-Vergleich) festgenagelt** → M1, der Test prüft es nicht
- [x] Keine Assertion, die immer zutrifft; kein Test ohne Inhaltsprüfung; kein Mock, der den geprüften Pfad umgeht
- [x] `data-testid`-Selektoren statt CSS-Klassen durchgehend
- [x] Beide Leerzustände unterschieden, inkl. Negativassertion gegen die falsche Meldung
- [x] Fehlertrennung auf der Detailseite geprüft (Lebensmittel **und** Nährwert überleben den Packungsfehler)
- [ ] **Code-Style (explizite Typen)** → M3, 56 hinzugefügte `var`-Zeilen
- [x] `is null`/`is not null`: kein `== null`/`!= null` im neuen Testcode
- [x] Arrange/Act/Assert erkennbar (zwei Ausnahmen → L6)
- [ ] Testnamen dateikonsistent → L7
- [x] Review hat keine Datei verändert (`git status` leer)

---

## Fazit

Der Kern sitzt: Die beiden Tests, die dieses Feature wirklich absichern mussten — die Nachsortierung nach dem Filtern und die Heute-Grenze im Filter „Nur abgelaufene" — sind so gebaut, dass sie bei einer Regression tatsächlich rot werden. Der `test-agent` hat die Notiz aus Code-Review 2 verstanden und nicht gegen den alten, widersprüchlichen Stand geschrieben. Das ist die schwierigere Hälfte der Aufgabe, und sie ist erledigt.

Was blockiert, ist die zweite Hälfte derselben Sache. Von den beiden Verhaltensentscheidungen, die das Code-Review erzwungen hat, ist eine gedeckt (M2) und eine nicht (M4) — und ausgerechnet für die ungedeckte existiert ein Test, dessen Name das Gegenteil behauptet. Zusammen mit der fehlenden Sortierprüfung im Expired-Pfad heißt das: Der Filter „Nur abgelaufene" ist der einzige der drei Pfade, dessen Eigenlogik weitgehend unbewacht bleibt, obwohl er der einzige ist, den dieses Feature neu erfunden hat. Beides kostet zwei Tests beziehungsweise zwei Erweiterungen bestehender Tests.

M3 ist Handwerk, kein Denkfehler, aber er steht in der Mängelliste, weil derselbe Punkt im selben Feature schon einmal einen Nachbesserungslauf gekostet hat — beim `dev-agent`, und dort zu Recht.

**Nächster Schritt**: Mängelliste M1–M3 zurück an den `test-agent`, Versuch 2/3. L1–L8 gern im selben Durchgang mitnehmen, sie sind alle ein- bis dreizeilig; H1 ausdrücklich **nicht** als Auflage. Der `doc-agent` wartet, bis die Tests freigegeben sind. Die Checkliste im Feature-Plan bleibt bei „bUnit-Tests geschrieben und grün" stehen und wird **nicht** abgehakt.

---

**Beurteilt durch**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 1/3 → ❌ **CHANGES REQUESTED**
