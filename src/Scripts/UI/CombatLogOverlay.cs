using System.Collections.Generic;
using System.Text;
using Godot;
using PathOfChaz.Core;

namespace PathOfChaz.UI;

public partial class CombatLogOverlay : PanelContainer
{
    private RichTextLabel _logText = null!;
    private int _lastEntryCount;

    public override void _Ready()
    {
        _logText = GetNode<RichTextLabel>("LogText");
    }

    public void UpdateFromLog(CombatLog log)
    {
        if (log.Entries.Count == _lastEntryCount)
            return;

        var newEntries = new List<CombatLogEntry>();
        for (int i = _lastEntryCount; i < log.Entries.Count; i++)
            newEntries.Add(log.Entries[i]);

        var formatted = FormatEntries(newEntries);

        if (_lastEntryCount > 0 && formatted.Length > 0)
            _logText.Text += " * " + formatted;
        else
            _logText.Text += formatted;

        _lastEntryCount = log.Entries.Count;
        CallDeferred(nameof(ScrollToBottom));
    }

    private void ScrollToBottom()
    {
        _logText.ScrollToLine(_logText.GetLineCount() - 1);
    }

    public static string FormatEntries(IReadOnlyList<CombatLogEntry> entries)
    {
        if (entries.Count == 0)
            return "";

        var sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0)
                sb.Append(" * ");
            sb.Append(FormatEntry(entries[i]));
        }
        return sb.ToString();
    }

    public static string FormatEntry(CombatLogEntry entry)
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

        return $"[color={color}]{entry.Description}[/color]";
    }
}
