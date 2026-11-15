# Quality Bar

Der eine Maßstab für Feature-Arbeit. Der `implementer` arbeitet danach, der `reviewer` urteilt
danach, die Hauptsession gibt danach frei. Das Projekt-Profil
([`PROJECT-PROFILE.md`](PROJECT-PROFILE.md)) liefert die konkreten Konventionen, diese Datei die
Methode. Bei Konventionen gewinnt das Profil, bei der Methode diese Datei.

## 1. Testgetrieben

- **Tests zuerst.** Sie werden vor jedem Produktivcode committet — der *rote Commit*.
  **Ausnahme UI-Schicht:** Wo Profil und Plan „Code zuerst“ festlegen, kommt der Code-Commit
  zuerst und danach ein *grüner* Test-Commit. Alle übrigen Regeln dieses Abschnitts gelten
  unverändert; „roter Commit“ heißt dort „Test-Commit“.
- **Rot aus dem richtigen Grund.** Die Tests scheitern, weil die Implementierung fehlt — nicht an
  einem Tippfehler, einem fehlenden `using` oder einem Compile-Fehler. Ein roter Test, der nicht
  kompiliert, ist eine kaputte Datei.
- **Abdeckung.** Jedes Akzeptanzkriterium im Plan hat mindestens einen Test. Happy Path,
  Fehlerpfade und Grenzfälle sind abgedeckt — eine Suite, die nur den Gutfall beweist, ist Theater.
- **Aufbau.** Ein Test prüft eine Sache, und sein Name sagt welche. Arrange, Act, Assert —
  sichtbar. Kein Test hängt von Überbleibseln eines anderen oder von der Reihenfolge ab. Keine
  Assertion auf Nebensächliches, das den Test zerbrechlich macht.
- **Tests sind die Spezifikation.** Nach dem roten Commit ändert sich kein Test — weder durch eine
  gelockerte Assertion noch durch einen gelöschten oder übersprungenen Fall. **Einzige Ausnahme:**
  ein Mangel des Reviewers, der den Test benennt. Diese Änderung kommt in einen eigenen Commit
  `fix tests per review`, und die Unveränderlichkeit zählt ab diesem Commit.
- **Grün heißt die ganze Suite**, nicht nur die neuen Tests.

## 2. Ins Projekt passen

- Konventionen kommen aus dem Profil und aus dem Code neben der Änderung. Beim Teststil gewinnen
  bestehende Tests, wo sie vom Profil abweichen. Persönliche Vorlieben entscheiden nichts.
- Keine neue Abhängigkeit, kein neues Framework, kein neuer Test-Runner, keine neue Assertion- oder
  Mocking-Bibliothek. Scheint eine nötig, wird sie gemeldet, nicht hinzugefügt.
- Fehler werden behandelt, wie das Projekt sie behandelt — explizit, nie stillschweigend
  verschluckt.
- Das Einfachste, was die Tests besteht. Nicht das Erweiterbarste, kein Framework.
- Nichts ändert sich außerhalb des Plans. Dateien, die das Profil als tabu markiert, werden nie
  angefasst.

## 3. Sicher

- Kein Secret, kein Passwort, kein Schlüssel im Code.
- Eingaben, die eine Grenze überschreiten, werden validiert; kein Injection-Pfad.
- Ressourcen werden freigegeben; keine offensichtliche N+1-Abfrage, keine versehentlich
  quadratische Schleife.

## 4. Dokumentation

- In dem Format und an den Orten, die das Profil nennt — hier der **4-Teil-Check**. Nie ein
  aufgezwungenes Format.
- Sie beschreibt, was der Code tut — geprüft am Code, nicht am Plan.
- Der Status des Features stimmt überall, wo es erwähnt wird. Im Code fertig und in einer Übersicht
  „geplant“ ist ein Mangel, kein Detail.
- Code-Kommentare sagen *warum*, nie *was*.
- Links und Verweise lösen auf.

## 5. Git und Sprache

- Ein Commit pro Phase — Tests, Implementierung, Dokumentation — plus ein Commit pro
  Korrekturrunde, im Commit-Stil des Profils.
- Nur die Hauptsession legt Branches an, pusht und öffnet Pull Requests. Niemand schreibt die
  Historie um.
- Alles, was das Orchester schreibt, ist **deutsch** — mit den Ausnahmen, die das Profil nennt
  (Commit-Nachrichten, bestehende Code-Bezeichner).
