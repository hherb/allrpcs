namespace Allrpcs.Application.Dialogue;

/// <summary>
/// One exact prepared generation input whose provider-neutral request a coordinator inspects.
/// </summary>
public interface ICharacterModelGenerationInput
{
    /// <summary>Gets the exact provider-neutral generation request.</summary>
    CharacterModelGenerationRequest GenerationRequest { get; }
}
