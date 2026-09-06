using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors the temporary Seeker inputs used to exercise focused search against greybox targets.
/// </summary>
public partial class TemporaryFocusedSearchContextSource : Node
{
    private const int CapabilityRequirementObjectIndex = 0;
    private const string CapabilityKey = "identify-healing-plants";
    private const string CapabilityDescription = "Identifies healing plants.";
    private const int RequiredWorldObjectCount = 2;
    private const int ToolRequirementObjectIndex = 1;
    private const string ToolObjectIdText = "field-lens";

    /// <summary>Gets the retained discovery conditions for the first two world objects.</summary>
    public DiscoveryConditionSet Conditions { get; private set; } = null!;

    /// <summary>Gets the retained temporary Seeker capability set.</summary>
    public CharacterCapabilitySet Capabilities { get; private set; } = null!;

    /// <summary>Gets the retained temporary Seeker inventory.</summary>
    public CharacterInventory Inventory { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        WorldObjectSnapshot worldObjects = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects;
        (
            DiscoveryConditionSet conditions,
            CharacterCapabilitySet capabilities,
            CharacterInventory inventory) = CreateContext(worldObjects);

        Conditions = conditions;
        Capabilities = capabilities;
        Inventory = inventory;
    }

    /// <summary>
    /// Creates the fixed temporary context from the first two world objects in authored order.
    /// </summary>
    private static (
        DiscoveryConditionSet Conditions,
        CharacterCapabilitySet Capabilities,
        CharacterInventory Inventory) CreateContext(WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(worldObjects);
        if (worldObjects.Entries.Count < RequiredWorldObjectCount)
        {
            throw new InvalidOperationException(
                $"Temporary focused-search context requires at least "
                + $"{RequiredWorldObjectCount} world objects.");
        }

        ObjectId capabilityTargetObjectId =
            worldObjects.Entries[CapabilityRequirementObjectIndex].Identity.Id;
        ObjectId toolTargetObjectId =
            worldObjects.Entries[ToolRequirementObjectIndex].Identity.Id;
        CharacterCapability capability = new(CapabilityKey, CapabilityDescription);
        ObjectId toolObjectId = new(ToolObjectIdText);
        DiscoveryCapabilityRequirement capabilityRequirement = new(
            capabilityTargetObjectId,
            capability.Key);
        DiscoveryToolRequirement toolRequirement = new(toolTargetObjectId, toolObjectId);
        DiscoveryCapabilityRequirementSet capabilityRequirements = new([capabilityRequirement]);
        DiscoveryToolRequirementSet toolRequirements = new([toolRequirement]);

        return (
            new DiscoveryConditionSet(capabilityRequirements, toolRequirements),
            new CharacterCapabilitySet([capability]),
            new CharacterInventory([toolObjectId]));
    }
}
