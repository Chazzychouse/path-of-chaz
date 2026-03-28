# Path of Chaz — Hello World Scaffold Design

## Goal

Bootstrap the Godot 4 + C# project so that `godot --path src/` opens and runs, `dotnet build` compiles, and the folder structure matches the architecture spec. This is the minimum viable proof that the engine, language, and project layout work together.

## What Gets Created

### Project Files

| File | Purpose |
|------|---------|
| `src/project.godot` | Godot 4 project file with C# enabled, pixel-art window settings |
| `src/PathOfChaz.csproj` | C# project targeting Godot 4 |
| `PathOfChaz.sln` | Solution file at repo root, references `src/PathOfChaz.csproj` |
| `.editorconfig` | C# naming conventions per project standards |

### Scene and Script

| File | Purpose |
|------|---------|
| `src/Scenes/Core/Main.tscn` | Main scene — a Control node with a centered Label saying "Path of Chaz" |
| `src/Scripts/Core/Main.cs` | Script on root node, prints "Path of Chaz is alive" in `_Ready()` |

### Empty Directory Structure

Created to match the architecture spec, ready for future work:

- `src/Scenes/UI/`
- `src/Scenes/World/`
- `src/Scripts/Characters/`
- `src/Scripts/Items/`
- `src/Scripts/World/`
- `src/Scripts/UI/`
- `src/Resources/`
- `src/Assets/Sprites/`
- `src/Assets/Audio/`
- `src/Assets/Fonts/`
- `src/Addons/`

## Explicit Non-Goals

- No turn system, combat, or game logic
- No SQLite or save system
- No unit test project (nothing to test yet)
- No resource definitions
- No input mapping beyond Godot defaults

## Success Criteria

1. `dotnet build` passes from repo root
2. `godot --path src/` opens without errors
3. Running the project displays the label and prints to the Godot output console

## Approach

Manual scaffold (Option 1) — all files created by hand to match the architecture spec exactly, rather than generating via Godot and reshaping.
