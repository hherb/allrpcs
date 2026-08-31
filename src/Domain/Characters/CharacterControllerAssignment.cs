using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Associates a character with its current authored controller type without creating a
/// controller instance or defining runtime behavior.
/// </summary>
public sealed record CharacterControllerAssignment
{
    /// <summary>
    /// Creates an immutable authoritative character-to-controller-type association.
    /// </summary>
    public CharacterControllerAssignment(CharacterId characterId, ControllerType controllerType)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(controllerType);

        CharacterId = characterId;
        ControllerType = controllerType;
    }

    /// <summary>
    /// Gets the stable identifier of the assigned character.
    /// </summary>
    public CharacterId CharacterId { get; }

    /// <summary>
    /// Gets the character's current authored controller type.
    /// </summary>
    public ControllerType ControllerType { get; }
}
