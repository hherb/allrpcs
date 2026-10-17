using Allrpcs.Domain.Collections;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Contains an ordered snapshot of duplicate-free world-event identifiers.
/// </summary>
public sealed class WorldEventLogSnapshot
{
    private readonly WorldEventIdIndex index;
    /// <summary>
    /// Creates a world-event log snapshot. An empty sequence explicitly means that no events are
    /// recorded.
    /// </summary>
    public WorldEventLogSnapshot(IEnumerable<WorldEventId> entries)
        : this(entries, null) { }

    internal WorldEventLogSnapshot(IEnumerable<WorldEventId> entries, AppendWork? work)
    {
        ArgumentNullException.ThrowIfNull(entries);

        AppendSequence<WorldEventId> orderedEntries = new(work);
        WorldEventIdIndex eventIds = new(work);

        foreach (WorldEventId entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "World-event log entries cannot contain null values.",
                    nameof(entries));
            }

            if (eventIds.Find(entry) is not null)
            {
                throw new ArgumentException(
                    "World-event log entries cannot contain duplicate event identifiers.",
                    nameof(entries));
            }

            orderedEntries = orderedEntries.Append(entry);
            eventIds = eventIds.Add(entry);
        }

        Entries = orderedEntries;
        index = eventIds;
    }

    private WorldEventLogSnapshot(AppendSequence<WorldEventId> entries, WorldEventIdIndex index)
    {
        Entries = entries;
        this.index = index;
    }

    internal WorldEventId? Find(WorldEventId id) => index.Find(id);

    internal WorldEventLogSnapshot Append(WorldEventId id) =>
        new(((AppendSequence<WorldEventId>)Entries).Append(id), index.Add(id));

    /// <summary>
    /// Gets world-event identifiers in their stable supplied order.
    /// </summary>
    public IReadOnlyList<WorldEventId> Entries { get; }
}
