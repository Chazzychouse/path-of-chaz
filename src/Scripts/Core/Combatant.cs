using System;

namespace PathOfChaz.Core;

public class Combatant
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int Attack { get; }
    public int Defense { get; }
    public int Accuracy { get; }

    public bool IsAlive => Health > 0;

    public Combatant(string name, int maxHealth, int attack, int defense, int accuracy)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
        Attack = attack;
        Defense = defense;
        Accuracy = accuracy;
    }

    public Combatant(string name, int maxHealth, int currentHealth, int attack, int defense, int accuracy)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = currentHealth;
        Attack = attack;
        Defense = defense;
        Accuracy = accuracy;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
    }
}
