using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeCompletionResultTests
{
    [Fact]
    public void RetainsExactActivityAndOutcomeSnapshotInstances()
    {
        FocusedSearchActivitySnapshot activities = Activities();
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();

        FocusedSearchOutcomeCompletionResult result = new(activities, outcomes);

        Assert.Same(activities, result.Activities);
        Assert.Same(outcomes, result.Outcomes);
    }

    [Fact]
    public void EqualityUsesRetainedSnapshotInstances()
    {
        FocusedSearchActivitySnapshot activities = Activities();
        FocusedSearchOutcomeSnapshot outcomes = Outcomes();
        FocusedSearchOutcomeCompletionResult first = new(activities, outcomes);
        FocusedSearchOutcomeCompletionResult same = new(activities, outcomes);
        FocusedSearchOutcomeCompletionResult differentActivities = new(Activities(), outcomes);
        FocusedSearchOutcomeCompletionResult differentOutcomes = new(activities, Outcomes());

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentActivities);
        Assert.NotEqual(first, differentOutcomes);
    }

    [Fact]
    public void RejectsNullSnapshots()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcomeCompletionResult(null!, Outcomes()));
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcomeCompletionResult(Activities(), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableCompletionResultSurface()
    {
        Type resultType = typeof(FocusedSearchOutcomeCompletionResult);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Activities", typeof(FocusedSearchActivitySnapshot)),
            ("Outcomes", typeof(FocusedSearchOutcomeSnapshot)),
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
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchOutcomeSnapshot),
                parameter.ParameterType));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static FocusedSearchActivitySnapshot Activities() =>
        new(Array.Empty<FocusedSearchActivity>());

    private static FocusedSearchOutcomeSnapshot Outcomes() =>
        new(Array.Empty<FocusedSearchOutcome>());
}
