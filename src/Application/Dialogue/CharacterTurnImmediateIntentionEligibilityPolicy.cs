using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Checks whether a response's immediate intention is currently permitted without applying that
/// current-time boundary to future commitments or executing any intention.
/// </summary>
public static class CharacterTurnImmediateIntentionEligibilityPolicy
{
    /// <summary>Returns whether the structurally valid immediate intention is currently permitted.</summary>
    public static bool IsEligible(
        CharacterTurnResponse response,
        CharacterIntentionAffordanceSnapshot affordances)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(affordances);

        CharacterIntention intention = response.ImmediateIntention;
        if (!CharacterIntentionValidationPolicy.Validate(intention).IsValid)
        {
            return false;
        }

        if (intention.Type == CharacterIntentionType.Wait)
        {
            return true;
        }

        return affordances.Entries.Any(affordance =>
            affordance.Type == intention.Type
            && affordance.TargetObjectId == intention.TargetObjectId);
    }
}
