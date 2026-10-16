using System.Globalization;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Dialogue;

/// <summary>Authors repeatable herb examination events, including their saved identifiers.</summary>
public static class HerbExaminationEventCatalog
{
    private const string Prefix = "herb-patch-examination-";

    /// <summary>Expands definitions purely from contiguous engine-authored examination identifiers.</summary>
    public static WorldEventSnapshot Expand(WorldEventSnapshot definitions, WorldEventLogSnapshot log)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(log);
        List<WorldEvent> events = definitions.Entries.ToList();
        int expected = 1;
        foreach (WorldEventId id in log.Entries)
        {
            if (!id.Value.StartsWith(Prefix, StringComparison.Ordinal)) { continue; }
            if (id.Value != Prefix + expected.ToString(CultureInfo.InvariantCulture))
            { throw new ArgumentException("Examination events must retain their contiguous order.", nameof(log)); }
            WorldEvent? existing = events.SingleOrDefault(entry => entry.EventId == id);
            if (existing is null) { events.Add(new(id, WorldEventTimeEffect.Advance)); }
            else if (existing.TimeEffect != WorldEventTimeEffect.Advance)
            { throw new ArgumentException("Examination events must advance world time.", nameof(definitions)); }
            expected++;
        }
        return events.Count == definitions.Entries.Count ? definitions : new(events);
    }

    /// <summary>Authors the next unique event from the current log, also after restoring an older save.</summary>
    public static WorldEvent Next(WorldEventSnapshot definitions, WorldEventLogSnapshot log)
    {
        _ = Expand(definitions, log);
        int next = checked(log.Entries.Count(id => id.Value.StartsWith(Prefix, StringComparison.Ordinal)) + 1);
        WorldEventId id = new(Prefix + next.ToString(CultureInfo.InvariantCulture));
        return WorldEventLookup.FindById(id, definitions) ?? new(id, WorldEventTimeEffect.Advance);
    }
}
