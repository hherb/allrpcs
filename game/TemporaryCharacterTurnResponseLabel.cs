using System.Globalization;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Streams only received complete character-response speech in the dialogue panel.</summary>
public partial class TemporaryCharacterTurnResponseLabel : Label
{
    private const string NoResponseText = "Character response: NONE";
    private const string NoSpeechText = "Character response: NO SPEECH";

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnResponse? lastPresentedResponse;
    private string? completeSpeechText;
    private int visibleLength;

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
        if (response is null)
        {
            return;
        }

        if (!ReferenceEquals(response, lastPresentedResponse))
        {
            lastPresentedResponse = response;
            completeSpeechText = response.Speech?.Text;
            visibleLength = 0;
            Text = completeSpeechText is null ? NoSpeechText : string.Empty;
            return;
        }

        if (completeSpeechText is null || visibleLength >= completeSpeechText.Length)
        {
            return;
        }

        visibleLength += StringInfo.GetNextTextElement(completeSpeechText, visibleLength).Length;
        Text = completeSpeechText[..visibleLength];
    }
}
