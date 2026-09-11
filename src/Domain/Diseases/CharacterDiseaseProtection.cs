using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Retains one active authored disease protection for a character.</summary>
public sealed class CharacterDiseaseProtection
{
    /// <summary>Creates an immutable active character protection.</summary>
    public CharacterDiseaseProtection(
        CharacterId characterId,
        DiseaseProtectionDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(definition);

        CharacterId = characterId;
        Definition = definition;
    }

    /// <summary>Gets the protected character identity.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact active protective-measure definition.</summary>
    public DiseaseProtectionDefinition Definition { get; }
}
