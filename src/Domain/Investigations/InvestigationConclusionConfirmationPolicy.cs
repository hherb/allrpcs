using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Investigations;

/// <summary>Checks authored evidence combinations against one character-filtered evidence view.</summary>
public static class InvestigationConclusionConfirmationPolicy
{
    /// <summary>Reports whether at least one complete authored route is visible.</summary>
    public static bool CanConfirm(
        InvestigationConclusionEvidencePlan plan,
        CharacterEvidenceViewSnapshot evidence)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(evidence);

        return plan.Routes.Any(route => route.RequiredEvidenceIds.All(
            required => evidence.EvidenceIds.Contains(required)));
    }
}
