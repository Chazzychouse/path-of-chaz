using System;

namespace PathOfChaz.Core;

public class RunState
{
    public int PlayerHealth { get; set; }
    public int EnemyHealth { get; set; }
    public int TurnCount { get; set; }
    public int RngSeed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
