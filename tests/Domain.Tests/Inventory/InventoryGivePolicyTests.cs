using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryGivePolicyTests
{
    [Fact]
    public void PermitsOwnedObjectWhenDistinctRecipientHasRemainingCapacity()
    {
        CharacterId giverCharacterId = new("travelling-healer");
        CharacterId recipientCharacterId = new("seeker");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, giverCharacterId.Value));
        InventoryCapacitySnapshot capacities = Capacities(
            (recipientCharacterId.Value, 1));

        bool result = InventoryGivePolicy.CanGiveObject(
            giverCharacterId,
            recipientCharacterId,
            objectId,
            ownerships,
            capacities);

        Assert.True(result);
    }

    [Fact]
    public void RejectsUnownedObject()
    {
        bool result = InventoryGivePolicy.CanGiveObject(
            new CharacterId("travelling-healer"),
            new CharacterId("seeker"),
            new ObjectId("healing-herb"),
            Ownerships(("stone-mortar", "travelling-healer")),
            Capacities(("seeker", 12)));

        Assert.False(result);
    }

    [Fact]
    public void RejectsObjectOwnedByAnotherCharacter()
    {
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, "sage"));

        bool result = InventoryGivePolicy.CanGiveObject(
            new CharacterId("travelling-healer"),
            new CharacterId("seeker"),
            objectId,
            ownerships,
            Capacities(("seeker", 12)));

        Assert.False(result);
    }

    [Fact]
    public void RejectsSelfGiveByValue()
    {
        CharacterId giverCharacterId = new("travelling-healer");
        CharacterId recipientCharacterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, giverCharacterId.Value));

        bool result = InventoryGivePolicy.CanGiveObject(
            giverCharacterId,
            recipientCharacterId,
            objectId,
            ownerships,
            Capacities((recipientCharacterId.Value, 12)));

        Assert.False(result);
        Assert.NotSame(giverCharacterId, recipientCharacterId);
    }

    [Theory]
    [InlineData(true, 0, 0, false)]
    [InlineData(false, 0, 0, false)]
    [InlineData(false, 2, 1, true)]
    [InlineData(false, 2, 2, false)]
    [InlineData(false, 2, 3, false)]
    public void UsesCapacityPolicyForRecipient(
        bool omitCapacity,
        int maximumObjectCount,
        int recipientOwnedObjectCount,
        bool expected)
    {
        CharacterId recipientCharacterId = new("seeker");
        InventoryOwnershipSnapshot ownerships = OwnershipsForRecipient(
            recipientCharacterId.Value,
            recipientOwnedObjectCount);
        InventoryOwnershipSnapshot ownershipsWithGivenObject = new(
            ownerships.Entries.Append(
                Ownership("healing-herb", "travelling-healer")));
        InventoryCapacitySnapshot capacities = omitCapacity
            ? Capacities(("sage", 100))
            : Capacities((recipientCharacterId.Value, maximumObjectCount));

        bool result = InventoryGivePolicy.CanGiveObject(
            new CharacterId("travelling-healer"),
            recipientCharacterId,
            new ObjectId("healing-herb"),
            ownershipsWithGivenObject,
            capacities);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IgnoresUnrelatedOwnershipAndCapacityEntries()
    {
        CharacterId giverCharacterId = new("travelling-healer");
        CharacterId recipientCharacterId = new("seeker");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("stone-mortar", "travelling-healer"),
            (objectId.Value, giverCharacterId.Value),
            ("journal", "seeker"),
            ("lantern", "sage"));
        InventoryCapacitySnapshot capacities = Capacities(
            ("travelling-healer", 0),
            ("sage", 0),
            (recipientCharacterId.Value, 2));

        bool result = InventoryGivePolicy.CanGiveObject(
            giverCharacterId,
            recipientCharacterId,
            objectId,
            ownerships,
            capacities);

        Assert.True(result);
    }

    [Fact]
    public void MatchesAllIdentifiersByValue()
    {
        CharacterId giverCharacterId = new("travelling-healer");
        CharacterId recipientCharacterId = new("seeker");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("healing-herb", "travelling-healer"));
        InventoryCapacitySnapshot capacities = Capacities(("seeker", 1));

        bool result = InventoryGivePolicy.CanGiveObject(
            giverCharacterId,
            recipientCharacterId,
            objectId,
            ownerships,
            capacities);

        Assert.True(result);
        Assert.NotSame(giverCharacterId, ownerships.Entries[0].OwnerCharacterId);
        Assert.NotSame(recipientCharacterId, capacities.Entries[0].CharacterId);
        Assert.NotSame(objectId, ownerships.Entries[0].ObjectId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId giverCharacterId = new("travelling-healer");
        CharacterId recipientCharacterId = new("seeker");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, giverCharacterId.Value));
        InventoryCapacitySnapshot capacities = Capacities(
            (recipientCharacterId.Value, 1));

        Assert.Throws<ArgumentNullException>(
            () => InventoryGivePolicy.CanGiveObject(
                null!, recipientCharacterId, objectId, ownerships, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryGivePolicy.CanGiveObject(
                giverCharacterId, null!, objectId, ownerships, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryGivePolicy.CanGiveObject(
                giverCharacterId, recipientCharacterId, null!, ownerships, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryGivePolicy.CanGiveObject(
                giverCharacterId, recipientCharacterId, objectId, null!, capacities));
        Assert.Throws<ArgumentNullException>(
            () => InventoryGivePolicy.CanGiveObject(
                giverCharacterId, recipientCharacterId, objectId, ownerships, null!));
    }

    [Fact]
    public void DoesNotMutateEitherSnapshot()
    {
        InventoryOwnership firstOwnership = Ownership(
            "healing-herb", "travelling-healer");
        InventoryOwnership secondOwnership = Ownership("journal", "seeker");
        InventoryCapacity firstCapacity = Capacity("sage", 0);
        InventoryCapacity secondCapacity = Capacity("seeker", 2);
        InventoryOwnershipSnapshot ownerships = new(
            new[] { firstOwnership, secondOwnership });
        InventoryCapacitySnapshot capacities = new(
            new[] { firstCapacity, secondCapacity });

        _ = InventoryGivePolicy.CanGiveObject(
            new CharacterId("travelling-healer"),
            new CharacterId("seeker"),
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
        Type policyType = typeof(InventoryGivePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanGiveObject", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
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

    private static InventoryOwnershipSnapshot OwnershipsForRecipient(
        string recipientCharacterId,
        int count) =>
        new(Enumerable.Range(1, count).Select(
            index => Ownership($"recipient-object-{index}", recipientCharacterId)));

    private static InventoryOwnership Ownership(string objectId, string ownerCharacterId) =>
        new(new ObjectId(objectId), new CharacterId(ownerCharacterId));

    private static InventoryCapacitySnapshot Capacities(
        params (string CharacterId, int MaximumObjectCount)[] entries) =>
        new(entries.Select(entry => Capacity(entry.CharacterId, entry.MaximumObjectCount)));

    private static InventoryCapacity Capacity(string characterId, int maximumObjectCount) =>
        new(new CharacterId(characterId), maximumObjectCount);
}
