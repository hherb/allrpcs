using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchOutcomeTests
{
    [Fact]
    public void RetainsExactRequestAndVisibilityStateInstances()
    {
        FocusedSearchRequest request = CreateRequest();
        VisibilityState resultingVisibilityState = VisibilityState.Identified;

        FocusedSearchOutcome outcome = new(request, resultingVisibilityState);

        Assert.Same(request, outcome.Request);
        Assert.Same(resultingVisibilityState, outcome.ResultingVisibilityState);
    }

    [Fact]
    public void EqualityUsesRequestAndVisibilityStateValues()
    {
        FocusedSearchOutcome first = new(CreateRequest(), VisibilityState.Identified);
        FocusedSearchOutcome same = new(CreateRequest(), VisibilityState.Identified);
        FocusedSearchOutcome differentRequest = new(
            new FocusedSearchRequest(
                new CharacterId("travelling-healer"),
                new ObjectId("weathered-inscription")),
            VisibilityState.Identified);
        FocusedSearchOutcome differentVisibilityState = new(
            CreateRequest(),
            VisibilityState.Understood);

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentRequest);
        Assert.NotEqual(first, differentVisibilityState);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcome(null!, VisibilityState.Unnoticed));
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchOutcome(CreateRequest(), null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableOutcomeSurface()
    {
        Type outcomeType = typeof(FocusedSearchOutcome);
        (string Name, Type Type)[] expectedProperties =
        [
            ("Request", typeof(FocusedSearchRequest)),
            ("ResultingVisibilityState", typeof(VisibilityState)),
        ];
        (string Name, Type Type, bool CanWrite)[] actualProperties = outcomeType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            outcomeType.GetConstructors());

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchRequest), parameter.ParameterType),
            parameter => Assert.Equal(typeof(VisibilityState), parameter.ParameterType));
        Assert.Empty(outcomeType.GetEvents());
        Assert.True(outcomeType.IsSealed);
    }

    private static FocusedSearchRequest CreateRequest() =>
        new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"));
}
