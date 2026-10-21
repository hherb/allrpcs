using Allrpcs.Application.Inventory;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Application.Tests.Inventory;

public sealed class InventoryRestorationPolicyTests
{
    private static readonly CharacterId Seeker = new("seeker");
    private static readonly CharacterId Healer = new("healer");
    private static readonly ObjectId Lens = new("field-lens");
    private static readonly ObjectId Leaves = new("moonwort-leaves");
    private static readonly InventoryOwnershipSnapshot Start = new([new InventoryOwnership(Lens, Seeker)]);
    private static readonly InventoryCapacitySnapshot Capacities =
        new([new InventoryCapacity(Seeker, 2), new InventoryCapacity(Healer, 2)]);

    [Fact]
    public void AcceptsKnownObjectsWithKnownOwnersWithinLimits()
    {
        Assert.True(CanRestore(new([new(Lens, Seeker), new(Leaves, Healer)]), [Leaves]));
        Assert.True(CanRestore(new([new(Lens, Healer)]), []));
    }

    [Fact]
    public void RejectsUnknownObjectsOwnersOverCapacityAndLostStartingObjects()
    {
        Assert.False(CanRestore(new([new(Lens, Seeker), new(new ObjectId("gold"), Seeker)]), []));
        Assert.False(CanRestore(new([new(Lens, new CharacterId("sage"))]), []));
        Assert.False(CanRestore(new([new(Leaves, Healer)]), [Leaves]));
        Assert.False(InventoryRestorationPolicy.CanRestore(
            new([new(Lens, Seeker), new(Leaves, Seeker)]),
            Start,
            [Leaves],
            new([new InventoryCapacity(Seeker, 1)])));
    }

    [Fact]
    public void RequiresAGatheredObjectToBeCarriedExactlyWhileTheHistorySaysItIsHeld()
    {
        // Carried although never gathered, or already used up: a duplicated herb.
        Assert.False(CanRestore(new([new(Lens, Seeker), new(Leaves, Healer)]), []));
        // Gathered and not yet used, but nobody carries it: a lost herb.
        Assert.False(CanRestore(new([new(Lens, Seeker)]), [Leaves]));
    }

    private static bool CanRestore(InventoryOwnershipSnapshot saved, IReadOnlyCollection<ObjectId> held) =>
        InventoryRestorationPolicy.CanRestore(saved, Start, held, Capacities);
}
