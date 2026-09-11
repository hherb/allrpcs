using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Requires one exact confirmed conclusion before one remedy may stabilize disease.</summary>
public sealed class StabilizingRemedyEvidenceRequirement
{
    /// <summary>Creates an immutable evidence requirement without confirming it.</summary>
    public StabilizingRemedyEvidenceRequirement(
        StabilizingRemedyDefinition remedy,
        InvestigationConclusion conclusion)
    {
        ArgumentNullException.ThrowIfNull(remedy);
        ArgumentNullException.ThrowIfNull(conclusion);

        Remedy = remedy;
        Conclusion = conclusion;
    }

    /// <summary>Gets the exact authored stabilizing remedy.</summary>
    public StabilizingRemedyDefinition Remedy { get; }

    /// <summary>Gets the exact authored conclusion required as evidence.</summary>
    public InvestigationConclusion Conclusion { get; }
}
