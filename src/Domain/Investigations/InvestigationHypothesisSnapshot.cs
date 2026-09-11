using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded authored hypotheses in stable supplied order.</summary>
public sealed class InvestigationHypothesisSnapshot
{
    /// <summary>Gets the maximum number of retained hypotheses.</summary>
    public const int MaximumHypothesisCount = 64;

    /// <summary>Creates an immutable copy of supplied hypotheses.</summary>
    public InvestigationHypothesisSnapshot(IEnumerable<InvestigationHypothesis> hypotheses)
    {
        ArgumentNullException.ThrowIfNull(hypotheses);

        List<InvestigationHypothesis> retainedHypotheses = [];
        HashSet<InvestigationHypothesisId> hypothesisIds = [];
        foreach (InvestigationHypothesis? hypothesis in hypotheses)
        {
            if (hypothesis is null)
            {
                throw new ArgumentException("Hypotheses cannot contain null.", nameof(hypotheses));
            }

            if (!hypothesisIds.Add(hypothesis.HypothesisId))
            {
                throw new ArgumentException(
                    "Hypotheses cannot contain duplicate identities.",
                    nameof(hypotheses));
            }

            retainedHypotheses.Add(hypothesis);
            if (retainedHypotheses.Count > MaximumHypothesisCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hypotheses),
                    $"A snapshot accepts at most {MaximumHypothesisCount} hypotheses.");
            }
        }

        Hypotheses = retainedHypotheses.AsReadOnly();
    }

    /// <summary>Gets exact hypotheses in stable supplied order.</summary>
    public IReadOnlyList<InvestigationHypothesis> Hypotheses { get; }
}
