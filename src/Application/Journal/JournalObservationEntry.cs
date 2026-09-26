using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Journal;

/// <summary>Retains one exact filtered visibility with its matching authoritative world object.</summary>
public sealed class JournalObservationEntry
{
    internal JournalObservationEntry(
        WorldObjectVisibility visibility,
        WorldObject worldObject)
    {
        Visibility = visibility;
        WorldObject = worldObject;
    }

    /// <summary>Gets the exact filtered visibility projected into this structural entry.</summary>
    public WorldObjectVisibility Visibility { get; }

    /// <summary>Gets the exact authoritative world object matching the visibility identifier.</summary>
    public WorldObject WorldObject { get; }
}
