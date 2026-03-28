using Godot;
using PathOfChaz.Characters;
using PathOfChaz.Core;
using PathOfChaz.UI;

namespace PathOfChaz.Core;

public partial class CombatScene : Control
{
    private Label _playerNameLabel = null!;
    private Label _playerHealthLabel = null!;
    private Label _enemyNameLabel = null!;
    private Label _enemyHealthLabel = null!;
    private CombatLogPanel _logPanel = null!;
    private Button _attackButton = null!;
    private Button _standButton = null!;
    private Button _prayButton = null!;
    private Label _resultLabel = null!;

    private TurnSystem _turnSystem = null!;
    private CombatLog _combatLog = null!;
    private Combatant _player = null!;
    private Combatant _enemy = null!;
    private bool _combatOver;
    private bool _turnInProgress;

    public override void _Ready()
    {
        _playerNameLabel = GetNode<Label>("MarginContainer/VBox/TopBar/PlayerInfo/PlayerName");
        _playerHealthLabel = GetNode<Label>("MarginContainer/VBox/TopBar/PlayerInfo/PlayerHealth");
        _enemyNameLabel = GetNode<Label>("MarginContainer/VBox/TopBar/EnemyInfo/EnemyName");
        _enemyHealthLabel = GetNode<Label>("MarginContainer/VBox/TopBar/EnemyInfo/EnemyHealth");
        _logPanel = GetNode<CombatLogPanel>("MarginContainer/VBox/CombatLogPanel");
        _attackButton = GetNode<Button>("MarginContainer/VBox/ActionBar/AttackButton");
        _standButton = GetNode<Button>("MarginContainer/VBox/ActionBar/StandButton");
        _prayButton = GetNode<Button>("MarginContainer/VBox/ActionBar/PrayButton");
        _resultLabel = GetNode<Label>("MarginContainer/VBox/ResultLabel");

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
        _resultLabel.Visible = false;

        _attackButton.Pressed += () => SubmitPlayerAction(CombatAction.Attack);
        _standButton.Pressed += () => SubmitPlayerAction(CombatAction.Stand);
        _prayButton.Pressed += () => SubmitPlayerAction(CombatAction.Pray);
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
        SetButtonsEnabled(false);

        var result = _turnSystem.SubmitAction(action);
        UpdateLabels();
        _logPanel.UpdateFromLog(_combatLog);

        // Brief pause for visual pacing
        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);

        switch (result)
        {
            case TurnResult.EnemyDead:
                _resultLabel.Text = "Victory!";
                _resultLabel.Visible = true;
                _combatOver = true;
                break;
            case TurnResult.PlayerDead:
                _resultLabel.Text = "Defeat!";
                _resultLabel.Visible = true;
                _combatOver = true;
                break;
            case TurnResult.Invalid:
                GD.PrintErr("CombatScene: TurnResult.Invalid — possible double-submit");
                break;
            default:
                SetButtonsEnabled(true);
                break;
        }

        _turnInProgress = false;
    }

    private void UpdateLabels()
    {
        _playerNameLabel.Text = _player.Name;
        _playerHealthLabel.Text = $"HP: {_player.Health} / {_player.MaxHealth}";
        _enemyNameLabel.Text = _enemy.Name;
        _enemyHealthLabel.Text = $"HP: {_enemy.Health} / {_enemy.MaxHealth}";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _attackButton.Disabled = !enabled;
        _standButton.Disabled = !enabled;
        _prayButton.Disabled = !enabled;
    }
}
