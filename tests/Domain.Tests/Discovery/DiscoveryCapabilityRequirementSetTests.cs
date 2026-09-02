using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryCapabilityRequirementSetTests
{
    [Fact]
    public void EmptySetIsExplicitlyValid()
    {
        DiscoveryCapabilityRequirementSet requirements = new(
            Array.Empty<DiscoveryCapabilityRequirement>());

        Assert.Empty(requirements.Entries);
    }

    [Fact]
    public void PreservesAuthoredOrderAndExactInstances()
    {
        DiscoveryCapabilityRequirement first = Create(
            "concealed-healing-herb",
            "identify-healing-plants");
        DiscoveryCapabilityRequirement second = Create(
            "partial-inscription",
            "read-ancient-script");

        DiscoveryCapabilityRequirementSet requirements = new(new[] { first, second });

        Assert.Collection(
            requirements.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void CopiesCallerCollection()
    {
        DiscoveryCapabilityRequirement original = Create(
            "concealed-healing-herb",
            "identify-healing-plants");
        DiscoveryCapabilityRequirement replacement = Create(
            "partial-inscription",
            "read-ancient-script");
        DiscoveryCapabilityRequirement[] source = [original];

        DiscoveryCapabilityRequirementSet requirements = new(source);
        source[0] = replacement;

        Assert.Collection(requirements.Entries, entry => Assert.Same(original, entry));
    }

    [Fact]
    public void RejectsDuplicateTargetObjectIdentifiersWithDifferentCapabilities()
    {
        DiscoveryCapabilityRequirement first = Create(
            "concealed-healing-herb",
            "identify-healing-plants");
        DiscoveryCapabilityRequirement duplicate = Create(
            "concealed-healing-herb",
            "forage-safely");

        Assert.Throws<ArgumentException>(
            () => new DiscoveryCapabilityRequirementSet(new[] { first, duplicate }));
    }

    [Fact]
    public void DetectsDuplicateTargetObjectIdentifiersByValue()
    {
        ObjectId firstObjectId = new("concealed-healing-herb");
        ObjectId valueEqualObjectId = new("concealed-healing-herb");
        DiscoveryCapabilityRequirement first = new(
            firstObjectId,
            "identify-healing-plants");
        DiscoveryCapabilityRequirement duplicate = new(
            valueEqualObjectId,
            "forage-safely");

        Assert.NotSame(firstObjectId, valueEqualObjectId);
        Assert.Throws<ArgumentException>(
            () => new DiscoveryCapabilityRequirementSet(new[] { first, duplicate }));
    }

    [Fact]
    public void RejectsNullCollectionAndEntries()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DiscoveryCapabilityRequirementSet(null!));
        Assert.Throws<ArgumentException>(
            () => new DiscoveryCapabilityRequirementSet(
                new DiscoveryCapabilityRequirement[] { null! }));
    }

    [Fact]
    public void ExposesOnlyTheReadOnlyRequirementSetSurface()
    {
        DiscoveryCapabilityRequirement entry = Create(
            "concealed-healing-herb",
            "identify-healing-plants");
        DiscoveryCapabilityRequirementSet requirements = new(new[] { entry });
        Type setType = typeof(DiscoveryCapabilityRequirementSet);

        Assert.Throws<NotSupportedException>(
            () => ((IList<DiscoveryCapabilityRequirement>)requirements.Entries).Add(entry));

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
                    typeof(IReadOnlyList<DiscoveryCapabilityRequirement>),
                    property.Type);
                Assert.False(property.CanWrite);
            });
        Assert.Collection(
            constructor.GetParameters(),
            parameter => Assert.Equal(
                typeof(IEnumerable<DiscoveryCapabilityRequirement>),
                parameter.ParameterType));
        Assert.Empty(setType.GetEvents());
        Assert.Empty(publicMethods);
        Assert.True(setType.IsSealed);
    }

    private static DiscoveryCapabilityRequirement Create(
        string objectId,
        string requiredCapabilityKey) =>
        new(new ObjectId(objectId), requiredCapabilityKey);
}
