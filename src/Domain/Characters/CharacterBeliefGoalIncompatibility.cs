namespace Allrpcs.Domain.Characters;

/// <summary>Binds two exact characters to explicitly incompatible authored beliefs and goals.</summary>
public sealed record CharacterBeliefGoalIncompatibility
{
    /// <summary>Gets the maximum supported authored rationale length.</summary>
    public const int MaximumRationaleLength = 500;

    /// <summary>Creates one immutable explicit belief-and-goal incompatibility declaration.</summary>
    public CharacterBeliefGoalIncompatibility(
        Character firstCharacter,
        CharacterBelief firstBelief,
        CharacterGoal firstGoal,
        Character secondCharacter,
        CharacterBelief secondBelief,
        CharacterGoal secondGoal,
        string rationale)
    {
        ArgumentNullException.ThrowIfNull(firstCharacter);
        ArgumentNullException.ThrowIfNull(firstBelief);
        ArgumentNullException.ThrowIfNull(firstGoal);
        ArgumentNullException.ThrowIfNull(secondCharacter);
        ArgumentNullException.ThrowIfNull(secondBelief);
        ArgumentNullException.ThrowIfNull(secondGoal);

        if (firstCharacter.Identity.Id == secondCharacter.Identity.Id)
        {
            throw new ArgumentException(
                "An incompatibility must bind two value-distinct character identities.",
                nameof(secondCharacter));
        }

        RequireExactBelief(firstCharacter, firstBelief, nameof(firstBelief));
        RequireExactGoal(firstCharacter, firstGoal, nameof(firstGoal));
        RequireExactBelief(secondCharacter, secondBelief, nameof(secondBelief));
        RequireExactGoal(secondCharacter, secondGoal, nameof(secondGoal));

        FirstCharacter = firstCharacter;
        FirstBelief = firstBelief;
        FirstGoal = firstGoal;
        SecondCharacter = secondCharacter;
        SecondBelief = secondBelief;
        SecondGoal = secondGoal;
        Rationale = AuthoredCharacterText.RequireNonBlank(
            rationale,
            MaximumRationaleLength,
            nameof(rationale));
    }

    /// <summary>Gets the exact first character.</summary>
    public Character FirstCharacter { get; }

    /// <summary>Gets the exact belief retained by the first character.</summary>
    public CharacterBelief FirstBelief { get; }

    /// <summary>Gets the exact goal retained by the first character.</summary>
    public CharacterGoal FirstGoal { get; }

    /// <summary>Gets the exact second character.</summary>
    public Character SecondCharacter { get; }

    /// <summary>Gets the exact belief retained by the second character.</summary>
    public CharacterBelief SecondBelief { get; }

    /// <summary>Gets the exact goal retained by the second character.</summary>
    public CharacterGoal SecondGoal { get; }

    /// <summary>Gets the bounded authored explanation declaring why both positions are incompatible.</summary>
    public string Rationale { get; }

    private static void RequireExactBelief(
        Character character,
        CharacterBelief belief,
        string parameterName)
    {
        if (!character.Beliefs.Entries.Any(entry => ReferenceEquals(entry, belief)))
        {
            throw new ArgumentException(
                "The belief must be retained by its exact owning character.",
                parameterName);
        }
    }

    private static void RequireExactGoal(
        Character character,
        CharacterGoal goal,
        string parameterName)
    {
        if (!character.Goals.Entries.Any(entry => ReferenceEquals(entry, goal)))
        {
            throw new ArgumentException(
                "The goal must be retained by its exact owning character.",
                parameterName);
        }
    }
}
