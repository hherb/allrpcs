using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Provides one temporary deterministic provider-neutral character turn.</summary>
public partial class TemporaryCharacterTurnSource : Node
{
    /// <summary>Gets the exact initial awaiting-response activity.</summary>
    public CharacterTurnActivity Activity { get; private set; } = null!;

    /// <summary>Gets the exact complete deterministic response for the activity request.</summary>
    public CharacterTurnResponse Response { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(
            seekerId,
            "Please wait while I examine the flowers.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));

        Activity = new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
        Response = DeterministicCharacterTurnResponseSelector.Select(request);
    }
}
