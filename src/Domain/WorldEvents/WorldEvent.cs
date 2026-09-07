using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Identifies one authoritative world event and whether it advances meaningful world time.
/// </summary>
public sealed record WorldEvent
{
    /// <summary>
    /// Creates an immutable world event from an existing identifier and time effect.
    /// </summary>
    public WorldEvent(WorldEventId eventId, WorldEventTimeEffect timeEffect)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(timeEffect);

        EventId = eventId;
        TimeEffect = timeEffect;
    }

    /// <summary>Gets the identifier of this world event.</summary>
    public WorldEventId EventId { get; }

    /// <summary>Gets whether this event advances meaningful world time.</summary>
    public WorldEventTimeEffect TimeEffect { get; }
}
