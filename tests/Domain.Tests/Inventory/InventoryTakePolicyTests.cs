using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryTakePolicyTests
{
    [Fact]
    public void PermitsUnownedObjectWhenCharacterHasRemainingCapacity()
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("stone-mortar", "travelling-healer"));
        InventoryCapacitySnapshot capacities = Capacities(("travelling-healer", 2));

        bool result = InventoryTakePolicy.CanTakeObject(
            characterId,
            objectId,
            ownerships,
            capacities);

        Assert.True(result);
    }

    [Theory]
    [InlineData("travelling-healer")]
    [InlineData("seeker")]
    public void RejectsObjectAlreadyOwnedByAnyCharacter(string existingOwnerId)
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, existingOwnerId));
        InventoryCapacitySnapshot capacities = Capacities((characterId.Value, 12));

        bool result = InventoryTakePolicy.CanTakeObject(
            characterId,
            objectId,
            ownerships,
            capacities);

        Assert.False(result);
    }

    [Theory]
    [InlineData(false, 0, 0, false)]
    [InlineData(false, 2, 1, true)]
    [InlineData(false, 2, 2, false)]
    [InlineData(false, 2, 3, false)]
    [InlineData(true, 2, 0, false)]
    public void UsesCapacityPolicyForUnownedObjects(
        bool omitCapacity,
        int maximumObjectCount,
        int ownedObjectCount,
        bool expected)
    {
        CharacterId characterId = new("travelling-healer");
        InventoryOwnershipSnapshot ownerships = OwnershipsFor(
            characterId.Value,
            ownedObjectCount);
        InventoryCapacitySnapshot capacities = omitCapacity
            ? Capacities(("seeker", 100))
            : Capacities((characterId.Value, maximumObjectCount));

        bool result = InventoryTakePolicy.CanTakeObject(
            characterId,
            new ObjectId("healing-herb"),
            ownerships,
            capacities);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IgnoresUnrelatedOwnershipAndCapacityEntries()
    {
        CharacterId characterId = new("travelling-healer");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("stone-mortar", "travelling-healer"),
            ("journal", "seeker"),
            ("lantern", "sage"));
        InventoryCapacitySnapshot capacities = Capacities(
            ("seeker", 0),
            ("travelling-healer", 2),
            ("sage", 0));

        bool result = InventoryTakePolicy.CanTakeObject(
            characterId,
            new ObjectId("healing-herb"),
            ownerships,
            capacities);

        Assert.True(result);
    }

    [Fact]
    public void MatchesCharacterAndObjectIdentifiersByValue()
    {
        CharacterId requestedCharacterId = new("travelling-healer");
        ObjectId requestedObjectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("healing-herb", "seeker"));
        InventoryCapacitySnapshot capacities = Capacities(("travelling-healer", 2));

        bool result = InventoryTakePolicy.CanTakeObject(
            requestedCharacterId,
            requestedObjectId,
            ownerships,
            capacities);

        Assert.False(result);
        Assert.NotSame(requestedObjectId, ownerships.Entries[0].ObjectId);
        Assert.NotSame(requestedCharacterId, capacities.Entries[0].CharacterId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships();
        InventoryCapacitySnapshot capacities = Capacities((characterId.Value, 2));

        Assert.Throws<ArgumentNullException>(
            () => InventoryTakePolicy.CanTakeObject(null!, objectId, ownerships, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryTakePolicy.CanTakeObject(characterId, null!, ownerships, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryTakePolicy.CanTakeObject(characterId, objectId, null!, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryTakePolicy.CanTakeObject(characterId, objectId, ownerships, null!));
    }

    [Fact]
    public void DoesNotMutateEitherSnapshot()
    {
        CharacterId characterId = new("travelling-healer");
        InventoryOwnership firstOwnership = Ownership("stone-mortar", "travelling-healer");
        InventoryOwnership secondOwnership = Ownership("journal", "seeker");
        InventoryCapacity firstCapacity = Capacity("seeker", 3);
        InventoryCapacity secondCapacity = Capacity("travelling-healer", 2);
        InventoryOwnershipSnapshot ownerships = new(
            new[] { firstOwnership, secondOwnership });
        InventoryCapacitySnapshot capacities = new(
            new[] { firstCapacity, secondCapacity });

        _ = InventoryTakePolicy.CanTakeObject(
            characterId,
            new ObjectId("healing-herb"),
            ownerships,
            capacities);

        Assert.Collection(
            ownerships.Entries,
            entry => Assert.Same(firstOwnership, entry),
            entry => Assert.Same(secondOwnership, entry));
        Assert.Collection(
            capacities.Entries,
            entry => Assert.Same(firstCapacity, entry),
            entry => Assert.Same(secondCapacity, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(InventoryTakePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanTakeObject", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(InventoryOwnershipSnapshot),
                parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(InventoryCapacitySnapshot),
                parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    private static InventoryOwnershipSnapshot Ownerships(
        params (string ObjectId, string OwnerCharacterId)[] entries) =>
        new(entries.Select(entry => Ownership(entry.ObjectId, entry.OwnerCharacterId)));

    private static InventoryOwnershipSnapshot OwnershipsFor(
        string ownerCharacterId,
        int count) =>
        new(Enumerable.Range(1, count).Select(
            index => Ownership($"owned-object-{index}", ownerCharacterId)));

    private static InventoryOwnership Ownership(string objectId, string ownerCharacterId) =>
        new(new ObjectId(objectId), new CharacterId(ownerCharacterId));

    private static InventoryCapacitySnapshot Capacities(
        params (string CharacterId, int MaximumObjectCount)[] entries) =>
        new(entries.Select(entry => Capacity(entry.CharacterId, entry.MaximumObjectCount)));

    private static InventoryCapacity Capacity(string characterId, int maximumObjectCount) =>
        new(new CharacterId(characterId), maximumObjectCount);
}
