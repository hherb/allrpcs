using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Identifies qualitative world time by the latest event that advanced it.
/// </summary>
public sealed record WorldTimeSnapshot
{
    /// <summary>
    /// Creates a world-time snapshot. A null event explicitly means time has not advanced yet.
    /// </summary>
    public WorldTimeSnapshot(WorldEventId? currentEventId)
    {
        CurrentEventId = currentEventId;
    }

    /// <summary>Gets the exact latest advancing event identifier, or null before any advance.</summary>
    public WorldEventId? CurrentEventId { get; }
}
