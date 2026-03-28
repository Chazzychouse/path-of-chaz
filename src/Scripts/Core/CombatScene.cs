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
