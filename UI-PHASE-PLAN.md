# Plan: UI-Phase FoodDatabase (UC5-Fix → Foundation → Blazor-UI)

## Kontext

Alle 10 Use-Cases sind auf Service-Ebene fertig (269 Tests, 258 grün). Laut CONTEXT_SUMMARY folgt jetzt die **Blazor-UI-Phase** + Integrationstests + UC5-Fix. Die Exploration ergab zwei kritische Fakten, die der Plan zuerst löst:

1. **Es existiert keinerlei Blazor-Infrastruktur** — `Program.cs` ist ein 33-Zeilen-Minimal-API-Stub, kein App.razor/Layout/wwwroot.
2. **Es existiert keine konkrete `IRepository<T>`-Implementierung** — alle Services liefen bisher nur gegen Moq-Mocks. Ohne EF-Repository + DI-Registrierung startet keine UI-Seite.

**Executor ist Claude Haiku**: Jeder Schritt ist explizit, klein, mit Verifikationskommando + erwartetem Ergebnis. Keine impliziten Annahmen — wo ein Interface-Detail gebraucht wird, steht "Datei X vorher lesen". Der Orchestrator delegiert die Arbeitspakete (WP) an die benannten Agenten.

**User-Entscheidungen (fix)**:
- UI-Tests: **nur bUnit** (v1.30.0, bereits referenziert), kein Playwright/E2E.
- Reihenfolge: **Implementierung zuerst, bUnit-Tests danach** (bewusste, vom User genehmigte Abweichung vom TDD-Workflow in CLAUDE.md — nur für die UI-Phase).
- **UC5-Fix ist WP1** (saubere Test-Baseline vor der UI).

**Verifizierte Basis-Fakten (nicht neu prüfen)**:
- Branch-Basis ist `master` (kein `develop` vorhanden).
- Kein Service ruft selbst `SaveChangesAsync()` auf (einzige Ausnahme: `LagerortService`, nutzt `FoodDatabaseContext` direkt) → **EfRepository muss in jeder Mutations-Methode sofort speichern**.
- `RezeptService.InvokeCreateAsync` (`RezeptService.cs:189`) ruft `CreateAsync` per Reflection auf → EfRepository braucht funktionierendes `CreateAsync`, das die Entity mit generierter ID zurückgibt.
- `Rezept`/`RezeptZutat` sind **keine** DbSets in `FoodDatabaseContext` → Migration nötig (WP2).
- `IRezeptRepository`/`IRezeptZutatRepository` werden von keinem Service benutzt → **nicht implementieren** (im Commit als bewusst offen dokumentieren).
- Code-Standards (verbindlich): `is null`/`is not null`, explizite Typen statt `var`, eine Klasse/Interface pro Datei, XML-Docs auf public Members, deutsche Domänensprache.

## Reihenfolge & Abhängigkeiten

```
WP1 (UC5-Fix) → WP2 (Foundation) → WP3 (UC1+UC3 UI) → WP4 (UC2/6/9/10 UI) → WP5 (UC4+UC5 UI) → WP6 (UC7+UC8 UI) → WP7 (Abschluss)
```
WP3–WP6 sequenziell: jedes WP kopiert die UI-/bUnit-Muster des vorherigen. Pro WP: eigener Branch, Conventional Commits, Review-Agent-Loop (max. 3), Doc-Agent-4-Teil-Check, MR gegen `master`.

---

## WP1: UC5 Rote Tests fixen

**Agenten**: Dev-Agent (Diagnose + Fix) → Review-Agent → Doc-Agent. **Branch**: `feat/uc5-testfix`

1. **Ist-Zustand erfassen (PFLICHT vor jeder Änderung)**:
   ```
   dotnet test src/Tests/FoodDatabase.Tests.csproj --logger "console;verbosity=detailed"
   ```
   Erwartung: 11 Failed. **Alle 11 Testnamen + Assertion-Meldungen wörtlich** in `reviews/uc5-fix-diagnose.md` notieren. Der dokumentierten Vermutung "Navigation-Property-Mock" NICHT blind folgen — die 3 benannten Tests (`RezeptNährwertServiceTests.cs`, `CalculateNährwerteFromZutaten_WithSingleZutat_CalculateCorrectly` u. a., Zeilen ~181/217/268) sehen gegen die aktuelle Implementierung erfüllbar aus; die übrigen ~8 liegen vermutlich in `NährwertCalculatorTests.cs`. Erst echte Fehlermeldungen lesen, dann diagnostizieren.
2. **Fix-Regel**: Tests sind die Spezifikation → Fix gehört in den Produktivcode (`src/App/Services/Classes/RezeptNährwertService.cs`, ggf. `NährwertCalculator.cs`). Ein Test darf NUR geändert werden, wenn er nachweislich der Spezifikation (`docs/features/UC5-Naehrwertberechnung.html` bzw. UC5-Feature-HTML) widerspricht — Begründung in den Commit-Text.
3. **Verifikation**:
   ```
   dotnet test src/Tests/FoodDatabase.Tests.csproj
   ```
   Erwartung: **269 passed / 0 failed** (keine Regression).
4. Commit `fix(uc5): Repariere Nährwertberechnung – 269/269 Tests grün` → Review-Agent (`reviews/uc5-fix-code-review-1.md`) → Doc-Agent-4-Teil-Check → MR.

**Done**: 269/269 grün; `reviews/uc5-fix-diagnose.md` existiert; MR erstellt.

---

## WP2: Foundation — EfRepository, Migration, DI, Blazor-Gerüst

**Agenten**: Dev-Agent (2a–2d) → Test-Agent (2e, nach Implementierung) → Review-Agent → Doc-Agent. **Branch**: `feat/blazor-foundation`
**Nach JEDEM Teilschritt**: `dotnet build FoodDatabase.sln` → 0 Fehler, dann `dotnet test` → alle grün.

### 2a. EfRepository<T> — neu: `src/App/Data/Repositories/EfRepository.cs`

`public class EfRepository<T> : IRepository<T> where T : class`, Konstruktor nimmt `FoodDatabaseContext`.
**Kritisch: jede Mutations-Methode speichert sofort** (Services rufen nie selbst SaveChanges):
- `GetByIdAsync(int id)` → `await _context.Set<T>().FindAsync(id)` (Nullrückgabe mit `!` wegen Nullable; Services prüfen selbst)
- `GetAllAsync()` → `await _context.Set<T>().ToListAsync()`
- `AddAsync(entity)` → `_context.Set<T>().Add(entity)` + `await _context.SaveChangesAsync()` + return entity
- `CreateAsync(entity)` → `return await AddAsync(entity);`
- `UpdateAsync(entity)` → `Update` + Save + return
- `DeleteAsync(int id)` → `FindAsync`; `is null` → `false`; sonst `Remove` + Save + `true`
- `SaveChangesAsync()` → `_context.SaveChangesAsync()`

### 2b. Rezept-DbSets + Migration — Änderung: `src/App/Data/FoodDatabaseContext.cs`

1. DbSets ergänzen: `DbSet<Rezept> Rezepte`, `DbSet<RezeptZutat> RezeptZutaten`.
2. `OnModelCreating` ergänzen:
   - `Rezept`: `HasIndex(r => r.Name).IsUnique()`
   - `RezeptZutat → Rezept`: FK `RezeptId`, `DeleteBehavior.Cascade`, `WithMany(r => r.Zutaten)`
   - `RezeptZutat → LebensmittelKatalog`: FK `LebensmittelId`, `DeleteBehavior.Restrict`
   - `HasIndex(z => new { z.RezeptId, z.Position }).IsUnique()`
   - **AutoIncludes** (generisches Repo lädt sonst keine Navigation-Props): `ProduktInstanz.LebensmittelKatalog`, `RezeptZutat.Lebensmittel`, `LebensmittelKatalog.Nährwert` — je `modelBuilder.Entity<X>().Navigation(x => x.Y).AutoInclude();`. **NICHT** `RezeptZutat.Rezept` oder `Rezept.Zutaten` (Zyklus!).
   - **Merken für UI-WPs**: `GetByIdAsync` nutzt `FindAsync` → lädt KEINE Includes. Detailseiten laden Zutaten über `IRezeptZutatService.GetZutatenAsync(rezeptId)`.
3. **DB-Backup + Migration**:
   ```
   Copy-Item src/App/fooddatabase.db src/App/fooddatabase.db.bak
   dotnet ef migrations add AddRezeptUndRezeptZutat --project src/App/FoodDatabase.App.csproj
   ```
   (falls `dotnet ef` fehlt: `dotnet tool install --global dotnet-ef --version 8.0.*`)
   **Migration vor Anwendung inspizieren**: darf NUR `CreateTable` für Rezepte/RezeptZutaten + Indizes enthalten. Enthält sie Drop/Alter auf bestehenden Tabellen → STOPP, Modellkonfiguration prüfen, nicht anwenden.
   ```
   dotnet ef database update --project src/App/FoodDatabase.App.csproj
   dotnet test src/Tests/FoodDatabase.Tests.csproj    # Erwartung: weiterhin 269/269
   ```

### 2c. Program.cs-Rewrite — `src/App/Program.cs`

Vor `builder.Build()`:
```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddSingleton(TimeProvider.System);   // UC7 (VerfallsdatumWarnungService)
// 13 Service-Registrierungen, alle AddScoped<Interface, Klasse>:
// ILebensmittelService, ILagerbestandService, INährwertService, IRezeptService,
// IRezeptZutatService, IEinheitConverter, INährwertCalculator, IRezeptNährwertService,
// IVerbrauchAusbuchangService, IVerfallsdatumWarnungService, IEinkaufslistenService,
// ILagerortService, IProduktInstanzService
```
Pipeline: bestehende Blöcke behalten, ergänzen: `app.UseAntiforgery();` (nach `UseStaticFiles`), dann `app.MapRazorComponents<App>().AddInteractiveServerRenderMode();`. Alten `MapGet("/")` **ersetzen** durch `app.MapGet("/health", () => Results.Ok("OK"));` (`/` wird Home.razor).

### 2d. Blazor-Server-Gerüst — neue Dateien

- `src/App/Components/App.razor` — HTML-Hülle: `<!DOCTYPE html>`, `<base href="/" />`, Links auf `bootstrap/bootstrap.min.css` + `app.css`, `<HeadOutlet />`, `<Routes @rendermode="InteractiveServer" />`, `<script src="_framework/blazor.web.js"></script>`
- `src/App/Components/Routes.razor` — `<Router AppAssembly="typeof(Program).Assembly">` + `RouteView DefaultLayout="typeof(MainLayout)"` + `FocusOnNavigate`
- `src/App/Components/_Imports.razor` — Standard-usings + `@using FoodDatabase.App.Components`, `.Components.Layout`, `.Models`, `.Services.Interfaces`, `.Services.Dtos`
- `src/App/Components/Layout/MainLayout.razor` + `NavMenu.razor` — Links: Start, Lebensmittel, Lagerbestand, Lagerorte, Rezepte, Warnungen, Einkaufsliste (Ziele entstehen ab WP3, Links jetzt schon anlegen)
- `src/App/Components/Pages/Home.razor` (`@page "/"`), `Pages/Error.razor` (`@page "/Error"`)
- **Bootstrap lokal beschaffen (kein Internet)** — aus dem Offline-SDK-Template kopieren:
  ```
  dotnet new blazor -o <scratchpad>/bz-template --interactivity Server
  Copy-Item -Recurse <scratchpad>/bz-template/wwwroot/bootstrap src/App/wwwroot/bootstrap
  Copy-Item <scratchpad>/bz-template/wwwroot/app.css src/App/wwwroot/app.css
  ```
- ⚠️ **`@rendermode` NUR am `<Routes>`-Element in App.razor — NIEMALS als Direktive in Seiten/Komponenten** (bricht sonst alle bUnit-Tests).

### 2e. Integrationstests EfRepository (Test-Agent, nach Implementierung)

**SQLite in-memory** (nicht EF-InMemory — echte Constraint-Treue für Unique-Indizes/Cascade/Restrict):
- `src/Tests/FoodDatabase.Tests.csproj`: `<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />` ergänzen.
- Neu `src/Tests/Integration/EfRepositoryTests.cs`: pro Test `new SqliteConnection("DataSource=:memory:")` + `Open()`, `DbContextOptionsBuilder<FoodDatabaseContext>().UseSqlite(connection)`, `context.Database.EnsureCreated()`. ~12 Tests: CRUD je Kernentität, `DeleteAsync` unbekannte ID → false, `AddAsync` persistiert sofort (zweiter Context auf derselben Connection sieht die Daten), Cascade Rezept→Zutaten, Restrict bei Lebensmittel-Löschung in Verwendung, AutoInclude liefert `ProduktInstanz.LebensmittelKatalog` bei `GetAllAsync`.

### Verifikation WP2 (Gesamt)
```
dotnet build FoodDatabase.sln                        # 0 Fehler
dotnet test src/Tests/FoodDatabase.Tests.csproj      # ~281/281 grün (269 + ~12 neue)
dotnet run --project src/App/FoodDatabase.App.csproj --urls http://localhost:5099   # Hintergrund
curl -s -o /dev/null -w "%{http_code}" http://localhost:5099/health   # → 200
curl -s http://localhost:5099/ | Select-String "NavMenu|nav"          # → HTML mit Navigation
```
**Done**: Build sauber, alle Tests grün, App startet gegen bestehende `fooddatabase.db`, Migration nur additiv. Getrennte Commits: `feat(foundation): Add EfRepository`, `feat(foundation): Add Rezept DbSets + migration`, `feat(foundation): Blazor Server scaffolding + DI`, `test(foundation): EfRepository integration tests`. Doc-Agent: 4-Teil-Check inkl. `diagrams/database-schema.drawio` (Rezept-Tabellen) + `architecture-components.drawio` (UI-Schicht).

---

## WP3–WP6: UI-Seiten (gemeinsames Muster)

**Muster pro WP**: Dev-Agent implementiert Seiten → Verifikation (build + App-Start + curl je Route → 200) → Test-Agent schreibt bUnit-Tests (Briefing unten ist PFLICHT) → `dotnet test` grün → Review-Agent → Doc-Agent-4-Teil-Check → MR.
**Alle Seiten**: explizite Typen, `data-testid`-Attribute auf interaktiven Elementen (bUnit-Selektoren), Lade-/Fehlerzustand nach Vorbild `src/App/Components/RezeptNährwertAnzeige.razor`.
**Regel für Haiku**: Vor Verwendung eines Service im UI **das Interface-File lesen** (exakte Signaturen/DTO-Felder), nichts aus dem Gedächtnis aufrufen.

### WP3: UC1+UC3 — Lebensmittel & Nährwerte (`feat/ui-lebensmittel`)
Neue Dateien unter `src/App/Components/Pages/Lebensmittel/`:
- `LebensmittelListe.razor` `@page "/lebensmittel"` — `ILebensmittelService.GetAllLebensmittelAsync()`; Suchfeld → `SearchLebensmittelAsync`; Löschen → `DeleteLebensmittelAsync` mit Bestätigung; Tabelle Name/Einheit/Kategorie; Links zu Detail/Neu.
- `LebensmittelForm.razor` `@page "/lebensmittel/neu"` + `@page "/lebensmittel/{Id:int}/bearbeiten"` — `EditForm` über `LebensmittelKatalog`; Create/Update; `ValidationException`/`ArgumentException` abfangen → Fehlermeldung; danach `NavigationManager.NavigateTo("/lebensmittel")`.
- `LebensmittelDetail.razor` `@page "/lebensmittel/{Id:int}"` — Detail + Nährwert-Formular über `INährwertService` (8 Felder pro 100g: Kalorien, Fett, GesättigteFettsäuren, Kohlenhydrate, Zucker, Protein, Ballaststoffe, Salz).

bUnit danach: `src/Tests/Ui/LebensmittelListeTests.cs` u. a. (~10 Tests: Liste rendert N Zeilen, Suche ruft Service mit Begriff, Submit ruft Create, Fehleranzeige bei Exception). **Der erste bUnit-Test ist das Referenzmuster — alle späteren WPs kopieren ihn.**

### WP4: UC2+UC6+UC9+UC10 — Lager (`feat/ui-lager`)
UC9 hier, weil das Lagerort-Autocomplete in den Bestandsformularen gebraucht wird.
- `Pages/Lager/Lagerbestand.razor` `@page "/lagerbestand"` — je Lebensmittel: `GetGesamtmengeAsync` + `CheckMindestbestandUnterschrittenAsync` (Badge); "Verbrauchen"-Button → `IVerbrauchAusbuchangService.VerbauchtProduktAsync` → `ServiceResult.Success/Message` anzeigen. Packungsliste: zuerst `IProduktInstanzService` lesen (hat `GetByLebensmittelAsync(int)`).
- `Pages/Lager/PackungHinzufuegen.razor` `@page "/lagerbestand/neu"` — `ILagerbestandService.AddToBestandAsync(lebensmittelKatalogId, menge, mindestbestand, verfallsdatum, lagerort)`; Lagerort-Vorschläge via `ILagerortService.GetLagerorteMitAutoComplete(prefix)` (HTML-`<datalist>` genügt, KISS).
- `Pages/Lager/Lagerorte.razor` `@page "/lagerorte"` — `GetAlleLagerorte()`, Anlegen via `GetOrCreateAsync(name)`.

bUnit: ~10 Tests (Übersicht, Ausbuchen Erfolg/Fehler via ServiceResult, Formular, Autocomplete-Aufruf).

### WP5: UC4+UC5 — Rezepte (`feat/ui-rezepte`) — braucht WP2-Migration
- `Pages/Rezepte/RezeptListe.razor` `@page "/rezepte"` — `GetAllRezepteAsync`, Suche, Löschen (`DeleteRezeptAsync`, Soft-Delete).
- `Pages/Rezepte/RezeptForm.razor` `@page "/rezepte/neu"` + `.../{Id:int}/bearbeiten` — baut `CreateRezeptRequest`/`UpdateRezeptRequest` (DTO-Felder vorher aus `src/App/Services/Dtos/` ablesen); Schwierigkeitsgrad-Select (Easy/Medium/Hard).
- `Pages/Rezepte/RezeptDetail.razor` `@page "/rezepte/{Id:int}"` — `GetRezeptByIdAsync` (lädt KEINE Zutaten!) + `IRezeptZutatService.GetZutatenAsync(Id)`; Zutaten-CRUD über `AddZutatAsync`/`UpdateZutatAsync`/`DeleteZutatAsync`; Zutat-Zeile: Lebensmittel-Dropdown + Menge + Einheit ("g","ml","Stück","TL","EL","Tasse","Prise"). **Vorhandene Komponente einbetten**: `<RezeptNährwertAnzeige RezeptId="@Id" />` (UC5-Anzeige inkl. Portionsanpassung ist fertig, `data-testid="reduce-portion"/"increase-portion"/"portion-input"` existieren).

bUnit: ~12 Tests, inkl. `RezeptNährwertAnzeige` (Loading, Fehlerpfad, Portionsbuttons → `AdjustNährwerteForPortionAsync`).

### WP6: UC7+UC8 — Warnungen & Einkaufsliste (`feat/ui-dashboard`)
- `Pages/Dashboard/Warnungen.razor` `@page "/warnungen"` — `IVerfallsdatumWarnungService.GetWarnungenAsync()` → Tabelle mit Farb-Badge je `VerfallsdatumStatus` (Ok/BaldAblaufend/Abgelaufen).
- `Pages/Dashboard/Einkaufsliste.razor` `@page "/einkaufsliste"` — `IEinkaufslistenService.GetEinkaufslisteAsync()` → Tabelle (Name, Gesamtmenge, Mindestbestand, Fehlmenge, Einheit).
- `Home.razor` erweitern: Kacheln mit Anzahl Warnungen + Einkaufslisten-Einträgen, verlinkt.

bUnit: ~8 Tests (leere Liste → Hinweistext, Status-Badges, Home-Kacheln). **UC7 in Tests**: `Services.AddSingleton<TimeProvider>(new FixedTimeProvider(...))` (Fake in `src/Tests/Unit/Helpers/FixedTimeProvider.cs`) — niemals `TimeProvider.System` in Tests.

### WP7: Abschluss (`docs/ui-phase-abschluss`)
Orchestrator: NavMenu-Vollständigkeit prüfen (alle 7 Routen erreichbar), Gesamtsuite + App-Start final, `CONTEXT_SUMMARY.md` aktualisieren (Session-Eintrag, Teststand), Memory `project_status.md` aktualisieren, Sammel-MR nach CLAUDE.md-Template.

---

## bUnit-Briefing für den Test-Agent (bunit 1.30.0 + xUnit + Moq) — PFLICHT

**Setup — genau so, keine Varianten:**
- Testklassen sind **reine .cs-Dateien** (KEINE .razor-Tests — Testprojekt nutzt `Microsoft.NET.Sdk`, .razor-Tests kompilieren nicht).
- Klasse erbt von `Bunit.TestContext`; xUnit erzeugt pro Test eine neue Instanz, Dispose automatisch. Kein `[Collection]`, kein Fixture.
- Erste Zeile im Konstruktor: `JSInterop.Mode = JSRuntimeMode.Loose;` (Default Strict wirft bei jedem JS-Call).
- Services VOR dem Rendern registrieren:
  ```csharp
  Mock<ILebensmittelService> mock = new Mock<ILebensmittelService>();
  mock.Setup(s => s.GetAllLebensmittelAsync()).ReturnsAsync(liste);
  Services.AddSingleton<ILebensmittelService>(mock.Object);
  ```

**Rendern & Interaktion:**
```csharp
IRenderedComponent<LebensmittelListe> cut = RenderComponent<LebensmittelListe>(
    parameters => parameters.Add(p => p.Id, 1));
cut.Find("[data-testid='suche']").Input("Mehl");   // input-Event
cut.Find("[data-testid='suche-btn']").Click();      // onclick
cut.Find("select").Change("3");                     // onchange/@bind
cut.Find("form").Submit();                          // EditForm
```
- `Find` wirft bei fehlendem Element; `FindAll` gibt (ggf. leere) Liste.
- **Stale-Element-Falle**: Nach jedem Re-Render sind alte `Find`-Ergebnisse ungültig → nach jeder Interaktion neu `Find(...)` aufrufen.

**Async (`OnInitializedAsync`)**: `RenderComponent` kehrt nach dem ersten Render zurück. Zur Sicherheit immer:
```csharp
cut.WaitForAssertion(() => Assert.Equal(3, cut.FindAll("tbody tr").Count), TimeSpan.FromSeconds(2));
```
Ladezustand explizit testen: Mock gibt `TaskCompletionSource.Task` zurück → Loading sichtbar → `tcs.SetResult(daten)` → `WaitForAssertion`.

**Navigation**: `FakeNavigationManager nav = Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();` (auto-registriert); dann `Assert.EndsWith("/lebensmittel", nav.Uri);`.

**Bekannte Fallen (haben frühere bUnit-Versuche zerstört):**
1. Kein `@rendermode` in Komponenten (nur in App.razor am `<Routes>`-Element).
2. Jeden `@inject`-Service mocken — fehlende Registrierung wirft `InvalidOperationException: Cannot provide a value for property ...`; die Meldung nennt den Typ → genau den registrieren.
3. Zustandsänderungen von außen via `cut.InvokeAsync(() => ...)`, nicht direkt über `cut.Instance`.
4. Markup-Assertions: `cut.Find("h3").TextContent`, `cut.Markup.Contains(...)`, tolerant: `cut.MarkupMatches(...)`.
5. Umlaute in `dotnet test --filter`-Strings immer quoten.

---

## Risiken & Rollback

| Risiko | Absicherung |
|---|---|
| Migration beschädigt `fooddatabase.db` | Backup `.bak` vor `database update`; Migration inspizieren (nur CreateTable); Rollback: `dotnet ef database update AddNährwertEntity` + Migrationsdateien löschen |
| EfRepository-Sofort-Save ändert Semantik | Bisher nur Moq-Unit-Tests → keine Erwartung verletzt; Integrationstests (2e) fixieren die neue Semantik |
| Program.cs-Rewrite bricht App-Start | `/health`-Endpoint + curl-Check; Program.cs als eigener Commit (einfaches Revert) |
| bUnit-API-Fehlversuche (wie früher) | Briefing verbindlich; erster bUnit-Test in WP3 = Referenzmuster, alle weiteren kopieren |
| `Rezept.Name`-Unique-Index vs. Service-Verhalten | `RezeptServiceTests` bleiben grün (Moq, DB-unabhängig); Integrationstest deckt Index ab |

## Verifikation (Gesamtphase)

Nach jedem WP: `dotnet build` (0 Fehler) + `dotnet test` (alle grün, Zielzahl steigt pro WP) + App-Start mit curl-Check der neuen Routen (HTTP 200). Am Ende (WP7): Gesamtsuite grün (~310+ Tests), App startet, alle 7 Routen liefern 200, manuelle Browser-Prüfung der Kern-Workflows (Lebensmittel anlegen → Packung hinzufügen → Warnung/Einkaufsliste sehen → Rezept mit Zutaten + Nährwertanzeige).
