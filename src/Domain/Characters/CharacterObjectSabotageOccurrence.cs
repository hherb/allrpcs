using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact object sabotage proposal to its authoritative target candidate.</summary>
public sealed record CharacterObjectSabotageOccurrence
{
    /// <summary>Creates one immutable occurrence candidate without applying an effect.</summary>
    public CharacterObjectSabotageOccurrence(
        CharacterObjectSabotageProposal proposal,
        WorldObject target)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(target);

        Proposal = proposal;
        Target = target;
    }

    /// <summary>Gets the exact eligible sabotage proposal.</summary>
    public CharacterObjectSabotageProposal Proposal { get; }

    /// <summary>Gets the exact authoritative target candidate.</summary>
    public WorldObject Target { get; }
}
