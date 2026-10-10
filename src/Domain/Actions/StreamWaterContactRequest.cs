using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Actions;

/// <summary>One explicit attempt retaining exact actor, environmental site and action state.</summary>
public sealed class StreamWaterContactRequest
{
    /// <summary>Retains supplied identities; only the current owner can admit this attempt.</summary>
    public StreamWaterContactRequest(Character actor, StreamWaterContactSite site, StreamWaterContactState state)
    {
        ArgumentNullException.ThrowIfNull(actor); ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(state);
        Actor = actor; Site = site; State = state;
    }
    /// <summary>The exact requesting character.</summary>
    public Character Actor { get; }
    /// <summary>The exact requested site.</summary>
    public StreamWaterContactSite Site { get; }
    /// <summary>The exact state against which the attempt was issued.</summary>
    public StreamWaterContactState State { get; }
}
