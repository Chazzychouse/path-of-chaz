# Path of Chaz — Stack & Architecture Design

## Stack Decision

**Engine:** Godot 4 with C#
**Platforms:** Windows, Linux, Mac (mobile later)
**Save format:** SQLite
**Art pipeline:** Aseprite -> PNG exports
**Testing:** xUnit/NUnit for core logic, Godot scene tests for integration
**Content definitions:** Godot Resource files (.tres)

### Why Godot 4 + C#

- Native export to all target platforms including eventual mobile
- First-class 2D pixel art pipeline (tilemaps, sprite animation, pixel-perfect rendering)
- C# provides strong typing for complex game systems
- Free, open source, no royalties
- Data-driven design via Godot Resources

### Why SQLite for saves

- Legacy system is inherently relational (deaths -> characters -> unlocks)
- Atomic transactions prevent save corruption on crash
- Queryable (e.g., "all deaths as this race")
- C# has excellent support via Microsoft.Data.Sqlite
- Single .db file per save — simple to sync later
- Run saves and legacy saves are separate databases

## Project Structure

```
path-of-chaz/
├── docs/                    # Design docs, specs
├── src/                     # Godot project root
│   ├── project.godot
│   ├── Scenes/              # .tscn scene files
│   │   ├── Core/            # Main game loop, turn manager
│   │   ├── UI/              # Menus, HUD, character sheets
│   │   └── World/           # Map, encounters, levels
│   ├── Scripts/             # C# scripts
│   │   ├── Core/            # Turn system, game state, save/load
│   │   ├── Characters/      # Race, class, god systems, stats
│   │   ├── Items/           # Equipment, consumables, cursed items
│   │   ├── World/           # Map generation, encounters, progression
│   │   └── UI/              # UI controllers
│   ├── Resources/           # Godot .tres resource files (data definitions)
│   ├── Assets/
│   │   ├── Sprites/         # Aseprite exports, tilesets
│   │   ├── Audio/
│   │   └── Fonts/
│   └── Addons/              # Third-party Godot plugins
├── PathOfChaz.sln           # C# solution at repo root for IDE support
└── .editorconfig
```

- Godot project in `src/` keeps repo root clean
- C# solution at repo root so IDEs find it naturally
- Scripts mirror Scenes for discoverability
- Resources folder for data-driven content definitions

## Core Systems

### Turn System

State machine with four states:

1. **WaitingForInput** — player picks action (attack, stand, pray, etc.)
2. **ResolvingTurn** — process player action, then enemies, then environment
3. **Animating** — play out results visually
4. **CheckState** — check for death, level-up, stage transitions, loop back

Input is deliberately simple (one-click). Complexity lives in turn resolution.

### Combat Log

A scrollable, persistent log visible to the player that records all combat resolution detail — every hit, miss, damage roll, buff application, status effect, and ability trigger. This is critical for a one-click game where the player needs to understand *what happened and why* after each turn resolves. The log lets players learn the system, debug their builds, and make informed strategic decisions.

Stored as an in-memory list during the run, rendered in a dedicated UI panel. Optionally persisted to the run save so players can review after reloading.

### Character System

Tiered composition — a character is composed of:

- **Race** — base stats, innate passives, visual identity
- **Class** — abilities, playstyle, stat scaling
- **God** — blessings, curses, divine abilities, alignment effects

Each tier is a Godot Resource defining stat modifiers, abilities, and passive effects. Final stats computed as the sum of all tiers plus run-acquired items/mutations.

You cannot win in character select, but informed choices make runs easier.

### Progression / Run System

A `Run` object holds:
- Current character (race + class + god + computed stats)
- Inventory
- Map state
- Current stage
- RNG seed

Stages represent the "lone infantry to Alexander the Great" arc:
- Each stage unlocks new systems (territory, resources, decisions)
- Stage transitions require the player to make increasingly important choices

### Legacy System

Deaths feed into a persistent Legacy database (separate from runs):
- Tracks death history, unlocks, achievements
- Unlocks include new races, classes, gods, starting bonuses
- Death is permanent for the run but meaningful for the meta

### Save Architecture

Two SQLite databases:
- **run.db** — current active run state, deleted on death
- **legacy.db** — persistent cross-run progression

Both are versioned with a `schema_version` table for future migrations. Designed to be cloud-syncable later (upload/download .db files, or row-level sync).

### Input Mapping

Three action styles mapped via Godot's InputMap:
- Keyboard: Tab, Space, Enter
- Mouse: Left click, Right click, Middle click
- Gamepad: mapped equivalents

Players can rebind via Godot's built-in input remapping.

## Testing Strategy

- **Core game logic** (turn resolution, stat computation, legacy tracking) in pure C# classes with no Godot dependencies — unit testable with xUnit/NUnit
- **Integration tests** via Godot's scene testing for UI and input flows
- **Save inspection** — SQLite databases browsable with any DB tool during dev

## Explicit Non-Goals (for now)

- Cloud sync infrastructure
- Mobile-specific input handling
- Procedural map generation beyond basic structure
- Audio system beyond placeholder hooks
- Online features (leaderboards, shared legacy)

The architecture supports all of these but none are implemented in the initial build.

## Design Pillars (from prototype doc)

Carried forward:
- **One-click gameplay** with multiple action styles
- **Emergent strategy** — win conditions shift as runs evolve
- **Tiered character customization** — Race, Class, God
- **Deeper character attachment** — longer multi-session runs
- **Grand strategy arc** — infantry to conqueror
- **Permanent death with legacy** — meaningful death, persistent progression
