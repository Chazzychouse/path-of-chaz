using PathOfChaz.Core;

namespace PathOfChaz.Tests;

public class CombatLogEntryTests
{
    [Fact]
    public void Entry_StoresAllFields()
    {
        var entry = new CombatLogEntry(
            TurnNumber: 1,
            Source: "Chaz",
            Target: "Goblin",
            Action: CombatAction.Attack,
            Result: ActionResult.Hit,
            Damage: 5,
            Description: "Chaz attacks Goblin for 5 damage."
        );

        Assert.Equal(1, entry.TurnNumber);
        Assert.Equal("Chaz", entry.Source);
        Assert.Equal("Goblin", entry.Target);
        Assert.Equal(CombatAction.Attack, entry.Action);
        Assert.Equal(ActionResult.Hit, entry.Result);
        Assert.Equal(5, entry.Damage);
        Assert.Equal("Chaz attacks Goblin for 5 damage.", entry.Description);
    }

    [Fact]
    public void Entry_MissHasZeroDamage()
    {
        var entry = new CombatLogEntry(
            TurnNumber: 1,
            Source: "Goblin",
            Target: "Chaz",
            Action: CombatAction.Attack,
            Result: ActionResult.Miss,
            Damage: 0,
            Description: "Goblin misses Chaz."
        );

        Assert.Equal(ActionResult.Miss, entry.Result);
        Assert.Equal(0, entry.Damage);
    }
}

public class CombatLogTests
{
    [Fact]
    public void Log_StartsEmpty()
    {
        var log = new CombatLog();
        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Log_AddEntry_AppearsInEntries()
    {
        var log = new CombatLog();
        var entry = new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Hit, 3, "Hit!");

        log.Add(entry);

        Assert.Single(log.Entries);
        Assert.Equal(entry, log.Entries[0]);
    }

    [Fact]
    public void Log_MultipleEntries_PreservesOrder()
    {
        var log = new CombatLog();
        var e1 = new CombatLogEntry(1, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Hit, 3, "Hit!");
        var e2 = new CombatLogEntry(1, "Goblin", "Chaz", CombatAction.Attack, ActionResult.Miss, 0, "Miss!");
        var e3 = new CombatLogEntry(2, "Chaz", "Goblin", CombatAction.Attack, ActionResult.Kill, 5, "Kill!");

        log.Add(e1);
        log.Add(e2);
        log.Add(e3);

        Assert.Equal(3, log.Entries.Count);
        Assert.Equal(e1, log.Entries[0]);
        Assert.Equal(e2, log.Entries[1]);
        Assert.Equal(e3, log.Entries[2]);
    }

    [Fact]
    public void Log_Clear_RemovesAllEntries()
    {
        var log = new CombatLog();
        log.Add(new CombatLogEntry(1, "A", "B", CombatAction.Attack, ActionResult.Hit, 1, "x"));

        log.Clear();

        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Log_Entries_IsReadOnly()
    {
        var log = new CombatLog();
        Assert.IsAssignableFrom<IReadOnlyList<CombatLogEntry>>(log.Entries);
    }
}
