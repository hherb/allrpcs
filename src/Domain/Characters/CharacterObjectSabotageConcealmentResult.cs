using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh concealment and evidence state from one accepted sabotage.</summary>
public sealed record CharacterObjectSabotageConcealmentResult
{
    /// <summary>Creates one immutable concealment result.</summary>
    public CharacterObjectSabotageConcealmentResult(
        WorldObjectConcealmentSnapshot concealment,
        CharacterObjectSabotageEvidenceResult evidenceResult)
    {
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidenceResult);

        Concealment = concealment;
        EvidenceResult = evidenceResult;
    }

    /// <summary>Gets the fresh snapshot containing the exact concealed target identifier.</summary>
    public WorldObjectConcealmentSnapshot Concealment { get; }

    /// <summary>Gets the existing fresh evidence-transition result for the same candidate.</summary>
    public CharacterObjectSabotageEvidenceResult EvidenceResult { get; }
}
