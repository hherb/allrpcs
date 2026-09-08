using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Selects credential-free deterministic responses for temporary live character turns.</summary>
public partial class TemporaryCharacterTurnResponseSource : Node
{
    private readonly Func<CharacterTurnRequest, ObjectId, CharacterTurnResponse> responseSelector =
        DeterministicExaminationResponseSelector.Select;

    /// <summary>
    /// Selects a fresh examination response retaining the exact supplied request and target.
    /// </summary>
    public CharacterTurnResponse CreateResponse(
        CharacterTurnRequest request,
        ObjectId targetObjectId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(targetObjectId);

        return responseSelector(request, targetObjectId);
    }
}
