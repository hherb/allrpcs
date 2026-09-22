namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact singleton action request to one action-specific candidate.</summary>
public abstract class CharacterSabotageActionCandidateBinding
{
    /// <summary>Creates one provider-neutral exact candidate binding.</summary>
    protected CharacterSabotageActionCandidateBinding(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionKind actionKind)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(actionKind);

        if (request.ActionKinds.Count != 1
            || !ReferenceEquals(request.ActionKinds[0], actionKind))
        {
            throw new ArgumentException(
                "A candidate binding requires the request's one exact action kind.",
                nameof(actionKind));
        }

        Request = request;
        ActionKind = actionKind;
    }

    /// <summary>Gets the exact singleton action request that authored the candidate.</summary>
    public CharacterSabotageActionConsiderationRequest Request { get; }

    /// <summary>Gets the request's exact canonical action kind.</summary>
    public CharacterSabotageActionKind ActionKind { get; }
}

/// <summary>Binds one exact singleton action request to one strongly typed candidate.</summary>
/// <typeparam name="TCandidate">The action-specific candidate type.</typeparam>
public sealed class CharacterSabotageActionCandidateBinding<TCandidate>
    : CharacterSabotageActionCandidateBinding
    where TCandidate : class
{
    /// <summary>Creates one immutable exact typed candidate binding.</summary>
    public CharacterSabotageActionCandidateBinding(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionKind actionKind,
        TCandidate candidate)
        : base(request, actionKind)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        Candidate = candidate;
    }

    /// <summary>Gets the exact action-specific candidate.</summary>
    public TCandidate Candidate { get; }
}
