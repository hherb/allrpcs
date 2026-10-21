using Allrpcs.Application.Inventory;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Application.Tests.Inventory;

public sealed class InventoryExchangeTransitionTests
{
    private static readonly CharacterId Seeker = new("seeker");
    private static readonly CharacterId Healer = new("healer");
    private static readonly ObjectId Lens = new("field-lens");
    private static readonly ObjectId Leaves = new("moonwort-leaves");

    [Fact]
    public void TakesAnUnownedObjectWithinCapacity()
    {
        InventoryOwnershipSnapshot ownerships = new([new InventoryOwnership(Lens, Seeker)]);

        InventoryOwnershipSnapshot? taken = InventoryExchangeTransition.Take(
            Healer, Leaves, ownerships, Capacities(1));

        Assert.NotNull(taken);
        Assert.Equal([Leaves], taken.InventoryOf(Healer).ObjectReferences);
        Assert.Single(ownerships.Entries);
        Assert.Null(InventoryExchangeTransition.Take(Seeker, Lens, ownerships, Capacities(4)));
        Assert.Null(InventoryExchangeTransition.Take(Seeker, Leaves, ownerships, Capacities(1)));
    }

    [Fact]
    public void GivesAnOwnedObjectToAnotherCharacterKeepingOrder()
    {
        InventoryOwnershipSnapshot ownerships = new(
            [new InventoryOwnership(Leaves, Healer), new InventoryOwnership(Lens, Seeker)]);

        InventoryOwnershipSnapshot? given = InventoryExchangeTransition.Give(
            Healer, Seeker, Leaves, ownerships, Capacities(4));

        Assert.NotNull(given);
        Assert.Equal([Leaves, Lens], given.InventoryOf(Seeker).ObjectReferences);
        Assert.Empty(given.InventoryOf(Healer).ObjectReferences);
        Assert.Null(InventoryExchangeTransition.Give(Healer, Healer, Leaves, ownerships, Capacities(4)));
        Assert.Null(InventoryExchangeTransition.Give(Seeker, Healer, Leaves, ownerships, Capacities(4)));
        Assert.Null(InventoryExchangeTransition.Give(Healer, Seeker, Leaves, ownerships, Capacities(1)));
    }

    [Fact]
    public void UsingAnOwnedObjectUsesItUp()
    {
        InventoryOwnershipSnapshot ownerships = new(
            [new InventoryOwnership(Leaves, Seeker), new InventoryOwnership(Lens, Seeker)]);

        InventoryOwnershipSnapshot? used = InventoryExchangeTransition.Use(Seeker, Leaves, ownerships);

        Assert.NotNull(used);
        Assert.Equal([Lens], used.InventoryOf(Seeker).ObjectReferences);
        Assert.Null(InventoryExchangeTransition.Use(Healer, Leaves, ownerships));
        Assert.Null(InventoryExchangeTransition.Use(Seeker, new ObjectId("absent"), ownerships));
    }

    private static InventoryCapacitySnapshot Capacities(int each) =>
        new([new InventoryCapacity(Seeker, each), new InventoryCapacity(Healer, each)]);
}
