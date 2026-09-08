using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Presents only complete character-response speech in the temporary dialogue panel.</summary>
public partial class TemporaryCharacterTurnResponseLabel : Label
{
    private const string NoResponseText = "Character response: NONE";
    private const string NoSpeechText = "Character response: NO SPEECH";

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnResponse? lastPresentedResponse;

    /// <inheritdoc />
    public override void _Ready()
    {
        turnController = GetNode<CharacterTurnActivityController>(
            "../../../CharacterTurnActivityController");
        Text = NoResponseText;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CharacterTurnResponse? response = turnController.Activity?.Response;
        if (response is null || ReferenceEquals(response, lastPresentedResponse))
        {
            return;
        }

        lastPresentedResponse = response;
        Text = response.Speech?.Text ?? NoSpeechText;
    }
}
