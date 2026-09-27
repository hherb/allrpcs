namespace Allrpcs.Application.Investigations;

/// <summary>Retains one completely preflighted fresh investigation-route lifecycle.</summary>
public sealed class InvestigationRouteSaveStateRestoration
{
    internal InvestigationRouteSaveStateRestoration(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> acceptedCompletions,
        OpeningRegionInvestigationRouteCompletion? consumedCompletion)
    {
        Progress = progress;
        AcceptedCompletions = acceptedCompletions;
        ConsumedCompletion = consumedCompletion;
    }

    /// <summary>Gets the fresh exact canonical progress.</summary>
    public OpeningRegionInvestigationRouteProgressSnapshot Progress { get; }

    /// <summary>Gets fresh accepted exact completion tokens in saved order.</summary>
    public IReadOnlyList<OpeningRegionInvestigationRouteCompletion> AcceptedCompletions { get; }

    /// <summary>Gets the exact consumed accepted token, or null.</summary>
    public OpeningRegionInvestigationRouteCompletion? ConsumedCompletion { get; }
}
