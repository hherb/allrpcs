namespace Allrpcs.Domain.Characters;

/// <summary>
/// Composes the complete immutable authored character model without defining runtime behavior.
/// </summary>
public sealed class Character
{
    /// <summary>
    /// Creates an immutable character from its existing validated components.
    /// </summary>
    public Character(
        CharacterIdentity identity,
        PublicCharacterBackground publicBackground,
        PrivateCharacterBackground privateBackground,
        CharacterCapabilitySet capabilities,
        CharacterLimitationSet limitations,
        CharacterControllerAssignment controllerAssignment,
        CharacterConditionSet conditions,
        CharacterInventory inventory,
        CharacterKnowledge knowledge,
        CharacterBeliefSet beliefs,
        CharacterRelationshipSet relationships,
        CharacterGoalSet goals)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(publicBackground);
        ArgumentNullException.ThrowIfNull(privateBackground);
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(limitations);
        ArgumentNullException.ThrowIfNull(controllerAssignment);
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(knowledge);
        ArgumentNullException.ThrowIfNull(beliefs);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(goals);

        if (controllerAssignment.CharacterId != identity.Id)
        {
            throw new ArgumentException(
                "The controller assignment must identify the composed character.",
                nameof(controllerAssignment));
        }

        Identity = identity;
        PublicBackground = publicBackground;
        PrivateBackground = privateBackground;
        Capabilities = capabilities;
        Limitations = limitations;
        ControllerAssignment = controllerAssignment;
        Conditions = conditions;
        Inventory = inventory;
        Knowledge = knowledge;
        Beliefs = beliefs;
        Relationships = relationships;
        Goals = goals;
    }

    /// <summary>
    /// Gets the character's stable identity.
    /// </summary>
    public CharacterIdentity Identity { get; }

    /// <summary>
    /// Gets the character's public authored background.
    /// </summary>
    public PublicCharacterBackground PublicBackground { get; }

    /// <summary>
    /// Gets the character's private authored background.
    /// </summary>
    public PrivateCharacterBackground PrivateBackground { get; }

    /// <summary>
    /// Gets the character's authored capabilities.
    /// </summary>
    public CharacterCapabilitySet Capabilities { get; }

    /// <summary>
    /// Gets the character's authored limitations.
    /// </summary>
    public CharacterLimitationSet Limitations { get; }

    /// <summary>
    /// Gets the character's controller assignment.
    /// </summary>
    public CharacterControllerAssignment ControllerAssignment { get; }

    /// <summary>
    /// Gets the character's qualitative conditions.
    /// </summary>
    public CharacterConditionSet Conditions { get; }

    /// <summary>
    /// Gets the character's inventory references.
    /// </summary>
    public CharacterInventory Inventory { get; }

    /// <summary>
    /// Gets the character's knowledge references.
    /// </summary>
    public CharacterKnowledge Knowledge { get; }

    /// <summary>
    /// Gets the character's authored beliefs.
    /// </summary>
    public CharacterBeliefSet Beliefs { get; }

    /// <summary>
    /// Gets the character's authored relationships.
    /// </summary>
    public CharacterRelationshipSet Relationships { get; }

    /// <summary>
    /// Gets the character's authored goals.
    /// </summary>
    public CharacterGoalSet Goals { get; }
}
