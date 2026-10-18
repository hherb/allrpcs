using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Infrastructure.Characters;
using Allrpcs.Infrastructure.Dialogue;

namespace Allrpcs.Game;

/// <summary>Composes one explicit provider-selected influence binding into one exact model source.</summary>
public sealed class TemporaryCharacterTargetSabotageInfluenceModelComposition
{
    private readonly TemporaryCharacterTargetSabotageInfluenceModelSource source;
    private CharacterTargetSabotageInfluenceModelBinding? binding;

    /// <summary>Creates an inert composition seam over one exact existing model source.</summary>
    public TemporaryCharacterTargetSabotageInfluenceModelComposition(
        TemporaryCharacterTargetSabotageInfluenceModelSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.source = source;
    }

    /// <summary>Creates and installs one binding only while this composition is uninitialized.</summary>
    public bool TryInitialize(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource? credentialSource = null,
        IAnthropicCredentialSource? anthropicCredentials = null, OllamaEndpoint? ollamaEndpoint = null)
    {
        CharacterTargetSabotageInfluenceModelBinding replacement =
            CharacterTargetSabotageInfluenceModelBindingFactory.Create(
                settings,
                client,
                credentialSource, anthropicCredentials: anthropicCredentials, ollamaEndpoint: ollamaEndpoint);
        if (binding is not null || !source.TryInitializeBinding(replacement))
        {
            return false;
        }

        binding = replacement;
        return true;
    }

    /// <summary>Creates and installs a replacement only for this composition's exact binding.</summary>
    public bool TryReplace(
        CharacterModelSettings settings,
        HttpClient client,
        IOpenAiCredentialSource? credentialSource = null,
        IAnthropicCredentialSource? anthropicCredentials = null, OllamaEndpoint? ollamaEndpoint = null)
    {
        CharacterTargetSabotageInfluenceModelBinding replacement =
            CharacterTargetSabotageInfluenceModelBindingFactory.Create(
                settings,
                client,
                credentialSource, anthropicCredentials: anthropicCredentials, ollamaEndpoint: ollamaEndpoint);
        if (binding is null || !source.TryReplaceBinding(binding, replacement))
        {
            return false;
        }

        binding = replacement;
        return true;
    }

    /// <summary>Clears only this composition's exact installed binding.</summary>
    public bool TryClear()
    {
        if (binding is null || !source.TryClearBinding(binding))
        {
            return false;
        }

        binding = null;
        return true;
    }
}
