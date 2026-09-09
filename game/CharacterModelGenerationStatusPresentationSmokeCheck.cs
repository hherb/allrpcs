using Allrpcs.Application.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves the temporary dialogue label can visibly present a safe timeout state.</summary>
public partial class CharacterModelGenerationStatusPresentationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_MODEL_STATUS_PRESENTATION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        TemporaryCharacterModelGenerationStatusLabel label =
            GetNode<TemporaryCharacterModelGenerationStatusLabel>(
                "../DialogueOverlay/Panel/ModelStatus");
        CharacterModelStreamSnapshot generating = new(
            [new CharacterModelTextDelta("private partial provider text")]);
        CharacterModelStreamTermination timedOutTermination = new(
            CharacterModelStreamTerminationStatus.TimedOut,
            null,
            null);
        CharacterModelStreamSnapshot timedOut = new([timedOutTermination]);

        if (label.Visible || label.Presentation is not null || label.Text.Length != 0)
        {
            Fail("Model status presentation did not begin hidden and empty.");
            return;
        }

        label.Present(generating);
        if (!label.Visible
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.Generating)
            || label.Text != CharacterModelGenerationStatusPresentation.Generating.Text)
        {
            Fail("Model status presentation did not show the safe generating state.");
            return;
        }

        label.Present(timedOut);
        if (!label.Visible
            || !ReferenceEquals(
                label.Presentation,
                CharacterModelGenerationStatusPresentation.TimedOut)
            || label.Text != "Character response: TIMED OUT"
            || !ReferenceEquals(timedOut.Updates[0], timedOutTermination)
            || timedOut.Updates.Count != 1
            || label.Text.Contains("provider", StringComparison.OrdinalIgnoreCase))
        {
            Fail("Model status presentation did not show the exact safe timeout state.");
            return;
        }

        label.ClearPresentation();
        if (label.Visible || label.Presentation is not null || label.Text.Length != 0)
        {
            Fail("Model status presentation did not clear without changing its input.");
            return;
        }

        GD.Print(
            "Character-model status presentation smoke passed: hidden initial state, safe "
            + "generating state, visible timeout, exact input preservation, and clear state.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
