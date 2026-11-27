# Plan-Format

Ein Feature-Plan liegt unter `.agents/plans/<feature>.md`. Die Hauptsession schreibt ihn nach dem
Mode-Gate, der `implementer` hakt seine Zeilen ab, und eine neue Sitzung setzt daran wieder an. Er
wird nicht committet und gelöscht, sobald die Pull Request offen ist.

## Aufbau

```markdown
# <Feature>

## Ziel
<Beobachtbares Verhalten, zwei oder drei Sätze.>

## Akzeptanzkriterien
1. <Konkret genug, um einen Test dagegen zu schreiben>

## Nicht enthalten
- <Was dieses Feature ausdrücklich nicht tut>

## Zu ändernde Dateien
- <Pfad> — <was sich ändert>

## Testfälle
- Kriterium 1: <Happy Path, Fehlerpfade, Grenzfälle>

## Reihenfolge
Schicht-Regel: <TDD (Tests zuerst) | UI (Code zuerst, dann grüne Tests)> — siehe Profil.
<Alles, was der implementer in einer bestimmten Reihenfolge tun muss.>

## Offene Fragen
<Nur falls welche bleiben — sonst den Abschnitt weglassen.>

## Fortschritt
- [ ] Tests geschrieben und rot aus dem richtigen Grund (implementer) — roter Commit: <sha>
- [ ] Implementierung fertig, ganze Suite grün (implementer)
- [ ] Dokumentation aktualisiert, 4-Teil-Check (implementer)
- [ ] Review bestanden (reviewer) — Versuch <n> von 3
- [ ] Pull Request offen (oder Text bereitgelegt, falls kein Token)
```

**Bei der UI-Regel** tauschen die ersten beiden Zeilen Platz und Wortlaut:

```markdown
- [ ] Implementierung fertig, Build sauber (implementer)
- [ ] Tests geschrieben, ganze Suite grün (implementer) — Test-Commit: <sha>
```

## Wer hakt was ab

| Zeile | Abgehakt von | Wann |
| --- | --- | --- |
| Tests geschrieben (rot bzw. grün) | `implementer` | nach dem Test-Commit, mit eingetragenem sha |
| Implementierung fertig | `implementer` | nach dem Implementierungs-Commit |
| Dokumentation aktualisiert | `implementer` | nach dem Doku-Commit |
| Review bestanden | Hauptsession | bei `VERDICT: PASS`, mit Versuchsnummer |
| Pull Request offen | Hauptsession | sobald die PR offen oder der Text auf der Platte ist |

Eine abgehakte Zeile wird nie wieder geöffnet. Die erste offene Zeile ist der nächste Schritt.

## Pläne im alten Format fortsetzen

Pläne aus der Zeit des fünfköpfigen Orchesters tragen die alte Checkliste mit sieben Zeilen.
Übertragen, den Abschnitt `Fortschritt` im neuen Format neu schreiben, dann an der ersten offenen
Zeile weitermachen:

| Alte Zeile(n) | Neue Zeile |
| --- | --- |
| Tests geschrieben (test-agent) · Tests freigegeben (review-agent) | Tests geschrieben — sha aus `git log` |
| Implementierung fertig, Tests grün (dev-agent) · Implementierung freigegeben | Implementierung fertig |
| Dokumentation aktualisiert (doc-agent) | Dokumentation aktualisiert |
| Dokumentation freigegeben (review-agent) | Review bestanden — nur wenn alle drei alten Review-Zeilen abgehakt waren |
| Pull Request offen | Pull Request offen |

Teilfreigaben der alten Gates ersetzen das abschließende Review nicht: Sofern nicht alle drei alten
Review-Zeilen abgehakt waren, läuft das eine Review über den ganzen Diff trotzdem.
