using PathOfChaz.Core;

namespace PathOfChaz.Tests;

public class CombatantTests
{
    [Fact]
    public void Combatant_InitializesWithFullHealth()
    {
        var c = new Combatant("Chaz", maxHealth: 20, attack: 8, defense: 3, accuracy: 80);

        Assert.Equal("Chaz", c.Name);
        Assert.Equal(20, c.Health);
        Assert.Equal(20, c.MaxHealth);
        Assert.Equal(8, c.Attack);
        Assert.Equal(3, c.Defense);
        Assert.Equal(80, c.Accuracy);
    }

    [Fact]
    public void Combatant_TakeDamage_ReducesHealth()
    {
        var c = new Combatant("Chaz", 20, 8, 3, 80);

        c.TakeDamage(5);

        Assert.Equal(15, c.Health);
    }

    [Fact]
    public void Combatant_TakeDamage_HealthFloorsAtZero()
    {
        var c = new Combatant("Chaz", 10, 8, 3, 80);

        c.TakeDamage(999);

        Assert.Equal(0, c.Health);
    }

    [Fact]
    public void Combatant_IsAlive_TrueWhenHealthAboveZero()
    {
        var c = new Combatant("Chaz", 10, 8, 3, 80);
        Assert.True(c.IsAlive);
    }

    [Fact]
    public void Combatant_IsAlive_FalseWhenHealthIsZero()
    {
        var c = new Combatant("Chaz", 10, 8, 3, 80);
        c.TakeDamage(10);
        Assert.False(c.IsAlive);
    }

    [Fact]
    public void Constructor_WithCurrentHealth_SetsHealthCorrectly()
    {
        var c = new Combatant("Chaz", 20, 12, 8, 3, 100);

        Assert.Equal(20, c.MaxHealth);
        Assert.Equal(12, c.Health);
        Assert.Equal("Chaz", c.Name);
        Assert.Equal(8, c.Attack);
        Assert.Equal(3, c.Defense);
        Assert.Equal(100, c.Accuracy);
    }
}
