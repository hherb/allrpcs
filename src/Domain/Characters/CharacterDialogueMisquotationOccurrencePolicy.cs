namespace Allrpcs.Domain.Characters;

/// <summary>Checks exact actor, text, audience, and proposal eligibility for a misquotation.</summary>
public static class CharacterDialogueMisquotationOccurrencePolicy
{
    /// <summary>Returns whether the exact delivered event can represent the eligible proposal.</summary>
    public static bool CanOccur(
        Character character,
        CharacterDialogueMisquotationOccurrence occurrence)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(occurrence);

        CharacterDialogueMisquotationProposal proposal = occurrence.Proposal;
        return CharacterDialogueMisquotationProposalPolicy.CanPropose(character, proposal)
            && ReferenceEquals(
                occurrence.DeliveredEvent.Utterance.SpeakerCharacterId,
                proposal.ActorId)
            && StringComparer.Ordinal.Equals(
                occurrence.DeliveredEvent.Utterance.Text,
                proposal.MisquotedText)
            && occurrence.DeliveredEvent.Audience.ListenerCharacterIds.Count > 0;
    }
}
