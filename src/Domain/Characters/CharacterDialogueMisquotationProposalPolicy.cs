namespace Allrpcs.Domain.Characters;

/// <summary>Checks actor identity, exact-source hearing, and capability for a misquotation.</summary>
public static class CharacterDialogueMisquotationProposalPolicy
{
    /// <summary>Returns whether one character can propose the exact retained misquotation.</summary>
    public static bool CanPropose(
        Character character,
        CharacterDialogueMisquotationProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(proposal);

        return character.Identity.Id == proposal.ActorId
            && proposal.SourceEvent.Audience.ListenerCharacterIds.Contains(proposal.ActorId)
            && character.Capabilities.Entries.Any(capability =>
                StringComparer.Ordinal.Equals(
                    capability.Key,
                    proposal.RequiredCapabilityKey));
    }
}
