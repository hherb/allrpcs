using System.Collections.ObjectModel;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded confidence and evidence references for one uncertain belief.</summary>
public sealed record CharacterBeliefAssessment
{
    /// <summary>Gets the minimum confidence in basis points.</summary>
    public const int MinimumConfidenceBasisPoints = 0;

    /// <summary>Gets the maximum confidence in basis points.</summary>
    public const int MaximumConfidenceBasisPoints = 10_000;

    /// <summary>Gets the maximum evidence references retained for either disposition.</summary>
    public const int MaximumEvidenceCountPerDisposition = 32;

    /// <summary>Creates one immutable assessment of an uncertain epistemic memory.</summary>
    public CharacterBeliefAssessment(
        CharacterEpistemicMemory belief,
        int confidenceBasisPoints,
        IEnumerable<EvidenceId> supportingEvidenceIds,
        IEnumerable<EvidenceId> contradictingEvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(belief);
        if (!ReferenceEquals(belief.Category, CharacterEpistemicMemoryCategory.UncertainBelief))
        {
            throw new ArgumentException(
                "Only an uncertain belief accepts a belief assessment.",
                nameof(belief));
        }

        if (confidenceBasisPoints is < MinimumConfidenceBasisPoints
            or > MaximumConfidenceBasisPoints)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidenceBasisPoints),
                confidenceBasisPoints,
                $"Confidence must be between {MinimumConfidenceBasisPoints} and "
                    + $"{MaximumConfidenceBasisPoints} basis points.");
        }

        ReadOnlyCollection<EvidenceId> supporting = RequireEvidence(
            supportingEvidenceIds,
            nameof(supportingEvidenceIds));
        ReadOnlyCollection<EvidenceId> contradicting = RequireEvidence(
            contradictingEvidenceIds,
            nameof(contradictingEvidenceIds));
        HashSet<EvidenceId> supportingIds = [.. supporting];
        if (contradicting.Any(supportingIds.Contains))
        {
            throw new ArgumentException(
                "One evidence identity cannot both support and contradict the same belief.",
                nameof(contradictingEvidenceIds));
        }

        Belief = belief;
        ConfidenceBasisPoints = confidenceBasisPoints;
        SupportingEvidenceIds = supporting;
        ContradictingEvidenceIds = contradicting;
    }

    /// <summary>Gets the exact uncertain epistemic memory being assessed.</summary>
    public CharacterEpistemicMemory Belief { get; }

    /// <summary>Gets stored confidence from zero to one thousand basis points.</summary>
    public int ConfidenceBasisPoints { get; }

    /// <summary>Gets exact supporting evidence identities in stable supplied order.</summary>
    public IReadOnlyList<EvidenceId> SupportingEvidenceIds { get; }

    /// <summary>Gets exact contradicting evidence identities in stable supplied order.</summary>
    public IReadOnlyList<EvidenceId> ContradictingEvidenceIds { get; }

    private static ReadOnlyCollection<EvidenceId> RequireEvidence(
        IEnumerable<EvidenceId> evidenceIds,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(evidenceIds, parameterName);

        List<EvidenceId> retainedIds = new();
        HashSet<EvidenceId> uniqueIds = new();
        foreach (EvidenceId evidenceId in evidenceIds)
        {
            if (evidenceId is null)
            {
                throw new ArgumentException(
                    "Evidence references cannot contain null values.",
                    parameterName);
            }

            if (!uniqueIds.Add(evidenceId))
            {
                throw new ArgumentException(
                    "Evidence references must be unique within one disposition.",
                    parameterName);
            }

            retainedIds.Add(evidenceId);
            if (retainedIds.Count > MaximumEvidenceCountPerDisposition)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    retainedIds.Count,
                    $"A belief accepts at most {MaximumEvidenceCountPerDisposition} evidence "
                        + "references per disposition.");
            }
        }

        return retainedIds.AsReadOnly();
    }
}
