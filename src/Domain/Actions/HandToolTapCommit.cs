namespace Allrpcs.Domain.Actions;

/// <summary>A single accepted abstract tool tap; no inventory, world-time or perception mutation.</summary>
public sealed class HandToolTapCommit
{
    /// <summary>Retains a consistent request and its next positive session-local sequence.</summary>
    public HandToolTapCommit(HandToolTapRequest request, long sequence)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ReferenceEquals(request.Actor, request.State.Actor) || !ReferenceEquals(request.Site, request.State.Site)
            || request.State.Sequence == long.MaxValue || sequence != request.State.Sequence + 1)
        { throw new ArgumentException("Tap must retain the exact request bindings and next sequence.", nameof(request)); }
        Request = request; Sequence = sequence;
    }
    /// <summary>The exact accepted request, not a copied or reconstructed attempt.</summary>
    public HandToolTapRequest Request { get; }
    /// <summary>The next accepted sequence within this transient action owner.</summary>
    public long Sequence { get; }
}
