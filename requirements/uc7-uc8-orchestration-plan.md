# Orchestrierungs-Plan: UC7 (Verfallsdatum-Warnungen) & UC8 (Einkaufsliste)

**Format**: Delegationsplan für den Orchestrator — jedes Task-Paket nennt den zuständigen Agenten, Inputs, Deliverables und Definition-of-Done.
**Erster Implementierungsschritt**: Diesen Plan als `requirements/uc7-uc8-orchestration-plan.md` ins Repo speichern, damit alle Agenten ihn referenzieren können.

---

## Kontext & fixierte User-Entscheidungen (für alle Agenten bindend)

- **Beide UCs on-demand berechnet**: keine neuen DB-Entities, keine EF-Migration, kein BackgroundService. Alles wird pro Aufruf aus `ProduktInstanz` abgeleitet (Felder `Verfallsdatum`, `Menge`, `MindestbestandMenge` existieren bereits).
- **UC8 ohne „Gekauft"-Abhaken** — reine berechnete flache Liste.
- **Nur Service-Layer** + Tests + Doku (Blazor-UI folgt später gesammelt).
- **Zwei getrennte Branches/MRs**: UC7 komplett abschließen (inkl. Merge), erst dann UC8 beginnen.
- **Phase-2-Vorgriff (nur Schnittstelle)**: `TimeProvider`-Injektion in UC7, damit später ein BackgroundService für Push-Benachrichtigungen andocken kann, ohne UC7 zu ändern.
- **Code-Style (verpflichtend)**: `is null`/`is not null`; explizite Typen statt `var` bei nicht-offensichtlichen Fällen; ein Typ pro Datei; XML-Docs auf allen public Members.
- **Baseline**: 233 Tests, 222 grün, **11 bekannte UC5-Fehler — müssen durchgehend exakt 11 bleiben**.
- Remote: `github.com/BenediktHelwig/FoodDatabase` → MRs via `gh pr create` gegen `master`.

---

# ═══════════ UC7: Verfallsdatum-Warnungen ═══════════

**Branch**: `feat/uc7-verfallsdatum-warnungen` (von `master`)

**Fachliche Spezifikation**:
- 🟡 `BaldAblaufend`: MHD in 0–3 Tagen (**MHD == heute ⇒ Gelb**, nicht Rot!)
- 🔴 `Abgelaufen`: MHD überschritten (negative Tage)
- Sortierung: Abgelaufene zuerst, dann Datum aufsteigend (ein `OrderBy(Verfallsdatum)` genügt)
- `Ok`-Einträge werden herausgefiltert; verbrauchte Produkte verschwinden automatisch (nicht mehr im Repository)

---

## 📦 Task UC7-1 → **Test-Agent** (TDD Rot-Phase)

**Commit**: `test(uc7): Add VerfallsdatumWarnungService tests (TDD Red Phase)`

**Zu erstellen** (Skelette für Kompilierbarkeit gehören mit in diesen Commit, Service wirft `NotImplementedException`):

| Datei | Inhalt |
|---|---|
| `src/App/Models/VerfallsdatumStatus.cs` | `enum VerfallsdatumStatus { Ok, BaldAblaufend, Abgelaufen }` |
| `src/App/Services/Dtos/VerfallsdatumWarnungDto.cs` | `ProduktInstanzId (int)`, `LebensmittelName (string)`, `Verfallsdatum (DateTime)`, `TageBisAblauf (int, negativ = abgelaufen)`, `Status (VerfallsdatumStatus)` |
| `src/App/Services/Interfaces/IVerfallsdatumWarnungService.cs` | `Task<List<VerfallsdatumWarnungDto>> GetWarnungenAsync();` |
| `src/App/Services/Classes/VerfallsdatumWarnungService.cs` | Skelett: Konstruktor `(IRepository<ProduktInstanz>, TimeProvider)`, Methoden werfen `NotImplementedException` |
| `src/Tests/Unit/Helpers/FixedTimeProvider.cs` | Manueller TimeProvider-Fake (siehe unten) — **kein** NuGet-Paket `Microsoft.Extensions.TimeProvider.Testing` hinzufügen (KISS) |
| `src/Tests/Unit/Services/VerfallsdatumWarnungServiceTests.cs` | 19 Tests (siehe Liste) |

**FixedTimeProvider** (macht `GetLocalNow()` maschinenunabhängig):
```csharp
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedNow;
    public FixedTimeProvider(DateTime fixedDate)
        => _fixedNow = new DateTimeOffset(DateTime.SpecifyKind(fixedDate, DateTimeKind.Utc));
    public override DateTimeOffset GetUtcNow() => _fixedNow;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
```

**Test-Stil** (Vorlage: `src/Tests/Unit/Services/LagerbestandServiceTests.cs`): xUnit + Moq, `Mock<IRepository<ProduktInstanz>>` als readonly-Feld mit Konstruktor-Setup, Namensschema `Method_Condition_ExpectedResult`, explizite `// Arrange` / `// Act` / `// Assert`-Kommentare, UC7 im XML-Doc-Header, Anker-Datum z.B. `new DateTime(2026, 7, 8)`.

**Die 19 Tests**:

`BerechneStatus` (pur, ohne Mocks — 7):
1. `BerechneStatus_VerfallsdatumGestern_ReturnsAbgelaufen`
2. `BerechneStatus_VerfallsdatumHeute_ReturnsBaldAblaufend` ← Boundary, Gelb-nicht-Rot!
3. `BerechneStatus_VerfallsdatumInEinemTag_ReturnsBaldAblaufend`
4. `BerechneStatus_VerfallsdatumInDreiTagen_ReturnsBaldAblaufend` (+3 inklusiv)
5. `BerechneStatus_VerfallsdatumInVierTagen_ReturnsOk` (+4 exklusiv)
6. `BerechneStatus_VerfallsdatumWeitInVergangenheit_ReturnsAbgelaufen`
7. `BerechneStatus_VerfallsdatumWeitInZukunft_ReturnsOk`

`GetWarnungenAsync` (12):
8. `GetWarnungenAsync_WithEmptyRepository_ReturnsEmptyList`
9. `GetWarnungenAsync_WithNullFromRepository_ReturnsEmptyList`
10. `GetWarnungenAsync_WithOnlyOkProdukten_ReturnsEmptyList`
11. `GetWarnungenAsync_WithAbgelaufenemProdukt_ReturnsAbgelaufenWarnung`
12. `GetWarnungenAsync_WithBaldAblaufendemProdukt_ReturnsBaldAblaufendWarnung`
13. `GetWarnungenAsync_WithMixedStatus_FiltersOutOkProdukte` (Ok+Gelb+Rot → 2 Ergebnisse)
14. `GetWarnungenAsync_WithMultipleWarnungen_SortsAbgelaufeneZuerstDannNachDatumAufsteigend` (+2, −5, 0, −1 → −5, −1, 0, +2)
15. `GetWarnungenAsync_WithVerfallsdatumHeute_ReturnsTageBisAblaufZero`
16. `GetWarnungenAsync_WithAbgelaufenemProdukt_ReturnsNegativeTageBisAblauf`
17. `GetWarnungenAsync_WithLebensmittelKatalogNavigation_MapsLebensmittelName`
18. `GetWarnungenAsync_WithNullLebensmittelKatalog_UsesFallbackName`
19. `GetWarnungenAsync_WithWarnung_MapsProduktInstanzIdAndVerfallsdatum`

**DoD**: `dotnet build` OK; `dotnet test` → 252 total, 222 passed, 30 failed (11 bekannte + 19 neue rote).

---

## 📦 Task UC7-2 → **Review-Agent** (Test-Review, max. 3 Loops)

**Fokus**: Testkonstruktion (AAA klar?), Abdeckung (alle Boundaries: heute/+3/+4/gestern?), Edge-Cases (leer/null/Nav-Prop null?), TDD-Spezifikationsqualität. **NICHT** prüfen, ob Tests bestehen (absichtlich rot!).
**Output**: `reviews/uc7-test-review-1.md` (Format wie `reviews/uc9-test-review-1.md`).
**Loop**: Feedback → Test-Agent überarbeitet → erneut. Nach 3 Fehlversuchen ⛔ User-Eskalation.

---

## 📦 Task UC7-3 → **Dev-Agent** (TDD Grün-Phase)

**Commit**: `feat(uc7): Implement VerfallsdatumWarnungService (TDD Green Phase, 19/19 GRÜN)`

**Implementierung** von `VerfallsdatumWarnungService`:
- `GetWarnungenAsync()`: `GetAllAsync()` → `is null` ⇒ leere Liste (Konvention wie `LagerbestandService.GetEinkaufslistenEintraegeAsync`, Z. 86–87). `DateTime heute = _timeProvider.GetLocalNow().Date;`. Pro Instanz: `TageBisAblauf = (Verfallsdatum.Date - heute).Days`, Status via `BerechneStatus`, `Ok` filtern, `OrderBy(w => w.Verfallsdatum)`.
- `public static VerfallsdatumStatus BerechneStatus(DateTime verfallsdatum, DateTime heute)` — **public static, pur** (Präzedenz: `NährwertCalculator`): `< 0` → Abgelaufen; `0..3` → BaldAblaufend; `> 3` → Ok.
- LebensmittelName: `instanz.LebensmittelKatalog?.Name ?? $"Lebensmittel #{instanz.LebensmittelKatalogId}"`.
- **Keine** Änderungen an `Program.cs` (registriert projektweit nichts) oder bestehenden Services.

**DoD**: 252 total, **241 passed, 11 failed**; `dotnet test --filter "FullyQualifiedName~VerfallsdatumWarnungServiceTests"` → 19/19; Style-Selbstcheck (kein `== null`, ein Typ pro Datei, XML-Docs).

---

## 📦 Task UC7-4 → **Review-Agent** (Code-Review, max. 3 Loops)

**Fokus**: Tests GRÜN? Clean Code/SOLID/KISS? Fehlerbehandlung? Style-Standards eingehalten? Keine Regression (11 bekannte Fehler unverändert)?
**Output**: `reviews/uc7-code-review-1.md`. Loop wie oben, nach 3 ⛔ Eskalation.

---

## 📦 Task UC7-5 → **Doc-Agent** (4-Teil-Check)

**Commit**: `docs(uc7): Add UC7 documentation, diagrams and review reports`

1. **Feature-HTML** `docs/features/UC7-Verfallswarnungen.html` (existiert als TODO-Draft!):
   - Persistierte `VerfallsdatumWarnung`-Entity + `RefreshAllWarningsAsync` aus Draft **entfernen** (durch On-Demand-Entscheidung überholt)
   - Tatsächliche API dokumentieren: `IVerfallsdatumWarnungService`, `VerfallsdatumWarnungDto`, `VerfallsdatumStatus`
   - ⚠️ **Z. 134 korrigieren**: Draft behauptet „Rot bei MHD = Heute" — richtig ist **Gelb**
   - Status-Badge: `⏳ TODO` → `✅ FERTIG (19/19 Tests GRÜN)`; Test-Dateipfad korrigieren
2. **`docs/architecture-overview.html`**: UC7 von ⏳ auf ✅ an **allen drei Stellen** (UC-Karten ~Z. 167–176, Roadmap-Tabelle ~Z. 233–238, Pending-Liste ~Z. 345–346)
3. **Diagramme**: `requirements/use-cases.drawio` Knoten Z. 50: `fillColor=#ffffff` → `#d4edda`, Label-Präfix „✅ ", Legende aktualisieren. Neu: `diagrams/sequence-uc7-verfallswarnungen.drawio` (UI → Service → Repository → LINQ → DTO-Liste)
4. **Konsistenz-Check**: Code / Feature-HTML / Overview / Diagramme sagen alle dasselbe

---

## 📦 Task UC7-6 → **Review-Agent** (Doku-Review, max. 3 Loops)

**Fokus**: Alle 4 Aspekte konsistent? Gelb-bei-heute überall korrekt? Status-Badges stimmen mit Code überein?
**Output**: `reviews/uc7-doku-review-1.md`. Nach 3 ⛔ Eskalation.

---

## 📦 Task UC7-7 → **Orchestrator** (MR & Merge)

1. Push Branch, `gh pr create` gegen `master` (Beschreibung nach CLAUDE.md-Template, Trailer `🤖 Generated with Claude Code`)
2. User-Review abwarten → nach Approval mergen
3. `CONTEXT_SUMMARY.md` aktualisieren (9/10 UCs, neue Testzahlen)
4. **Gate**: Erst nach Merge UC8 starten

---

# ═══════════ UC8: Einkaufsliste ═══════════

**Branch**: `feat/uc8-einkaufsliste` (vom aktualisierten `master` nach UC7-Merge)

**Fachliche Spezifikation**:
- Flache Liste, **eine Zeile pro Lebensmittel** (aggregiert über alle Instanzen desselben `LebensmittelKatalogId`)
- Trigger: `Gesamtmenge <= MindestbestandMenge` (**„erreicht/unterschritten"** ⇒ `<=`, bewusst anders als UC2s `<`)
- `Fehlmenge = Mindestbestand − Gesamtmenge`; Sortierung nach `LebensmittelName`
- Kein Abhaken, keine Persistenz

**⚠️ Architektur-Vorgabe**: **Neuer dedizierter `EinkaufslistenService`.** `LagerbestandService.GetEinkaufslistenEintraegeAsync()` (Z. 83–106) bleibt **unangetastet** — es ist UC2-API (pro Instanz, `<`-Vergleich, durch `LagerbestandServiceTests` gepinnt). Die ~10 Zeilen GroupBy-LINQ werden bewusst dupliziert statt refactored (KISS, ein Service pro UC).

---

## 📦 Task UC8-1 → **Test-Agent** (TDD Rot-Phase)

**Commit**: `test(uc8): Add EinkaufslistenService tests (TDD Red Phase)`

**Zu erstellen** (inkl. kompilierbarer Skelette):

| Datei | Inhalt |
|---|---|
| `src/App/Services/Dtos/EinkaufslistenEintragDto.cs` | `LebensmittelKatalogId (int)`, `LebensmittelName (string)`, `AktuelleGesamtmenge (decimal)`, `MindestbestandMenge (decimal)`, `Fehlmenge (decimal)`, `Einheit (string)` |
| `src/App/Services/Interfaces/IEinkaufslistenService.cs` | `Task<List<EinkaufslistenEintragDto>> GetEinkaufslisteAsync();` |
| `src/App/Services/Classes/EinkaufslistenService.cs` | Skelett: Konstruktor `(IRepository<ProduktInstanz>)`, `NotImplementedException` |
| `src/Tests/Unit/Services/EinkaufslistenServiceTests.cs` | 17 Tests (siehe Liste) |

**Die 17 Tests** (gleicher Stil wie UC7):
1. `GetEinkaufslisteAsync_WithEmptyRepository_ReturnsEmptyList`
2. `GetEinkaufslisteAsync_WithNullFromRepository_ReturnsEmptyList`
3. `GetEinkaufslisteAsync_WithAllMengenUeberMindestbestand_ReturnsEmptyList`
4. `GetEinkaufslisteAsync_WithMengeUnterMindestbestand_ReturnsEintrag`
5. `GetEinkaufslisteAsync_WithMengeGleichMindestbestand_ReturnsEintrag` ← `<=`-Boundary!
6. `GetEinkaufslisteAsync_WithMengeKnappUeberMindestbestand_ReturnsEmptyList` (100.01 vs. 100)
7. `GetEinkaufslisteAsync_WithMultipleInstanzenDesselbenLebensmittels_AggregiertGesamtmenge` (30+30 vs. 100 → 1 Eintrag, Gesamtmenge 60)
8. `GetEinkaufslisteAsync_WithSummeUeberMindestbestand_ReturnsEmptyList` (60+60 vs. 100)
9. `GetEinkaufslisteAsync_WithMehrerenUnterschrittenenLebensmitteln_ReturnsEinEintragProLebensmittel`
10. `GetEinkaufslisteAsync_WithMixedLebensmitteln_ReturnsOnlyUnterschritteneEintraege`
11. `GetEinkaufslisteAsync_WithUnterschreitung_BerechnetFehlmengeKorrekt` (100−40=60)
12. `GetEinkaufslisteAsync_WithMengeGleichMindestbestand_ReturnsFehlmengeZero`
13. `GetEinkaufslisteAsync_WithLebensmittelKatalogNavigation_MapsNameUndEinheit`
14. `GetEinkaufslisteAsync_WithNullLebensmittelKatalog_UsesFallbackName`
15. `GetEinkaufslisteAsync_WithMindestbestandZero_ReturnsEmptyList`
16. `GetEinkaufslisteAsync_WithMehrerenEintraegen_SortsByLebensmittelName`
17. `GetEinkaufslisteAsync_WithEintrag_MapsLebensmittelKatalogIdUndMindestbestand`

**DoD**: Build OK; 269 total, 241 passed, 28 failed (11 + 17 neue rote).

---

## 📦 Task UC8-2 → **Review-Agent** (Test-Review, max. 3 Loops)

**Fokus**: `<=`-Boundary abgedeckt? Aggregations-Szenarien (Summe über/unter)? Edge-Cases?
**Output**: `reviews/uc8-test-review-1.md`. Nach 3 ⛔ Eskalation.

---

## 📦 Task UC8-3 → **Dev-Agent** (TDD Grün-Phase)

**Commit**: `feat(uc8): Implement EinkaufslistenService (TDD Green Phase, 17/17 GRÜN)`

**Implementierung** von `GetEinkaufslisteAsync()`:
- `GetAllAsync()` → `is null` ⇒ leere Liste
- `GroupBy(x => x.LebensmittelKatalogId)`; pro Gruppe: `Gesamtmenge = Sum(Menge)`, `Mindestbestand = First().MindestbestandMenge` (Erste-Instanz-Konvention wie `LagerbestandService` Z. 96), Name/Einheit aus `First().LebensmittelKatalog?` mit Fallbacks (`$"Lebensmittel #{id}"` / `string.Empty`)
- Filter `Gesamtmenge <= Mindestbestand`; `Fehlmenge = Mindestbestand - Gesamtmenge`; `OrderBy(LebensmittelName)`
- **`LagerbestandService` nicht anfassen!**

**DoD**: 269 total, **258 passed, 11 failed**; Filter-Checks: `~EinkaufslistenServiceTests` → 17/17 **und** `~LagerbestandServiceTests` unverändert grün (beweist UC2 unangetastet); Style-Selbstcheck.

---

## 📦 Task UC8-4 → **Review-Agent** (Code-Review, max. 3 Loops)

**Fokus**: Tests GRÜN? UC2-Regression ausgeschlossen? Clean Code/KISS?
**Output**: `reviews/uc8-code-review-1.md`. Nach 3 ⛔ Eskalation.

---

## 📦 Task UC8-5 → **Doc-Agent** (4-Teil-Check)

**Commit**: `docs(uc8): Add UC8 documentation, diagrams and review reports`

1. **Feature-HTML** `docs/features/UC8-Einkaufslisten.html` (TODO-Draft): `Einkaufsliste`-Entity + Abhaken-Draft **entfernen**; `IEinkaufslistenService` + `<=`-Trigger dokumentieren; **bekannte Einschränkung dokumentieren** (nicht lösen): Lebensmittel ohne verbleibende Instanzen können nicht auf der Liste erscheinen, da Mindestbestand auf Instanzen lebt; Status-Badge → `✅ FERTIG (17/17 Tests GRÜN)`
2. **`docs/architecture-overview.html`**: UC8 an allen drei Stellen auf ✅
3. **Diagramme**: `use-cases.drawio` Knoten Z. 55 → `#d4edda` + „✅ "; Legende final (10/10 = 100 %); neu: `diagrams/sequence-uc8-einkaufslisten.drawio`
4. **Konsistenz-Check** über alle 4 Aspekte

---

## 📦 Task UC8-6 → **Review-Agent** (Doku-Review, max. 3 Loops)

**Output**: `reviews/uc8-doku-review-1.md`. Nach 3 ⛔ Eskalation.

---

## 📦 Task UC8-7 → **Orchestrator** (MR, Merge & Abschluss)

1. Push, `gh pr create`, User-Review, Merge
2. `CONTEXT_SUMMARY.md` final aktualisieren: **10/10 UCs (100 %)**, 258/269 Tests, nächste Schritte = Blazor-UI, Integration-Tests, UC5-Setup-Fixes, Deployment

---

## Verifikations-Matrix (für Orchestrator-Gates)

| Gate | Kommando | Erwartung |
|---|---|---|
| Baseline | `dotnet test src/Tests/FoodDatabase.Tests.csproj` | 233 total / 222 passed / 11 failed |
| Nach UC7-Rot | ebd. | 252 / 222 / 30 |
| Nach UC7-Grün | ebd. | 252 / **241** / 11 |
| Nach UC8-Rot | ebd. | 269 / 241 / 28 |
| Nach UC8-Grün | ebd. | 269 / **258** / 11 |

Alle Commits: Conventional Commits + Trailer `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.

## Kritische Referenz-Dateien

- `src/App/Services/Classes/LagerbestandService.cs` (Z. 83–106) — Aggregations-Muster; **darf nicht verändert werden**
- `src/Tests/Unit/Services/LagerbestandServiceTests.cs` — Test-Stil-Vorlage
- `src/App/Services/Interfaces/IRepository.cs` — einzige Abhängigkeit beider Services
- `src/App/Models/ProduktInstanz.cs` — Quell-Entity (Nav-Prop `LebensmittelKatalog` nullable)
- `docs/features/UC7-Verfallswarnungen.html`, `docs/features/UC8-Einkaufslisten.html` — existierende TODO-Drafts (überarbeiten, nicht neu anlegen)
