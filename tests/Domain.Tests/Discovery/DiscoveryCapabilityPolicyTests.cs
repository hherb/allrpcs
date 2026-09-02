using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryCapabilityPolicyTests
{
    [Fact]
    public void SatisfiesRequirementWhenCapabilityKeyMatches()
    {
        DiscoveryCapabilityRequirement requirement = Requirement("identify-healing-plants");
        CharacterCapabilitySet capabilities = Capabilities(
            ("identify-healing-plants", "Recognizes healing plants."));

        bool result = DiscoveryCapabilityPolicy.IsSatisfied(requirement, capabilities);

        Assert.True(result);
    }

    [Fact]
    public void FailsClosedForEmptyOrNonmatchingCapabilitySet()
    {
        DiscoveryCapabilityRequirement requirement = Requirement("identify-healing-plants");

        Assert.False(DiscoveryCapabilityPolicy.IsSatisfied(requirement, Capabilities()));
        Assert.False(
            DiscoveryCapabilityPolicy.IsSatisfied(
                requirement,
                Capabilities(("read-ancient-script", "Reads ancient inscriptions."))));
    }

    [Fact]
    public void IgnoresUnrelatedCapabilitiesAndAuthoredOrder()
    {
        DiscoveryCapabilityRequirement requirement = Requirement("identify-healing-plants");
        CharacterCapabilitySet capabilities = Capabilities(
            ("read-ancient-script", "Reads ancient inscriptions."),
            ("identify-healing-plants", "Recognizes healing plants."),
            ("field-medicine", "Assesses injuries."));

        bool result = DiscoveryCapabilityPolicy.IsSatisfied(requirement, capabilities);

        Assert.True(result);
    }

    [Fact]
    public void MatchesCanonicalCapabilityKeysByOrdinalValue()
    {
        string requirementKey = new("identify-healing-plants".ToCharArray());
        string capabilityKey = new("identify-healing-plants".ToCharArray());
        DiscoveryCapabilityRequirement requirement = Requirement(requirementKey);
        CharacterCapability capability = new(capabilityKey, "Recognizes healing plants.");

        bool result = DiscoveryCapabilityPolicy.IsSatisfied(
            requirement,
            new CharacterCapabilitySet(new[] { capability }));

        Assert.True(result);
        Assert.NotSame(requirement.RequiredCapabilityKey, capability.Key);
    }

    [Fact]
    public void IgnoresAuthoredCapabilityDescription()
    {
        DiscoveryCapabilityRequirement requirement = Requirement("identify-healing-plants");
        CharacterCapabilitySet first = Capabilities(
            ("identify-healing-plants", "Recognizes common healing plants."));
        CharacterCapabilitySet second = Capabilities(
            ("identify-healing-plants", "Completely different authored detail."));

        Assert.True(DiscoveryCapabilityPolicy.IsSatisfied(requirement, first));
        Assert.True(DiscoveryCapabilityPolicy.IsSatisfied(requirement, second));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DiscoveryCapabilityRequirement requirement = Requirement("identify-healing-plants");
        CharacterCapabilitySet capabilities = Capabilities();

        Assert.Throws<ArgumentNullException>(
            () => DiscoveryCapabilityPolicy.IsSatisfied(null!, capabilities));
        Assert.Throws<ArgumentNullException>(
            () => DiscoveryCapabilityPolicy.IsSatisfied(requirement, null!));
    }

    [Fact]
    public void RetainsInputsWithoutMutation()
    {
        ObjectId objectId = new("concealed-healing-herb");
        DiscoveryCapabilityRequirement requirement = new(objectId, "identify-healing-plants");
        CharacterCapability first = new("read-ancient-script", "Reads inscriptions.");
        CharacterCapability second = new(
            "identify-healing-plants",
            "Recognizes healing plants.");
        CharacterCapabilitySet capabilities = new(new[] { first, second });

        _ = DiscoveryCapabilityPolicy.IsSatisfied(requirement, capabilities);

        Assert.Same(objectId, requirement.ObjectId);
        Assert.Equal("identify-healing-plants", requirement.RequiredCapabilityKey);
        Assert.Collection(
            capabilities.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEvaluationOperation()
    {
        Type policyType = typeof(DiscoveryCapabilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsSatisfied", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(DiscoveryCapabilityRequirement),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterCapabilitySet), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static DiscoveryCapabilityRequirement Requirement(string capabilityKey) =>
        new(new ObjectId("concealed-healing-herb"), capabilityKey);

    private static CharacterCapabilitySet Capabilities(
        params (string Key, string Description)[] entries) =>
        new(entries.Select(entry => new CharacterCapability(entry.Key, entry.Description)));
}
