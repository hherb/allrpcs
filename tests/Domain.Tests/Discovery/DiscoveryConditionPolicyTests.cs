using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryConditionPolicyTests
{
    [Fact]
    public void SatisfiesCapabilityOnlyCondition()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement(targetObjectId, "identify-healing-plants"),
            ]);

        bool result = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory());

        Assert.True(result);
    }

    [Fact]
    public void SatisfiesToolOnlyCondition()
    {
        ObjectId targetObjectId = new("concealed-inscription");
        DiscoveryConditionSet conditions = Conditions(
            toolRequirements:
            [
                ToolRequirement(targetObjectId, "field-lens"),
            ]);

        bool result = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            conditions,
            Capabilities(),
            Inventory("field-lens"));

        Assert.True(result);
    }

    [Fact]
    public void RequiresBothConditionsWhenBothAreAuthored()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement(targetObjectId, "identify-healing-plants"),
            ],
            toolRequirements:
            [
                ToolRequirement(targetObjectId, "field-lens"),
            ]);

        Assert.True(
            DiscoveryConditionPolicy.IsSatisfied(
                targetObjectId,
                conditions,
                Capabilities("identify-healing-plants"),
                Inventory("field-lens")));
        Assert.False(
            DiscoveryConditionPolicy.IsSatisfied(
                targetObjectId,
                conditions,
                Capabilities(),
                Inventory("field-lens")));
        Assert.False(
            DiscoveryConditionPolicy.IsSatisfied(
                targetObjectId,
                conditions,
                Capabilities("identify-healing-plants"),
                Inventory()));
    }

    [Fact]
    public void FailsClosedWhenTargetHasNoCondition()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");

        bool result = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            Conditions(),
            Capabilities("identify-healing-plants"),
            Inventory("field-lens"));

        Assert.False(result);
    }

    [Fact]
    public void IgnoresRequirementsForUnrelatedTargets()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement(new ObjectId("partial-inscription"), "read-ancient-script"),
                CapabilityRequirement(targetObjectId, "identify-healing-plants"),
            ],
            toolRequirements:
            [
                ToolRequirement(new ObjectId("hidden-residue"), "sample-vial"),
                ToolRequirement(targetObjectId, "field-lens"),
            ]);

        bool result = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory("field-lens"));

        Assert.True(result);
    }

    [Fact]
    public void MatchesTargetIdentifiersByValue()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        ObjectId capabilityTargetObjectId = new("concealed-healing-herb");
        ObjectId toolTargetObjectId = new("concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement(capabilityTargetObjectId, "identify-healing-plants"),
            ],
            toolRequirements:
            [
                ToolRequirement(toolTargetObjectId, "field-lens"),
            ]);

        bool result = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory("field-lens"));

        Assert.True(result);
        Assert.NotSame(targetObjectId, capabilityTargetObjectId);
        Assert.NotSame(targetObjectId, toolTargetObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions();
        CharacterCapabilitySet capabilities = Capabilities();
        CharacterInventory inventory = Inventory();

        Assert.Throws<ArgumentNullException>(
            () => DiscoveryConditionPolicy.IsSatisfied(null!, conditions, capabilities, inventory));
        Assert.Throws<ArgumentNullException>(
            () => DiscoveryConditionPolicy.IsSatisfied(targetObjectId, null!, capabilities, inventory));
        Assert.Throws<ArgumentNullException>(
            () => DiscoveryConditionPolicy.IsSatisfied(targetObjectId, conditions, null!, inventory));
        Assert.Throws<ArgumentNullException>(
            () => DiscoveryConditionPolicy.IsSatisfied(targetObjectId, conditions, capabilities, null!));
    }

    [Fact]
    public void RetainsInputsWithoutMutation()
    {
        ObjectId targetObjectId = new("concealed-healing-herb");
        DiscoveryCapabilityRequirement capabilityRequirement = CapabilityRequirement(
            targetObjectId,
            "identify-healing-plants");
        DiscoveryToolRequirement toolRequirement = ToolRequirement(targetObjectId, "field-lens");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements: [capabilityRequirement],
            toolRequirements: [toolRequirement]);
        CharacterCapability capability = new(
            "identify-healing-plants",
            "Recognizes healing plants.");
        CharacterCapabilitySet capabilities = new(new[] { capability });
        ObjectId toolObjectId = new("field-lens");
        CharacterInventory inventory = new(new[] { toolObjectId });

        _ = DiscoveryConditionPolicy.IsSatisfied(
            targetObjectId,
            conditions,
            capabilities,
            inventory);

        Assert.Same(capabilityRequirement, Assert.Single(conditions.CapabilityRequirements.Entries));
        Assert.Same(toolRequirement, Assert.Single(conditions.ToolRequirements.Entries));
        Assert.Same(capability, Assert.Single(capabilities.Entries));
        Assert.Same(toolObjectId, Assert.Single(inventory.ObjectReferences));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEvaluationOperation()
    {
        Type policyType = typeof(DiscoveryConditionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsSatisfied", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(DiscoveryConditionSet), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterCapabilitySet), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterInventory), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static DiscoveryConditionSet Conditions(
        DiscoveryCapabilityRequirement[]? capabilityRequirements = null,
        DiscoveryToolRequirement[]? toolRequirements = null) =>
        new(
            new DiscoveryCapabilityRequirementSet(
                capabilityRequirements ?? Array.Empty<DiscoveryCapabilityRequirement>()),
            new DiscoveryToolRequirementSet(
                toolRequirements ?? Array.Empty<DiscoveryToolRequirement>()));

    private static DiscoveryCapabilityRequirement CapabilityRequirement(
        ObjectId objectId,
        string capabilityKey) =>
        new(objectId, capabilityKey);

    private static DiscoveryToolRequirement ToolRequirement(ObjectId objectId, string toolObjectId) =>
        new(objectId, new ObjectId(toolObjectId));

    private static CharacterCapabilitySet Capabilities(params string[] keys) =>
        new(keys.Select(key => new CharacterCapability(key, $"Capability {key}.")));

    private static CharacterInventory Inventory(params string[] objectIds) =>
        new(objectIds.Select(objectId => new ObjectId(objectId)));
}
