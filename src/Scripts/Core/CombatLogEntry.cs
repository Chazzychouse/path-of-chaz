namespace PathOfChaz.Core;

public record CombatLogEntry(
    int TurnNumber,
    string Source,
    string Target,
    CombatAction Action,
    ActionResult Result,
    int Damage,
    string Description
);
