using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Contains an ordered snapshot of authoritative world events with duplicate-free identifiers.
/// </summary>
public sealed class WorldEventSnapshot
{
    /// <summary>
    /// Creates a world-event snapshot. An empty sequence explicitly means no events are defined.
    /// </summary>
    public WorldEventSnapshot(IEnumerable<WorldEvent> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<WorldEvent> orderedEntries = new();
        HashSet<WorldEventId> eventIds = new();

        foreach (WorldEvent entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "World-event entries cannot contain null values.",
                    nameof(entries));
            }

            if (!eventIds.Add(entry.EventId))
            {
                throw new ArgumentException(
                    "World-event entries cannot contain duplicate event identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>Gets authoritative world events in their stable supplied order.</summary>
    public IReadOnlyList<WorldEvent> Entries { get; }
}
