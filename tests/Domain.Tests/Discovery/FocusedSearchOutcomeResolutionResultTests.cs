using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeResolutionResultTests
{
    [Fact]
    public void RetainsExactPerceptionAndOutcomeSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        FocusedSearchOutcomeResolutionResult result = new(perception, outcomes);

        Assert.Same(perception, result.Perception);
        Assert.Same(outcomes, result.Outcomes);
    }

    [Fact]
    public void EqualityUsesRetainedSnapshotInstances()
    {
        CharacterPerceptionSnapshot perception = Perception();
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();
        FocusedSearchOutcomeResolutionResult first = new(perception, outcomes);
        FocusedSearchOutcomeResolutionResult same = new(perception, outcomes);
        FocusedSearchOutcomeResolutionResult differentPerception = new(Perception(), outcomes);
        FocusedSearchOutcomeResolutionResult differentOutcomes = new(perception, Outcomes());

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentPerception);
        Assert.NotEqual(first, differentOutcomes);
    }

    [Fact]
    public void RejectsNullSnapshots()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcomeResolutionResult(null!, Outcomes()));
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcomeResolutionResult(Perception(), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableResolutionResultSurface()
    {
        Type resultType = typeof(FocusedSearchOutcomeResolutionResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Outcomes", typeof(FocusedSearchOutcomeSnapshot)),
            ("Perception", typeof(CharacterPerceptionSnapshot)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            resultType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterPerceptionSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static CharacterPerceptionSnapshot Perception() =>
        new(new CharacterId("seeker"), Array.Empty<WorldObjectVisibility>());

    private static FocusedSearchOutcomeSnapshot Outcomes() =>
        new(Array.Empty<FocusedSearchOutcome>());
}
