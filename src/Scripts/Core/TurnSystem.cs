using System;

namespace PathOfChaz.Core;

public enum TurnState
{
    WaitingForInput,
    ResolvingTurn,
    Animating,
    CheckState,
}

public enum TurnResult
{
    Continue,
    PlayerDead,
    EnemyDead,
    Invalid,
}

public class TurnSystem
{
    private readonly Combatant _player;
    private readonly Combatant _enemy;
    private readonly CombatLog _log;
    private readonly Random _rng;

    public TurnState State { get; private set; } = TurnState.WaitingForInput;
    public int TurnNumber { get; private set; } = 1;

    public TurnSystem(Combatant player, Combatant enemy, CombatLog log, Random rng = null)
    {
        _player = player;
        _enemy = enemy;
        _log = log;
        _rng = rng ?? new Random();
    }

    public TurnResult SubmitAction(CombatAction action)
    {
        if (State != TurnState.WaitingForInput)
            return TurnResult.Invalid;

        // ResolvingTurn
        State = TurnState.ResolvingTurn;
        ResolvePlayerAction(action);
        if (_enemy.IsAlive)
            ResolveEnemyAction();

        // Animating (placeholder — immediate transition)
        State = TurnState.Animating;

        // CheckState
        State = TurnState.CheckState;
        var result = CheckOutcome();

        // Back to WaitingForInput for next turn
        TurnNumber++;
        State = TurnState.WaitingForInput;

        return result;
    }

    private void ResolvePlayerAction(CombatAction action)
    {
        switch (action)
        {
            case CombatAction.Attack:
                ResolveAttack(_player, _enemy);
                break;
            case CombatAction.Stand:
                _log.Add(new CombatLogEntry(TurnNumber, _player.Name, _player.Name,
                    CombatAction.Stand, ActionResult.Hit, 0,
                    $"{_player.Name} stands their ground."));
                break;
            case CombatAction.Pray:
                _log.Add(new CombatLogEntry(TurnNumber, _player.Name, _player.Name,
                    CombatAction.Pray, ActionResult.Hit, 0,
                    $"{_player.Name} prays to the gods. Nothing happens."));
                break;
        }
    }

    private void ResolveEnemyAction()
    {
        ResolveAttack(_enemy, _player);
    }

    private void ResolveAttack(Combatant attacker, Combatant defender)
    {
        int roll = _rng.Next(100);
        if (roll >= attacker.Accuracy)
        {
            _log.Add(new CombatLogEntry(TurnNumber, attacker.Name, defender.Name,
                CombatAction.Attack, ActionResult.Miss, 0,
                $"{attacker.Name} misses {defender.Name}."));
            return;
        }

        int damage = Math.Max(1, attacker.Attack - defender.Defense);
        defender.TakeDamage(damage);

        var result = defender.IsAlive ? ActionResult.Hit : ActionResult.Kill;
        _log.Add(new CombatLogEntry(TurnNumber, attacker.Name, defender.Name,
            CombatAction.Attack, result, damage,
            result == ActionResult.Kill
                ? $"{attacker.Name} strikes {defender.Name} for {damage} damage. {defender.Name} is slain!"
                : $"{attacker.Name} hits {defender.Name} for {damage} damage."));
    }

    private TurnResult CheckOutcome()
    {
        if (!_enemy.IsAlive) return TurnResult.EnemyDead;
        if (!_player.IsAlive) return TurnResult.PlayerDead;
        return TurnResult.Continue;
    }
}
