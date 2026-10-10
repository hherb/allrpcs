namespace Allrpcs.Domain.Actions;

/// <summary>One accepted abstract cover adjustment, without contents or durable-state authority.</summary>
public sealed class CaskCoverHandlingCommit
{
    /// <summary>Retains a consistent exact request and its next positive session sequence.</summary>
    public CaskCoverHandlingCommit(CaskCoverHandlingRequest request, long sequence)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ReferenceEquals(request.Actor, request.State.Actor)
            || !ReferenceEquals(request.Material, request.State.Material)
            || request.State.Sequence == long.MaxValue || sequence != request.State.Sequence + 1)
        { throw new ArgumentException("Handling must retain exact bindings and next sequence.", nameof(request)); }
        Request = request; Sequence = sequence;
    }
    /// <summary>The exact accepted attempt.</summary>
    public CaskCoverHandlingRequest Request { get; }
    /// <summary>The next accepted session-local sequence.</summary>
    public long Sequence { get; }
}
