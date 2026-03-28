# SQLite run.db Foundation — Design Spec

**Issue:** #7
**Date:** 2026-03-28
**Depends on:** #2 (Turn system)

## Summary

Add SQLite persistence with a minimal `run.db` schema. Save and load combat encounter state to prove the persistence architecture works end-to-end.

## Dependencies

Add `Microsoft.Data.Sqlite` NuGet package to `src/PathOfChaz.csproj` and `tests/Tests.csproj`.

## Data Model

### RunState (pure C#, no Godot dependencies)

```csharp
public class RunState
{
    public int PlayerHealth { get; set; }
    public int EnemyHealth { get; set; }
    public int TurnCount { get; set; }
    public long RngSeed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

Single-row model — only one active run exists at a time.

## Schema (v1)

```sql
CREATE TABLE schema_version (version INTEGER NOT NULL);
INSERT INTO schema_version VALUES (1);

CREATE TABLE run_state (
    player_health INTEGER NOT NULL,
    enemy_health  INTEGER NOT NULL,
    turn_count    INTEGER NOT NULL,
    rng_seed      INTEGER NOT NULL,
    created_at    TEXT NOT NULL,
    updated_at    TEXT NOT NULL
);
```

- Dates stored as ISO 8601 text (SQLite convention).
- Single row, no primary key — there is only ever one active run.
- `schema_version` table enables future migrations.

## API

### RunDatabase (pure C#, no Godot dependencies)

```csharp
public class RunDatabase : IDisposable
{
    public RunDatabase(string dbPath)
    public void Initialize()
    public void Save(RunState state)
    public RunState? Load()
    public void Delete()
}
```

**Constructor:** Takes an OS file path. Godot layer resolves `user://run.db` via `ProjectSettings.GlobalizePath()` before passing it in. Tests pass a temp file path.

**Initialize():** Opens the connection, creates tables if they don't exist, verifies `schema_version`. Called once at startup.

**Save(RunState):** Upserts the single row. Wrapped in a transaction for atomicity. Sets `updated_at` to `DateTime.UtcNow`. On first save, also sets `created_at`.

**Load():** Returns the active run state, or `null` if no row exists. This lets callers branch cleanly between "resume" and "new run."

**Delete():** Deletes all rows from `run_state`. Called on player death or enemy death (run is over).

**Dispose():** Closes the SQLite connection.

## File Locations

- `src/Scripts/Core/RunState.cs` — data class
- `src/Scripts/Core/RunDatabase.cs` — persistence logic
- Both files are pure C# with no Godot dependencies.

## Integration

Integration happens in `CombatScene.cs` (the Godot layer):

- **On scene start (`_Ready`):** Call `Load()`. If non-null, reconstruct `Combatant` objects with saved health values and seed `Random` with saved `RngSeed`. If null, start a new run.
- **After each turn:** If `SubmitAction` returns `Continue`, call `Save()` with current state.
- **On death/victory:** Call `Delete()` to remove the run.

## Testing

xUnit tests in `tests/RunDatabaseTests.cs` using temp file paths (`Path.GetTempFileName()`).

Test cases:
- `Initialize` creates tables and sets schema version to 1
- `Save` then `Load` round-trips all fields correctly
- `Load` returns null when no run exists
- `Delete` removes the active run (subsequent `Load` returns null)
- `Save` twice overwrites (upsert, not duplicate rows)
- `Dispose` closes connection cleanly

## Constraints

- All persistence code is pure C# — no Godot dependencies
- Database path is injected via constructor (no hardcoded paths)
- Transactions for atomic saves
- Schema versioning from day one
