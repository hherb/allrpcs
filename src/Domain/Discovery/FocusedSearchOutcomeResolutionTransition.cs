using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Applies and consumes one eligible focused-search outcome as a single pure resolution.
/// </summary>
public static class FocusedSearchOutcomeResolutionTransition
{
    /// <summary>
    /// Returns the perception and outcome snapshots produced by resolving the supplied outcome.
    /// </summary>
    public static FocusedSearchOutcomeResolutionResult Resolve(
        FocusedSearchOutcome outcome,
        CharacterPerceptionSnapshot perception,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        if (!FocusedSearchOutcomeResolutionPolicy.CanResolve(outcome, perception, outcomes))
        {
            throw new InvalidOperationException(
                "Only a perception-changing retained focused-search outcome may be resolved.");
        }

        CharacterPerceptionSnapshot resolvedPerception =
            FocusedSearchOutcomeApplicationTransition.Apply(outcome, perception);
        FocusedSearchOutcomeSnapshot remainingOutcomes =
            FocusedSearchOutcomeConsumptionTransition.Consume(outcome, outcomes);

        return new FocusedSearchOutcomeResolutionResult(
            resolvedPerception,
            remainingOutcomes);
    }
}
