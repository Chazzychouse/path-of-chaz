# Path of Chaz

## Project Overview

Turn-based roguelike inspired by Path of Achra. Built with Godot 4 + C#.

## Stack

- **Engine:** Godot 4 (latest stable)
- **Language:** C#
- **VCS:** jj (Jujutsu) co-located with Git
- **Save system:** SQLite via Microsoft.Data.Sqlite
- **Art:** Aseprite -> PNG sprite exports
- **Testing:** xUnit for core logic, Godot scene tests for integration

## Project Structure

- `src/` — Godot project root
- `src/Scripts/` — All C# game code
- `src/Scenes/` — Godot scene files (.tscn)
- `src/Resources/` — Data definitions as Godot Resources (.tres)
- `src/Assets/` — Sprites, audio, fonts
- `docs/` — Design documents and specs
- `PathOfChaz.sln` — C# solution file (repo root)

## Architecture Rules

- **Core game logic must be engine-independent.** Turn resolution, stat computation, save/load, and legacy tracking go in pure C# classes with no Godot dependencies. This keeps them unit testable.
- **Game content is data-driven.** Races, classes, gods, items, and encounters are defined as Godot Resource files, not hardcoded in scripts.
- **Character system uses composition, not inheritance.** A character is Race + Class + God, each a Resource providing stat modifiers and abilities.
- **SQLite for all persistence.** Two databases: `run.db` (active run, deleted on death) and `legacy.db` (permanent cross-run progression). Both use a `schema_version` table.
- **Combat log is required.** Every turn resolution must emit structured log entries describing what happened and why. The player sees these in a scrollable UI panel.

## Code Conventions

- Follow standard C# naming: PascalCase for public members, camelCase for private fields with `_` prefix
- Godot node references use `[Export]` attributes, not magic strings
- One class per file, filename matches class name
- Scripts go in the subfolder matching their domain (Core/, Characters/, Items/, World/, UI/)
- Keep scripts under 300 lines — split if they grow beyond that

## When Modifying Code

- Do not add Godot dependencies to core logic classes
- Do not hardcode game content — create a Resource definition instead
- Run unit tests after modifying core logic: `dotnet test`
- Ensure combat log entries are emitted for any new combat mechanics

## Common Commands

```bash
# Run unit tests
dotnet test

# Open in Godot (from repo root)
godot --path src/

# Export (example)
godot --path src/ --export-release "Linux" builds/linux/PathOfChaz

# VCS (prefer jj over raw git)
jj log              # view history
jj new              # start new change
jj commit -m "msg"  # commit current change
jj bookmark track main --remote=gh  # track remote bookmark
```
