namespace Allrpcs.Domain.Characters;

/// <summary>Orients one explicit incompatibility from one exact declared character's perspective.</summary>
public sealed record CharacterBeliefGoalIncompatibilityPerspective
{
    /// <summary>Creates one exact subject-to-counterpart orientation of the declaration.</summary>
    public CharacterBeliefGoalIncompatibilityPerspective(
        CharacterBeliefGoalIncompatibility declaration,
        Character subjectCharacter)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        ArgumentNullException.ThrowIfNull(subjectCharacter);

        if (ReferenceEquals(subjectCharacter, declaration.FirstCharacter))
        {
            Declaration = declaration;
            SubjectCharacter = declaration.FirstCharacter;
            SubjectBelief = declaration.FirstBelief;
            SubjectGoal = declaration.FirstGoal;
            CounterpartCharacter = declaration.SecondCharacter;
            CounterpartBelief = declaration.SecondBelief;
            CounterpartGoal = declaration.SecondGoal;
            return;
        }

        if (ReferenceEquals(subjectCharacter, declaration.SecondCharacter))
        {
            Declaration = declaration;
            SubjectCharacter = declaration.SecondCharacter;
            SubjectBelief = declaration.SecondBelief;
            SubjectGoal = declaration.SecondGoal;
            CounterpartCharacter = declaration.FirstCharacter;
            CounterpartBelief = declaration.FirstBelief;
            CounterpartGoal = declaration.FirstGoal;
            return;
        }

        throw new ArgumentException(
            "The subject must be one exact character retained by the declaration.",
            nameof(subjectCharacter));
    }

    /// <summary>Gets the exact incompatibility declaration.</summary>
    public CharacterBeliefGoalIncompatibility Declaration { get; }

    /// <summary>Gets the exact declared subject character.</summary>
    public Character SubjectCharacter { get; }

    /// <summary>Gets the subject's exact declared belief.</summary>
    public CharacterBelief SubjectBelief { get; }

    /// <summary>Gets the subject's exact declared goal.</summary>
    public CharacterGoal SubjectGoal { get; }

    /// <summary>Gets the exact other character in the declaration.</summary>
    public Character CounterpartCharacter { get; }

    /// <summary>Gets the counterpart's exact declared belief.</summary>
    public CharacterBelief CounterpartBelief { get; }

    /// <summary>Gets the counterpart's exact declared goal.</summary>
    public CharacterGoal CounterpartGoal { get; }
}
