using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityPerspectiveTests
{
    [Fact]
    public void OrientsTheExactFirstCharacterAgainstTheExactSecondCharacter()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();

        CharacterBeliefGoalIncompatibilityPerspective perspective = new(
            declaration,
            declaration.FirstCharacter);

        Assert.Same(declaration, perspective.Declaration);
        Assert.Same(declaration.FirstCharacter, perspective.SubjectCharacter);
        Assert.Same(declaration.FirstBelief, perspective.SubjectBelief);
        Assert.Same(declaration.FirstGoal, perspective.SubjectGoal);
        Assert.Same(declaration.SecondCharacter, perspective.CounterpartCharacter);
        Assert.Same(declaration.SecondBelief, perspective.CounterpartBelief);
        Assert.Same(declaration.SecondGoal, perspective.CounterpartGoal);
    }

    [Fact]
    public void OrientsTheExactSecondCharacterAgainstTheExactFirstCharacter()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();

        CharacterBeliefGoalIncompatibilityPerspective perspective = new(
            declaration,
            declaration.SecondCharacter);

        Assert.Same(declaration, perspective.Declaration);
        Assert.Same(declaration.SecondCharacter, perspective.SubjectCharacter);
        Assert.Same(declaration.SecondBelief, perspective.SubjectBelief);
        Assert.Same(declaration.SecondGoal, perspective.SubjectGoal);
        Assert.Same(declaration.FirstCharacter, perspective.CounterpartCharacter);
        Assert.Same(declaration.FirstBelief, perspective.CounterpartBelief);
        Assert.Same(declaration.FirstGoal, perspective.CounterpartGoal);
    }

    [Fact]
    public void RejectsNullAndValueEqualReplacementSubjects()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        Character replacementFirst = Copy(declaration.FirstCharacter);
        Character replacementSecond = Copy(declaration.SecondCharacter);

        Assert.NotSame(declaration.FirstCharacter, replacementFirst);
        Assert.Equal(declaration.FirstCharacter.Identity.Id, replacementFirst.Identity.Id);
        Assert.NotSame(declaration.SecondCharacter, replacementSecond);
        Assert.Equal(declaration.SecondCharacter.Identity.Id, replacementSecond.Identity.Id);
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityPerspective(null!, declaration.FirstCharacter));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterBeliefGoalIncompatibilityPerspective(declaration, null!));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityPerspective(declaration, replacementFirst));
        Assert.Throws<ArgumentException>(() =>
            new CharacterBeliefGoalIncompatibilityPerspective(declaration, replacementSecond));
    }

    private static Character Copy(Character character) => new(
        character.Identity,
        character.PublicBackground,
        character.PrivateBackground,
        character.Capabilities,
        character.Limitations,
        character.ControllerAssignment,
        character.Conditions,
        character.Inventory,
        character.Knowledge,
        character.Beliefs,
        character.Relationships,
        character.Goals);
}
