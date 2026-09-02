using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class DiscoveryToolPolicyTests
{
    [Fact]
    public void SatisfiesRequirementWhenInventoryContainsRequiredTool()
    {
        DiscoveryToolRequirement requirement = Requirement("field-lens");
        CharacterInventory inventory = Inventory("field-lens");

        bool result = DiscoveryToolPolicy.IsSatisfied(requirement, inventory);

        Assert.True(result);
    }

    [Fact]
    public void FailsClosedForEmptyInventory()
    {
        DiscoveryToolRequirement requirement = Requirement("field-lens");

        bool result = DiscoveryToolPolicy.IsSatisfied(requirement, Inventory());

        Assert.False(result);
    }

    [Fact]
    public void FailsClosedForNonmatchingInventory()
    {
        DiscoveryToolRequirement requirement = Requirement("field-lens");

        bool result = DiscoveryToolPolicy.IsSatisfied(requirement, Inventory("sample-vial"));

        Assert.False(result);
    }

    [Fact]
    public void IgnoresUnrelatedInventoryEntriesAndAuthoredOrder()
    {
        DiscoveryToolRequirement requirement = Requirement("field-lens");
        CharacterInventory inventory = Inventory("sample-vial", "field-lens", "wax-tablet");

        bool result = DiscoveryToolPolicy.IsSatisfied(requirement, inventory);

        Assert.True(result);
    }

    [Fact]
    public void MatchesToolIdentifiersByValue()
    {
        ObjectId requiredToolObjectId = new("field-lens");
        ObjectId inventoryObjectId = new("field-lens");
        DiscoveryToolRequirement requirement = new(
            new ObjectId("concealed-inscription"),
            requiredToolObjectId);
        CharacterInventory inventory = new(new[] { inventoryObjectId });

        bool result = DiscoveryToolPolicy.IsSatisfied(requirement, inventory);

        Assert.True(result);
        Assert.NotSame(requiredToolObjectId, inventoryObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        DiscoveryToolRequirement requirement = Requirement("field-lens");
        CharacterInventory inventory = Inventory();

        Assert.Throws<ArgumentNullException>(() => DiscoveryToolPolicy.IsSatisfied(null!, inventory));
        Assert.Throws<ArgumentNullException>(() => DiscoveryToolPolicy.IsSatisfied(requirement, null!));
    }

    [Fact]
    public void RetainsInputsWithoutMutation()
    {
        ObjectId targetObjectId = new("concealed-inscription");
        ObjectId requiredToolObjectId = new("field-lens");
        DiscoveryToolRequirement requirement = new(targetObjectId, requiredToolObjectId);
        ObjectId first = new("sample-vial");
        ObjectId second = new("field-lens");
        CharacterInventory inventory = new(new[] { first, second });

        _ = DiscoveryToolPolicy.IsSatisfied(requirement, inventory);

        Assert.Same(targetObjectId, requirement.ObjectId);
        Assert.Same(requiredToolObjectId, requirement.RequiredToolObjectId);
        Assert.Collection(
            inventory.ObjectReferences,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEvaluationOperation()
    {
        Type policyType = typeof(DiscoveryToolPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("IsSatisfied", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(DiscoveryToolRequirement), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterInventory), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static DiscoveryToolRequirement Requirement(string toolObjectId) =>
        new(new ObjectId("concealed-inscription"), new ObjectId(toolObjectId));

    private static CharacterInventory Inventory(params string[] objectIds) =>
        new(objectIds.Select(objectId => new ObjectId(objectId)));
}
