using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Names the authored conclusion that a dose of one stabilizing remedy tests.</summary>
/// <remarks>Historical name: the conclusion is no longer required before a dose.</remarks>
public sealed class StabilizingRemedyEvidenceRequirement
{
    /// <summary>Pairs the exact authored remedy with the conclusion its dose tests.</summary>
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

    /// <summary>Gets the exact authored conclusion the remedy dose tests.</summary>
    public InvestigationConclusion Conclusion { get; }
}
