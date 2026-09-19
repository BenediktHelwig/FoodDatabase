# UC10 Code-Phase Review (WP4 „MHD-Sichten für Produktinstanzen", UI) — Versuch 2/3

**Datum**: 19.09.2026
**Reviewer**: Review-Agent
**Phase**: Code-Review WP4 UC10
**Versuch**: 2/3
**Prüfgegenstand**: Commit `38d7bda` (Nachbesserung), im Gesamtbild `git diff master..HEAD`, Branch `feat/ui-produktinstanzen-uc10`
**Maßstab**: `reviews/uc10-wp4-code-review-1.md` (M1–M6, L1–L6)
**Urteil**: ✅ **APPROVED**

---

## 📊 Verifikation (selbst ausgeführt)

```
dotnet build FoodDatabase.sln                   → 0 Fehler (132 Warnungen, alle vorbestehend)
dotnet test src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 361, gesamt: 361, Dauer: 790 ms
git status --porcelain                          → leer (Review hat nichts verändert)
```

Zusätzlich das Razor-Generat mit `-p:EmitCompilerGeneratedFiles=true --no-incremental` neu emittiert,
um M1 und M3 am erzeugten Code zu prüfen statt an der Quelle zu vermuten (Belege bei den jeweiligen
Befunden). Der erste Emit-Lauf lieferte ein veraltetes Generat aus dem inkrementellen Cache — erst
mit `--no-incremental` war es der aktuelle Stand. Wer das nachstellt, muss diesen Schritt mitgehen,
sonst prüft er den Commit vor der Nachbesserung.

**Umfang der Nachbesserung**: `22/-30` + `8/-32` Zeilen in den beiden Razor-Seiten, `28/-0` in der
neuen `src/App/Models/MhdTextFormatter.cs`. Netto **58 hinzugefügt gegen 62 entfernt** — die
Korrektur macht die Codebasis kleiner, obwohl eine Klasse dazukam. Keine Testdatei angefasst, kein
Service, kein Schema. Genau der Zuschnitt, der beauftragt war.

---

## Alte Befunde — einzeln

### M1 — HIGH · Datumsformat → ✅ **behoben**, Umfang über den Auftrag hinaus korrekt erweitert

Alle drei Stellen nutzen jetzt `.ToString("dd.MM.yyyy")`. Beweis aus dem neu erzeugten Generat: die
Ausdrücke stehen als echte Methodenaufrufe im Baum
(`LagerbestandBearbeiten…g.cs:398` / `:434`, `LebensmittelDetail…g.cs:829`), und

```
grep 'AddContent(…, ":D")' / ':d' über beide .g.cs   → kein Treffer
```

Die Literale sind restlos weg.

**Zur Scope-Erweiterung (vormals L6)**: mitgetragen und richtig entschieden. Es war derselbe Defekt
in derselben Tabelle; hätte der `dev-agent` nur Zeile 246 der Detailseite angefasst, stünden in der
Lagerbestandstabelle weiter zwei kaputte Zellen direkt neben einer reparierten. Die Mitnahme kostet
zwei Zeilen und beseitigt eine für den Nutzer sichtbare Kaputtheit — der Verzicht wäre hier die
teurere Entscheidung gewesen.

**Zur Formatwahl**: `dd.MM.yyyy` ist **besser als mein eigener Vorschlag `"D"`**, und das gehört
festgehalten. `Program.cs` konfiguriert **keine** Culture (`grep "Culture|Localization"` → kein
Treffer); `"D"` hätte also die OS-Culture des Servers gezogen und auf einem en-US-Host
„Monday, September 20, 2027" ausgegeben — in einer durchgehend deutschen Oberfläche. `dd.MM.yyyy`
ist culture-stabil (der Punkt ist in benutzerdefinierten Formatstrings ein Literal, kein
Separator-Platzhalter) und deckt sich mit dem, was das Repo ohnehin führt: `LagerortListe.razor:41`
und `LebensmittelDetail.razor:52`. Einheitlichkeit über beide Tabellen und über die Nachbarseiten
hinweg ist hergestellt. Dass `Einkaufsdatum` vorher `:d` (kurz) und `Verfallsdatum` `:D` (lang)
sein sollte, ging bei der Vereinheitlichung verloren — das ist gewollt und richtig so, beide Spalten
tragen jetzt dieselbe Schreibweise.

### M2 — MEDIUM · „Nur abgelaufene" zeigt heute ablaufende Packungen → ✅ **behoben**

`LagerbestandBearbeiten.razor:180-182` filtert den Service-Rückgabewert auf `< DateTime.Today` nach;
der Service bleibt bei seinem `<=` und ist unangetastet.

**Die Richtungsentscheidung trage ich mit, und zwar nicht nur widerspruchsfrei, sondern als die
sachlich richtige der beiden Varianten.** „Mindestens haltbar bis" heißt, dass die Packung am
MHD-Tag noch gut ist — eine Oberfläche, die sie an diesem Tag unter „abgelaufen" führt, würde zum
Wegwerfen genau dessen auffordern, das die App eigentlich verhindern soll. Das Umbenennen der Option
auf „inkl. heute" hätte den Widerspruch nur beschriftet.

**Die Grenze ist jetzt überall dieselbe** — nachgeprüft über die gesamte `src/App`:

| Stelle | Ausdruck |
|---|---|
| `LagerbestandBearbeiten.razor:76` (Färbung) | `Verfallsdatum < DateTime.Today` |
| `LagerbestandBearbeiten.razor:181` (Filter) | `Verfallsdatum < DateTime.Today` |
| `LebensmittelDetail.razor:230` (Färbung) | `Verfallsdatum < DateTime.Today` |

Damit gilt für jede Zeile im Filter „Nur abgelaufene": `tagesBisMhd < 0` → Text „vor N Tagen
abgelaufen" **und** `table-danger`. Das Abnahmekriterium des Plans (Zeile 185, „zeigt ausschließlich
Zeilen mit `table-danger`") ist erfüllt. Der Kommentar bei `:178-179` begründet das WHY, nicht das
WHAT — richtige Kommentarsorte an der richtigen Stelle.

Der Preis ist ein zusätzlicher In-Memory-Durchlauf über eine Liste, die ohnehin im Speicher liegt.
Vernachlässigbar.

### M3 — MEDIUM · Filterleiste verschwindet, Auswahl geht verloren → ✅ **behoben** (Kern), Restpunkt als Hinweis

Ich habe die von dir benannte Stolperstelle geprüft statt vermutet. **`value="@x"` an einem
`<select>` setzt in Blazor die Auswahl** — Beleg aus dem Generat:

```csharp
__builder.OpenElement(8, "select");
__builder.AddAttribute(9,  "id", "statusFilter");
__builder.AddAttribute(10, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnStatusFilterChanged));
__builder.AddAttribute(11, "value", statusFilter);
```

`value` landet als echtes Render-Tree-Attribut am `<select>`, nicht als toter HTML-String. Das ist
zeichengleich das, was `@bind="statusFilter"` erzeugt — `@bind` ist auf einem Select nichts anderes
als `value` + `onchange`. Der Blazor-Renderer behandelt `value` auf `INPUT`/`SELECT`/`TEXTAREA` als
Sonderfall und wendet den Wert bei einem `<select>` **verzögert** an, erst nachdem die
`<option>`-Kinder eingehängt sind; genau deshalb funktioniert es auch bei dem hier vorliegenden
Fall, dass der ganze Teilbaum neu erzeugt wird. Die App läuft interaktiv
(`App.razor:14` setzt `InteractiveServerRenderMode` global auf `<Routes>`, `Program.cs:57`), das
Sonderverhalten greift also. Im Prerender ist `statusFilter` noch `"all"` und `lagerortFilter` noch
`""` — beides die jeweils erste Option, kein sichtbarer Unterschied. **Der Kern des Mangels ist weg:
Der Nutzer sieht nicht mehr „Alle" über einer gefilterten Liste.**

Die Sackgasse im Fehlerfall ist ebenfalls weg: Bei gesetztem `fehler` ist `lädt` false, die Leiste
rendert, der Filter lässt sich zurücksetzen (`:19` gegen `:49`).

**Was bleibt** und warum es nicht blockiert: Die Leiste hängt weiter an `@if (!lädt)`, der Teilbaum
wird also bei jedem Filterwechsel und jedem Löschvorgang entfernt und neu erzeugt. Folge ist ein
kurzes Springen des Layouts und ein verlorener Tastaturfokus auf dem gerade bedienten Select. Das
ist Bedienkomfort, kein falscher Zustand und keine Sackgasse — die beiden Teile mit Nutzerwirkung
sind behoben. Geht als N2 in die Hinweise; das erneut blockierend zu machen wäre genau die
Formulierungsnuance, die ein Verdict nicht tragen soll.

### M4 — MEDIUM · Zwei Lagerort-Semantiken → ✅ **behoben**

`:188` lautet jetzt `string.Equals(p.Lagerort, lagerortFilter, StringComparison.Ordinal)`.

- **Eine Semantik**: Der Expired-Pfad vergleicht ordinal, der Alle-Pfad delegiert an
  `GetByLagerortAsync`, das intern `p.Lagerort == lagerort` prüft
  (`ProduktInstanzService.cs:117`) — ebenfalls ordinal. Derselbe Lagerort liefert jetzt in beiden
  Statusfiltern dieselbe Menge. Der Datensatz `"kühlschrank"` fällt in beiden Pfaden gleichermaßen
  heraus; das ist konsistent, und die verbleibende Ursache ist die dokumentierte Lagerort-Doppelung
  (L5), nicht dieser Code.
- **Null-Sicherheit**: Die statische Form `string.Equals(a, b, …)` wirft bei `a is null` nicht mehr.
  Das NRE-Risiko aus einer NULL-Spalte oder einer Testfixture ohne Lagerort ist damit weg, und der
  generische `catch` fängt keine „Object reference not set…"-Meldung mehr ein, die der Nutzer nicht
  deuten kann.

Die gewählte Richtung ist die, die sich dem Service anpasst statt ihn zu unterlaufen — richtig für
ein UI-only-Feature.

### M5 — MEDIUM · DRY, `GetMhdText` doppelt → ✅ **behoben**

`src/App/Models/MhdTextFormatter.cs`, `public static class`, eine Klasse pro Datei, beide Views
rufen `MhdTextFormatter.GetMhdText(tagesBisMhd)`. Die 16 duplizierten Zeilen sind in beiden Seiten
restlos entfernt, der Rumpf wurde unverändert übernommen — beide Seiten zeigen also garantiert
dieselben Texte. Gegenprobe über `src/App`: `GetMhdText` existiert genau einmal.

Den **file-scoped Namespace** nehme ich wie angekündigt nicht als Befund auf; das Repo führt beide
Formen, vier Bestandsdateien machen es genauso. XML-Doc ist vorhanden und sagt WHY/WAS-für-wen, nicht
das Offensichtliche.

Ein Rest bleibt, den ich in Versuch 1 mitgemeint hatte: Die Signatur nimmt `int tagesBisMhd` statt
`DateTime`, also steht `(instanz.Verfallsdatum - DateTime.Today).Days` weiterhin in beiden Views
(`:80` bzw. `:234`) plus ein drittes Mal im Service (`:220`). Das ist eine identische Einzeile, kein
Block, und beide Kopien sind zeichengleich — die Driftgefahr, die M5 begründet hat, ist auf ein
Minimum geschrumpft. Geht als N3 in die Hinweise, nicht in die Mängel.

### M6 — MEDIUM · `var` statt expliziter Typen → ✅ **behoben**

Alle fünf benannten Stellen sind explizit typisiert. Härtere Gegenprobe über den **gesamten**
Branch-Diff:

```
git diff master..HEAD -- src/App/Components/ src/App/Models/ | grep "^+" | grep -E "\bvar\b|== null|!= null"
   → kein Treffer
```

Keine einzige **hinzugefügte** Zeile des Branches enthält noch ein `var` oder ein `== null`/`!= null`.
Das Kriterium ist nicht punktuell abgearbeitet, sondern über den Commit hinweg sauber.

---

## Kleine Befunde aus Versuch 1

- **L1 (toter Lade-Zweig)** → ✅ umgesetzt, und zwar über die von mir bevorzugte Variante: ersatzlos
  gestrichen statt mit einem eigenen `lädtPackungen`-Feld erreichbar gemacht. Sieben Zeilen weg,
  keine Funktion verloren — KISS. Verloren geht nichts, weil der Zweig nachweislich unerreichbar war.
- **L2 (Fehler und Leerzustand gleichzeitig)** → ✅ umgesetzt. `else if (string.IsNullOrEmpty(fehlerPackungen))`
  (`LebensmittelDetail.razor:254`) verhindert, dass neben dem roten Alert noch „Keine Packungen
  vorhanden." steht. Zwei Aussagen, von denen eine falsch war, sind jetzt eine richtige.
- **L3 (Magic Strings)** → ⚠️ **fast** umgesetzt. `StatusFilterAll` / `StatusFilterExpired` sind
  eingeführt und an vier von fünf Stellen verwendet; `GetEmptyStateMessage()` (`:246`) vergleicht
  weiter gegen das Literal `"expired"`. Siehe N1 — LOW, kein Freigabe-Hindernis.
- **L4 (Redundanzen)** → bewusst nicht angefasst. **Trage ich mit.** Ich habe es in Versuch 1 selbst
  als „alles harmlos, alles Rauschen" eingestuft; es an derselben Stelle erneut aufzuwärmen wäre
  Geschmack, nicht Kriterium.
- **L5 (Lagerort-Doppelung)** → bewusst nicht angefasst, wird im Doku-Schritt festgehalten. **Trage
  ich mit**, es ist die ausdrückliche Entscheidung des Users (Plan, „Entscheidungen des Users",
  Punkt 2) und liegt außerhalb von UC10 (Plan, „Ausdrücklich nicht Teil von UC10"). **Aber es ist
  damit eine Bringschuld des Doku-Schritts, keine erledigte Sache**: Der Plan verlangt bei Schritt 4
  ausdrücklich, dass die Einschränkung benannt wird — „genau der Befundtyp M2 aus dem UC9-Review".
  Ich werde im `docs`-Review darauf prüfen; fehlt der Hinweis dort, wird er dort zum Mangel.

---

## Neue Befunde

**Keine, die die Freigabe blockieren.** Die drei gezielt geprüften Risiken der Nachbesserung sind
sauber:

- **Beide Seiten zeigen dieselben Texte** — eine gemeinsame Quelle, Rumpf unverändert übernommen.
- **Beide Seiten rechnen dieselben Tage** — `(Verfallsdatum - DateTime.Today).Days`, identisch zu
  `ProduktInstanzService.cs:220`, die Anzeige widerspricht dem Service also weiterhin nicht.
- **Beide Seiten färben nach derselben Grenze** und zeigen dasselbe Datumsformat.

Bei der Zentralisierung ist nichts verrutscht; die Testsuite bestätigt das mit unveränderten 361
grünen Tests, ohne dass eine Testdatei angefasst wurde.

---

## HINWEISE — kein Freigabe-Hindernis

- **N1 · `LagerbestandBearbeiten.razor:246` — die eine Stelle, die L3 übersehen hat.**
  `GetEmptyStateMessage()` vergleicht `statusFilter == "expired"` gegen das Literal, während die
  vier anderen Vergleiche jetzt `StatusFilterExpired` nutzen. Das Verhalten stimmt, weil die Werte
  zufällig gleich sind — aber wer die Konstante ändert, bricht still die Leerzustandsmeldung, und
  das ist exakt der Fehler, gegen den die Konstante eingeführt wurde. Ein-Wort-Korrektur, gern beim
  nächsten Anfassen der Datei.
- **N2 · Filterleiste flackert weiter** (`:19`, Rest von M3). `@if (!lädt)` entfernt den
  Select-Teilbaum bei jedem Filterwechsel und jedem Löschvorgang. Zustand und Anzeige bleiben dank
  `value` korrekt; verloren gehen Layoutruhe und der Tastaturfokus auf dem gerade bedienten Select.
  Die Bedingung ganz zu streichen wäre eine Ein-Zeilen-Verbesserung gewesen — nachholbar, nicht
  blockierend.
- **N3 · Rest von M5.** `GetMhdText(int)` statt `Formatiere(DateTime)` lässt die Tagesberechnung als
  Einzeiler in beiden Views stehen. Eine Überladung `Formatiere(DateTime verfallsdatum)` würde die
  Heute-Grenze endgültig an einen Ort binden.
- **N4 · Leerer Packungs-Karten-Körper im Fehlerfall.** Ist `fehlerPackungen` gesetzt, rendert die
  Karte „Packungen" mit Kopf, aber ohne Inhalt; der Alert steht darüber außerhalb der Karte
  (`LebensmittelDetail.razor:203-208`). Korrekt — nur eine wahre Aussage —, optisch etwas nackt.
- **N5 · Stilgefälle zwischen den beiden Tabellen.** Die Kopien in `LebensmittelDetail` sind jetzt
  explizit typisiert, die Originale in `LagerbestandBearbeiten:70-77` und `:217` tragen weiter `var`.
  Unveränderte Zeilen, also außerhalb dieses Reviews — nur: die beiden Tabellenkörper lesen sich nun
  unterschiedlich. Kandidat für das nächste Anfassen der Datei.
- **Für den `test-agent` (nicht für den `dev-agent`)**: Durch M2 zeigt die UI im Expired-Filter
  **weniger** als `GetVerfallenenAsync` liefert. Ein Mock, der eine heute ablaufende Packung
  zurückgibt, muss **null** Zeilen erwarten und die Leerzustandsmeldung „Keine Treffer für diesen
  Filter." Wer hier eine Zeile erwartet, schreibt den Test gegen den alten, widersprüchlichen Stand.
  Ebenso: Der Lagerortvergleich ist jetzt case-**sensitiv** in beiden Pfaden.

---

## Checkliste Code-Phase Review

- [x] Tests ALLE GRÜN: 361/361 (selbst ausgeführt)
- [x] Build: 0 Fehler
- [x] **Code-Style-Standards (explizite Typen)** — kein `var` in einer hinzugefügten Zeile des Branches
- [x] Null-Prüfungen `is null`/`is not null`: kein `== null`/`!= null` im neuen Code
- [x] Datei-Struktur: eine Klasse pro Datei (`MhdTextFormatter.cs`)
- [x] **Clean Code / DRY** — `GetMhdText` existiert genau einmal
- [x] KISS: Nachbesserung ist netto kleiner als der Ausgangsstand, keine neuen Abstraktionen
- [x] **Fehler-/Randfälle**: Heute-Grenze überall gleich, keine Filter-Sackgasse, kein NRE-Risiko
- [x] Fehlertrennung Packungen/Nährwert/Lebensmittel korrekt, Leerzustand nicht mehr widersprüchlich
- [x] Performance: kein N+1, ein Service-Aufruf je Ladevorgang, Nachfilter in-memory
- [x] Sortierung auf allen drei Filterpfaden
- [x] Security: kein XSS (Blazor escaped), keine Injection, keine Secrets, keine neue Eingabefläche
- [x] **Darstellung**: Datumsformat am Generat verifiziert, repo-einheitlich, culture-stabil
- [x] Bestehendes Verhalten unverändert (`/lagerbestand` ohne Filter, Lösch-Dialog)
- [x] Diff-Hygiene: keine Formatdrift, keine gekippten Zeilenenden (die Razor-Dateien bleiben CRLF,
      die neue `.cs` ist LF wie acht der zehn Dateien in `src/App/Models/`), keine
      Trailing-Whitespace, Diff-Größe der Mängelliste angemessen
- [x] Kein Service, kein Schema, keine Migration, keine Testdatei angefasst
- [x] Review hat keine Datei verändert (`git status` leer)

---

## Fazit

Sechs von sechs Mängeln sind **in der Substanz** behoben, nicht kosmetisch überdeckt — geprüft am
Verhalten, bei M1 und M3 am erzeugten Code, nicht an der guten Absicht der Commit-Message. Die
beiden Entscheidungen des Orchestrators halten der Prüfung stand: Die Scope-Erweiterung bei M1 war
die billigere und ehrlichere Variante, und die Richtungswahl bei M2 ist die inhaltlich richtige —
„mindestens haltbar bis" schließt den MHD-Tag ein, und die Grenze ist nun an allen drei Stellen
dieselbe.

Bemerkenswert bleibt das Größenverhältnis: 58 hinzugefügte gegen 62 entfernte Zeilen, inklusive
einer neuen Klasse. Eine Nachbesserung, die die Codebasis verkleinert, hat an den Ursachen gearbeitet
und nicht Sonderfälle drübergelegt. Auch L1 bis L3 wurden mitgenommen, ohne den Diff aufzublähen.

Offen bleiben fünf Kleinigkeiten (N1–N5), von denen keine ein falsches Verhalten erzeugt: ein
vergessenes Literal, ein Layout-Flackern, ein Einzeiler-Rest von DRY, eine nackte Karte und ein
Stilgefälle in unverändertem Code. Dafür einen dritten Versuch zu verbrauchen, wäre schlecht
gewirtschaftet — der letzte Versuch soll für etwas verfügbar bleiben, das den Nutzer trifft.

**Nächster Schritt**: Freigabe der Implementierung. Der `test-agent` kann ansetzen; die beiden
Punkte unter „Für den `test-agent`" gehören in seinen Delegationsprompt, weil M2 und M4 die
erwarteten Testaussagen verändert haben. Die Tests müssen hier **grün** sein (UI-Ausnahme von TDD).
Für den späteren `docs`-Schritt vorgemerkt: der Lagerort-Hinweis aus L5 ist dort eine Bringschuld
und wird im `docs`-Review geprüft.

Die Checkliste im Feature-Plan kann bei „Implementierung freigegeben (review-agent, Modus `code`)"
abgehakt werden.

---

**Beurteilt durch**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 2/3 → ✅ **APPROVED**
