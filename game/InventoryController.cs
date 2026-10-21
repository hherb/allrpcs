using Allrpcs.Application.Inventory;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the one inventory-ownership snapshot: who carries which world objects.</summary>
public partial class InventoryController : Node
{
    /// <summary>The moonwort leaves the Healer gathers from the herb patch.</summary>
    internal static readonly ObjectId MoonwortLeaves = new("moonwort-leaves");

    private const int CarryLimit = 4;
    private InventoryOwnershipSnapshot? initial;

    /// <summary>Gets the current ownership snapshot, once initialized.</summary>
    public InventoryOwnershipSnapshot? Ownerships { get; private set; }

    /// <summary>Gets the per-character carrying limits, once initialized.</summary>
    public InventoryCapacitySnapshot? Capacities { get; private set; }

    /// <summary>Initializes once from each authored character's starting inventory.</summary>
    public bool TryInitialize(IReadOnlyList<(CharacterId Character, CharacterInventory Inventory)> authored)
    {
        ArgumentNullException.ThrowIfNull(authored);
        if (Ownerships is not null)
        {
            return false;
        }

        initial = new(authored.SelectMany(entry => entry.Inventory.ObjectReferences.Select(
            objectId => new InventoryOwnership(objectId, entry.Character))));
        Capacities = new(authored.Select(entry => new InventoryCapacity(entry.Character, CarryLimit)));
        Ownerships = initial;
        return true;
    }

    /// <summary>Gets one character's inventory view of the current snapshot.</summary>
    public CharacterInventory InventoryOf(CharacterId characterId) =>
        (Ownerships ?? throw new InvalidOperationException("Inventory is used before initialization."))
            .InventoryOf(characterId);

    /// <summary>Reports whether the character currently carries the object.</summary>
    public bool Carries(CharacterId characterId, ObjectId objectId) =>
        Ownerships?.Entries.Any(entry => entry.ObjectId == objectId && entry.OwnerCharacterId == characterId)
            == true;

    /// <summary>Takes an unowned object, within the character's carrying limit.</summary>
    public bool TryTake(CharacterId characterId, ObjectId objectId) =>
        Ownerships is { } ownerships && Capacities is { } capacities
        && Install(InventoryExchangeTransition.Take(characterId, objectId, ownerships, capacities));

    /// <summary>Reports whether the character could take the unowned object now.</summary>
    public bool CanTake(CharacterId characterId, ObjectId objectId) =>
        Ownerships is { } ownerships && Capacities is { } capacities
        && InventoryTakePolicy.CanTakeObject(characterId, objectId, ownerships, capacities);

    /// <summary>Gives a carried object to another character with room for it.</summary>
    public bool TryGive(CharacterId giverId, CharacterId recipientId, ObjectId objectId) =>
        Ownerships is { } ownerships && Capacities is { } capacities
        && Install(InventoryExchangeTransition.Give(giverId, recipientId, objectId, ownerships, capacities));

    /// <summary>Uses up a carried object.</summary>
    public bool TryUse(CharacterId characterId, ObjectId objectId) =>
        Ownerships is { } ownerships
        && Install(InventoryExchangeTransition.Use(characterId, objectId, ownerships));

    /// <summary>
    /// Checks a saved inventory against the authored objects, characters and carrying limits and
    /// against the saved history (whether the gathered moonwort is still held), and returns its
    /// install. A save without an inventory restores the authored start, with a still-held
    /// moonwort in the gatherer's hands. Throws so the whole load fails.
    /// </summary>
    internal Action PrepareRestoration(InventoryOwnershipSnapshot? saved, bool moonwortHeld, CharacterId gatherer)
    {
        ArgumentNullException.ThrowIfNull(gatherer);
        InventoryOwnershipSnapshot start = initial
            ?? throw new InvalidOperationException("Inventory restoration requires initialization.");
        InventoryOwnershipSnapshot restored = saved
            ?? (moonwortHeld ? new([.. start.Entries, new InventoryOwnership(MoonwortLeaves, gatherer)]) : start);
        if (!InventoryRestorationPolicy.CanRestore(restored, start, moonwortHeld ? [MoonwortLeaves] : [], Capacities!))
        {
            throw new InvalidOperationException(
                $"The saved inventory [{string.Join(", ", restored.Entries.Select(entry => $"{entry.ObjectId.Value}:{entry.OwnerCharacterId.Value}"))}] "
                + $"does not fit the authored world and history (moonwort held: {moonwortHeld}).");
        }

        return () => Ownerships = restored;
    }

    private bool Install(InventoryOwnershipSnapshot? next)
    {
        if (next is null)
        {
            return false;
        }

        Ownerships = next;
        return true;
    }
}
