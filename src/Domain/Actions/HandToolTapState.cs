using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Actions;

/// <summary>An immutable session-local action state, absent from durable world saves.</summary>
public sealed class HandToolTapState
{
    /// <summary>Retains exact actor/site identities, accepted sequence and remaining cooldown.</summary>
    public HandToolTapState(Character actor, HandToolTapSite site, long sequence = 0, double cooldown = 0)
    {
        ArgumentNullException.ThrowIfNull(actor); ArgumentNullException.ThrowIfNull(site);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        if (!double.IsFinite(cooldown) || cooldown < 0 || cooldown > 1)
        { throw new ArgumentOutOfRangeException(nameof(cooldown)); }
        Actor = actor; Site = site; Sequence = sequence; Cooldown = cooldown;
    }
    /// <summary>The exact current character retained by this action owner.</summary>
    public Character Actor { get; }
    /// <summary>The exact scene-bound site retained by this action owner.</summary>
    public HandToolTapSite Site { get; }
    /// <summary>The last accepted session-local action sequence.</summary>
    public long Sequence { get; }
    /// <summary>Remaining active time before another explicit request may commit.</summary>
    public double Cooldown { get; }
}
