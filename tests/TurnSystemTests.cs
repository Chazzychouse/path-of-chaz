using PathOfChaz.Core;

namespace PathOfChaz.Tests;

public class TurnSystemTests
{
    private static Combatant MakePlayer() => new("Chaz", 20, 8, 3, 100);
    private static Combatant MakeEnemy() => new("Goblin", 10, 5, 2, 100);

    [Fact]
    public void StartsInWaitingForInput()
    {
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), new CombatLog());
        Assert.Equal(TurnState.WaitingForInput, ts.State);
    }

    [Fact]
    public void TurnNumber_StartsAtOne()
    {
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), new CombatLog());
        Assert.Equal(1, ts.TurnNumber);
    }

    [Fact]
    public void SubmitAction_TransitionsToCheckState()
    {
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), new CombatLog());

        var result = ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(TurnState.WaitingForInput, ts.State);
        Assert.NotEqual(TurnResult.Invalid, result);
    }

    [Fact]
    public void SubmitAction_RejectsWhenNotWaiting()
    {
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), new CombatLog());
        ts.SubmitAction(CombatAction.Attack); // resolve first turn

        // Already back to WaitingForInput, so this should work
        var result = ts.SubmitAction(CombatAction.Attack);
        Assert.NotEqual(TurnResult.Invalid, result);
    }

    [Fact]
    public void Attack_DealsDamage_AttackMinusDefense()
    {
        // Player attack=8, enemy defense=2 => 6 damage
        var player = MakePlayer();
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        ts.SubmitAction(CombatAction.Attack);

        Assert.True(enemy.Health < enemy.MaxHealth);
        Assert.Equal(enemy.MaxHealth - (player.Attack - enemy.Defense), enemy.Health);
    }

    [Fact]
    public void Attack_DamageFloorsAtOne()
    {
        // Player attack=1, enemy defense=100 => should deal 1 not negative
        var player = new Combatant("Weak", 20, 1, 3, 100);
        var enemy = new Combatant("Tank", 50, 5, 100, 100);
        var ts = new TurnSystem(player, enemy, new CombatLog());

        ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(49, enemy.Health);
    }

    [Fact]
    public void EnemyAttacksBack()
    {
        // Enemy attack=5, player defense=3 => 2 damage to player
        var player = MakePlayer();
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(player.MaxHealth - (enemy.Attack - player.Defense), player.Health);
    }

    [Fact]
    public void EnemyDeath_ReturnsTurnResultEnemyDead()
    {
        // One-shot enemy: player attack=99, enemy has 10 hp, defense=2
        var player = new Combatant("Chaz", 20, 99, 3, 100);
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        var result = ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(TurnResult.EnemyDead, result);
    }

    [Fact]
    public void PlayerDeath_ReturnsTurnResultPlayerDead()
    {
        // Weak player vs strong enemy
        var player = new Combatant("Chaz", 1, 1, 0, 100);
        var enemy = new Combatant("Dragon", 999, 99, 100, 100);
        var ts = new TurnSystem(player, enemy, new CombatLog());

        var result = ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(TurnResult.PlayerDead, result);
    }

    [Fact]
    public void ContinueTurn_ReturnsContinue()
    {
        var player = MakePlayer();
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        var result = ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(TurnResult.Continue, result);
    }

    [Fact]
    public void TurnNumber_IncrementsAfterTurn()
    {
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), new CombatLog());

        ts.SubmitAction(CombatAction.Attack);

        Assert.Equal(2, ts.TurnNumber);
    }

    [Fact]
    public void Stand_SkipsTurn_EnemyStillAttacks()
    {
        var player = MakePlayer();
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        ts.SubmitAction(CombatAction.Stand);

        // Enemy still deals damage
        Assert.True(player.Health < player.MaxHealth);
        // Enemy takes no damage
        Assert.Equal(enemy.MaxHealth, enemy.Health);
    }

    [Fact]
    public void Pray_NoEffect_EnemyStillAttacks()
    {
        var player = MakePlayer();
        var enemy = MakeEnemy();
        var ts = new TurnSystem(player, enemy, new CombatLog());

        ts.SubmitAction(CombatAction.Pray);

        Assert.True(player.Health < player.MaxHealth);
        Assert.Equal(enemy.MaxHealth, enemy.Health);
    }

    [Fact]
    public void EmitsCombatLogEntries()
    {
        var log = new CombatLog();
        var ts = new TurnSystem(MakePlayer(), MakeEnemy(), log);

        ts.SubmitAction(CombatAction.Attack);

        // Should have at least 2 entries: player attack + enemy attack
        Assert.True(log.Entries.Count >= 2);
        Assert.Equal("Chaz", log.Entries[0].Source);
        Assert.Equal("Goblin", log.Entries[0].Target);
    }

    [Fact]
    public void Miss_WhenAccuracyIsZero()
    {
        // Accuracy=0 means always miss
        var player = new Combatant("Chaz", 20, 8, 3, 0);
        var enemy = new Combatant("Goblin", 10, 5, 2, 0);
        var log = new CombatLog();
        var ts = new TurnSystem(player, enemy, log);

        ts.SubmitAction(CombatAction.Attack);

        // Both should miss, no damage dealt
        Assert.Equal(player.MaxHealth, player.Health);
        Assert.Equal(enemy.MaxHealth, enemy.Health);
        Assert.All(log.Entries, e => Assert.Equal(ActionResult.Miss, e.Result));
    }

    [Fact]
    public void KillEntry_EmittedOnEnemyDeath()
    {
        var player = new Combatant("Chaz", 20, 99, 3, 100);
        var enemy = MakeEnemy();
        var log = new CombatLog();
        var ts = new TurnSystem(player, enemy, log);

        ts.SubmitAction(CombatAction.Attack);

        Assert.Contains(log.Entries, e => e.Result == ActionResult.Kill);
    }
}
