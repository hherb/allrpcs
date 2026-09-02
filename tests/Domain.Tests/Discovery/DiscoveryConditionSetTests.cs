using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryConditionSetTests
{
    [Fact]
    public void RetainsExactRequirementSetInstances()
    {
        DiscoveryCapabilityRequirementSet capabilityRequirements = new(
            new[] { CreateCapabilityRequirement("concealed-healing-herb") });
        DiscoveryToolRequirementSet toolRequirements = new(
            new[] { CreateToolRequirement("partial-inscription") });

        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);

        Assert.Same(capabilityRequirements, conditions.CapabilityRequirements);
        Assert.Same(toolRequirements, conditions.ToolRequirements);
    }

    [Fact]
    public void EmptyConstituentSetsAreValid()
    {
        DiscoveryCapabilityRequirementSet capabilityRequirements = new(
            Array.Empty<DiscoveryCapabilityRequirement>());
        DiscoveryToolRequirementSet toolRequirements = new(
            Array.Empty<DiscoveryToolRequirement>());

        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);

        Assert.Empty(conditions.CapabilityRequirements.Entries);
        Assert.Empty(conditions.ToolRequirements.Entries);
    }

    [Fact]
    public void PermitsTargetObjectOverlapAcrossConstituentSets()
    {
        const string sharedTargetObjectId = "concealed-healing-herb";
        DiscoveryCapabilityRequirement capabilityRequirement =
            CreateCapabilityRequirement(sharedTargetObjectId);
        DiscoveryToolRequirement toolRequirement = CreateToolRequirement(sharedTargetObjectId);
        DiscoveryCapabilityRequirementSet capabilityRequirements = new(
            new[] { capabilityRequirement });
        DiscoveryToolRequirementSet toolRequirements = new(new[] { toolRequirement });

        DiscoveryConditionSet conditions = new(capabilityRequirements, toolRequirements);

        Assert.Same(capabilityRequirement, Assert.Single(conditions.CapabilityRequirements.Entries));
        Assert.Same(toolRequirement, Assert.Single(conditions.ToolRequirements.Entries));
    }

    [Fact]
    public void RejectsNullRequirementSets()
    {
        DiscoveryCapabilityRequirementSet capabilityRequirements = new(
            Array.Empty<DiscoveryCapabilityRequirement>());
        DiscoveryToolRequirementSet toolRequirements = new(
            Array.Empty<DiscoveryToolRequirement>());

        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryConditionSet(null!, toolRequirements));
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryConditionSet(capabilityRequirements, null!));
    }

    [Fact]
    public void ExposesOnlyTheAuthoredConditionSurface()
    {
        Type conditionType = typeof(DiscoveryConditionSet);

        (string Name, Type Type, bool CanWrite)[] actualProperties = conditionType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = conditionType
            .GetMethods()
            .Where(method => method.DeclaringType == conditionType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(conditionType.GetConstructors());

        Assert.Collection(
            actualProperties,
            property =>
            {
                Assert.Equal("CapabilityRequirements", property.Name);
                Assert.Equal(typeof(DiscoveryCapabilityRequirementSet), property.Type);
                Assert.False(property.CanWrite);
            },
            property =>
            {
                Assert.Equal("ToolRequirements", property.Name);
                Assert.Equal(typeof(DiscoveryToolRequirementSet), property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(DiscoveryCapabilityRequirementSet),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(DiscoveryToolRequirementSet),
                parameter.ParameterType));
        Assert.Empty(conditionType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(conditionType.IsSealed);
    }

    private static DiscoveryCapabilityRequirement CreateCapabilityRequirement(string objectId) =>
        new(new ObjectId(objectId), "identify-healing-plants");

    private static DiscoveryToolRequirement CreateToolRequirement(string objectId) =>
        new(new ObjectId(objectId), new ObjectId("field-lens"));
}
