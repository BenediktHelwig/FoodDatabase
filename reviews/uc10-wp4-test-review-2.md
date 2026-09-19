# UC10 Test-Phase Review (WP4 „MHD-Sichten für Produktinstanzen", UI) — Versuch 2/3

**Datum**: 19.09.2026
**Reviewer**: Review-Agent
**Phase**: Test-Review WP4 UC10
**Versuch**: 2/3
**Prüfgegenstand**: Commit `df800bf`, im Gesamtbild `git diff master..HEAD -- src/Tests/`, Branch `feat/ui-produktinstanzen-uc10`
**Maßstab**: `reviews/uc10-wp4-test-review-1.md` (M1–M3, L1–L8, H1–H5), Feature-Plan `.agents/plans/uc10-mhd-sichten.md` (Schritt 3), `CLAUDE.md` (Code-Style)
**Urteil**: ✅ **APPROVED** — beide blockierenden Mängel sind nachweislich behoben; was offen bleibt, ist ausschließlich LOW und geht als Vormerkung weiter

---

## 📊 Verifikation (selbst ausgeführt)

```
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 383, gesamt: 383, Dauer: 847 ms
git diff master..HEAD --stat -- src/Tests/
   3 Dateien, 1041 insertions(+), 0 deletions(-)   → gegen master rein additiv
git diff master..HEAD -- src/Tests/ | grep "^+" | grep -cE "\bvar\b"   → 1  (nicht 0, siehe M3)
git diff --check master..HEAD -- src/Tests/                            → leer
Zeilenenden: alle drei Dateien 100 % CRLF, kein Mischstand
git status --porcelain → nur docs/features/UC10-Produktinstanzen.html (parallele Doku-Arbeit, außerhalb des Scopes)
```

383/383 grün, wie gemeldet. Ich habe **keine Datei verändert** und den Produktivcode wie angewiesen nicht angefasst — die Wirksamkeitsnachweise unten sind aus Mock-Konstellation und durchlaufenem Codepfad hergeleitet, nicht durch einen Toggle erzeugt.

**Wichtig für deinen eigenen Toggle-Test**: Meine Herleitung liefert zwei falsifizierbare Vorhersagen. Wenn sie nicht eintreffen, stimmt meine Analyse nicht, und die Freigabe gehört zurückgenommen.

| Eingriff im Produktivcode | Es fallen **genau** diese Tests, sonst keiner |
|---|---|
| `LagerbestandBearbeiten.razor:188` `Ordinal` → `OrdinalIgnoreCase` | **nur** `ExpiredFilter_IsCaseSensitiveForLagerort`, und zwar an `Assert.Single(rows)` mit 2 statt 1 Zeile |
| `LagerbestandBearbeiten.razor:193-195` `.OrderBy(p => p.Verfallsdatum)` gestrichen | **nur** `ExpiredFilter_IsSortedByVerfallsdatum`, und zwar an `rows[1]` (erwartet `zeile-2`, bekommt `zeile-1`); `rows[0]` passt zufällig weiter |

---

## Alte Mängel — Status

### M1 (HIGH) — ✅ **behoben**

`LagerortFilter_IsCaseSensitive` ist gelöscht, `ExpiredFilter_IsCaseSensitiveForLagerort` (`src/Tests/Ui/LagerbestandBearbeitenTests.cs:715-782`) tritt an seine Stelle. Der Nachweis trägt, Schritt für Schritt am Codepfad:

1. `GetVerfallenenAsync` liefert Id=1 `Lagerort="Kühlschrank"`, `Verfallsdatum = Today-2` und Id=2 `Lagerort="kühlschrank"`, `Verfallsdatum = Today-1`.
2. `razor:180-182` filtert auf `< DateTime.Today` — **beide** Packungen überleben. Das ist die Pointe: Die Trennung kann anschließend nur noch am Stringvergleich entstehen, an keiner anderen Stelle.
3. `razor:185-190` vergleicht `string.Equals(p.Lagerort, "Kühlschrank", StringComparison.Ordinal)` → Id=1 bleibt, Id=2 fällt.
4. Assertion: `Assert.Single(rows)`, `rows[0].OuterHtml` enthält `data-testid="zeile-1"`, und `Assert.DoesNotContain("Käse", cut.Markup)` — „Käse" ist der Name zu `LebensmittelKatalogId = 2` und erscheint nur, wenn Id=2 gerendert wird.

Mit `OrdinalIgnoreCase` überleben beide, `Assert.Single` bekommt 2 Zeilen und bricht — zusätzlich schlägt die Negativassertion an. Das ist genau der Beweis, den der Testname verspricht, und er läuft durch den einzigen Zweig, in dem die Seite überhaupt vergleicht. Die M4-Entscheidung aus Code-Review 2 ist damit festgenagelt.

Kein Deckungsverlust durch die Löschung: Die einzige Aussage des alten Tests (`GetByLagerortAsync("Kühlschrank")` liefert eine Zeile) trägt weiterhin `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` (`:263`) — dort jetzt sogar mit `Verify` schärfer als vorher.

### M2 (MEDIUM) — ✅ **behoben**

`ExpiredFilter_IsSortedByVerfallsdatum` (`:784-855`) füttert den Mock nachweislich unsortiert: Id=3 bei −30, Id=1 bei −10, Id=2 bei −15 Tagen, in genau dieser Reihenfolge. Kein Lagerortfilter gesetzt, also läuft der Expired-Zweig ohne Nachfilter bis `razor:193-195`. Erwartet wird `zeile-3`, `zeile-2`, `zeile-1` — die korrekte aufsteigende Datumsfolge, die sich von der Mock-Reihenfolge unterscheidet.

Ohne `OrderBy` käme 3-1-2 durch: `rows[0]` bliebe grün (Zufall, Id=3 steht auch unsortiert vorn), `rows[1]` erwartet `zeile-2` und bekommt `zeile-1` → rot. Die Assertion prüft alle drei Positionen, also fängt sie auch jede andere Permutation. Die Forderung des Plans („gefilterte Liste ist nach Verfallsdatum sortiert") ist jetzt über **beide** Nachsortier-Stellen eingelöst, nicht nur über den Lagerort-Zweig.

### M3 (MEDIUM) — ⚠️ **weitgehend behoben, eine benannte Restfundstelle offen → herabgestuft auf LOW**

57 von 58 `var` sind umgestellt, `using AngleSharp.Dom` ergänzt, die Bestandstests in `LagerbestandBearbeitenTests.cs:18-207` korrekt unangetastet gelassen. Offen ist **eine** Zeile, die ich in Versuch 1 namentlich mit Zeilennummer aufgeführt hatte:

```
src/Tests/Ui/LebensmittelDetailTests.cs:841
    var rows = cut.FindAll("[data-testid^='zeile-packung-']");
```

Sie steht in `Sollte_Packungsliste_Nach_Verfallsdatum_Sortieren` und ist eine von diesem Branch hinzugefügte Zeile, fällt also unter die Regel. Die Schwesterzeile `:764` im Test direkt darüber wurde umgestellt — es ist ein Übersehen, kein Einwand.

**Warum das die Freigabe nicht mehr blockiert**: 58 `var` über elf Tests waren ein systematisches Ignorieren eines verbindlichen Standards und damit gleichwertig zu dem, was den `dev-agent` einen Lauf gekostet hat. Eine übersehene Zeile ist ein Ausrutscher anderer Art. Ich stufe den Rest deshalb nach LOW herab und behandle ihn wie L3/L4 — nach meinem eigenen Maßstab aus Versuch 1 sind LOW-Befunde kein Freigabe-Hindernis. Das ist eine Herabstufung des Befunds, **kein** Vorbehalt am Urteil.

---

## Kleine Befunde aus Versuch 1 — Status

| Befund | Status | Prüfung |
|---|---|---|
| **L1** Verify fehlt | ✅ behoben | `Verify(..., Times.Once)` in `StatusFilter_…Auf` (`:258`), `LagerortFilter_…Auf` (`:310`), `WithoutFilter_…` (`:483`) |
| **L2** Heute-Grenze der Färbung unbewacht | ✅ behoben | `MhdColumn_ShowsTodayExpiring` (`:610-613`): `Assert.DoesNotContain("table-danger", row.ClassList)`. Wirksam — kippt `razor:76` auf `<=`, wird `verfallKlasse` gesetzt und die Assertion bricht, während der Text „Heute ablaufend" unverändert bliebe |
| **L3** Zeileninhalt der Packungsliste | ❌ **offen** | siehe Übertrag unten |
| **L4** Dropdown gegen `LagerortKonstanten.AlleWerte` | ❌ **offen** | siehe Übertrag unten |
| **L5** `Assert.True(...Contains(...))` | ✅ behoben | `git diff master..HEAD -- src/Tests/ \| grep -E "^\+.*Assert\.(True\|False)\(.*Contains"` → **0 Treffer**. Die verbliebenen 16 xUnit2009-Warnungen im Build (`LagerbestandBearbeitenTests:70,71,96,207`, `LebensmittelDetailTests:66-68,482,514,602,653,697-700`) sowie CS8620 (`LebensmittelDetailTests:461`) sitzen sämtlich auf Zeilen, die dieser Branch nicht hinzugefügt hat — Altbestand, wie in Versuch 1 abgegrenzt |
| **L6** Act im Assert-Block | ⚠️ **halb behoben** | siehe N1 |
| **L7** deutsche Namen in englischer Datei | ❌ **offen**, Begründung trägt nicht | siehe N2 |
| **L8** `WithoutFilter_…` inhaltliches Duplikat | ✅ behoben | Der `Verify` aus L1 gibt ihm die eigene Aussage, die ihm gefehlt hat |

---

## Neue Befunde (alle LOW, keiner blockierend)

### N1 · L6 ist nur zur Hälfte umgesetzt — die Interaktion steckt weiter im Retry-Block

**Fundstellen**: `LagerbestandBearbeitenTests.cs:700-705` und `:764-772`

Verschoben wurde das `cut.Find(...)`, nicht das `.Change(...)`. Übrig bleibt:

```csharp
IElement statusFilter = cut.Find("[data-testid='select-status-filter']");

cut.WaitForAssertion(() =>
{
    statusFilter.Change("expired");
}, TimeSpan.FromSeconds(2));
```

Ein `WaitForAssertion`, dessen Rumpf **ausschließlich** eine Interaktion und **keine einzige Assertion** enthält. Der in Versuch 1 benannte Nachteil steht unverändert: Wirft `Change` eines Tages doch, wird das Change-Event bis zum Timeout wiederholt gefeuert, und der Test verschleiert die Ursache, statt sie zu melden. Neu hinzu kommt, dass die Konstruktion jetzt auch beim Lesen keinen Sinn mehr ergibt — vorher war immerhin das `Find` eine Begründung dafür, zu warten.

Neun der elf neuen Tests machen es richtig; `ExpiredFilter_IsCaseSensitiveForLagerort` macht es in **derselben Methode** vier Zeilen höher richtig (`:764-765`, `statusFilter.Change("expired")` frei stehend) und vier Zeilen tiefer falsch. Die Korrektur ist das Entfernen von zwei Zeilen Wrapper.

**Nicht blockierend**: LOW in Versuch 1, LOW jetzt. Die Meldung „L6 umgesetzt" stimmt allerdings nicht — das gehört in den Übertrag, nicht unter den Tisch.

### N2 · Die Begründung zu L7 trifft nicht zu

Gemeldet wurde „geprüft, kein Handlungsbedarf (die Datei ist in sich konsistent)". Der Stand von `LagerbestandBearbeitenTests.cs` ist: **15 englische Testnamen, 2 deutsche** (`StatusFilter_Ruft_GetVerfallenenAsync_Auf` `:211`, `LagerortFilter_Ruft_GetByLagerortAsync_Mit_Wert_Auf` `:263`). Die Datei ist also gerade nicht in sich konsistent — das war der ganze Inhalt von L7. Der Befund bleibt LOW und blockiert nicht; ich halte nur fest, dass er mit einer unzutreffenden Feststellung geschlossen wurde, nicht mit einem Argument.

### N3 · Kein Assertion-Verlust durch den Umbau — geprüft, nichts gefunden

Ich habe den Auftrag ernst genommen, gezielt nach Abschwächungen zu suchen, und keine gefunden:

- **Gegen `master` ist der gesamte Test-Diff rein additiv** (1041 Zeilen hinzu, 0 entfernt). Die 87 Löschungen in `df800bf` sind ausnahmslos Zeilen, die derselbe Branch in `f9d3b8a` selbst angelegt hatte. Kein Test, der vor UC10 existierte, wurde berührt — außer den `produktInstanzMock`-Ergänzungen in den 13 Detailseiten-Tests, die technisch nötig waren.
- **Die `Assert.True` → `Assert.Contains`-Umstellungen (L5)** sind alle bedeutungserhaltend. Auch die Umkehrung `Assert.False(cut.Markup.Contains(...))` → `Assert.DoesNotContain(...)` in `EmptyState_WithFilter_ShowsDifferentMessage` (`:711`) behält die Negativassertion gegen die falsche Leermeldung — der Punkt, den ich in Versuch 1 ausdrücklich gelobt hatte, steht weiter.
- **Der Typwechsel `var` → `IReadOnlyList<IElement>`** ist an allen elf Stellen der korrekte statische Typ von `cut.FindAll(...)`. Kein Fall von Down- oder Upcast, der eine Assertion anders auflösen ließe; die `Assert.Single`/`Assert.Equal(n, rows.Count)`-Aufrufe binden unverändert an die Collection-Überladung.
- **Kein neuer Test ist schwächer als der, den er ersetzt** — für M1 oben einzeln durchgerechnet.

---

## `MhdTextFormatterTests` (H2 aus Versuch 1) — vollständig und korrekt

**Datei**: `src/Tests/Unit/MhdTextFormatterTests.cs` (27 Zeilen, neu)

Ich habe die fünf erwarteten Texte **gegen die Quelle** geprüft, nicht gegen meine Erwartung:

| `[InlineData]` | Zweig in `src/App/Models/MhdTextFormatter.cs` | Literal in der Quelle | Deckung |
|---|---|---|---|
| `-5` → „vor 5 Tagen abgelaufen" | `:14-18`, `tagedavor != 1` | `:17` `$"vor {tagedavor} Tagen abgelaufen"` | ✅ |
| `-1` → „vor 1 Tag abgelaufen" | `:14-18`, `tagedavor == 1` | `:17` `"vor 1 Tag abgelaufen"` | ✅ Singular, vorher ungedeckt |
| `0` → „Heute ablaufend" | `:19-22` | `:21` | ✅ |
| `1` → „1 Tag verbleibend" | `:23-26`, `== 1` | `:25` `"1 Tag verbleibend"` | ✅ Singular, vorher ungedeckt |
| `5` → „5 Tage verbleibend" | `:23-26`, `!= 1` | `:25` `$"{tagesBisMhd} Tage verbleibend"` | ✅ |

Alle fünf Zweige, beide Singularfälle, `Assert.Equal` statt `Contains` — also schärfer als die drei Rendertests, die nur auf Teilstrings im Markup prüfen. `Math.Abs` ist über `-5`/`-1` mitgeprüft. Das ist genau das, was H2 vorgeschlagen hatte, und es ist sauber umgesetzt.

Eine Randnotiz ohne Befundcharakter: Die Datei liegt direkt in `src/Tests/Unit/`, während alle anderen Testklassen dort in `Unit/Services/` liegen (`Unit/Helpers/` enthält nur `FixedTimeProvider`). Da `MhdTextFormatter` in `App/Models` und nicht in `App/Services` wohnt, gibt es für eine Models-Ebene bisher kein Vorbild — die flache Ablage ist vertretbar, Namespace `FoodDatabase.Tests.Unit` passt zum Pfad. Kein Handlungsbedarf, nur damit es beim nächsten Test zu einer Models-Klasse bewusst entschieden wird.

---

## Übertrag — was offen bleibt und wohin es gehört

Diese vier Punkte sind **keine Auflage für dieses Gate**. Sie gehören in `CONTEXT_SUMMARY.md` oder den nächsten Plan, der diese Dateien anfasst, damit sie nicht schlicht verschwinden:

1. **M3-Rest** — `LebensmittelDetailTests.cs:841`, ein `var`. Eine Zeile.
2. **N1 / L6-Rest** — `LagerbestandBearbeitenTests.cs:700-705` und `:764-772`: `WaitForAssertion`-Wrapper um die reine Interaktion entfernen. Zwei mal zwei Zeilen.
3. **L3** — Eine Packungszeile der Detailseite exemplarisch **vollständig** prüfen: Menge, Verfallsdatum, Spalte „Tage bis MHD" und das `href` auf `/lagerbestand/{id}/bearbeiten` (`LebensmittelDetail.razor:243-247`). Der vom Plan geforderte Link ist von keinem Test gedeckt; bricht er, merkt es niemand.
4. **L4** — Dropdown an `LagerortKonstanten.AlleWerte` binden: `Assert.Equal(LagerortKonstanten.AlleWerte.Length + 1, …QuerySelectorAll("option").Length)`. Deckt die Plan-Forderung „nicht hartcodieren" ab und fängt einen künftigen fünften Lagerort, dem das Dropdown nicht folgt.

Zur Frage, ob L3/L4 eine dritte Runde rechtfertigen — meine Antwort, unabhängig von der Begründung des `test-agent`: **nein.** Nicht, weil die Begründung zufällig zum richtigen Ergebnis geführt hat (sie war falsch, dein Auftrag hat die beiden mitverlangt), sondern weil beide unveränderte LOW-Befunde meiner eigenen Einstufung sind und ich in Versuch 1 ausdrücklich geschrieben habe, dass sie kein Freigabe-Hindernis sind. Sie jetzt zum Blocker zu machen, hieße den Maßstab nachträglich zu verschieben — und dafür den letzten Versuch zu verbrauchen, der für einen echten Regressionsdurchlass gebraucht würde. Beides sind **Deckungslücken**, keine irreführenden Tests; das ist der Unterschied zu M1, der blockieren musste.

---

## HINWEISE

- **H1 (Setup-Consolidation) bleibt ausdrücklich keine Auflage.** So habe ich es in Versuch 1 eingestuft, dabei bleibt es. Der Umbau hat das Muster erwartungsgemäß noch einmal verlängert (zwei weitere 25-Zeilen-Arrange-Blöcke); die Empfehlung eines privaten `SetupServices(...)` steht unverändert für das nächste größere Anfassen dieser Datei — als Empfehlung, nicht als Forderung.
- **H3 (Fehlerpfad `/lagerbestand` ungetestet)** und **H4 (Filter-Zurücksetzen ungedeckt)** sind unverändert offen und unverändert kein Gegenstand dieses Features. Beide sind Kandidaten für denselben Übertrag wie L3/L4.
- **H5 bleibt gültig: kein Befund am Produktivcode.** Die sechs neuen Tests haben nichts aufgedeckt, was die Freigabe der Implementierung in Frage stellt. N2–N5 aus Code-Review 2 bleiben wie vereinbart offen.
- **Zur Bringschuld beim `docs`-Gate**: Ich habe mir vorgemerkt, dort die Lagerort-Einschränkung zu prüfen. `ExpiredFilter_IsCaseSensitiveForLagerort` macht die Ordinal-Semantik jetzt auch für einen Leser der Tests sichtbar — die Dokumentation sollte dieselbe Aussage treffen, sonst weichen Test und Doku in genau dem Punkt voneinander ab, an dem zwei Reviews gehangen haben.

---

## Checkliste Test-Phase Review

- [x] Tests laufen: 383/383 grün, selbst ausgeführt (grün ist hier korrekt — UI-Ausnahme, `UI-PHASE-PLAN.md:144`)
- [x] Gegen `master` rein additiv, kein vorbestehender Test verändert, entfernt oder entschärft
- [x] Zentraler Fallstrick „Sortierung nach Filter": **beide** Nachsortier-Stellen gedeckt, Mocks nachweislich unsortiert → M2
- [x] M2-Entscheidung aus Code-Review 1/2 (Heute-Grenze `<` statt `<=`) festgenagelt — jetzt zusätzlich über die Färbung (L2)
- [x] **M4-Entscheidung (Ordinal-Vergleich) festgenagelt** → M1, Test läuft durch den vergleichenden Zweig
- [x] `MhdTextFormatter`: alle fünf Zweige, Erwartungen gegen die Quelle abgeglichen
- [x] Keine Assertion, die immer zutrifft; kein Test ohne Inhaltsprüfung; kein Mock, der den geprüften Pfad umgeht
- [x] Keine Assertion beim Umbau verloren gegangen (L5- und Typumstellungen bedeutungserhaltend)
- [x] `data-testid`-Selektoren statt CSS-Klassen durchgehend
- [x] Keine neuen Analyzer-Warnungen auf hinzugefügten Zeilen (0 Treffer)
- [~] Code-Style (explizite Typen): 57/58 → eine Restfundstelle, auf LOW herabgestuft
- [x] `is null`/`is not null`: kein `== null`/`!= null` im neuen Testcode
- [~] Arrange/Act/Assert erkennbar (zwei Interaktionen weiterhin im `WaitForAssertion` → N1)
- [ ] Testnamen dateikonsistent → L7 offen, Übertrag
- [x] Zeilenenden konsistent CRLF, `git diff --check` leer
- [x] Review hat keine Datei verändert; Produktivcode nicht angefasst

---

## Fazit

Die zwei Dinge, die in Versuch 1 blockiert haben, sind erledigt, und zwar richtig erledigt. `ExpiredFilter_IsCaseSensitiveForLagerort` stellt eine Mock-Konstellation her, in der der Stringvergleich die **einzige** verbleibende Unterscheidung ist — schwächer geht es nicht bauen, ohne den Beweis zu verlieren, und der `test-agent` hat den vorgeschlagenen Aufbau nicht nur abgeschrieben, sondern mit der Negativassertion auf „Käse" noch eine zweite, unabhängige Bruchstelle eingezogen. `ExpiredFilter_IsSortedByVerfallsdatum` prüft alle drei Positionen statt nur der ersten und fängt damit jede Permutation, nicht nur die eine, die sich der Autor vorgestellt hat. Beide Verhaltensentscheidungen, um die das Code-Review gerungen hat, sind jetzt von je einem Test bewacht, der bei ihrer Rücknahme rot wird.

Was offen bleibt, ist der Rest, den ich selbst als LOW eingestuft habe: eine übersehene `var`-Zeile, zwei überflüssige `WaitForAssertion`-Wrapper, zwei Deckungslücken und zwei Testnamen in der falschen Sprache. Nichts davon lässt eine Regression durch, nichts davon behauptet fälschlich eine Absicherung — das war der Unterschied, der M1 blockieren ließ, und er liegt hier nicht vor. Zwei der Begründungen für das Auslassen trafen sachlich nicht zu (L3/L4 waren verlangt, L7 ist nicht konsistent); ich habe das benannt, statt es zu glätten, aber es ändert die Einstufung der Befunde selbst nicht. Den letzten Versuch dafür zu verbrauchen, wäre ein schlechter Tausch: Er wird gebraucht, falls doch noch etwas Substanzielles auftaucht, und er ist in diesem Feature schon einmal teuer geworden.

**Nächster Schritt**: Die Checkliste im Feature-Plan bei „Tests freigegeben (review-agent)" abhaken. Die vier Übertragspunkte (M3-Rest, N1, L3, L4) in `CONTEXT_SUMMARY.md` oder den nächsten Plan aufnehmen — sie sind nicht erledigt, sie sind nur nicht hier fällig. Dann `doc-agent` mit dem 4-Teil-Check; das `docs`-Review prüft danach unter anderem, ob die Lagerort-Einschränkung in der Dokumentation dieselbe Aussage trifft wie jetzt der Test.

Deinen Toggle-Test führ trotzdem durch — er kostet zwei Minuten und die Vorhersagen oben sind so formuliert, dass er sie widerlegen kann. Fällt bei einem der beiden Eingriffe **kein** Test oder ein **anderer** als der vorhergesagte, ist meine Analyse falsch und diese Freigabe gehört zurückgenommen.

---

**Beurteilt durch**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 2/3 → ✅ **APPROVED**
