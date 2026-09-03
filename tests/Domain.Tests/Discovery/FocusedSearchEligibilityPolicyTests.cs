using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Discovery;

public sealed class FocusedSearchEligibilityPolicyTests
{
    [Fact]
    public void AllowsEligibleSearchWithSatisfiedCapabilityOnlyCondition()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("concealed-healing-herb", "identify-healing-plants"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(),
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory());

        Assert.True(result);
    }

    [Fact]
    public void AllowsEligibleSearchWithSatisfiedToolOnlyCondition()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-inscription");
        DiscoveryConditionSet conditions = Conditions(
            toolRequirements:
            [
                ToolRequirement("concealed-inscription", "field-lens"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(),
            conditions,
            Capabilities(),
            Inventory("field-lens"));

        Assert.True(result);
    }

    [Fact]
    public void RequiresEveryAuthoredConditionWhenCapabilityAndToolAreCombined()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("concealed-healing-herb", "identify-healing-plants"),
            ],
            toolRequirements:
            [
                ToolRequirement("concealed-healing-herb", "field-lens"),
            ]);

        Assert.True(
            FocusedSearchEligibilityPolicy.CanStartSearch(
                request,
                Activities(),
                conditions,
                Capabilities("identify-healing-plants"),
                Inventory("field-lens")));
        Assert.False(
            FocusedSearchEligibilityPolicy.CanStartSearch(
                request,
                Activities(),
                conditions,
                Capabilities(),
                Inventory("field-lens")));
        Assert.False(
            FocusedSearchEligibilityPolicy.CanStartSearch(
                request,
                Activities(),
                conditions,
                Capabilities("identify-healing-plants"),
                Inventory()));
    }

    [Fact]
    public void FailsClosedWhenTargetHasNoCondition()
    {
        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            Request("seeker", "concealed-healing-herb"),
            Activities(),
            Conditions(),
            Capabilities("identify-healing-plants"),
            Inventory("field-lens"));

        Assert.False(result);
    }

    [Fact]
    public void FailsClosedWhenTargetConditionIsUnsatisfied()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("concealed-healing-herb", "identify-healing-plants"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(),
            conditions,
            Capabilities("read-ancient-script"),
            Inventory());

        Assert.False(result);
    }

    [Fact]
    public void IgnoresUnrelatedActivitiesAndRequirements()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("weathered-inscription", "read-ancient-script"),
                CapabilityRequirement("concealed-healing-herb", "identify-healing-plants"),
            ],
            toolRequirements:
            [
                ToolRequirement("wagon-residue", "sample-vial"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(("travelling-healer", "concealed-healing-herb")),
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory());

        Assert.True(result);
    }

    [Fact]
    public void RejectsSearchWhenSearcherAlreadyHasAnActivity()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("concealed-healing-herb", "identify-healing-plants"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(("seeker", "weathered-inscription")),
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory());

        Assert.False(result);
    }

    [Fact]
    public void DelegatesConditionEvaluationForTheRequestsTargetByValue()
    {
        ObjectId requestTargetObjectId = new("concealed-healing-herb");
        ObjectId conditionTargetObjectId = new("concealed-healing-herb");
        FocusedSearchRequest request = new(new CharacterId("seeker"), requestTargetObjectId);
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements:
            [
                CapabilityRequirement("weathered-inscription", "read-ancient-script"),
                new DiscoveryCapabilityRequirement(
                    conditionTargetObjectId,
                    "identify-healing-plants"),
            ]);

        bool result = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            Activities(),
            conditions,
            Capabilities("identify-healing-plants"),
            Inventory());

        Assert.True(result);
        Assert.NotSame(requestTargetObjectId, conditionTargetObjectId);
        Assert.Equal(requestTargetObjectId, conditionTargetObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        FocusedSearchRequest request = Request("seeker", "concealed-healing-herb");
        FocusedSearchActivitySnapshot activities = Activities();
        DiscoveryConditionSet conditions = Conditions();
        CharacterCapabilitySet capabilities = Capabilities();
        CharacterInventory inventory = Inventory();

        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchEligibilityPolicy.CanStartSearch(
                null!, activities, conditions, capabilities, inventory));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchEligibilityPolicy.CanStartSearch(
                request, null!, conditions, capabilities, inventory));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchEligibilityPolicy.CanStartSearch(
                request, activities, null!, capabilities, inventory));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchEligibilityPolicy.CanStartSearch(
                request, activities, conditions, null!, inventory));
        Assert.Throws<ArgumentNullException>(
            () => FocusedSearchEligibilityPolicy.CanStartSearch(
                request, activities, conditions, capabilities, null!));
    }

    [Fact]
    public void RetainsEveryInputInstanceAndOrderWithoutMutation()
    {
        CharacterId searcherCharacterId = new("seeker");
        ObjectId targetObjectId = new("concealed-healing-herb");
        FocusedSearchRequest request = new(searcherCharacterId, targetObjectId);
        FocusedSearchActivity firstActivity = Activity("travelling-healer", "weathered-inscription");
        FocusedSearchActivity secondActivity = Activity("sage", "wagon-residue");
        FocusedSearchActivitySnapshot activities = new(new[] { firstActivity, secondActivity });
        DiscoveryCapabilityRequirement firstRequirement =
            CapabilityRequirement("weathered-inscription", "read-ancient-script");
        DiscoveryCapabilityRequirement secondRequirement =
            new(targetObjectId, "identify-healing-plants");
        DiscoveryToolRequirement firstToolRequirement =
            ToolRequirement("weathered-inscription", "field-lens");
        DiscoveryToolRequirement secondToolRequirement =
            new(targetObjectId, new ObjectId("sample-vial"));
        DiscoveryConditionSet conditions = Conditions(
            capabilityRequirements: [firstRequirement, secondRequirement],
            toolRequirements: [firstToolRequirement, secondToolRequirement]);
        CharacterCapability firstCapability = new(
            "read-ancient-script",
            "Reads ancient script.");
        CharacterCapability secondCapability = new(
            "identify-healing-plants",
            "Identifies healing plants.");
        CharacterCapabilitySet capabilities = new(new[] { firstCapability, secondCapability });
        ObjectId firstObject = new("field-lens");
        ObjectId secondObject = new("sample-vial");
        CharacterInventory inventory = new(new[] { firstObject, secondObject });

        _ = FocusedSearchEligibilityPolicy.CanStartSearch(
            request,
            activities,
            conditions,
            capabilities,
            inventory);

        Assert.Same(searcherCharacterId, request.SearcherCharacterId);
        Assert.Same(targetObjectId, request.TargetObjectId);
        Assert.Collection(
            activities.Entries,
            entry => Assert.Same(firstActivity, entry),
            entry => Assert.Same(secondActivity, entry));
        Assert.Collection(
            conditions.CapabilityRequirements.Entries,
            entry => Assert.Same(firstRequirement, entry),
            entry => Assert.Same(secondRequirement, entry));
        Assert.Collection(
            conditions.ToolRequirements.Entries,
            entry => Assert.Same(firstToolRequirement, entry),
            entry => Assert.Same(secondToolRequirement, entry));
        Assert.Collection(
            capabilities.Entries,
            entry => Assert.Same(firstCapability, entry),
            entry => Assert.Same(secondCapability, entry));
        Assert.Collection(
            inventory.ObjectReferences,
            entry => Assert.Same(firstObject, entry),
            entry => Assert.Same(secondObject, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(FocusedSearchEligibilityPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanStartSearch", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(FocusedSearchRequest), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(FocusedSearchActivitySnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(DiscoveryConditionSet), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterCapabilitySet), parameter.ParameterType),
            parameter => Assert.Equal(typeof(CharacterInventory), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static FocusedSearchRequest Request(string characterId, string targetObjectId) =>
        new(new CharacterId(characterId), new ObjectId(targetObjectId));

    private static FocusedSearchActivity Activity(string characterId, string targetObjectId) =>
        new(Request(characterId, targetObjectId));

    private static FocusedSearchActivitySnapshot Activities(
        params (string CharacterId, string TargetObjectId)[] entries) =>
        new(entries.Select(entry => Activity(entry.CharacterId, entry.TargetObjectId)));

    private static DiscoveryConditionSet Conditions(
        DiscoveryCapabilityRequirement[]? capabilityRequirements = null,
        DiscoveryToolRequirement[]? toolRequirements = null) =>
        new(
            new DiscoveryCapabilityRequirementSet(
                capabilityRequirements ?? Array.Empty<DiscoveryCapabilityRequirement>()),
            new DiscoveryToolRequirementSet(
                toolRequirements ?? Array.Empty<DiscoveryToolRequirement>()));

    private static DiscoveryCapabilityRequirement CapabilityRequirement(
        string objectId,
        string capabilityKey) =>
        new(new ObjectId(objectId), capabilityKey);

    private static DiscoveryToolRequirement ToolRequirement(string objectId, string toolObjectId) =>
        new(new ObjectId(objectId), new ObjectId(toolObjectId));

    private static CharacterCapabilitySet Capabilities(params string[] keys) =>
        new(keys.Select(key => new CharacterCapability(key, $"Capability {key}.")));

    private static CharacterInventory Inventory(params string[] objectIds) =>
        new(objectIds.Select(objectId => new ObjectId(objectId)));
}
