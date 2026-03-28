using Godot;

namespace PathOfChaz.Characters;

[GlobalClass]
public partial class CharacterData : Resource
{
    [Export] public string CharacterName { get; set; } = "";
    [Export] public int BaseHealth { get; set; } = 10;
    [Export] public int Attack { get; set; } = 5;
    [Export] public int Defense { get; set; } = 3;
    [Export] public int Accuracy { get; set; } = 80;
}
