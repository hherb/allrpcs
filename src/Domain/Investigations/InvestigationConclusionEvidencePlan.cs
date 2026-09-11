namespace Allrpcs.Domain.Investigations;

/// <summary>Retains alternate authored evidence combinations for one exact conclusion.</summary>
public sealed class InvestigationConclusionEvidencePlan
{
    /// <summary>Gets the maximum number of alternate confirmation routes.</summary>
    public const int MaximumRouteCount = 8;

    /// <summary>Creates an immutable authored evidence plan without evaluating it.</summary>
    public InvestigationConclusionEvidencePlan(
        InvestigationConclusion conclusion,
        IEnumerable<InvestigationConclusionEvidenceRoute> routes)
    {
        ArgumentNullException.ThrowIfNull(conclusion);
        ArgumentNullException.ThrowIfNull(routes);

        List<InvestigationConclusionEvidenceRoute> retainedRoutes = [];
        foreach (InvestigationConclusionEvidenceRoute? route in routes)
        {
            if (route is null)
            {
                throw new ArgumentException("Evidence routes cannot contain null.", nameof(routes));
            }

            if (!ReferenceEquals(route.Conclusion, conclusion))
            {
                throw new ArgumentException(
                    "Every evidence route must retain the plan's exact conclusion.",
                    nameof(routes));
            }

            if (retainedRoutes.Any(existing => HaveSameEvidenceCombination(existing, route)))
            {
                throw new ArgumentException(
                    "An evidence plan cannot repeat an evidence combination.",
                    nameof(routes));
            }

            retainedRoutes.Add(route);
            if (retainedRoutes.Count > MaximumRouteCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(routes),
                    $"An evidence plan accepts at most {MaximumRouteCount} routes.");
            }
        }

        if (retainedRoutes.Count == 0)
        {
            throw new ArgumentException("An evidence plan requires at least one route.", nameof(routes));
        }

        Conclusion = conclusion;
        Routes = retainedRoutes.AsReadOnly();
    }

    /// <summary>Gets the exact authored conclusion.</summary>
    public InvestigationConclusion Conclusion { get; }

    /// <summary>Gets exact alternate routes in stable authored order.</summary>
    public IReadOnlyList<InvestigationConclusionEvidenceRoute> Routes { get; }

    private static bool HaveSameEvidenceCombination(
        InvestigationConclusionEvidenceRoute left,
        InvestigationConclusionEvidenceRoute right) =>
        left.RequiredEvidenceIds.Count == right.RequiredEvidenceIds.Count
        && left.RequiredEvidenceIds.All(right.RequiredEvidenceIds.Contains);
}
