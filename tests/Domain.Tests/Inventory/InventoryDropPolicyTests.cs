using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Domain.Tests.Inventory;

public sealed class InventoryDropPolicyTests
{
    [Fact]
    public void PermitsObjectOwnedByIntendedCharacter()
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, characterId.Value));

        bool result = InventoryDropPolicy.CanDropObject(
            characterId,
            objectId,
            ownerships);

        Assert.True(result);
    }

    [Fact]
    public void RejectsUnownedObject()
    {
        bool result = InventoryDropPolicy.CanDropObject(
            new CharacterId("travelling-healer"),
            new ObjectId("healing-herb"),
            Ownerships(("stone-mortar", "travelling-healer")));

        Assert.False(result);
    }

    [Fact]
    public void RejectsObjectOwnedByAnotherCharacter()
    {
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, "seeker"));

        bool result = InventoryDropPolicy.CanDropObject(
            new CharacterId("travelling-healer"),
            objectId,
            ownerships);

        Assert.False(result);
    }

    [Fact]
    public void IgnoresUnrelatedOwnershipEntries()
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("stone-mortar", "travelling-healer"),
            (objectId.Value, characterId.Value),
            ("journal", "seeker"));

        bool result = InventoryDropPolicy.CanDropObject(
            characterId,
            objectId,
            ownerships);

        Assert.True(result);
    }

    [Fact]
    public void MatchesCharacterAndObjectIdentifiersByValue()
    {
        CharacterId requestedCharacterId = new("travelling-healer");
        ObjectId requestedObjectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            ("healing-herb", "travelling-healer"));

        bool result = InventoryDropPolicy.CanDropObject(
            requestedCharacterId,
            requestedObjectId,
            ownerships);

        Assert.True(result);
        Assert.NotSame(requestedObjectId, ownerships.Entries[0].ObjectId);
        Assert.NotSame(requestedCharacterId, ownerships.Entries[0].OwnerCharacterId);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId characterId = new("travelling-healer");
        ObjectId objectId = new("healing-herb");
        InventoryOwnershipSnapshot ownerships = Ownerships(
            (objectId.Value, characterId.Value));

        Assert.Throws<ArgumentNullException>(
            () => InventoryDropPolicy.CanDropObject(null!, objectId, ownerships));
        Assert.Throws<ArgumentNullException>(
            () => InventoryDropPolicy.CanDropObject(characterId, null!, ownerships));
        Assert.Throws<ArgumentNullException>(
            () => InventoryDropPolicy.CanDropObject(characterId, objectId, null!));
    }

    [Fact]
    public void DoesNotMutateOwnershipSnapshot()
    {
        InventoryOwnership first = Ownership("stone-mortar", "travelling-healer");
        InventoryOwnership second = Ownership("healing-herb", "travelling-healer");
        InventoryOwnership third = Ownership("journal", "seeker");
        InventoryOwnershipSnapshot ownerships = new(new[] { first, second, third });

        _ = InventoryDropPolicy.CanDropObject(
            new CharacterId("travelling-healer"),
            new ObjectId("healing-herb"),
            ownerships);

        Assert.Collection(
            ownerships.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry),
            entry => Assert.Same(third, entry));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(InventoryDropPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType
                .GetMethods()
                .Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanDropObject", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterId), parameter.ParameterType),
            parameter => Assert.Equal(typeof(ObjectId), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(InventoryOwnershipSnapshot),
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

    private static InventoryOwnership Ownership(string objectId, string ownerCharacterId) =>
        new(new ObjectId(objectId), new CharacterId(ownerCharacterId));
}
