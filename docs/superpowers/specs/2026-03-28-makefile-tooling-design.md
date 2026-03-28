# Path of Chaz — Makefile & Project Tooling Design

## Goal

A Makefile at the repo root as the single entry point for build, run, test, versioning, and compound jj operations. Thin wrappers only — don't abstract simple commands.

## Files

| File | Purpose |
|------|---------|
| `Makefile` | All project targets |
| `VERSION` | Single line containing the current version string (e.g., `0.0.1`) |

## Targets

### Build & Run

| Target | Command | Description |
|--------|---------|-------------|
| `make build` | `dotnet build` | Build the C# project |
| `make run` | `godot --path src/` | Launch Godot with the project |
| `make test` | `dotnet test` | Run xUnit tests |

### jj Workflow (compound commands only)

| Target | Command | Description |
|--------|---------|-------------|
| `make push b=<name>` | `jj bookmark create $(b) -r @ && jj git push --bookmark $(b)` | Create bookmark and push |
| `make sync` | `jj git fetch && jj rebase -d main` | Fetch remote and rebase onto main |

Simple jj commands (`jj diff`, `jj log`, `jj status`, `jj new`) are not wrapped — use them directly.

### Versioning

| Target | Command | Description |
|--------|---------|-------------|
| `make version` | `cat VERSION` | Print current version |
| `make tag v=<version>` | Write `v` to `VERSION`, describe the current jj change as `release: v$(v)`, create git tag `v$(v)` | Tag a version |
| `make release v=<version>` | Run `tag`, push bookmark, `gh release create v$(v)` | Tag + push + GitHub release |

### Help

| Target | Description |
|--------|-------------|
| `make help` | Default target. Parses `## comment` annotations and prints a table of all targets. |

## Constraints

- All targets are `.PHONY`
- Targets requiring arguments (`tag`, `release`, `push`) validate that the argument is set and error with a message if missing
- `make help` is the default target (first in file)
- VERSION file is created with `0.0.0` initially — first real tag will be `0.0.1`

## Explicit Non-Goals

- No version bumping logic (patch/minor/major) — pass the version explicitly
- No wrapping simple jj commands
- No complex build pipelines or CI integration
