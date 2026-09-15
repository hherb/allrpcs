namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh substitution and evidence state from one accepted sabotage.</summary>
public sealed record CharacterObjectSabotageSubstitutionResult
{
    /// <summary>Creates one immutable substitution result.</summary>
    public CharacterObjectSabotageSubstitutionResult(
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        CharacterObjectSabotageEvidenceResult evidenceResult)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidenceResult);

        Substitutions = substitutions;
        EvidenceResult = evidenceResult;
    }

    /// <summary>Gets the fresh snapshot containing the exact accepted substitution.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot Substitutions { get; }

    /// <summary>Gets the existing fresh evidence-transition result for the same candidate.</summary>
    public CharacterObjectSabotageEvidenceResult EvidenceResult { get; }
}
