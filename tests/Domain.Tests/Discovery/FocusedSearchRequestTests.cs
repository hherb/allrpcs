using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchRequestTests
{
    [Fact]
    public void RetainsSearcherAndTargetIdentifierInstances()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("weathered-inscription");

        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
    }

    [Fact]
    public void EqualityUsesSearcherAndTargetIdentifierValues()
    {
        FocusedSearchRequest first = new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"));
        FocusedSearchRequest same = new(
            new CharacterId("seeker"),
            new ObjectId("weathered-inscription"));
        FocusedSearchRequest differentSearcher = new(
            new CharacterId("travelling-healer"),
            new ObjectId("weathered-inscription"));
        FocusedSearchRequest differentTarget = new(
            new CharacterId("seeker"),
            new ObjectId("concealed-healing-herb"));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentSearcher);
        Assert.NotEqual(first, differentTarget);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchRequest(
                null!,
                new ObjectId("weathered-inscription")));
        Assert.Throws<ArgumentNullException>(
            () => new FocusedSearchRequest(
                new CharacterId("seeker"),
                null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRequestSurface()
    {
        Type requestType = typeof(FocusedSearchRequest);
        (string Name, Type Type)[] expectedProperties =
        [
            ("SearcherCharacterId", typeof(CharacterId)),
            ("TargetObjectId", typeof(ObjectId)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = requestType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }
}
