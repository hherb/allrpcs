using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryToolRequirementSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        DiscoveryToolRequirementSet requirements = new(
            Array.Empty<DiscoveryToolRequirement>());

        Assert.Empty(requirements.Entries);
    }

    [Fact]
    public void PreservesAuthoredOrderAndExactInstances()
    {
        DiscoveryToolRequirement first = Create(
            "concealed-healing-herb",
            "field-lens");
        DiscoveryToolRequirement second = Create(
            "partial-inscription",
            "charcoal-rubbing-kit");

        DiscoveryToolRequirementSet requirements = new(new[] { first, second });

        Assert.Collection(
            requirements.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        DiscoveryToolRequirement original = Create(
            "concealed-healing-herb",
            "field-lens");
        DiscoveryToolRequirement replacement = Create(
            "partial-inscription",
            "charcoal-rubbing-kit");
        DiscoveryToolRequirement[] source = [original];

        DiscoveryToolRequirementSet requirements = new(source);
        source[0] = replacement;

        Assert.Collection(requirements.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateTargetObjectIdentifiersWithDifferentTools()
    {
        DiscoveryToolRequirement first = Create(
            "concealed-healing-herb",
            "field-lens");
        DiscoveryToolRequirement duplicate = Create(
            "concealed-healing-herb",
            "garden-trowel");

        Assert.Throws<ArgumentException>(
            () => new DiscoveryToolRequirementSet(new[] { first, duplicate }));
    }

    [Fact]
    public void DetectsDuplicateTargetObjectIdentifiersByValue()
    {
        ObjectId firstObjectId = new("concealed-healing-herb");
        ObjectId valueEqualObjectId = new("concealed-healing-herb");
        DiscoveryToolRequirement first = new(
            firstObjectId,
            new ObjectId("field-lens"));
        DiscoveryToolRequirement duplicate = new(
            valueEqualObjectId,
            new ObjectId("garden-trowel"));

        Assert.NotSame(firstObjectId, valueEqualObjectId);
        Assert.Throws<ArgumentException>(
            () => new DiscoveryToolRequirementSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryToolRequirementSet(null!));
        Assert.Throws<ArgumentException>(
            () => new DiscoveryToolRequirementSet(
                new DiscoveryToolRequirement[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlyRequirementSetSurface()
    {
        DiscoveryToolRequirement entry = Create(
            "concealed-healing-herb",
            "field-lens");
        DiscoveryToolRequirementSet requirements = new(new[] { entry });
        Type setType = typeof(DiscoveryToolRequirementSet);

        Assert.Throws<NotSupportedException>(
            () => ((IList<DiscoveryToolRequirement>)requirements.Entries).Add(entry));

        (string Name, Type Type, bool CanWrite)[] actualProperties = setType
            .GetProperties()
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();
        string[] publicMethods = setType
            .GetMethods()
            .Where(method => method.DeclaringType == setType && !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();
        System.Reflection.ConstructorInfo constructor = Assert.Single(setType.GetConstructors());

        Assert.Collection(
            actualProperties,
            property =>
            {
                Assert.Equal("Entries", property.Name);
                Assert.Equal(
                    typeof(IReadOnlyList<DiscoveryToolRequirement>),
                    property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<DiscoveryToolRequirement>),
                parameter.ParameterType));
        Assert.Empty(setType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(setType.IsSealed);
    }

    private static DiscoveryToolRequirement Create(
        string objectId,
        string requiredToolObjectId) =>
        new(new ObjectId(objectId), new ObjectId(requiredToolObjectId));
}
