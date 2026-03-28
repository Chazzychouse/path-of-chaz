# Combat HUD Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the scaffold form-style combat UI with a Path of Achra-style HUD: docked top bar + sidebar, stubbed game viewport, and floating combat log overlay.

**Architecture:** Hybrid layout — top status bar, resource counters, and sidebar are docked Godot containers. The game viewport fills remaining space via SubViewportContainer (stubbed dark for now). Combat log and result text are CanvasLayer overlays floating above the viewport. No visible action buttons — input is keyboard/mouse only.

**Tech Stack:** Godot 4, C#, xUnit

**Spec:** `docs/superpowers/specs/2026-03-28-combat-hud-layout-design.md`

**Reference:** `docs/path-of-achra-reference.png`

---

### Task 1: Create CombatLogOverlay Script

Replace the old `CombatLogPanel` (discrete RichTextLabel per entry) with a new `CombatLogOverlay` that renders a single continuous BBCode text stream with `*` separators between entries.

**Files:**
- Create: `src/Scripts/UI/CombatLogOverlay.cs`
- Create: `tests/CombatLogOverlayFormatTests.cs`

- [ ] **Step 1: Write formatting tests**

The `CombatLogOverlay` will depend on Godot (`RichTextLabel`), but the BBCode formatting logic can be tested as a pure static method. Write tests for the format function.

```csharp
// tests/CombatLogOverlayFormatTests.cs
using PathOfChaz.Core;
using PathOfChaz.UI;
using Xunit;

namespace PathOfChaz.Tests;

public class CombatLogOverlayFormatTests
{
    [Fact]
    public void FormatEntry_Attack_Hit_ReturnsWhiteWithDamage()
    {
        var entry = new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Hit, 5,
            "Chaz attacks Goblin for 5 damage");

        var result = CombatLogOverlay.FormatEntry(entry);

        Assert.Contains("[color=white]", result);
        Assert.Contains("Chaz attacks Goblin for 5 damage", result);
    }

    [Fact]
    public void FormatEntry_Attack_Miss_ReturnsGray()
    {
        var entry = new CombatLogEntry(2, "Goblin", "Chaz", CombatAction.Attack, ActionResult.Miss, 0,
            "Goblin misses Chaz");

        var result = CombatLogOverlay.FormatEntry(entry);

        Assert.Contains("[color=gray]", result);
    }

    [Fact]
    public void FormatEntry_Attack_Kill_ReturnsRed()
    {
        var entry = new CombatLogEntry(3, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Kill, 10,
            "Chaz kills Goblin");

        var result = CombatLogOverlay.FormatEntry(entry);

        Assert.Contains("[color=red]", result);
    }

    [Fact]
    public void FormatEntry_Stand_ReturnsYellow()
    {
        var entry = new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Stand, ActionResult.Hit, 0,
            "Chaz stands firm");

        var result = CombatLogOverlay.FormatEntry(entry);

        Assert.Contains("[color=yellow]", result);
    }

    [Fact]
    public void FormatEntry_Pray_ReturnsPurple()
    {
        var entry = new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Pray, ActionResult.Hit, 0,
            "Chaz prays");

        var result = CombatLogOverlay.FormatEntry(entry);

        Assert.Contains("[color=purple]", result);
    }

    [Fact]
    public void FormatEntries_MultipleEntries_JoinedWithStarSeparator()
    {
        var entries = new[]
        {
            new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Hit, 5,
                "Chaz attacks for 5"),
            new CombatLogEntry(1, "Goblin", "Chaz", CombatAction.Attack, ActionResult.Miss, 0,
                "Goblin misses"),
        };

        var result = CombatLogOverlay.FormatEntries(entries);

        Assert.Contains(" * ", result);
        Assert.Contains("Chaz attacks for 5", result);
        Assert.Contains("Goblin misses", result);
    }

    [Fact]
    public void FormatEntries_EmptyList_ReturnsEmptyString()
    {
        var result = CombatLogOverlay.FormatEntries(Array.Empty<CombatLogEntry>());

        Assert.Equal("", result);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/ --filter "CombatLogOverlayFormatTests" -v quiet`
Expected: Build failure — `CombatLogOverlay` does not exist yet.

- [ ] **Step 3: Write CombatLogOverlay**

```csharp
// src/Scripts/UI/CombatLogOverlay.cs
using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using PathOfChaz.Core;

namespace PathOfChaz.UI;

public partial class CombatLogOverlay : PanelContainer
{
    private RichTextLabel _logText = null!;
    private int _lastEntryCount;

    public override void _Ready()
    {
        _logText = GetNode<RichTextLabel>("LogText");
    }

    public void UpdateFromLog(CombatLog log)
    {
        if (log.Entries.Count == _lastEntryCount)
            return;

        var newEntries = new List<CombatLogEntry>();
        for (int i = _lastEntryCount; i < log.Entries.Count; i++)
            newEntries.Add(log.Entries[i]);

        var formatted = FormatEntries(newEntries);

        if (_lastEntryCount > 0 && formatted.Length > 0)
            _logText.Text += " * " + formatted;
        else
            _logText.Text += formatted;

        _lastEntryCount = log.Entries.Count;
        CallDeferred(nameof(ScrollToBottom));
    }

    private void ScrollToBottom()
    {
        _logText.ScrollToLine(_logText.GetLineCount() - 1);
    }

    public static string FormatEntries(IReadOnlyList<CombatLogEntry> entries)
    {
        if (entries.Count == 0)
            return "";

        var sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0)
                sb.Append(" * ");
            sb.Append(FormatEntry(entries[i]));
        }
        return sb.ToString();
    }

    public static string FormatEntry(CombatLogEntry entry)
    {
        var color = entry.Result switch
        {
            ActionResult.Kill => "red",
            ActionResult.Miss => "gray",
            _ => entry.Action switch
            {
                CombatAction.Attack => "white",
                CombatAction.Stand => "yellow",
                CombatAction.Pray => "purple",
                _ => "white",
            },
        };

        return $"[color={color}]{entry.Description}[/color]";
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/ --filter "CombatLogOverlayFormatTests" -v quiet`
Expected: All 7 tests pass.

- [ ] **Step 5: Commit**

```bash
jj new
# (write code)
jj describe -m "feat: add CombatLogOverlay with continuous BBCode text stream"
jj new
```

---

### Task 2: Rebuild Main.tscn with HUD Layout

Replace the entire scaffold scene with the new scene tree. This is a full rewrite of the `.tscn` file.

**Files:**
- Modify: `src/Scenes/Core/Main.tscn`

- [ ] **Step 1: Write the new scene file**

Replace `src/Scenes/Core/Main.tscn` with:

```
[gd_scene load_steps=3 format=3 uid="uid://main_scene"]

[ext_resource type="Script" path="res://Scripts/Core/CombatScene.cs" id="1"]
[ext_resource type="Script" path="res://Scripts/UI/CombatLogOverlay.cs" id="2"]

[node name="CombatScene" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1")

[node name="VBox" type="VBoxContainer" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2

[node name="TopStatusBar" type="HBoxContainer" parent="VBox"]
layout_mode = 2
custom_minimum_size = Vector2(0, 28)
theme_override_constants/separation = 12

[node name="TopStatusBarBg" type="ColorRect" parent="VBox/TopStatusBar"]
layout_mode = 2
size_flags_horizontal = 3
size_flags_vertical = 3
color = Color(0.29, 0.055, 0.055, 1)
z_index = -1

[node name="HealthLabel" type="Label" parent="VBox/TopStatusBar"]
layout_mode = 2
theme_override_font_sizes/font_size = 13
theme_override_colors/font_color = Color(0.267, 0.8, 0.267, 1)
text = "0 /0 100%"

[node name="IdentityLabel" type="Label" parent="VBox/TopStatusBar"]
layout_mode = 2
size_flags_horizontal = 3
theme_override_font_sizes/font_size = 13
theme_override_colors/font_color = Color(1.0, 0.533, 0.0, 1)
text = "Chaz"

[node name="EnemyLabel" type="Label" parent="VBox/TopStatusBar"]
layout_mode = 2
horizontal_alignment = 2
theme_override_font_sizes/font_size = 13
theme_override_colors/font_color = Color(0.8, 0.267, 0.267, 1)
text = "Goblin 0/0"

[node name="ResourceCounters" type="HBoxContainer" parent="VBox"]
layout_mode = 2
custom_minimum_size = Vector2(0, 18)
theme_override_constants/separation = 40

[node name="ResourceCountersBg" type="ColorRect" parent="VBox/ResourceCounters"]
layout_mode = 2
size_flags_horizontal = 3
size_flags_vertical = 3
color = Color(0.067, 0.067, 0.067, 1)
z_index = -1

[node name="Placeholder1" type="Label" parent="VBox/ResourceCounters"]
layout_mode = 2
theme_override_font_sizes/font_size = 11
theme_override_colors/font_color = Color(0.8, 0.533, 0.267, 1)
text = "[1] Gula 0 /20"

[node name="Placeholder2" type="Label" parent="VBox/ResourceCounters"]
layout_mode = 2
theme_override_font_sizes/font_size = 11
theme_override_colors/font_color = Color(0.8, 0.533, 0.267, 1)
text = "[2] Umam 0 /5"

[node name="Placeholder3" type="Label" parent="VBox/ResourceCounters"]
layout_mode = 2
theme_override_font_sizes/font_size = 11
theme_override_colors/font_color = Color(0.8, 0.533, 0.267, 1)
text = "[3] Nam-Kalag 0 /2"

[node name="MainArea" type="HBoxContainer" parent="VBox"]
layout_mode = 2
size_flags_vertical = 3

[node name="Sidebar" type="VBoxContainer" parent="VBox/MainArea"]
layout_mode = 2
custom_minimum_size = Vector2(52, 0)
theme_override_constants/separation = 8

[node name="SidebarBg" type="ColorRect" parent="VBox/MainArea/Sidebar"]
layout_mode = 2
size_flags_horizontal = 3
size_flags_vertical = 3
color = Color(0.051, 0.051, 0.051, 1)
z_index = -1

[node name="PortraitRect" type="ColorRect" parent="VBox/MainArea/Sidebar"]
layout_mode = 2
custom_minimum_size = Vector2(40, 48)
size_flags_horizontal = 4
color = Color(0.227, 0.165, 0.102, 1)

[node name="AbilitySlot" type="ColorRect" parent="VBox/MainArea/Sidebar"]
layout_mode = 2
custom_minimum_size = Vector2(36, 36)
size_flags_horizontal = 4
color = Color(0.165, 0.102, 0.227, 1)

[node name="ViewportContainer" type="SubViewportContainer" parent="VBox/MainArea"]
layout_mode = 2
size_flags_horizontal = 3
size_flags_vertical = 3
stretch = true

[node name="GameViewport" type="SubViewport" parent="VBox/MainArea/ViewportContainer"]
handle_input_locally = false
render_target_clear_mode = 2
render_target_update_mode = 4

[node name="ViewportBg" type="ColorRect" parent="VBox/MainArea/ViewportContainer/GameViewport"]
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
color = Color(0.102, 0.078, 0.063, 1)

[node name="CombatLogOverlay" type="CanvasLayer" parent="."]
layer = 1

[node name="LogPanel" type="PanelContainer" parent="CombatLogOverlay"]
anchors_preset = 12
anchor_top = 1.0
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 52.0
offset_top = -120.0
grow_horizontal = 2
grow_vertical = 0
script = ExtResource("2")

[node name="LogText" type="RichTextLabel" parent="CombatLogOverlay/LogPanel"]
layout_mode = 2
bbcode_enabled = true
scroll_following = true
theme_override_font_sizes/font_size = 12

[node name="ResultOverlay" type="CanvasLayer" parent="."]
layer = 2

[node name="ResultLabel" type="Label" parent="ResultOverlay"]
anchors_preset = 8
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -100.0
offset_top = -20.0
offset_right = 100.0
offset_bottom = 20.0
grow_horizontal = 2
grow_vertical = 2
horizontal_alignment = 1
vertical_alignment = 1
theme_override_font_sizes/font_size = 32
visible = false
```

Note: The `TopStatusBarBg`, `ResourceCountersBg`, and `SidebarBg` `ColorRect` nodes are used as backgrounds because `HBoxContainer`/`VBoxContainer` don't have a direct background color property. They use `z_index = -1` to render behind siblings and `size_flags` to fill the container.

- [ ] **Step 2: Verify scene file is valid**

Run: `cd /home/chazzy/projects/games/path-of-chaz && grep -c '\[node' src/Scenes/Core/Main.tscn`
Expected: 22 (the number of nodes in the new scene)

- [ ] **Step 3: Commit**

```bash
jj describe -m "feat: rebuild Main.tscn with Achra-style HUD layout"
jj new
```

---

### Task 3: Update CombatScene.cs for New Scene Tree

Update the script to reference the new node paths, remove button logic, and use `CombatLogOverlay`.

**Files:**
- Modify: `src/Scripts/Core/CombatScene.cs`

- [ ] **Step 1: Rewrite CombatScene.cs**

```csharp
// src/Scripts/Core/CombatScene.cs
using System;
using Godot;
using PathOfChaz.Characters;
using PathOfChaz.Core;
using PathOfChaz.UI;

namespace PathOfChaz.Core;

public partial class CombatScene : Control
{
    private Label _healthLabel = null!;
    private Label _identityLabel = null!;
    private Label _enemyLabel = null!;
    private CombatLogOverlay _logOverlay = null!;
    private Label _resultLabel = null!;

    private TurnSystem _turnSystem = null!;
    private CombatLog _combatLog = null!;
    private Combatant _player = null!;
    private Combatant _enemy = null!;
    private bool _combatOver;
    private bool _turnInProgress;

    private RunDatabase _runDb = null!;
    private int _rngSeed;
    private DateTime _runCreatedAt;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("VBox/TopStatusBar/HealthLabel");
        _identityLabel = GetNode<Label>("VBox/TopStatusBar/IdentityLabel");
        _enemyLabel = GetNode<Label>("VBox/TopStatusBar/EnemyLabel");
        _logOverlay = GetNode<CombatLogOverlay>("CombatLogOverlay/LogPanel");
        _resultLabel = GetNode<Label>("ResultOverlay/ResultLabel");

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
            _turnSystem = new TurnSystem(_player, _enemy, _combatLog, new Random(_rngSeed));
            _turnSystem.TurnNumber = savedRun.TurnCount;
        }
        else
        {
            _rngSeed = Random.Shared.Next();
            _runCreatedAt = DateTime.UtcNow;
            _player = new Combatant(
                playerData.CharacterName, playerData.BaseHealth,
                playerData.Attack, playerData.Defense, playerData.Accuracy);
            _enemy = new Combatant(
                enemyData.CharacterName, enemyData.BaseHealth,
                enemyData.Attack, enemyData.Defense, enemyData.Accuracy);
            _turnSystem = new TurnSystem(_player, _enemy, _combatLog, new Random(_rngSeed));
        }

        UpdateLabels();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_combatOver || _turnInProgress || !@event.IsPressed())
            return;

        if (@event.IsAction("action_attack"))
        {
            SubmitPlayerAction(CombatAction.Attack);
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsAction("action_stand"))
        {
            SubmitPlayerAction(CombatAction.Stand);
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsAction("action_pray"))
        {
            SubmitPlayerAction(CombatAction.Pray);
            GetViewport().SetInputAsHandled();
        }
    }

    private async void SubmitPlayerAction(CombatAction action)
    {
        if (_combatOver || _turnInProgress)
            return;

        _turnInProgress = true;

        var result = _turnSystem.SubmitAction(action);
        UpdateLabels();
        _logOverlay.UpdateFromLog(_combatLog);

        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);

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
                break;
        }

        _turnInProgress = false;
    }

    public override void _ExitTree()
    {
        _runDb.Dispose();
    }

    private void UpdateLabels()
    {
        var pct = _player.MaxHealth > 0 ? _player.Health * 100 / _player.MaxHealth : 0;
        _healthLabel.Text = $"{_player.Health} /{_player.MaxHealth} {pct}%";
        _identityLabel.Text = _player.Name;
        _enemyLabel.Text = $"{_enemy.Name} {_enemy.Health}/{_enemy.MaxHealth}";
    }
}
```

Key changes from the original:
- Removed `_playerNameLabel`, `_playerHealthLabel`, `_enemyNameLabel`, `_enemyHealthLabel` → replaced with `_healthLabel`, `_identityLabel`, `_enemyLabel`
- Removed `_attackButton`, `_standButton`, `_prayButton` and their `Pressed` signal connections
- Removed `SetButtonsEnabled()` method
- Removed `_resultLabel.Visible = false` from `_Ready()` (now set in `.tscn`)
- Replaced `_logPanel` (`CombatLogPanel`) with `_logOverlay` (`CombatLogOverlay`)
- `UpdateLabels()` now shows Achra-style `"HP /MaxHP Pct%"` format
- `SubmitPlayerAction` no longer toggles button state

- [ ] **Step 2: Verify the project compiles**

Run: `cd /home/chazzy/projects/games/path-of-chaz && dotnet build src/PathOfChaz.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Run all tests to verify nothing is broken**

Run: `dotnet test tests/ -v quiet`
Expected: All existing tests pass. (Core logic is unchanged.)

- [ ] **Step 4: Commit**

```bash
jj describe -m "feat: update CombatScene.cs for Achra-style HUD layout"
jj new
```

---

### Task 4: Delete Old CombatLogPanel

Remove the old scaffold UI script that's no longer referenced.

**Files:**
- Delete: `src/Scripts/UI/CombatLogPanel.cs`

- [ ] **Step 1: Delete the file**

```bash
rm src/Scripts/UI/CombatLogPanel.cs
```

- [ ] **Step 2: Verify the project still compiles**

Run: `cd /home/chazzy/projects/games/path-of-chaz && dotnet build src/PathOfChaz.csproj`
Expected: Build succeeded. Nothing references `CombatLogPanel` anymore.

- [ ] **Step 3: Run tests**

Run: `dotnet test tests/ -v quiet`
Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
jj describe -m "chore: remove old CombatLogPanel scaffold"
jj new
```

---

### Task 5: Add .superpowers/ to .gitignore

The brainstorming session created a `.superpowers/` directory for visual companion mockups. This should not be tracked.

**Files:**
- Modify: `.gitignore`

- [ ] **Step 1: Verify .superpowers/ is in .gitignore**

Check that `.superpowers/` appears in `.gitignore`. If not, append it.

```bash
grep -q "^\.superpowers/" .gitignore || echo ".superpowers/" >> .gitignore
```

- [ ] **Step 2: Commit**

```bash
jj describe -m "chore: add .superpowers/ to gitignore"
jj new
```

---

### Task 6: Manual Smoke Test

Verify the new HUD layout renders correctly in Godot.

**Files:** None (manual verification)

- [ ] **Step 1: Open in Godot**

Run: `godot --path src/`

- [ ] **Step 2: Verify layout**

Check the following:
- Top status bar is visible with dark red background, showing player HP and enemy info
- Resource counters row is visible below the top bar with placeholder text
- Left sidebar is visible with portrait and ability placeholders
- Main viewport area shows a dark stub background
- No action buttons are visible

- [ ] **Step 3: Verify input and combat log**

- Press Tab (or left-click) to attack — combat log text should appear at the bottom of the screen as a continuous stream
- Press Space to stand, Enter to pray — log entries should append with `*` separators
- Play until victory or defeat — result label should appear centered on screen

- [ ] **Step 4: Verify save/load**

- Start a combat, take a few turns, then close and reopen — run should resume from saved state
- Win or lose — run.db should be deleted
