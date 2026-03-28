using Godot;

namespace PathOfChaz.Core;

[GlobalClass]
public partial class ActionData : Resource
{
    [Export] public string ActionName { get; set; } = "";
    [Export] public CombatAction ActionType { get; set; } = CombatAction.Attack;
    [Export] public float DamageModifier { get; set; } = 1.0f;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
}
