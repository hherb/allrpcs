namespace Allrpcs.Domain.Characters;

/// <summary>Checks whether one actor may propose one character-targeted sabotage action.</summary>
public static class CharacterTargetSabotageEligibilityPolicy
{
    /// <summary>Returns whether the actor and target satisfy the proposal's complete access requirement.</summary>
    public static bool CanPropose(
        Character actor,
        Character target,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(proposal);

        return proposal.ActorId == actor.Identity.Id
            && CharacterTargetAccessPolicy.IsSatisfied(
                actor,
                target,
                interactionAccess,
                proposal.AccessRequirement);
    }
}
