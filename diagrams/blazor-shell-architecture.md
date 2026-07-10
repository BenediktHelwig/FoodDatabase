# Blazor Shell Architecture (UI-Shell-Layout)

**Status**: ✅ Implementiert in Session 2026-07-10  
**Diagram-Format**: Text-Diagramm (für draw.io Manual-Update: `architecture-components.drawio`)

---

## 🏗️ Komponenten-Hierarchie

```
┌─────────────────────────────────────────────────────────┐
│                   App.razor (Root)                      │
│   └─ Head: Bootstrap 5.3.3 CSS + JS Imports            │
│   └─ Routes.razor (Razor Router)                        │
│       └─ MainLayout.razor (LayoutComponentBase)         │
│           ├─ div.page (flex container)                  │
│           │   ├─ div.sidebar (width: 250px, sticky)     │
│           │   │   └─ NavMenu.razor                      │
│           │   │       ├─ nav.navbar (navbar-expand-md)  │
│           │   │       │   ├─ navbar-brand (Desktop)     │
│           │   │       │   ├─ navbar-toggler (Mobile)    │
│           │   │       │   └─ offcanvas#navbarOffcanvas  │
│           │   │       │       ├─ offcanvas-header (d-md-none)
│           │   │       │       └─ ul.navbar-nav          │
│           │   │       │           ├─ li: Start (/)      │
│           │   │       │           ├─ li: Lebensmittel   │
│           │   │       │           ├─ li: Lagerbestand   │
│           │   │       │           ├─ li: Lagerorte      │
│           │   │       │           ├─ li: Rezepte        │
│           │   │       │           ├─ li: Warnungen      │
│           │   │       │           └─ li: Einkaufsliste  │
│           │   │                                          │
│           │   └─ main (flex: 1)                          │
│           │       └─ article.content                     │
│           │           └─ @Body (Page Content)           │
│           │               ├─ LebensmittelListe.razor     │
│           │               ├─ LebensmittelForm.razor      │
│           │               ├─ LebensmittelDetail.razor    │
│           │               └─ ... (andere UCs)            │
│           │                                              │
│           └─ Assets                                      │
│               └─ wwwroot/bootstrap/                      │
│                   ├─ bootstrap.min.css (232 KB)         │
│                   └─ bootstrap.bundle.min.js (80 KB)    │
└─────────────────────────────────────────────────────────┘
```

---

## 📐 Layout-Responsivität

### Desktop (≥ 768px)

```
┌──────────────────────────────────────────┐
│ HEADER (FoodDatabase Logo)               │
├─────────────────────┬────────────────────┤
│                     │                    │
│   SIDEBAR           │   MAIN CONTENT     │
│   (250px, sticky)   │   (Flex: 1)       │
│                     │                    │
│  - Start            │ [Page Content]     │
│  - Lebensmittel     │                    │
│  - Lagerbestand     │                    │
│  - Lagerorte        │                    │
│  - Rezepte          │                    │
│  - Warnungen        │                    │
│  - Einkaufsliste    │                    │
│                     │                    │
└─────────────────────┴────────────────────┘
```

### Mobile (< 768px)

```
┌──────────────────────────────────────────┐
│ [≡] Logo             FoodDatabase        │  ← Hamburger + Brand
├──────────────────────────────────────────┤
│ [Page Content]                           │
│                                          │
│                                          │
│ ┌────────────┐  ← Off-Canvas Panel      │
│ │ Navigation│  (slides from left)       │
│ │ ──────────│                           │
│ │ - Start   │  Backdrop (dimmed)        │
│ │ - Lebens..│  ESC closes               │
│ │ - Lager...│                           │
│ │ - ...     │                           │
│ └────────────┘                           │
└──────────────────────────────────────────┘
```

---

## 🎨 CSS-Integration (app.css)

| Selektor | Regel | Zweck |
|----------|-------|-------|
| `.page` | `display: flex; flex-direction: column` | Basis-Layout (vertikal) |
| `main` | `flex: 1` | Nimmt restlichen Platz |
| `.sidebar` | `background: gradient; (sticky @ md)` | Navigation sidebar |
| `#navbarOffcanvas` | `background: gradient; (fixed @ mobile)` | Panel-Hintergrund |
| `@media (min-width: 768px)` | `.page { flex-direction: row }` | Horizontales Layout |

---

## 🔧 Bootstrap 5.3.3 Klassen

### Navigation

- `navbar` - Container
- `navbar-expand-md` - Breakpoint für Collapse
- `navbar-brand` - Logo
- `navbar-toggler` - Hamburger-Button
- `data-bs-toggle="offcanvas"` - Trigger-Attribut
- `data-bs-target="#navbarOffcanvas"` - Target-Panel
- `offcanvas` - Off-Canvas-Panel-Basis
- `offcanvas-start` - Slide von links
- `data-bs-dismiss="offcanvas"` - Link schließt Panel

### Utility-Klassen

- `d-flex` - Display Flex
- `w-100` - Width 100%
- `align-items-center` - Flex-Align
- `d-md-none` - Hidden @ md+
- `flex-column` - Flex-Direction Column
- `px-3`, `py-2` - Padding
- `gap-2` - Gap zwischen Items

---

## 📋 Entities & Dependencies

### Blazor-Komponenten

- **App.razor**: Bootstrap-JS + CSS Import
- **Routes.razor**: Router Setup
- **MainLayout.razor**: Page-Container + NavMenu
- **NavMenu.razor**: Navigation mit Off-Canvas
- **_Imports.razor**: `@using Microsoft.AspNetCore.Components.Routing`

### CSS-Dateien

- **app.css**: Custom-Styling (`.page`, `.sidebar`, Responsive-Media-Queries)
- **bootstrap/bootstrap.min.css**: Bootstrap 5.3.3 Core (local, no CDN)
- **bootstrap/bootstrap.bundle.min.js**: JavaScript für Off-Canvas + Modals

---

## ✅ Validierung & Test-Coverage

| Test | Komponente | Status |
|------|-----------|--------|
| `RendersAllSevenNavLinks` | NavMenu | ✅ |
| `NavElementHasExpandMdClass` | NavMenu | ✅ |
| `TogglerTargetsOffcanvasPanel` | NavMenu | ✅ |
| `PanelSlidesFromLeft` | NavMenu | ✅ |
| `EveryNavLinkDismissesOffcanvas` | NavMenu | ✅ |
| `HomeLinkUsesExactMatch` | NavMenu | ✅ |
| `RendersSidebarAndMain` | MainLayout | ✅ |
| `BodyIsRenderedInsideContentArticle` | MainLayout | ✅ |
| `DoesNotContainMicrosoftTemplateLink` | MainLayout | ✅ |

---

## 🔗 Integration mit Draw.io

Für Manual-Update von `architecture-components.drawio`:

1. Öffne `diagrams/architecture-components.drawio` im Draw.io-Editor
2. Aktualisiere Komponenten-Diagramm:
   - Füge `App.razor` → `Routes` → `MainLayout` → `NavMenu` Hierarchie ein
   - Zeige `.page` Flex-Container
   - Zeige `.sidebar` (Desktop) + `#navbarOffcanvas` (Mobile)
   - Bootstrap 5.3.3 Asset-Box
3. Farbcodierung: Blazor-Komponenten (blau), CSS (grün), Bootstrap-Assets (orange)
4. Legend aktualisieren

---

**Next Step**: Draw.io Manual-Update oder automated XML-Generator-Integration.
