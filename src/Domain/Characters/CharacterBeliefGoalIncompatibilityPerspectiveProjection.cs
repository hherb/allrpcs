namespace Allrpcs.Domain.Characters;

/// <summary>Projects both exact orientations of one explicit character incompatibility.</summary>
public static class CharacterBeliefGoalIncompatibilityPerspectiveProjection
{
    /// <summary>Returns exactly two fresh perspectives in stable declaration order.</summary>
    public static IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> Project(
        CharacterBeliefGoalIncompatibility declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);

        return Array.AsReadOnly(new[]
        {
            new CharacterBeliefGoalIncompatibilityPerspective(
                declaration,
                declaration.FirstCharacter),
            new CharacterBeliefGoalIncompatibilityPerspective(
                declaration,
                declaration.SecondCharacter),
        });
    }
}
