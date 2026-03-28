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
