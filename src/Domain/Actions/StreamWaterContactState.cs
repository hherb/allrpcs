using System.Numerics;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Actions;

/// <summary>Identifies one bound environmental contact site without creating a world object.</summary>
public sealed class StreamWaterContactSite
{
    /// <summary>Retains the authored site key and finite projection-local contact point.</summary>
    public StreamWaterContactSite(string key, Vector2 position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
        { throw new ArgumentOutOfRangeException(nameof(position)); }
        Key = key; Position = position;
    }
    /// <summary>The stable environmental site key, distinct from an object identifier.</summary>
    public string Key { get; }
    /// <summary>The finite point bound by the site's live scene owner.</summary>
    public Vector2 Position { get; }
}

/// <summary>An immutable session-local action state, absent from durable world saves.</summary>
public sealed class StreamWaterContactState
{
    /// <summary>Retains exact actor/site identities, accepted sequence and remaining cooldown.</summary>
    public StreamWaterContactState(Character actor, StreamWaterContactSite site, long sequence = 0, double cooldown = 0)
    {
        ArgumentNullException.ThrowIfNull(actor); ArgumentNullException.ThrowIfNull(site);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        if (!double.IsFinite(cooldown) || cooldown < 0 || cooldown > 2)
        { throw new ArgumentOutOfRangeException(nameof(cooldown)); }
        Actor = actor; Site = site; Sequence = sequence; Cooldown = cooldown;
    }
    /// <summary>The exact current character retained by this action owner.</summary>
    public Character Actor { get; }
    /// <summary>The exact scene-bound site retained by this action owner.</summary>
    public StreamWaterContactSite Site { get; }
    /// <summary>The last accepted session-local action sequence.</summary>
    public long Sequence { get; }
    /// <summary>Remaining active time before another explicit request may commit.</summary>
    public double Cooldown { get; }
}
