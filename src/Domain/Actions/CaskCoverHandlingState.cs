using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Actions;

/// <summary>The newly authorized wooden lid/cotton cover binding on one exact environmental cask.</summary>
public sealed class CaskCoverHandlingMaterial
{
    /// <summary>The sole existing world-object identifier authorized for this action.</summary>
    public const string ObjectIdText = "village-water-cask";
    /// <summary>Retains the exact target; material authority never creates an inventory item.</summary>
    public CaskCoverHandlingMaterial(WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target.Identity.Id.Value != ObjectIdText)
        { throw new ArgumentException("Cover handling is authorized only for the village cask.", nameof(target)); }
        Target = target;
    }
    /// <summary>The exact retained existing world object.</summary>
    public WorldObject Target { get; }
    /// <summary>The specifically approved material declaration, independent of artwork or contents.</summary>
    public string Description { get; } = "Wooden lid and cotton outer cover";
}

/// <summary>Immutable session-local handling state; no durable world or inventory mutation.</summary>
public sealed class CaskCoverHandlingState
{
    /// <summary>Retains exact actor/material bindings, sequence and bounded active-time cooldown.</summary>
    public CaskCoverHandlingState(Character actor, CaskCoverHandlingMaterial material, long sequence = 0, double cooldown = 0)
    {
        ArgumentNullException.ThrowIfNull(actor); ArgumentNullException.ThrowIfNull(material);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        if (!double.IsFinite(cooldown) || cooldown < 0 || cooldown > 4)
        { throw new ArgumentOutOfRangeException(nameof(cooldown)); }
        Actor = actor; Material = material; Sequence = sequence; Cooldown = cooldown;
    }
    /// <summary>The current exact character.</summary>
    public Character Actor { get; }
    /// <summary>The exact specifically authorized target/material binding.</summary>
    public CaskCoverHandlingMaterial Material { get; }
    /// <summary>The last accepted session-local sequence.</summary>
    public long Sequence { get; }
    /// <summary>Remaining bounded active seconds before another fresh activation.</summary>
    public double Cooldown { get; }
}
