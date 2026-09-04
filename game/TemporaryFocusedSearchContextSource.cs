using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Authors the temporary Seeker inputs used to exercise focused search against greybox targets.
/// </summary>
public partial class TemporaryFocusedSearchContextSource : Node
{
    private const string CapabilityKey = "identify-healing-plants";
    private const string CapabilityDescription = "Identifies healing plants.";
    private const string ToolObjectIdText = "field-lens";

    /// <summary>Gets the retained discovery conditions for the visible greybox targets.</summary>
    public DiscoveryConditionSet Conditions { get; private set; } = null!;

    /// <summary>Gets the retained temporary Seeker capability set.</summary>
    public CharacterCapabilitySet Capabilities { get; private set; } = null!;

    /// <summary>Gets the retained temporary Seeker inventory.</summary>
    public CharacterInventory Inventory { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        ObjectId herbPatchObjectId = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/HerbPatch").ObjectId;
        ObjectId stoneMarkerObjectId = GetNode<GreyboxInteractionTarget>(
            "../InteractionCandidates/StoneMarker").ObjectId;
        CharacterCapability capability = new(CapabilityKey, CapabilityDescription);
        ObjectId toolObjectId = new(ToolObjectIdText);
        DiscoveryCapabilityRequirement capabilityRequirement = new(
            herbPatchObjectId,
            capability.Key);
        DiscoveryToolRequirement toolRequirement = new(stoneMarkerObjectId, toolObjectId);
        DiscoveryCapabilityRequirementSet capabilityRequirements = new([capabilityRequirement]);
        DiscoveryToolRequirementSet toolRequirements = new([toolRequirement]);

        Conditions = new DiscoveryConditionSet(capabilityRequirements, toolRequirements);
        Capabilities = new CharacterCapabilitySet([capability]);
        Inventory = new CharacterInventory([toolObjectId]);
    }
}
