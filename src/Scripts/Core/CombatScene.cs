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
    private bool _turnInProgress;

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
