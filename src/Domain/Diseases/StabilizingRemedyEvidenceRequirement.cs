using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Names the authored conclusion that a dose of one stabilizing remedy tests.</summary>
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

    /// <summary>Gets the exact authored conclusion the remedy dose confirms.</summary>
    public InvestigationConclusion Conclusion { get; }
}
