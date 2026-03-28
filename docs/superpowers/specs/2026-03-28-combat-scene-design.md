# Combat Scene (Godot) — Design Spec

## Goal

Wire together the turn system, combat log, content resources, and player input into a playable single encounter. This is the Godot integration layer — no new core logic, just connecting existing pure C# systems to the engine.

## Scene Tree

```
CombatScene (Control) — CombatScene.cs
├── TopBar (HBoxContainer)
│   ├── PlayerInfo (VBoxContainer)
│   │   ├── PlayerName (Label)
│   │   └── PlayerHealth (Label)       # "HP: 15 / 20"
│   └── EnemyInfo (VBoxContainer)
│       ├── EnemyName (Label)
│       └── EnemyHealth (Label)
├── CombatLogPanel (PanelContainer)    # existing CombatLogPanel.cs
├── ActionBar (HBoxContainer)
│   ├── AttackButton (Button)          # "Attack [Tab]"
│   ├── StandButton (Button)           # "Stand [Space]"
│   └── PrayButton (Button)            # "Pray [Enter]"
└── ResultLabel (Label)                # hidden until win/lose
```

- Scene file: `src/Scenes/Core/Main.tscn` (replaces the current hello-world scene)
- Script file: `src/Scripts/Core/CombatScene.cs` (replaces `Main.cs`)

## Behavior

### Scene load (`_Ready`)

1. Load `Chaz.tres` and `Goblin.tres` via `GD.Load<CharacterData>()`
2. Create `Combatant` instances from the resource data
3. Instantiate `TurnSystem` with both combatants and a new `CombatLog`
4. Set player/enemy name and health labels
5. Hide `ResultLabel`
6. Connect button signals to action handlers
7. State starts as `WaitingForInput` — buttons enabled

### WaitingForInput

- Action buttons are enabled
- Player clicks a button or presses a mapped key
- Handler calls `SubmitPlayerAction(CombatAction)`

### ResolvingTurn (inside `SubmitPlayerAction`)

1. Disable all action buttons
2. Call `TurnSystem.SubmitAction(action)` — returns a `TurnResult`
3. Update player and enemy health labels
4. Refresh the combat log panel with new entries

### Animating

Brief 0.3s pause via `await ToSignal(GetTree().CreateTimer(0.3), "timeout")` for visual pacing. No actual animation.

### CheckState

- If `TurnResult.EnemyDead`: show "Victory!" in `ResultLabel`, keep buttons disabled
- If `TurnResult.PlayerDead`: show "Defeat!" in `ResultLabel`, keep buttons disabled
- If `TurnResult.Continue`: re-enable buttons, return to WaitingForInput

## Input Map

Three Godot InputMap actions added to `project.godot`:

| Action          | Key   | Mouse          |
|-----------------|-------|----------------|
| `action_attack` | Tab   | Left click     |
| `action_stand`  | Space | Right click    |
| `action_pray`   | Enter | Middle click   |

Handled via `_UnhandledInput` in `CombatScene.cs`, dispatching to the same `SubmitPlayerAction` method used by buttons. Input is ignored when not in `WaitingForInput` state.

## Node references

All child nodes referenced via `[Export]` attributes on `CombatScene.cs`, set in the scene file. No magic strings for node paths.

## Files changed

| File | Action | Notes |
|------|--------|-------|
| `src/Scripts/Core/CombatScene.cs` | Create | New scene script |
| `src/Scripts/Core/Main.cs` | Delete | Replaced by CombatScene.cs |
| `src/Scenes/Core/Main.tscn` | Rewrite | New scene tree |
| `src/project.godot` | Edit | Add input map actions |

## Existing systems used as-is (no modifications)

- `TurnSystem` — state machine + combat resolution
- `Combatant` — entity with stats
- `CombatAction` / `CombatLog` / `CombatLogEntry` — action types and logging
- `CombatLogPanel` — UI component for displaying log
- `CharacterData` / `ActionData` — resource definitions
- `Chaz.tres` / `Goblin.tres` — character resources
