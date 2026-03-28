using Godot;
using PathOfChaz.Core;

namespace PathOfChaz.UI;

public partial class CombatLogPanel : PanelContainer
{
    [Export] public int MaxVisibleEntries { get; set; } = 100;

    private VBoxContainer _entryContainer = null!;
    private ScrollContainer _scrollContainer = null!;
    private int _lastEntryCount;

    public override void _Ready()
    {
        _scrollContainer = new ScrollContainer();
        _scrollContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
        _scrollContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_scrollContainer);

        _entryContainer = new VBoxContainer();
        _entryContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _scrollContainer.AddChild(_entryContainer);
    }

    public void UpdateFromLog(CombatLog log)
    {
        if (log.Entries.Count == _lastEntryCount)
            return;

        for (int i = _lastEntryCount; i < log.Entries.Count; i++)
        {
            var entry = log.Entries[i];
            var label = new RichTextLabel();
            label.BbcodeEnabled = true;
            label.FitContent = true;
            label.ScrollActive = false;
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            label.Text = FormatEntry(entry);
            _entryContainer.AddChild(label);
        }

        _lastEntryCount = log.Entries.Count;

        // Trim old entries if over max
        while (_entryContainer.GetChildCount() > MaxVisibleEntries)
        {
            var oldest = _entryContainer.GetChild(0);
            _entryContainer.RemoveChild(oldest);
            oldest.QueueFree();
        }

        // Auto-scroll to bottom
        CallDeferred(nameof(ScrollToBottom));
    }

    private void ScrollToBottom()
    {
        _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
    }

    private static string FormatEntry(CombatLogEntry entry)
    {
        var color = entry.Result switch
        {
            ActionResult.Kill => "red",
            ActionResult.Miss => "gray",
            _ => entry.Action switch
            {
                CombatAction.Attack => "white",
                CombatAction.Stand => "yellow",
                CombatAction.Pray => "purple",
                _ => "white",
            },
        };

        return $"[color={color}][b]T{entry.TurnNumber}[/b] {entry.Description}[/color]";
    }
}
