# Path of Chaz

A turn-based roguelike inspired by [Path of Achra](https://store.steampowered.com/app/2128270/Path_of_Achra/). One-click gameplay, deep character customization, emergent strategy, and permanent death with legacy progression.

## Status

Early prototype / pre-production.

## Tech Stack

- **Engine:** Godot 4
- **Language:** C#
- **Platforms:** Windows, Linux, Mac (mobile planned)

## Prerequisites

- [Godot 4](https://godotengine.org/) with .NET/C# support
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Aseprite](https://www.aseprite.org/) (for sprite editing, optional)

## Getting Started

```bash
# Clone the repo
git clone <repo-url>
cd path-of-chaz

# Open in Godot
godot --path src/

# Run tests
dotnet test
```

## Project Structure

```
path-of-chaz/
├── docs/           # Design documents and specs
├── src/            # Godot project root
│   ├── Scenes/     # Scene files
│   ├── Scripts/    # C# game code
│   ├── Resources/  # Data definitions (.tres)
│   └── Assets/     # Sprites, audio, fonts
└── PathOfChaz.sln  # C# solution
```

## Design Pillars

- **One-click gameplay** — simple inputs, deep consequences
- **Emergent strategy** — win conditions shift as your run evolves
- **Tiered character identity** — Race, Class, and God each fundamentally shape your build
- **Grand strategy arc** — progress from lone infantry to a conqueror
- **Meaningful death** — permanent, but legacy systems reward the journey

## License

TBD
