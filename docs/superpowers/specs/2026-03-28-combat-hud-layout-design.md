# Combat HUD Layout Design

## Goal

Replace the current scaffold UI (form-style layout with buttons and a panel) with a combat HUD that emulates Path of Achra's layout: docked top status bar and left sidebar, a full-screen game viewport (stubbed for now), and a floating combat log overlay at the bottom.

Reference screenshot: `docs/path-of-achra-reference.png`

## Layout Structure

The layout uses a hybrid approach: the top bar, resource counters, and sidebar are standard Godot containers that occupy fixed screen space. The game viewport fills the remaining area. The combat log is a `CanvasLayer` overlay that floats over the bottom of the viewport with a semi-transparent background.

### Scene Tree

```
CombatScene (Control — anchored full window) — CombatScene.cs
├── VBox (VBoxContainer — full window)
│   ├── TopStatusBar (HBoxContainer — 28px tall)
│   │   ├── HealthLabel (Label)
│   │   ├── IdentityLabel (Label — expand fill)
│   │   └── EnemyLabel (Label — right-aligned)
│   ├── ResourceCounters (HBoxContainer — 18px tall)
│   │   └── [placeholder labels for god resources]
│   └── MainArea (HBoxContainer — expand fill)
│       ├── Sidebar (VBoxContainer — 52px wide)
│       │   ├── PortraitRect (TextureRect — 40x48, placeholder)
│       │   └── AbilitySlot (TextureRect — 36x36, placeholder)
│       └── ViewportContainer (SubViewportContainer — expand fill)
│           └── GameViewport (SubViewport — stub, dark fill)
├── CombatLogOverlay (CanvasLayer — layer 1)
│   └── LogPanel (PanelContainer — anchored bottom, 120px max height)
│       └── LogText (RichTextLabel — single continuous BBCode stream)
└── ResultOverlay (CanvasLayer — layer 2)
    └── ResultLabel (Label — centered, large font, hidden until combat ends)
```

### Removed from Current Layout

- `MarginContainer` wrapper (no outer margins — HUD goes edge-to-edge)
- `ActionBar` with AttackButton, StandButton, PrayButton (input is keyboard/mouse only)
- The old `CombatLogPanel` (PanelContainer with discrete RichTextLabel entries per line)

## Component Details

### TopStatusBar

- Height: 28px, full width
- Background: dark red (`#4a0e0e`), bottom border purple (`#8b3a8b`)
- Left side: `HealthLabel` showing `"HP /MaxHP Pct%"` in green
- Right side: `IdentityLabel` showing character identity
  - For now: just the character name (e.g., "Chaz")
  - Future: `"Name the Race Class of God"` with each part color-coded
- Font: monospace, ~13px

### ResourceCounters

- Height: 18px, full width
- Background: near-black (`#111111`), subtle bottom border
- Contains placeholder labels for future god resource system (e.g., `"[1] Gula 1 /20"`)
- For now: shows static placeholder text or is empty
- Font: monospace, ~11px

### Sidebar

- Width: 52px, full height of MainArea
- Background: near-black (`#0d0d0d`), right border
- **PortraitRect**: 40x48 placeholder rectangle with a border, centered horizontally
- **AbilitySlot**: 36x36 placeholder rectangle with purple border, centered horizontally
- Both use placeholder colored rectangles until art exists

### ViewportContainer / GameViewport

- Fills all remaining space after top bar, resource counters, and sidebar
- Uses `SubViewportContainer` + `SubViewport` so a tilemap can render here later
- For now: renders a solid dark color (`#1a1410`) as the "dungeon floor" stub
- The `SubViewport` size should match the container size (stretch enabled)

### CombatLogOverlay

- `CanvasLayer` on layer 1
- Contains a `PanelContainer` anchored to the bottom of the screen
- Semi-transparent black background (`rgba(0, 0, 0, 0.75)`)
- Max height: ~120px, anchored to bottom-left and bottom-right
- Left margin offset: 52px (to not overlap the sidebar)

#### LogText (RichTextLabel)

- Single continuous text stream with BBCode
- New entries are appended inline with ` * ` separator (matching Achra's style)
- Color coding by action/result type (same palette as current):
  - Kill: red
  - Miss: gray
  - Attack: white
  - Stand: yellow
  - Pray: purple
  - Entity names: green (enemies), orange (player)
  - Damage numbers: yellow bold
- Auto-scrolls to show most recent text
- This is a **rewrite of `CombatLogPanel`** — the new class should be called `CombatLogOverlay` (in `Scripts/UI/`), and it will be a `Control` that manages a single `RichTextLabel` instead of a `VBoxContainer` of individual labels
- **Scope note**: if the rewrite is large, it can be a separate implementation plan

### ResultOverlay

- `CanvasLayer` on layer 2 (above the log)
- Contains a centered `Label` for "Victory!" / "Defeat!"
- Font size: 32px, centered on screen
- Hidden until combat ends

## CombatScene.cs Changes

The script needs updates to match the new scene tree:

1. **Remove** references to `_attackButton`, `_standButton`, `_prayButton` and their signal connections
2. **Remove** `SetButtonsEnabled()` method
3. **Update** `GetNode` paths to match the new tree structure
4. **Replace** `_logPanel` (CombatLogPanel) with a reference to the new `CombatLogOverlay`
5. **Keep** `_UnhandledInput` as the sole input handler (already works for keyboard/mouse)
6. **Keep** all turn resolution, save/load, and persistence logic unchanged
7. **Update** `SubmitPlayerAction` to remove button enable/disable logic (guard via `_turnInProgress` flag only)
8. **Update** label references: `_playerNameLabel`/`_playerHealthLabel` become `_healthLabel`/`_identityLabel`; enemy info moves to the viewport layer later (for now, can be shown in the top bar or omitted)

### Enemy Info

In the Achra reference, enemy info is shown contextually in the game world (not in a fixed HUD position). For now, since we have no world view:
- Add `EnemyLabel` to `TopStatusBar`, right-aligned, showing `"EnemyName HP/MaxHP"`
- Styled in red/orange to contrast with the green player health
- This is temporary — enemy info will move to the viewport when the tilemap exists

## Styling

- No Godot Theme resource for now — styling is inline in the `.tscn` via `theme_override_*` properties
- Colors follow the Achra palette: dark reds, purples, greens, yellows on near-black backgrounds
- Font: Godot's default monospace or a pixel font if one is added to Assets/Fonts later

## What This Does NOT Include

- Tilemap or dungeon rendering (viewport is a stub)
- Character sprite rendering
- Race/Class/God system (identity label shows name only)
- God resource mechanics (counter row is placeholder)
- Art assets (all slots use placeholder rectangles)
- Combat log rewrite may be split into a separate plan if scope warrants it
