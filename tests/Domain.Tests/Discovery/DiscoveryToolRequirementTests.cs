using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryToolRequirementTests
{
    [Fact]
    public void RetainsTargetAndRequiredToolIdentifierInstances()
    {
        ObjectId objectId = new("concealed-healing-herb");
        ObjectId requiredToolObjectId = new("bronze-gardening-trowel");

        DiscoveryToolRequirement requirement = new(objectId, requiredToolObjectId);

        Assert.Same(objectId, requirement.ObjectId);
        Assert.Same(requiredToolObjectId, requirement.RequiredToolObjectId);
    }

    [Fact]
    public void EqualityUsesTargetAndRequiredToolIdentifierValues()
    {
        DiscoveryToolRequirement first = new(
            new ObjectId("concealed-healing-herb"),
            new ObjectId("bronze-gardening-trowel"));
        DiscoveryToolRequirement same = new(
            new ObjectId("concealed-healing-herb"),
            new ObjectId("bronze-gardening-trowel"));
        DiscoveryToolRequirement differentTarget = new(
            new ObjectId("hidden-residue"),
            new ObjectId("bronze-gardening-trowel"));
        DiscoveryToolRequirement differentTool = new(
            new ObjectId("concealed-healing-herb"),
            new ObjectId("herbalists-hand-lens"));

        Assert.Equal(first, same);
        Assert.True(first == same);
        Assert.NotEqual(first, differentTarget);
        Assert.NotEqual(first, differentTool);
    }

    [Fact]
    public void DoesNotAddSameObjectPolicy()
    {
        ObjectId sharedObjectId = new("weathered-inscription");

        DiscoveryToolRequirement requirement = new(sharedObjectId, sharedObjectId);

        Assert.Same(sharedObjectId, requirement.ObjectId);
        Assert.Same(sharedObjectId, requirement.RequiredToolObjectId);
    }

    [Fact]
    public void RejectsNullValues()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryToolRequirement(
                null!,
                new ObjectId("bronze-gardening-trowel")));
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryToolRequirement(
                new ObjectId("concealed-healing-herb"),
                null!));
    }

    [Fact]
    public void ExposesOnlyTheImmutableRequirementSurface()
    {
        Type requirementType = typeof(DiscoveryToolRequirement);
        (string Name, Type Type)[] expectedProperties =
        [
            ("ObjectId", typeof(ObjectId)),
            ("RequiredToolObjectId", typeof(ObjectId)),
        ];

        (string Name, Type Type, bool CanWrite)[] actualProperties = requirementType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            expectedProperties,
            actualProperties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(actualProperties, property => Assert.False(property.CanWrite));
        Assert.Empty(requirementType.GetEvents());
        Assert.True(requirementType.IsSealed);
    }
}
