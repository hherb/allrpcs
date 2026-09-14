using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterBeliefGoalIncompatibilityPerspectiveProjectionTests
{
    [Fact]
    public void ProjectsExactlyTwoPerspectivesInStableDeclarationOrderWithoutMutation()
    {
        CharacterBeliefGoalIncompatibility declaration =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBelief[] firstBeliefs = [.. declaration.FirstCharacter.Beliefs.Entries];
        CharacterGoal[] firstGoals = [.. declaration.FirstCharacter.Goals.Entries];
        CharacterBelief[] secondBeliefs = [.. declaration.SecondCharacter.Beliefs.Entries];
        CharacterGoal[] secondGoals = [.. declaration.SecondCharacter.Goals.Entries];

        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> perspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(declaration);

        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterBeliefGoalIncompatibilityPerspective>>(perspectives);
        Assert.Collection(
            perspectives,
            perspective => AssertPerspective(
                perspective,
                declaration,
                declaration.FirstCharacter,
                declaration.FirstBelief,
                declaration.FirstGoal,
                declaration.SecondCharacter,
                declaration.SecondBelief,
                declaration.SecondGoal),
            perspective => AssertPerspective(
                perspective,
                declaration,
                declaration.SecondCharacter,
                declaration.SecondBelief,
                declaration.SecondGoal,
                declaration.FirstCharacter,
                declaration.FirstBelief,
                declaration.FirstGoal));
        Assert.Equal(firstBeliefs, declaration.FirstCharacter.Beliefs.Entries);
        Assert.Equal(firstGoals, declaration.FirstCharacter.Goals.Entries);
        Assert.Equal(secondBeliefs, declaration.SecondCharacter.Beliefs.Entries);
        Assert.Equal(secondGoals, declaration.SecondCharacter.Goals.Entries);
    }

    [Fact]
    public void FreshFixturesProduceIndependentPerspectivesWithExactNestedReferences()
    {
        CharacterBeliefGoalIncompatibility first =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();
        CharacterBeliefGoalIncompatibility second =
            RepresentativeSageEnemyAgentIncompatibilitySource.Create();

        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> firstPerspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(first);
        IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective> secondPerspectives =
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(second);

        Assert.NotSame(firstPerspectives, secondPerspectives);
        Assert.NotSame(firstPerspectives[0], secondPerspectives[0]);
        Assert.NotSame(firstPerspectives[1], secondPerspectives[1]);
        Assert.Same(first.FirstCharacter, firstPerspectives[0].SubjectCharacter);
        Assert.Same(first.SecondCharacter, firstPerspectives[1].SubjectCharacter);
        Assert.Same(second.FirstCharacter, secondPerspectives[0].SubjectCharacter);
        Assert.Same(second.SecondCharacter, secondPerspectives[1].SubjectCharacter);
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePureProjection()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterBeliefGoalIncompatibilityPerspectiveProjection.Project(null!));

        Type projectionType = typeof(CharacterBeliefGoalIncompatibilityPerspectiveProjection);
        System.Reflection.MethodInfo method = Assert.Single(
            projectionType.GetMethods().Where(candidate => candidate.DeclaringType == projectionType));

        Assert.Equal("Project", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(
            typeof(IReadOnlyList<CharacterBeliefGoalIncompatibilityPerspective>),
            method.ReturnType);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterBeliefGoalIncompatibility),
                parameter.ParameterType));
        Assert.Empty(projectionType.GetConstructors());
        Assert.Empty(projectionType.GetProperties());
        Assert.Empty(projectionType.GetEvents());
        Assert.True(projectionType.IsAbstract && projectionType.IsSealed);
    }

    private static void AssertPerspective(
        CharacterBeliefGoalIncompatibilityPerspective perspective,
        CharacterBeliefGoalIncompatibility declaration,
        Character subject,
        CharacterBelief subjectBelief,
        CharacterGoal subjectGoal,
        Character counterpart,
        CharacterBelief counterpartBelief,
        CharacterGoal counterpartGoal)
    {
        Assert.Same(declaration, perspective.Declaration);
        Assert.Same(subject, perspective.SubjectCharacter);
        Assert.Same(subjectBelief, perspective.SubjectBelief);
        Assert.Same(subjectGoal, perspective.SubjectGoal);
        Assert.Same(counterpart, perspective.CounterpartCharacter);
        Assert.Same(counterpartBelief, perspective.CounterpartBelief);
        Assert.Same(counterpartGoal, perspective.CounterpartGoal);
    }
}
