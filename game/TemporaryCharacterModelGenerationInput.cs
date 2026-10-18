using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Game;

/// <summary>
/// Retains one temporary generation request with its exact response-validation snapshots.
/// </summary>
public sealed record TemporaryCharacterModelGenerationInput : ICharacterModelGenerationInput
{
    /// <summary>Creates one immutable temporary generation-input bundle.</summary>
    public TemporaryCharacterModelGenerationInput(
        CharacterModelGenerationRequest generationRequest,
        WorldObjectSnapshot worldObjects,
        CharacterIntentionAffordanceSnapshot affordances)
    {
        ArgumentNullException.ThrowIfNull(generationRequest);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(affordances);

        GenerationRequest = generationRequest;
        WorldObjects = worldObjects;
        Affordances = affordances;
    }

    /// <summary>Gets the exact provider-neutral generation request.</summary>
    public CharacterModelGenerationRequest GenerationRequest { get; }

    /// <summary>Gets the exact authoritative world objects used for response validation.</summary>
    public WorldObjectSnapshot WorldObjects { get; }

    /// <summary>Gets the exact current targeted-affordance validation snapshot.</summary>
    public CharacterIntentionAffordanceSnapshot Affordances { get; }
}
