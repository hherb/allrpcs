namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact character-targeted sabotage proposal to its exact target candidate.</summary>
public sealed class CharacterTargetSabotageOccurrence
{
    /// <summary>Creates one immutable occurrence candidate without applying an effect.</summary>
    public CharacterTargetSabotageOccurrence(
        CharacterTargetSabotageProposal proposal,
        Character target)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(target);

        Proposal = proposal;
        Target = target;
    }

    /// <summary>Gets the exact eligible sabotage proposal.</summary>
    public CharacterTargetSabotageProposal Proposal { get; }

    /// <summary>Gets the exact target character candidate.</summary>
    public Character Target { get; }
}
