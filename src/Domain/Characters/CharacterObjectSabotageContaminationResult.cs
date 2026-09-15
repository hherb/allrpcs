using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh contamination and evidence state from one accepted sabotage.</summary>
public sealed record CharacterObjectSabotageContaminationResult
{
    /// <summary>Creates one immutable contamination result.</summary>
    public CharacterObjectSabotageContaminationResult(
        WorldObjectContaminationSnapshot contamination,
        CharacterObjectSabotageEvidenceResult evidenceResult)
    {
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidenceResult);

        Contamination = contamination;
        EvidenceResult = evidenceResult;
    }

    /// <summary>Gets the fresh snapshot containing the exact contaminated target identifier.</summary>
    public WorldObjectContaminationSnapshot Contamination { get; }

    /// <summary>Gets the existing fresh evidence-transition result for the same candidate.</summary>
    public CharacterObjectSabotageEvidenceResult EvidenceResult { get; }
}
