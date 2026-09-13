namespace Allrpcs.Domain.Characters;

/// <summary>Checks whether one character may propose one object-targeted sabotage action.</summary>
public static class CharacterObjectSabotageEligibilityPolicy
{
    /// <summary>Returns whether the matching actor satisfies the proposal's complete access requirement.</summary>
    public static bool CanPropose(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        CharacterObjectSabotageProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(proposal);

        return proposal.ActorId == character.Identity.Id
            && CharacterActionAccessPolicy.IsSatisfied(
                character,
                physicalAccess,
                proposal.AccessRequirement);
    }
}
