# SQLite run.db Foundation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add SQLite persistence that saves and loads combat encounter state via `run.db`.

**Architecture:** Pure C# `RunDatabase` class wraps `Microsoft.Data.Sqlite`, stores a single-row `run_state` table. `CombatScene` (Godot layer) calls Save/Load/Delete at the right lifecycle points. All persistence logic is engine-independent and unit-testable.

**Tech Stack:** C# / .NET 8, Microsoft.Data.Sqlite, xUnit

**Spec:** `docs/superpowers/specs/2026-03-28-sqlite-run-db-design.md`

---

## File Map

| Action | File | Responsibility |
|--------|------|---------------|
| Create | `src/Scripts/Core/RunState.cs` | Plain data class for run state |
| Create | `src/Scripts/Core/RunDatabase.cs` | SQLite persistence (Initialize, Save, Load, Delete) |
| Modify | `src/Scripts/Core/Combatant.cs` | Add constructor overload for restoring saved health |
| Modify | `src/PathOfChaz.csproj` | Add Microsoft.Data.Sqlite dependency |
| Modify | `tests/PathOfChaz.Tests.csproj` | Add Microsoft.Data.Sqlite dependency |
| Create | `tests/RunDatabaseTests.cs` | Unit tests for persistence layer |
| Modify | `src/Scripts/Core/CombatScene.cs` | Wire Save/Load/Delete into combat lifecycle |

---

### Task 1: Add NuGet Dependencies

**Files:**
- Modify: `src/PathOfChaz.csproj`
- Modify: `tests/PathOfChaz.Tests.csproj`

- [ ] **Step 1: Add Microsoft.Data.Sqlite to the game project**

In `src/PathOfChaz.csproj`, add an `<ItemGroup>` with the package reference:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.3" />
</ItemGroup>
```

- [ ] **Step 2: Add Microsoft.Data.Sqlite to the test project**

In `tests/PathOfChaz.Tests.csproj`, add to the existing `<ItemGroup>` containing package references:

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.3" />
```

- [ ] **Step 3: Restore packages**

Run: `dotnet restore`
Expected: Success, no errors.

- [ ] **Step 4: Commit**

```bash
jj describe -m "chore: add Microsoft.Data.Sqlite NuGet dependency" && jj new
```

---

### Task 2: Create RunState Data Class (TDD)

**Files:**
- Create: `src/Scripts/Core/RunState.cs`
- Create: `tests/RunDatabaseTests.cs` (started here, expanded in Task 3)

- [ ] **Step 1: Write a test that constructs RunState**

Create `tests/RunDatabaseTests.cs`:

```csharp
using PathOfChaz.Core;

namespace PathOfChaz.Tests;

public class RunDatabaseTests
{
    [Fact]
    public void RunState_RoundTripsProperties()
    {
        var now = DateTime.UtcNow;
        var state = new RunState
        {
            PlayerHealth = 15,
            EnemyHealth = 7,
            TurnCount = 3,
            RngSeed = 42L,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Assert.Equal(15, state.PlayerHealth);
        Assert.Equal(7, state.EnemyHealth);
        Assert.Equal(3, state.TurnCount);
        Assert.Equal(42L, state.RngSeed);
        Assert.Equal(now, state.CreatedAt);
        Assert.Equal(now, state.UpdatedAt);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/ --filter RunState_RoundTripsProperties`
Expected: FAIL — `RunState` does not exist.

- [ ] **Step 3: Create RunState.cs**

Create `src/Scripts/Core/RunState.cs`:

```csharp
using System;

namespace PathOfChaz.Core;

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

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/ --filter RunState_RoundTripsProperties`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
jj describe -m "feat: add RunState data class with tests" && jj new
```

---

### Task 3: Implement RunDatabase — Initialize (TDD)

**Files:**
- Create: `src/Scripts/Core/RunDatabase.cs`
- Modify: `tests/RunDatabaseTests.cs`

- [ ] **Step 1: Write Initialize test**

Add to `tests/RunDatabaseTests.cs`:

```csharp
[Fact]
public void Initialize_CreatesTablesAndSetsSchemaVersion()
{
    var path = Path.GetTempFileName();
    try
    {
        using var db = new RunDatabase(path);
        db.Initialize();

        // Verify schema_version table exists and has version 1
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT version FROM schema_version";
        var version = Convert.ToInt32(cmd.ExecuteScalar());
        Assert.Equal(1, version);
    }
    finally
    {
        File.Delete(path);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/ --filter Initialize_CreatesTablesAndSetsSchemaVersion`
Expected: FAIL — `RunDatabase` does not exist.

- [ ] **Step 3: Implement RunDatabase with Initialize**

Create `src/Scripts/Core/RunDatabase.cs`:

```csharp
using System;
using Microsoft.Data.Sqlite;

namespace PathOfChaz.Core;

public class RunDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public RunDatabase(string dbPath)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();
    }

    public void Initialize()
    {
        using var transaction = _connection.BeginTransaction();

        using var createSchema = _connection.CreateCommand();
        createSchema.Transaction = transaction;
        createSchema.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_version (
                version INTEGER NOT NULL
            );
            """;
        createSchema.ExecuteNonQuery();

        using var checkVersion = _connection.CreateCommand();
        checkVersion.Transaction = transaction;
        checkVersion.CommandText = "SELECT COUNT(*) FROM schema_version";
        var count = Convert.ToInt32(checkVersion.ExecuteScalar());

        if (count == 0)
        {
            using var insertVersion = _connection.CreateCommand();
            insertVersion.Transaction = transaction;
            insertVersion.CommandText = "INSERT INTO schema_version VALUES (1)";
            insertVersion.ExecuteNonQuery();
        }

        using var createRunState = _connection.CreateCommand();
        createRunState.Transaction = transaction;
        createRunState.CommandText = """
            CREATE TABLE IF NOT EXISTS run_state (
                player_health INTEGER NOT NULL,
                enemy_health  INTEGER NOT NULL,
                turn_count    INTEGER NOT NULL,
                rng_seed      INTEGER NOT NULL,
                created_at    TEXT NOT NULL,
                updated_at    TEXT NOT NULL
            );
            """;
        createRunState.ExecuteNonQuery();

        transaction.Commit();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/ --filter Initialize_CreatesTablesAndSetsSchemaVersion`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
jj describe -m "feat: add RunDatabase with Initialize (creates schema)" && jj new
```

---

### Task 4: Implement Save and Load (TDD)

**Files:**
- Modify: `src/Scripts/Core/RunDatabase.cs`
- Modify: `tests/RunDatabaseTests.cs`

- [ ] **Step 1: Write Save/Load round-trip test**

Add to `tests/RunDatabaseTests.cs`:

```csharp
[Fact]
public void Save_ThenLoad_RoundTripsAllFields()
{
    var path = Path.GetTempFileName();
    try
    {
        using var db = new RunDatabase(path);
        db.Initialize();

        var saved = new RunState
        {
            PlayerHealth = 15,
            EnemyHealth = 7,
            TurnCount = 3,
            RngSeed = 42L,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        db.Save(saved);
        var loaded = db.Load();

        Assert.NotNull(loaded);
        Assert.Equal(15, loaded.PlayerHealth);
        Assert.Equal(7, loaded.EnemyHealth);
        Assert.Equal(3, loaded.TurnCount);
        Assert.Equal(42L, loaded.RngSeed);
        Assert.Equal(saved.CreatedAt, loaded.CreatedAt);
        Assert.Equal(saved.UpdatedAt, loaded.UpdatedAt);
    }
    finally
    {
        File.Delete(path);
    }
}
```

- [ ] **Step 2: Write Load-returns-null test**

Add to `tests/RunDatabaseTests.cs`:

```csharp
[Fact]
public void Load_ReturnsNull_WhenNoRunExists()
{
    var path = Path.GetTempFileName();
    try
    {
        using var db = new RunDatabase(path);
        db.Initialize();

        var loaded = db.Load();

        Assert.Null(loaded);
    }
    finally
    {
        File.Delete(path);
    }
}
```

- [ ] **Step 3: Write Save-twice-upserts test**

Add to `tests/RunDatabaseTests.cs`:

```csharp
[Fact]
public void Save_Twice_UpsertsInsteadOfDuplicating()
{
    var path = Path.GetTempFileName();
    try
    {
        using var db = new RunDatabase(path);
        db.Initialize();

        var state = new RunState
        {
            PlayerHealth = 20,
            EnemyHealth = 10,
            TurnCount = 1,
            RngSeed = 99L,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        db.Save(state);

        state.PlayerHealth = 12;
        state.TurnCount = 5;
        state.UpdatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        db.Save(state);

        var loaded = db.Load();
        Assert.NotNull(loaded);
        Assert.Equal(12, loaded.PlayerHealth);
        Assert.Equal(5, loaded.TurnCount);
    }
    finally
    {
        File.Delete(path);
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run: `dotnet test tests/ --filter "Save_ThenLoad|Load_ReturnsNull|Save_Twice"`
Expected: FAIL — `Save` and `Load` methods don't exist.

- [ ] **Step 5: Implement Save and Load**

Add these methods to `RunDatabase` in `src/Scripts/Core/RunDatabase.cs`, after `Initialize()`:

```csharp
public void Save(RunState state)
{
    using var transaction = _connection.BeginTransaction();

    using var delete = _connection.CreateCommand();
    delete.Transaction = transaction;
    delete.CommandText = "DELETE FROM run_state";
    delete.ExecuteNonQuery();

    using var insert = _connection.CreateCommand();
    insert.Transaction = transaction;
    insert.CommandText = """
        INSERT INTO run_state (player_health, enemy_health, turn_count, rng_seed, created_at, updated_at)
        VALUES (@ph, @eh, @tc, @rs, @ca, @ua)
        """;
    insert.Parameters.AddWithValue("@ph", state.PlayerHealth);
    insert.Parameters.AddWithValue("@eh", state.EnemyHealth);
    insert.Parameters.AddWithValue("@tc", state.TurnCount);
    insert.Parameters.AddWithValue("@rs", state.RngSeed);
    insert.Parameters.AddWithValue("@ca", state.CreatedAt.ToString("o"));
    insert.Parameters.AddWithValue("@ua", state.UpdatedAt.ToString("o"));
    insert.ExecuteNonQuery();

    transaction.Commit();
}

public RunState? Load()
{
    using var cmd = _connection.CreateCommand();
    cmd.CommandText = "SELECT player_health, enemy_health, turn_count, rng_seed, created_at, updated_at FROM run_state LIMIT 1";
    using var reader = cmd.ExecuteReader();

    if (!reader.Read())
        return null;

    return new RunState
    {
        PlayerHealth = reader.GetInt32(0),
        EnemyHealth = reader.GetInt32(1),
        TurnCount = reader.GetInt32(2),
        RngSeed = reader.GetInt64(3),
        CreatedAt = DateTime.Parse(reader.GetString(4)).ToUniversalTime(),
        UpdatedAt = DateTime.Parse(reader.GetString(5)).ToUniversalTime(),
    };
}
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test tests/ --filter "Save_ThenLoad|Load_ReturnsNull|Save_Twice"`
Expected: all 3 PASS.

- [ ] **Step 7: Commit**

```bash
jj describe -m "feat: add RunDatabase Save and Load methods" && jj new
```

---

### Task 5: Implement Delete (TDD)

**Files:**
- Modify: `src/Scripts/Core/RunDatabase.cs`
- Modify: `tests/RunDatabaseTests.cs`

- [ ] **Step 1: Write Delete test**

Add to `tests/RunDatabaseTests.cs`:

```csharp
[Fact]
public void Delete_RemovesActiveRun()
{
    var path = Path.GetTempFileName();
    try
    {
        using var db = new RunDatabase(path);
        db.Initialize();

        var state = new RunState
        {
            PlayerHealth = 20,
            EnemyHealth = 10,
            TurnCount = 1,
            RngSeed = 1L,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Save(state);
        db.Delete();

        Assert.Null(db.Load());
    }
    finally
    {
        File.Delete(path);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/ --filter Delete_RemovesActiveRun`
Expected: FAIL — `Delete` method does not exist.

- [ ] **Step 3: Implement Delete**

Add to `RunDatabase` in `src/Scripts/Core/RunDatabase.cs`, after `Load()`:

```csharp
public void Delete()
{
    using var cmd = _connection.CreateCommand();
    cmd.CommandText = "DELETE FROM run_state";
    cmd.ExecuteNonQuery();
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/ --filter Delete_RemovesActiveRun`
Expected: PASS

- [ ] **Step 5: Run all tests**

Run: `dotnet test`
Expected: All tests pass (existing 31 + new 5).

- [ ] **Step 6: Commit**

```bash
jj describe -m "feat: add RunDatabase Delete method" && jj new
```

---

### Task 6: Add Combatant Health Restoration Constructor

**Files:**
- Modify: `src/Scripts/Core/Combatant.cs`
- Modify: `tests/CombatantTests.cs`

To restore a saved run, we need to construct a `Combatant` with a specific current health (not always `MaxHealth`). Currently, the constructor sets `Health = maxHealth`. Add a second constructor that accepts `currentHealth`.

- [ ] **Step 1: Write test for health-restoration constructor**

Add to `tests/CombatantTests.cs`:

```csharp
[Fact]
public void Constructor_WithCurrentHealth_SetsHealthCorrectly()
{
    var c = new Combatant("Chaz", 20, 12, 8, 3, 100);

    Assert.Equal(20, c.MaxHealth);
    Assert.Equal(12, c.Health);
    Assert.Equal("Chaz", c.Name);
    Assert.Equal(8, c.Attack);
    Assert.Equal(3, c.Defense);
    Assert.Equal(100, c.Accuracy);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/ --filter Constructor_WithCurrentHealth`
Expected: FAIL — no matching constructor.

- [ ] **Step 3: Add constructor overload**

In `src/Scripts/Core/Combatant.cs`, add a second constructor after the existing one:

```csharp
public Combatant(string name, int maxHealth, int currentHealth, int attack, int defense, int accuracy)
{
    Name = name;
    MaxHealth = maxHealth;
    Health = currentHealth;
    Attack = attack;
    Defense = defense;
    Accuracy = accuracy;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/ --filter Constructor_WithCurrentHealth`
Expected: PASS

- [ ] **Step 5: Run all tests**

Run: `dotnet test`
Expected: All pass.

- [ ] **Step 6: Commit**

```bash
jj describe -m "feat: add Combatant constructor overload for save restoration" && jj new
```

---

### Task 7: Wire Persistence into CombatScene

**Files:**
- Modify: `src/Scripts/Core/CombatScene.cs`

This task modifies the Godot layer only. No unit tests — this is integration code tested by running the game.

- [ ] **Step 1: Add RunDatabase field and initialization**

In `src/Scripts/Core/CombatScene.cs`, add a field alongside the other private fields:

```csharp
private RunDatabase _runDb = null!;
private long _rngSeed;
private DateTime _runCreatedAt;
```

- [ ] **Step 2: Modify _Ready to load or create a run**

Replace the combatant creation and turn system setup block in `_Ready()`. The current code (lines 39-50):

```csharp
var playerData = GD.Load<CharacterData>("res://Resources/Characters/Chaz.tres");
var enemyData = GD.Load<CharacterData>("res://Resources/Characters/Goblin.tres");

_player = new Combatant(
    playerData.CharacterName, playerData.BaseHealth,
    playerData.Attack, playerData.Defense, playerData.Accuracy);
_enemy = new Combatant(
    enemyData.CharacterName, enemyData.BaseHealth,
    enemyData.Attack, enemyData.Defense, enemyData.Accuracy);

_combatLog = new CombatLog();
_turnSystem = new TurnSystem(_player, _enemy, _combatLog);
```

Replace with:

```csharp
var playerData = GD.Load<CharacterData>("res://Resources/Characters/Chaz.tres");
var enemyData = GD.Load<CharacterData>("res://Resources/Characters/Goblin.tres");

var dbPath = ProjectSettings.GlobalizePath("user://run.db");
_runDb = new RunDatabase(dbPath);
_runDb.Initialize();

_combatLog = new CombatLog();
var savedRun = _runDb.Load();

if (savedRun != null)
{
    _rngSeed = savedRun.RngSeed;
    _runCreatedAt = savedRun.CreatedAt;
    _player = new Combatant(
        playerData.CharacterName, playerData.BaseHealth, savedRun.PlayerHealth,
        playerData.Attack, playerData.Defense, playerData.Accuracy);
    _enemy = new Combatant(
        enemyData.CharacterName, enemyData.BaseHealth, savedRun.EnemyHealth,
        enemyData.Attack, enemyData.Defense, enemyData.Accuracy);
    _turnSystem = new TurnSystem(_player, _enemy, _combatLog, new Random((int)_rngSeed));
    _turnSystem.TurnNumber = savedRun.TurnCount;
}
else
{
    _rngSeed = Random.Shared.NextInt64();
    _runCreatedAt = DateTime.UtcNow;
    _player = new Combatant(
        playerData.CharacterName, playerData.BaseHealth,
        playerData.Attack, playerData.Defense, playerData.Accuracy);
    _enemy = new Combatant(
        enemyData.CharacterName, enemyData.BaseHealth,
        enemyData.Attack, enemyData.Defense, enemyData.Accuracy);
    _turnSystem = new TurnSystem(_player, _enemy, _combatLog, new Random((int)_rngSeed));
}
```

**Note:** This requires `TurnSystem.TurnNumber` to have a public setter. Add `{ get; set; }` to the `TurnNumber` property in `TurnSystem.cs` (change `private set` to `set`).

- [ ] **Step 3: Make TurnNumber publicly settable**

In `src/Scripts/Core/TurnSystem.cs`, change:

```csharp
public int TurnNumber { get; private set; } = 1;
```

to:

```csharp
public int TurnNumber { get; set; } = 1;
```

- [ ] **Step 4: Add Save call after each continuing turn**

In `SubmitPlayerAction`, after `UpdateLabels()` and `_logPanel.UpdateFromLog(_combatLog)`, but before the `await`, add the save logic. In the `default` case (TurnResult.Continue), save state. Replace the switch block:

```csharp
switch (result)
{
    case TurnResult.EnemyDead:
        _resultLabel.Text = "Victory!";
        _resultLabel.Visible = true;
        _combatOver = true;
        _runDb.Delete();
        break;
    case TurnResult.PlayerDead:
        _resultLabel.Text = "Defeat!";
        _resultLabel.Visible = true;
        _combatOver = true;
        _runDb.Delete();
        break;
    case TurnResult.Invalid:
        GD.PrintErr("CombatScene: TurnResult.Invalid — possible double-submit");
        break;
    default:
        _runDb.Save(new RunState
        {
            PlayerHealth = _player.Health,
            EnemyHealth = _enemy.Health,
            TurnCount = _turnSystem.TurnNumber,
            RngSeed = _rngSeed,
            CreatedAt = _runCreatedAt,
            UpdatedAt = DateTime.UtcNow,
        });
        SetButtonsEnabled(true);
        break;
}
```

- [ ] **Step 5: Dispose database on exit**

Add to `CombatScene.cs`:

```csharp
public override void _ExitTree()
{
    _runDb.Dispose();
}
```

- [ ] **Step 6: Run all unit tests to confirm no regression**

Run: `dotnet test`
Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
jj describe -m "feat: wire RunDatabase into CombatScene (save/load/delete)" && jj new
```

---

### Task 8: Manual Smoke Test

- [ ] **Step 1: Start the game fresh**

Run: `godot --path src/`
- Delete `user://run.db` if it exists (check `~/.local/share/godot/app_userdata/PathOfChaz/`)
- Start the game, attack a couple times
- Close the game mid-combat

- [ ] **Step 2: Resume the game**

Run: `godot --path src/`
- Verify health values match where you left off
- Verify turn count is restored

- [ ] **Step 3: Finish the fight**

- Win or lose the fight
- Close and reopen
- Verify a new run starts (run.db was deleted on death/victory)

- [ ] **Step 4: Final commit**

```bash
jj describe -m "docs: SQLite run.db implementation complete (closes #7)" && jj new
```
