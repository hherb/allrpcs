using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Actions;

/// <summary>One explicit attempt against exact current actor, target/material and owner state.</summary>
public sealed class CaskCoverHandlingRequest
{
    /// <summary>Retains identities; only the issuing owner can admit this request.</summary>
    public CaskCoverHandlingRequest(Character actor, CaskCoverHandlingMaterial material, CaskCoverHandlingState state)
    {
        ArgumentNullException.ThrowIfNull(actor); ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(state); Actor = actor; Material = material; State = state;
    }
    /// <summary>The exact requesting character.</summary>
    public Character Actor { get; }
    /// <summary>The exact approved material binding.</summary>
    public CaskCoverHandlingMaterial Material { get; }
    /// <summary>The exact existing target, never an inventory copy.</summary>
    public WorldObject Target => Material.Target;
    /// <summary>The exact state against which this request was issued.</summary>
    public CaskCoverHandlingState State { get; }
}
