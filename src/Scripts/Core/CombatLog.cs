using System.Collections.Generic;

namespace PathOfChaz.Core;

public class CombatLog
{
    private readonly List<CombatLogEntry> _entries = new();

    public IReadOnlyList<CombatLogEntry> Entries => _entries;

    public void Add(CombatLogEntry entry) => _entries.Add(entry);

    public void Clear() => _entries.Clear();
}
