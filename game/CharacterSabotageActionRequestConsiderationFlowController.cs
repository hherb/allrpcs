using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts consideration only for the exact current composed action request.</summary>
public partial class CharacterSabotageActionRequestConsiderationFlowController : Node
{
    private CharacterSabotageActionRequestComposer composer = null!;
    private TemporaryCharacterSabotageActionConsiderationFlowController flow = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        flow = GetNode<TemporaryCharacterSabotageActionConsiderationFlowController>(
            "../TemporaryCharacterSabotageActionConsiderationFlowController");
    }

    /// <summary>Starts once only when the caller still holds the composer's exact request.</summary>
    public bool TryStart(CharacterSabotageActionConsiderationRequest expectedRequest)
    {
        ArgumentNullException.ThrowIfNull(expectedRequest);
        return ReferenceEquals(composer.Request, expectedRequest)
            && flow.TryStart(expectedRequest);
    }
}
