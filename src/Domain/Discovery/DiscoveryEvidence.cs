using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>Associates authored evidence with the object visibility required to reveal it.</summary>
public sealed record DiscoveryEvidence
{
    /// <summary>Creates one immutable evidence-to-world-object discovery definition.</summary>
    public DiscoveryEvidence(
        EvidenceId evidenceId,
        ObjectId objectId,
        VisibilityState minimumVisibilityState,
        bool isCritical)
    {
        ArgumentNullException.ThrowIfNull(evidenceId);
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(minimumVisibilityState);

        EvidenceId = evidenceId;
        ObjectId = objectId;
        MinimumVisibilityState = minimumVisibilityState;
        IsCritical = isCritical;
    }

    /// <summary>Gets the stable identifier of the evidence revealed by this discovery.</summary>
    public EvidenceId EvidenceId { get; }

    /// <summary>Gets the identifier of the world object that can reveal the evidence.</summary>
    public ObjectId ObjectId { get; }

    /// <summary>Gets the minimum object visibility state that reveals the evidence.</summary>
    public VisibilityState MinimumVisibilityState { get; }

    /// <summary>Gets whether the opening scenario depends on this evidence remaining reachable.</summary>
    public bool IsCritical { get; }
}
