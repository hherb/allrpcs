using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Contains an ordered snapshot of duplicate-free world-event identifiers.
/// </summary>
public sealed class WorldEventLogSnapshot
{
    /// <summary>
    /// Creates a world-event log snapshot. An empty sequence explicitly means that no events are
    /// recorded.
    /// </summary>
    public WorldEventLogSnapshot(IEnumerable<WorldEventId> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<WorldEventId> orderedEntries = new();
        HashSet<WorldEventId> eventIds = new();

        foreach (WorldEventId entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "World-event log entries cannot contain null values.",
                    nameof(entries));
            }

            if (!eventIds.Add(entry))
            {
                throw new ArgumentException(
                    "World-event log entries cannot contain duplicate event identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets world-event identifiers in their stable supplied order.
    /// </summary>
    public IReadOnlyList<WorldEventId> Entries { get; }
}
