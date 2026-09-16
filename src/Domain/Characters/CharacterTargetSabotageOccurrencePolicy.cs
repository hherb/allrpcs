namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether an eligible proposal names its exact retained target character.</summary>
public static class CharacterTargetSabotageOccurrencePolicy
{
    /// <summary>Returns whether one exact retained target may receive the proposed action.</summary>
    public static bool CanOccur(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageOccurrence candidate)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Proposal.AccessRequirement.TargetCharacterId == candidate.Target.Identity.Id
            && CharacterTargetSabotageEligibilityPolicy.CanPropose(
                actor,
                candidate.Target,
                interactionAccess,
                candidate.Proposal);
    }
}
