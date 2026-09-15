namespace Allrpcs.Domain.Characters;

/// <summary>Checks whether one non-actor heard the exact source and delivered misquotation.</summary>
public static class CharacterDialogueMisquotationEvidencePolicy
{
    /// <summary>Returns whether the exact candidate is observable witness evidence.</summary>
    public static bool CanObserve(
        Character actor,
        CharacterDialogueMisquotationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(evidence);

        CharacterDialogueMisquotationOccurrence occurrence = evidence.Occurrence;
        CharacterDialogueMisquotationProposal proposal = occurrence.Proposal;
        return CharacterDialogueMisquotationOccurrencePolicy.CanOccur(actor, occurrence)
            && evidence.WitnessCharacterId != proposal.ActorId
            && proposal.SourceEvent.Audience.ListenerCharacterIds.Contains(
                evidence.WitnessCharacterId)
            && occurrence.DeliveredEvent.Audience.ListenerCharacterIds.Contains(
                evidence.WitnessCharacterId);
    }
}
