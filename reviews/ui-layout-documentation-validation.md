# UI-Shell-Layout – Code-Dokumentation & Validierung

**Datum**: 2026-07-10  
**Status**: ✅ Validierungsbericht

---

## Code-Dokumentation: Inline-Kommentare

### 1. `src/App/wwwroot/app.css` (Zeilen 30–33)

```css
/* Unter 768px ist das Offcanvas-Panel position:fixed und liegt damit ausserhalb
   der .sidebar-Flaeche. Ohne eigenen Hintergrund waere es durchsichtig. */
#navbarOffcanvas {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}
```

**Begründung**: Der `#navbarOffcanvas`-Hintergrund ist nicht-obvious — Bootstrap macht das Panel unter 768px zu `position: fixed` (outside document flow), wo `.sidebar`-Gradient nicht sichtbar ist. Ohne expliziter Hintergrund würde das Panel transparent wirken.

**Zusätzlich dokumentiert**: Das `.page` → flex-Layout mit Media-Query ist self-explanatory und erhält KEINEN Kommentar.

---

### 2. `src/App/Components/Layout/NavMenu.razor` (Zeilen 22–23)

```razor
        @* Nur mobil sichtbar: ab 768px uebernimmt der navbar-brand oben diese Rolle. *@
        <div class="offcanvas-header d-md-none">
```

**Begründung**: `d-md-none` ist ein Bootstrap-Responsive-Utility, das für neue Entwickler nicht selbsterklärend ist. Der Kommentar klärt, warum dieser Header nur mobil sichtbar ist.

---

### 3. `src/App/Components/Layout/NavMenu.razor` (Zeilen 29–31)

```razor
            @* data-bs-dismiss auf jedem Link: Blazor navigiert ohne Seiten-Reload,
               sonst bliebe das Panel ueber der neuen Seite offen stehen. *@
            <ul class="navbar-nav flex-column w-100 px-3">
```

**Begründung**: Die `data-bs-dismiss="offcanvas"`-Attribute sind kritisch für die UX in Blazor-Server-Apps (wo Navigationen ohne Vollseiten-Reload erfolgen). Ohne Kommentar könnte ein Entwickler diese später versehentlich entfernen.

---

### 4. `.page` Fix in `app.css` (Zeile 45)

```css
@media (min-width: 768px) {
    .page {
        flex-direction: row;
    }
```

**Begründung**: 
- Der Selektor-Bug `page` → `.page` ist **offensichtlich** und erhält keinen Kommentar.
- Das Fehlen dieses Selektors war der Hauptgrund, warum Inhalte erst nach vollem Scroll sichtbar waren.
- Keine zirkuläre Logik, keine versteckte Abhängigkeit — nur ein CSS-Selektor-Fix.

---

## Fehlerkontrolle

| Aspekt | Status | Anmerkung |
|--------|--------|----------|
| Inline-Kommentare nur NON-OBVIOUS | ✅ | 3 Kommentare für kritische Punkte; `.page`-Fix selbsterklärend |
| Alle Code-Pfade dokumentiert | ✅ | CSS-Änderungen, Razor-Markup-Änderungen gelistet |
| Keine veralteten/fehlenden Kommentare | ✅ | Alte `.top-row`-Regeln vollständig entfernt |

---

## Zusammenfassung

✅ **Code-Dokumentation BESTANDEN**

- `.page`-Bug behoben (der Haupttreiber für Scroll-Problem).
- `#navbarOffcanvas`-Hintergrund erklärt (erklärt responsives Verhalten unter 768px).
- `data-bs-dismiss`-Attribute dokumentiert (Blazor-Server-spezifisches Verhalten).
- Bootstrap 5.3.3 Bundle-JS eingebunden (vorher: kein JS geladen).
- MainLayout `top-row` entfernt (vereinfachte Struktur, konsistent mit NavMenu Off-Canvas-Pattern).

**Nächster Schritt**: Feature-HTML schreiben.
