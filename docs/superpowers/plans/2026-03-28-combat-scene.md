# Combat Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Wire existing turn system, combat log, resources, and player input into a playable single-encounter Godot scene.

**Architecture:** One scene (`Main.tscn`) with one script (`CombatScene.cs`) that instantiates the pure C# `TurnSystem`, `CombatLog`, and `Combatant` classes, connects them to Godot UI nodes, and handles player input via buttons and InputMap actions. No new core logic — pure integration wiring.

**Tech Stack:** Godot 4.6, C#, existing `PathOfChaz.Core` and `PathOfChaz.UI` classes

---

### Task 1: Add input map actions to project.godot

**Files:**
- Modify: `src/project.godot`

- [ ] **Step 1: Add input actions to project.godot**

Append an `[input]` section to `src/project.godot` with three actions:

```ini
[input]

action_attack={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194306,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
, Object(InputEventMouseButton,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"button_mask":1,"position":Vector2(0, 0),"global_position":Vector2(0, 0),"factor":1.0,"button_index":1,"canceled":false,"pressed":true,"double_click":false,"script":null)
]
}
action_stand={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":32,"key_label":0,"unicode":32,"location":0,"echo":false,"script":null)
, Object(InputEventMouseButton,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"button_mask":2,"position":Vector2(0, 0),"global_position":Vector2(0, 0),"factor":1.0,"button_index":2,"canceled":false,"pressed":true,"double_click":false,"script":null)
]
}
action_pray={
"deadzone": 0.2,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194309,"key_label":0,"unicode":0,"location":0,"echo":false,"script":null)
, Object(InputEventMouseButton,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"button_mask":4,"position":Vector2(0, 0),"global_position":Vector2(0, 0),"factor":1.0,"button_index":3,"canceled":false,"pressed":true,"double_click":false,"script":null)
]
}
```

Key codes: `4194306` = Tab, `32` = Space, `4194309` = Enter. Mouse button indices: `1` = Left, `2` = Right, `3` = Middle.

- [ ] **Step 2: Verify input actions load**

Run: `cd src && godot --headless --quit 2>&1 | head -20`

Expected: Godot exits cleanly with no input map errors.

- [ ] **Step 3: Commit**

```bash
jj describe -m "feat: add combat input map actions (Tab/Space/Enter + mouse)"
jj new
```

---

### Task 2: Create CombatScene.cs script

**Files:**
- Create: `src/Scripts/Core/CombatScene.cs`

- [ ] **Step 1: Write CombatScene.cs**

```csharp
using Godot;
using PathOfChaz.Characters;
using PathOfChaz.Core;
using PathOfChaz.UI;

namespace PathOfChaz.Core;

public partial class CombatScene : Control
{
    [Export] public Label PlayerNameLabel { get; set; } = null!;
    [Export] public Label PlayerHealthLabel { get; set; } = null!;
    [Export] public Label EnemyNameLabel { get; set; } = null!;
    [Export] public Label EnemyHealthLabel { get; set; } = null!;
    [Export] public CombatLogPanel LogPanel { get; set; } = null!;
    [Export] public Button AttackButton { get; set; } = null!;
    [Export] public Button StandButton { get; set; } = null!;
    [Export] public Button PrayButton { get; set; } = null!;
    [Export] public Label ResultLabel { get; set; } = null!;

    private TurnSystem _turnSystem = null!;
    private CombatLog _combatLog = null!;
    private Combatant _player = null!;
    private Combatant _enemy = null!;
    private bool _combatOver;

    public override void _Ready()
    {
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

        UpdateLabels();
        ResultLabel.Visible = false;

        AttackButton.Pressed += () => SubmitPlayerAction(CombatAction.Attack);
        StandButton.Pressed += () => SubmitPlayerAction(CombatAction.Stand);
        PrayButton.Pressed += () => SubmitPlayerAction(CombatAction.Pray);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_combatOver || !@event.IsPressed())
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
        if (_combatOver)
            return;

        SetButtonsEnabled(false);

        var result = _turnSystem.SubmitAction(action);
        UpdateLabels();
        LogPanel.UpdateFromLog(_combatLog);

        // Brief pause for visual pacing
        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);

        switch (result)
        {
            case TurnResult.EnemyDead:
                ResultLabel.Text = "Victory!";
                ResultLabel.Visible = true;
                _combatOver = true;
                break;
            case TurnResult.PlayerDead:
                ResultLabel.Text = "Defeat!";
                ResultLabel.Visible = true;
                _combatOver = true;
                break;
            default:
                SetButtonsEnabled(true);
                break;
        }
    }

    private void UpdateLabels()
    {
        PlayerNameLabel.Text = _player.Name;
        PlayerHealthLabel.Text = $"HP: {_player.Health} / {_player.MaxHealth}";
        EnemyNameLabel.Text = _enemy.Name;
        EnemyHealthLabel.Text = $"HP: {_enemy.Health} / {_enemy.MaxHealth}";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        AttackButton.Disabled = !enabled;
        StandButton.Disabled = !enabled;
        PrayButton.Disabled = !enabled;
    }
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build src/PathOfChaz.csproj`

Expected: Build succeeds with no errors.

- [ ] **Step 3: Commit**

```bash
jj describe -m "feat: add CombatScene.cs wiring turn system to Godot UI"
jj new
```

---

### Task 3: Rebuild Main.tscn and delete Main.cs

**Files:**
- Rewrite: `src/Scenes/Core/Main.tscn`
- Delete: `src/Scripts/Core/Main.cs`

- [ ] **Step 1: Write the new Main.tscn scene file**

Replace the entire contents of `src/Scenes/Core/Main.tscn`:

```tscn
[gd_scene load_steps=3 format=3 uid="uid://main_scene"]

[ext_resource type="Script" path="res://Scripts/Core/CombatScene.cs" id="1"]
[ext_resource type="Script" path="res://Scripts/UI/CombatLogPanel.cs" id="2"]

[node name="CombatScene" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1")
PlayerNameLabel = NodePath("MarginContainer/VBox/TopBar/PlayerInfo/PlayerName")
PlayerHealthLabel = NodePath("MarginContainer/VBox/TopBar/PlayerInfo/PlayerHealth")
EnemyNameLabel = NodePath("MarginContainer/VBox/TopBar/EnemyInfo/EnemyName")
EnemyHealthLabel = NodePath("MarginContainer/VBox/TopBar/EnemyInfo/EnemyHealth")
LogPanel = NodePath("MarginContainer/VBox/CombatLogPanel")
AttackButton = NodePath("MarginContainer/VBox/ActionBar/AttackButton")
StandButton = NodePath("MarginContainer/VBox/ActionBar/StandButton")
PrayButton = NodePath("MarginContainer/VBox/ActionBar/PrayButton")
ResultLabel = NodePath("MarginContainer/VBox/ResultLabel")

[node name="MarginContainer" type="MarginContainer" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
theme_override_constants/margin_left = 20
theme_override_constants/margin_top = 20
theme_override_constants/margin_right = 20
theme_override_constants/margin_bottom = 20

[node name="VBox" type="VBoxContainer" parent="MarginContainer"]
layout_mode = 2

[node name="TopBar" type="HBoxContainer" parent="MarginContainer/VBox"]
layout_mode = 2

[node name="PlayerInfo" type="VBoxContainer" parent="MarginContainer/VBox/TopBar"]
layout_mode = 2
size_flags_horizontal = 3

[node name="PlayerName" type="Label" parent="MarginContainer/VBox/TopBar/PlayerInfo"]
layout_mode = 2
text = "Player"
theme_override_font_sizes/font_size = 20

[node name="PlayerHealth" type="Label" parent="MarginContainer/VBox/TopBar/PlayerInfo"]
layout_mode = 2
text = "HP: 0 / 0"

[node name="EnemyInfo" type="VBoxContainer" parent="MarginContainer/VBox/TopBar"]
layout_mode = 2
size_flags_horizontal = 3

[node name="EnemyName" type="Label" parent="MarginContainer/VBox/TopBar/EnemyInfo"]
layout_mode = 2
text = "Enemy"
horizontal_alignment = 2
theme_override_font_sizes/font_size = 20

[node name="EnemyHealth" type="Label" parent="MarginContainer/VBox/TopBar/EnemyInfo"]
layout_mode = 2
text = "HP: 0 / 0"
horizontal_alignment = 2

[node name="CombatLogPanel" type="PanelContainer" parent="MarginContainer/VBox"]
layout_mode = 2
size_flags_vertical = 3
custom_minimum_size = Vector2(0, 300)
script = ExtResource("2")

[node name="ActionBar" type="HBoxContainer" parent="MarginContainer/VBox"]
layout_mode = 2
alignment = 1

[node name="AttackButton" type="Button" parent="MarginContainer/VBox/ActionBar"]
layout_mode = 2
custom_minimum_size = Vector2(120, 40)
text = "Attack [Tab]"

[node name="StandButton" type="Button" parent="MarginContainer/VBox/ActionBar"]
layout_mode = 2
custom_minimum_size = Vector2(120, 40)
text = "Stand [Space]"

[node name="PrayButton" type="Button" parent="MarginContainer/VBox/ActionBar"]
layout_mode = 2
custom_minimum_size = Vector2(120, 40)
text = "Pray [Enter]"

[node name="ResultLabel" type="Label" parent="MarginContainer/VBox"]
layout_mode = 2
horizontal_alignment = 1
theme_override_font_sizes/font_size = 32
```

- [ ] **Step 2: Delete Main.cs**

Delete `src/Scripts/Core/Main.cs` — it's been replaced by `CombatScene.cs`.

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build src/PathOfChaz.csproj`

Expected: Build succeeds. `Main.cs` was not referenced by any other file.

- [ ] **Step 4: Run existing tests to verify no regressions**

Run: `dotnet test`

Expected: All existing tests pass (TurnSystem, Combatant, CombatLog tests). The scene script is Godot-dependent and not unit tested.

- [ ] **Step 5: Commit**

```bash
jj describe -m "feat: rebuild Main.tscn as combat scene, delete Main.cs"
jj new
```

---

### Task 4: Manual smoke test

This is a Godot integration scene — verify by running the game.

- [ ] **Step 1: Launch the game**

Run: `cd src && godot --path . 2>&1` (or open in Godot editor and press F5)

- [ ] **Step 2: Verify scene loads**

Expected: Window shows player name "Chaz" with HP 20/20, enemy "Goblin" with HP 10/10, three action buttons, empty combat log panel, no result label visible.

- [ ] **Step 3: Test button input**

Click "Attack [Tab]". Expected: HP values update, combat log shows attack entry, buttons disable briefly then re-enable.

- [ ] **Step 4: Test keyboard input**

Press Tab, Space, Enter. Expected: Each triggers the corresponding action (Attack, Stand, Pray). Combat log updates accordingly.

- [ ] **Step 5: Test combat resolution**

Keep attacking until the goblin dies. Expected: "Victory!" label appears, buttons stay disabled, combat log shows the kill entry in red.

- [ ] **Step 6: Final commit with any fixes**

If any fixes were needed during smoke testing, commit them:

```bash
jj describe -m "feat: combat scene smoke test fixes"
jj new
```
