using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Journal;

/// <summary>Projects structural observation entries from validated world-bound observations.</summary>
public static class JournalObservationEntryProjection
{
    /// <summary>Returns read-only entries in filtered-perception order with exact source instances.</summary>
    public static IReadOnlyList<JournalObservationEntry> Project(
        JournalObservationWorldBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        List<JournalObservationEntry> entries = new(binding.Observations.Perception.Entries.Count);
        foreach (WorldObjectVisibility visibility in binding.Observations.Perception.Entries)
        {
            WorldObject worldObject = WorldObjectLookup.FindById(
                visibility.ObjectId,
                binding.WorldObjects)
                ?? throw new InvalidOperationException(
                    "Validated journal observations must retain authoritative world coverage.");
            entries.Add(new JournalObservationEntry(visibility, worldObject));
        }

        return entries.AsReadOnly();
    }
}
