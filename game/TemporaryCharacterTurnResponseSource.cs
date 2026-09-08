using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Selects credential-free deterministic responses for temporary live character turns.</summary>
public partial class TemporaryCharacterTurnResponseSource : Node
{
    private readonly Func<CharacterTurnRequest, CharacterTurnResponse> responseSelector =
        DeterministicCharacterTurnResponseSelector.Select;

    /// <summary>Selects a fresh deterministic response retaining the exact supplied request.</summary>
    public CharacterTurnResponse CreateResponse(CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return responseSelector(request);
    }
}
