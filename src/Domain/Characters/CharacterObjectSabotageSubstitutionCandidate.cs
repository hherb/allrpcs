namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact substitution to discoverable evidence for the same occurrence.</summary>
public sealed class CharacterObjectSabotageSubstitutionCandidate
{
    /// <summary>Creates one immutable substitution candidate without applying either state change.</summary>
    public CharacterObjectSabotageSubstitutionCandidate(
        CharacterObjectSabotageSubstitution substitution,
        CharacterObjectSabotageEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(substitution);
        ArgumentNullException.ThrowIfNull(evidence);

        if (!ReferenceEquals(substitution.Occurrence, evidence.Occurrence))
        {
            throw new ArgumentException(
                "Substitution and evidence must retain the same exact occurrence.",
                nameof(evidence));
        }

        Substitution = substitution;
        Evidence = evidence;
    }

    /// <summary>Gets the exact occurrence-bound substitution.</summary>
    public CharacterObjectSabotageSubstitution Substitution { get; }

    /// <summary>Gets the exact occurrence-bound discoverable evidence.</summary>
    public CharacterObjectSabotageEvidence Evidence { get; }
}
