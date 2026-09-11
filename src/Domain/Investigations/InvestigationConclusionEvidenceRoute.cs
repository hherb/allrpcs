using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Defines one authored combination of evidence required for a conclusion.</summary>
public sealed class InvestigationConclusionEvidenceRoute
{
    /// <summary>Gets the maximum number of evidence identities in one route.</summary>
    public const int MaximumEvidenceCount = 16;

    /// <summary>Creates an immutable authored evidence route without evaluating it.</summary>
    public InvestigationConclusionEvidenceRoute(
        InvestigationConclusion conclusion,
        IEnumerable<EvidenceId> requiredEvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(conclusion);
        ArgumentNullException.ThrowIfNull(requiredEvidenceIds);

        List<EvidenceId> retainedEvidenceIds = [];
        HashSet<EvidenceId> identities = [];
        foreach (EvidenceId? evidenceId in requiredEvidenceIds)
        {
            if (evidenceId is null)
            {
                throw new ArgumentException(
                    "Required evidence identities cannot contain null.",
                    nameof(requiredEvidenceIds));
            }

            if (!identities.Add(evidenceId))
            {
                throw new ArgumentException(
                    "An evidence route cannot repeat an evidence identity.",
                    nameof(requiredEvidenceIds));
            }

            retainedEvidenceIds.Add(evidenceId);
            if (retainedEvidenceIds.Count > MaximumEvidenceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredEvidenceIds),
                    $"An evidence route accepts at most {MaximumEvidenceCount} identities.");
            }
        }

        if (retainedEvidenceIds.Count == 0)
        {
            throw new ArgumentException(
                "An evidence route requires at least one evidence identity.",
                nameof(requiredEvidenceIds));
        }

        Conclusion = conclusion;
        RequiredEvidenceIds = retainedEvidenceIds.AsReadOnly();
    }

    /// <summary>Gets the exact authored conclusion requiring this evidence.</summary>
    public InvestigationConclusion Conclusion { get; }

    /// <summary>Gets exact required evidence identities in stable authored order.</summary>
    public IReadOnlyList<EvidenceId> RequiredEvidenceIds { get; }
}
