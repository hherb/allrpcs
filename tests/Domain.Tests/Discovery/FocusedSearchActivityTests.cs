using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchActivityTests
{
    [Fact]
    public void RetainsExactRequestInstance()
    {
        FocusedSearchRequest request = CreateRequest();

        FocusedSearchActivity activity = new(request);

        Assert.Same(request, activity.Request);
    }

    [Fact]
    public void EqualityUsesRequestValue()
    {
        FocusedSearchActivity first = new(CreateRequest());
        FocusedSearchActivity same = new(CreateRequest());
        FocusedSearchActivity different = new(
            new FocusedSearchRequest(
                new CharacterId("travelling-healer"),
                new ObjectId("weathered-inscription")));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void RejectsNullRequest()
    {
        Assert.Throws<ArgumentNullException>(() => new FocusedSearchActivity(null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableActivitySurface()
    {
        Type activityType = typeof(FocusedSearchActivity);
        System.Reflection.PropertyInfo property = Assert.Single(activityType.GetProperties());
        System.Reflection.ConstructorInfo constructor = Assert.Single(
            activityType.GetConstructors());
        System.Reflection.ParameterInfo parameter = Assert.Single(constructor.GetParameters());

        Assert.Equal("Request", property.Name);
        Assert.Equal(typeof(FocusedSearchRequest), property.PropertyType);
        Assert.False(property.CanWrite);
        Assert.Equal(typeof(FocusedSearchRequest), parameter.ParameterType);
        Assert.Empty(activityType.GetEvents());
        Assert.True(activityType.IsSealed);
    }

    private static FocusedSearchRequest CreateRequest() =>
        new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"));
}
