# UC10 Code-Phase Review (WP4 „MHD-Sichten für Produktinstanzen", UI) — Versuch 1/3

**Datum**: 2026-09-19
**Reviewer**: Review-Agent
**Phase**: Code-Review WP4 UC10
**Versuch**: 1/3
**Prüfgegenstand**: Commit `fbaff43`, Branch `feat/ui-produktinstanzen-uc10` (`git diff master..HEAD`)
**Urteil**: ❌ **CHANGES REQUESTED**

---

## 📊 Verifikation (selbst ausgeführt)

```
dotnet build src/App/FoodDatabase.App.csproj   → 0 Fehler
dotnet test  src/Tests/FoodDatabase.Tests.csproj
   Bestanden! Fehler: 0, erfolgreich: 361, gesamt: 361, Dauer: 798 ms
git status --porcelain                          → leer (Review hat nichts verändert)
```

Zusätzlich: Razor-Generat mit `-p:EmitCompilerGeneratedFiles=true` in den Scratchpad emittiert, um
Befund M1 zu beweisen statt zu behaupten (siehe dort).

**Umfang**: `109/-2` Zeilen `LagerbestandBearbeiten.razor`, `102/-0` Zeilen `LebensmittelDetail.razor`,
`90/-0` Zeilen `LebensmittelDetailTests.cs`.

---

## ✅ Was stimmt

- **Service-Locator-Rückfall ist sauber aufgelöst.** `LebensmittelDetail.razor:6` nutzt reguläres
  `@inject IProduktInstanzService ProduktInstanzService`; `grep -rn "IServiceProvider\|GetService("
  src/App/Components/` liefert keinen Treffer. Bei fehlender Registrierung schlägt das Rendern jetzt
  laut fehl, statt die Packungsliste stillschweigend zu verschlucken. Befund erledigt.
- **Kein N+1.** „Tage bis MHD" wird in beiden Tabellen aus dem geladenen Objekt gerechnet
  (`(Verfallsdatum - DateTime.Today).Days`), nicht über `GetTagesBisVerfallAsync(id)`. Die Formel ist
  identisch mit `ProduktInstanzService.cs:220` — die Anzeige widerspricht dem Service also nicht.
- **Ein Service-Aufruf pro Ladevorgang** in allen vier Filterzuständen; der zweite Filter wirkt als
  In-Memory-Nachfilter, wie im Plan vorgesehen.
- **Sortierung**: Alle drei Pfade in `AlleAbrufen()` (`LagerbestandBearbeiten.razor:170-202`) liefern
  nach `Verfallsdatum` aufsteigend — die beiden `Where`-Pfade explizit per `OrderBy`, der dritte über
  `GetNachVerfallsdatumSortiertAsync()`. Der Fallstrick aus Plan-Zeile 85 ist **nicht** eingetreten.
  Auch `LebensmittelDetail.razor:326-328` sortiert nach. Selbst nachgeprüft, Pfad für Pfad.
- **Bestehendes Verhalten**: `/lagerbestand` ohne Filter ruft unverändert
  `GetNachVerfallsdatumSortiertAsync()`; Lösch-Dialog, `zuLöschendeId`, `LöschenBestätigt()` sind
  unangetastet und laden nach dem Löschen mit den aktiven Filtern neu (richtig so).
- **Lagerort-Dropdown** iteriert `LagerortKonstanten.AlleWerte` statt zu hartcodieren (Plan-Vorgabe).
- **`data-testid`** auf beiden Filter-Selects, der Packungstabelle, jeder Packungszeile, jedem
  Bearbeiten-Link und dem Packungs-Fehler-Alert. Kein `@rendermode`. Keine `== null`/`!= null`.
- **Testdatei**: 15 `[Fact]`, 15 hinzugefügte Mock-Blöcke, **0 gelöschte Zeilen**. Keine Assertion
  geändert, kein Testfall entschärft — der Auftrag ist exakt eingehalten.

---

## ❌ MÄNGEL — blockieren die Freigabe

### M1 — HIGH · `src/App/Components/Pages/Lebensmittel/LebensmittelDetail.razor:246`

```razor
<td>@instanz.Verfallsdatum:D</td>
```

Razor-Implizit-Ausdrücke kennen **keinen** Format-Specifier. Das `:D` ist Literaltext. Beweis aus dem
generierten Code (`Components_Pages_Lebensmittel_LebensmittelDetail_razor.g.cs`):

```csharp
__builder.AddContent(261, ...);
instanz.Verfallsdatum
__builder.AddContent(262, ":D");
```

Die Zelle zeigt also `20.09.2027 00:00:00:D` — Uhrzeit inklusive, Doppelpunkt-D hintendran. In der
Packungsliste ist das neu eingeführt; ein Nutzer sieht die kaputte Ausgabe auf jeder Detailseite mit
Packungen.

**Korrektur**: `<td>@instanz.Verfallsdatum.ToString("D")</td>` (oder `@(...)`-Klammerform).

*(Dieselbe Stelle existiert vorbestehend in `LagerbestandBearbeiten.razor:86` und `:89` — siehe
HINWEISE, nicht Teil dieses Mangels.)*

### M2 — MEDIUM · `LagerbestandBearbeiten.razor:170-187` (+ `:76`, `:242-245`)

**„Nur abgelaufene" zeigt Packungen, die die Seite selbst als nicht abgelaufen darstellt.**

`GetVerfallenenAsync` filtert `p.Verfallsdatum <= referenzdatum` (`ProduktInstanzService.cs:146`) —
**heute** ablaufende Packungen sind eingeschlossen. Die Zeilenfärbung nutzt aber
`Verfallsdatum < DateTime.Today` (`:76`) und `GetMhdText(0)` liefert „Heute ablaufend". Ergebnis: Im
Filter „Nur abgelaufene" steht eine weiße, nicht `table-danger`-gefärbte Zeile mit dem Text „Heute
ablaufend". Das widerspricht dem Filterlabel, der Färbung **und** dem Abnahmekriterium des Plans
(Zeile 185: „„Nur abgelaufene" zeigt ausschließlich Zeilen mit `table-danger`").

**Korrektur** (UI-only, Service bleibt unangetastet, `table-danger`-Semantik bleibt wie vorgegeben
unverändert): im Expired-Pfad auf dieselbe Grenze nachfiltern, die die Seite anzeigt:

```csharp
abgerufeneBestaende = abgerufeneBestaende
    .Where(p => p.Verfallsdatum < DateTime.Today)
    .ToList();
```

Alternative, falls die Service-Definition Vorrang haben soll: Option umbenennen auf „Abgelaufen
(inkl. heute)". Eine der beiden Varianten muss gewählt werden — der jetzige Zustand ist in sich
widersprüchlich.

### M3 — MEDIUM · `LagerbestandBearbeiten.razor:19-40`

**Die Filterleiste verschwindet, und die Auswahl geht dabei sichtbar verloren.**

Zwei Teilprobleme mit einer gemeinsamen Ursache:

1. Die Leiste hängt an `@if (!lädt && string.IsNullOrEmpty(fehler))`. `AlleAbrufen()` setzt
   `lädt = true` vor dem `await`; bei einem echten (nicht synchron abgeschlossenen) Service-Aufruf
   rendert Blazor dazwischen — der `<select>`-Teilbaum wird entfernt und danach neu erzeugt.
2. Die beiden `<select>` tragen **kein** `value`-Attribut, nur `@onchange`. Das neu erzeugte Element
   rendert daher wieder mit `Alle` / `Alle Lagerorte` als angezeigtem Wert, während `statusFilter`
   im Zustand weiter `"expired"` ist. Der Nutzer sieht „Alle", bekommt aber die gefilterte Liste.
   Dasselbe nach jedem Löschvorgang (`LöschenBestätigt` → `lädt = true` → `AlleAbrufen`).

Zusätzlich: Im Fehlerfall (`fehler` gesetzt) verschwindet die Leiste vollständig — der Nutzer kann
den Filter, der den Fehler ausgelöst hat, nicht mehr zurücksetzen. Einziger Ausweg ist ein
Seiten-Reload. Eine Sackgasse in der neu gebauten Funktion.

**Korrektur**: `value="@statusFilter"` bzw. `value="@lagerortFilter"` an die Selects, und die
Bedingung auf die Leiste entfernen (oder auf `string.IsNullOrEmpty(fehler)` reduzieren), damit
Auswahl und Bedienbarkeit über Laden und Fehler hinweg erhalten bleiben.

**Hinweis für den test-agent-Schritt**: In bUnit laufen die gemockten Tasks synchron durch, ein
Zwischenrender findet dort meist nicht statt. Dieser Mangel wird von Tests voraussichtlich **nicht**
gefangen — er muss jetzt im Code behoben werden, nicht später über eine rote Testfarbe.

### M4 — MEDIUM · `LagerbestandBearbeiten.razor:179` gegen `:191`

**Derselbe Lagerort bedeutet je nach Statusfilter etwas anderes.**

- Expired-Pfad: In-Memory-Vergleich mit `StringComparison.OrdinalIgnoreCase`.
- Alle-Pfad: delegiert an `GetByLagerortAsync`, das intern `p.Lagerort == lagerort` prüft
  (`ProduktInstanzService.cs:117`) — **ordinal, case-sensitiv**.

Das ist kein theoretischer Unterschied: `LagerortKonstanten.IsValidLagerort` validiert
case-insensitive (`LagerortKonstanten.cs:37`), `CreateAsync` speichert den übergebenen String
unverändert. Ein über API/Seed angelegter Datensatz mit `"kühlschrank"` taucht damit unter
„Nur abgelaufene + Kühlschrank" auf, unter „Alle + Kühlschrank" aber nicht. Zwei Pfade derselben
Bedienhandlung, zwei Ergebnismengen.

Zweitens ist `p.Lagerort.Equals(...)` nicht null-sicher. `ProduktInstanz.Lagerort` ist ein
`string` ohne Initialisierer; bei `Nullable=enable` warnt der Compiler nicht, zur Laufzeit (EF-Materialisierung
einer NULL-Spalte, Testfixture ohne gesetzten Lagerort) fliegt eine `NullReferenceException` mitten
in den generischen `catch`-Block und der Nutzer sieht „Fehler beim Abrufen des Lagerbestands:
Object reference not set…".

**Korrektur**: Vergleich vereinheitlichen und null-sicher machen —
`.Where(p => string.Equals(p.Lagerort, lagerortFilter, StringComparison.Ordinal))` passt zum
Service-Verhalten; wenn bewusst case-insensitive gewünscht ist, gehört das in beiden Pfaden gleich
(dann als bewusste Abweichung im Kommentar begründen). Entscheidend ist: **eine** Semantik.

### M5 — MEDIUM · `LagerbestandBearbeiten.razor:235-250` und `LebensmittelDetail.razor:337-352`

**`GetMhdText` ist zeichengleich in zwei Dateien dupliziert** (16 Zeilen), dazu die Tagesberechnung
`(instanz.Verfallsdatum - DateTime.Today).Days` in beiden Views — und ein drittes Mal im Service
(`ProduktInstanzService.cs:220`). DRY ist in `CLAUDE.md` ein explizites Review-Kriterium. Die Kopie
ist nicht akademisch: Sobald jemand die Wortwahl oder die Heute-Grenze an einer Stelle anpasst
(siehe M2!), driften die beiden Seiten auseinander.

**Korrektur**: In eine gemeinsame statische Hilfsklasse ziehen, eine Klasse pro Datei, z. B.
`src/App/Models/MhdTextFormatter.cs` mit `public static string Formatiere(DateTime verfallsdatum)`.
Beide Views rufen sie auf; die Tagesberechnung lebt dann ebenfalls nur noch einmal im UI.

### M6 — MEDIUM · Code-Style `CLAUDE.md` (erstes Kriterium) — `var` statt expliziter Typen in neuem Code

Alle folgenden Stellen sind **neu hinzugefügt**, nicht vorbestehend, und haben keine Typ-Variabilität:

| Datei:Zeile | Ist | Soll |
|---|---|---|
| `LagerbestandBearbeiten.razor:33` | `@foreach (var lagerort in …AlleWerte)` | `string lagerort` |
| `LebensmittelDetail.razor:235` | `@foreach (var instanz in packungen)` | `ProduktInstanz instanz` |
| `LebensmittelDetail.razor:237` | `var istAbgelaufen = …` | `bool istAbgelaufen` |
| `LebensmittelDetail.razor:238` | `var verfallKlasse = …` | `string verfallKlasse` |
| `LebensmittelDetail.razor:324` | `var abgerufenePackungen = await …GetByLebensmittelAsync(Id)` | `List<ProduktInstanz> abgerufenePackungen` |

Positiv zum Vergleich: `LagerbestandBearbeiten.razor:80` (`int tagesBisMhd`), `:81`
(`string mhdText`), `:168` (`List<ProduktInstanz> abgerufeneBestaende = new()`) und
`LebensmittelDetail.razor:241-242` sind korrekt explizit. Die Verstöße sind Kopien aus dem alten
Tabellenkörper — sie wandern durch das Copy-Paste aber in neuen Code und zählen damit.

---

## HINWEISE — kein Freigabe-Hindernis

- **L1 · Toter Lade-Zweig.** `LebensmittelDetail.razor:215-221` („Lädt Packungen…") ist
  unerreichbar: Der äußere `@if (lädt)` auf `:10` schließt den gesamten `else`-Block aus, und
  `lädt` steht beim Laden der Packungen ohnehin schon auf `false` (`:306`, `finally` läuft vor dem
  Packungs-Abruf). Umgekehrt zeigt die Karte in der Zwischenzeit kurz „Keine Packungen vorhanden."
  für Lebensmittel, die welche haben. Beides spiegelt exakt den vorhandenen Nährwert-Block
  (`:70-76`), ist also **konsistent mit dem Nachbarn** — deshalb LOW. Wenn der Zweig bleibt, sollte
  wenigstens ein eigenes `lädtPackungen`-Feld ihn erreichbar machen; sonst ersatzlos streichen
  (KISS: drei Zeilen ohne Funktion).
- **L2 · Fehler und Leerzustand gleichzeitig.** Schlägt `GetByLebensmittelAsync` fehl, zeigt die
  Seite den roten Alert **und** darunter „Keine Packungen vorhanden." — zwei Aussagen, eine davon
  falsch. Ebenfalls dem Nährwert-Block nachempfunden. Sauberer wäre, den Leerzustand nur bei
  `string.IsNullOrEmpty(fehlerPackungen)` zu rendern. Wichtig: Die Trennung selbst ist **richtig
  umgesetzt** — ein Fehler beim Packungsladen verdeckt weder das Lebensmittel noch den Nährwert
  (`LebensmittelDetail.razor:321-333`, eigener try/catch, eigenes Feld). Die Plan-Vorgabe ist
  erfüllt.
- **L3 · Magic Strings.** `"all"` / `"expired"` stehen an vier Stellen
  (`:25-26`, `:170`, `:225`, `:254`) als Literale, dazu englisch in einer sonst deutschen Oberfläche.
  Eine kleine `enum` oder zwei `const string` würden das Kriterium „keine Magic Strings" erfüllen
  und Tippfehler ausschließen.
- **L4 · Redundanzen.** `List<ProduktInstanz> abgerufeneBestaende = new();` (`:168`) wird in jedem
  Zweig überschrieben; `bestaende is not null` (`:206`) kann nach der Zuweisung nicht mehr null sein.
  `@lebensmittel?.Einheit` (`LebensmittelDetail.razor:245`) steht in einem Zweig, in dem
  `lebensmittel` garantiert nicht null ist. Alles harmlos, alles Rauschen.
- **L5 · Lagerort-Doppelung (wie beauftragt LOW).** Der Filter erreicht die frei angelegten
  UC9-`Lagerort`-Einträge nicht, weil `GetByLagerortAsync` gegen `LagerortKonstanten` validiert. Als
  bewusste Entscheidung akzeptiert. Anmerkung für den Doku-Schritt: Aus der Oberfläche selbst ist
  diese Einschränkung nicht erkennbar — ein Nutzer, der unter `/lagerorte` „Keller" angelegt hat,
  sucht ihn im Dropdown vergeblich, ohne zu erfahren warum. Ein kurzer Hinweistext unter der
  Filterleiste wäre die billigste Abhilfe; zwingend ist nur die Dokumentation.
- **L6 · Vorbestehend, außerhalb dieses Reviews:** `LagerbestandBearbeiten.razor:86` und `:89`
  tragen denselben Defekt wie M1 (`@instanz.Verfallsdatum:D`, `@instanz.Einkaufsdatum:d` → Literale
  „:D"/„:d" in der Ausgabe). Die Zeilen sind unverändert und damit nicht Gegenstand dieses Commits.
  Da der `dev-agent` für M1 ohnehin in dieselbe Konstruktion greift, wäre die Mitnahme naheliegend —
  das ist eine Scope-Entscheidung des Orchestrators, kein Review-Befund.

---

## Checkliste Code-Phase Review

- [x] Tests ALLE GRÜN: 361/361 (selbst ausgeführt)
- [x] Build: 0 Fehler
- [ ] **Code-Style-Standards (explizite Typen)** → M6
- [x] Null-Prüfungen `is null`/`is not null`: kein `== null` im neuen Code
- [x] Datei-Struktur: keine neuen Klassen, keine Verstöße
- [ ] **Clean Code / DRY** → M5
- [x] KISS: keine Over-Engineering, keine überflüssigen Abstraktionen
- [ ] **Fehler-/Randfälle** → M2 (heute ablaufend), M3 (Sackgasse im Fehlerfall), M4 (NRE-Risiko)
- [x] Fehlertrennung Packungen/Nährwert/Lebensmittel korrekt
- [x] Performance: kein N+1, ein Service-Aufruf je Ladevorgang
- [x] Sortierung auf allen Filterpfaden
- [x] Security: kein XSS (Blazor escaped), keine Injection, keine Secrets
- [ ] **Darstellung** → M1 (Datumsformat)
- [x] Bestehendes Verhalten unverändert (`/lagerbestand` ohne Filter, Lösch-Dialog)
- [x] Testdatei rein additiv, keine Assertion geändert, kein Testfall entschärft
- [x] Review hat keine Datei verändert (`git status` leer)

---

## Fazit

Die Substanz stimmt: Der Service-Locator ist sauber ersetzt, die Sortierfalle ist auf allen drei
Pfaden vermieden, N+1 ist vermieden, die Fehlertrennung auf der Detailseite folgt dem richtigen
Vorbild, und an der Testdatei wurde exakt das getan, was beauftragt war. Das ist ein tragfähiges
Fundament — die Mängel sind Korrektionen an der Oberfläche dieses Fundaments, keine Neukonstruktion.

Freigabe scheitert an sechs Punkten, davon drei mit Nutzerwirkung (M1 kaputtes Datum, M2
widersprüchlicher „abgelaufen"-Begriff, M3 Filter-Sackgasse), einem Konsistenzbruch (M4) und zwei
Standardverstößen (M5 DRY, M6 explizite Typen). Alle sechs sind lokal und ohne Service-Änderung
behebbar; der Aufwand liegt bei einer überschaubaren Nachbesserung, nicht bei einem Neuanlauf.

**Nächster Schritt**: Mängelliste M1–M6 zurück an den `dev-agent` (Versuch 2/3). Danach erneutes
Code-Review; der `test-agent` sollte erst nach der Freigabe ansetzen, weil M2 und M3 die erwarteten
Testaussagen verändern.

---

**Beurteilt durch**: Review-Agent
**Datum**: 19.09.2026
**Versuch**: 1/3 → ❌ **CHANGES REQUESTED**
