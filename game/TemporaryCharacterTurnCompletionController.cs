using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Completes a temporary live turn after observing its exact response-speech occurrence.</summary>
public partial class TemporaryCharacterTurnCompletionController : Node
{
    private CharacterTurnActivityController turnController = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private CharacterTurnResponse? observedResponse;

    /// <inheritdoc />
    public override void _Ready()
    {
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CharacterTurnActivity? activity = turnController.Activity;
        CharacterTurnResponse? response = activity?.Response;
        if (activity is null
            || response?.Speech is not { } speech
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking))
        {
            return;
        }

        if (!ReferenceEquals(response, observedResponse))
        {
            CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
            if (ReferenceEquals(eventController.LastOccurredEvent?.Utterance, speech)
                && transcript is { Entries.Count: > 0 }
                && ReferenceEquals(transcript.Entries[^1].SpokenStatement, speech))
            {
                observedResponse = response;
            }

            return;
        }

        bool completed = budgetController.Budget is null
            ? turnController.TryComplete()
            : budgetController.TryCompleteTurn(turnController);
        if (!completed)
        {
            throw new InvalidOperationException(
                "The observed temporary character turn could not complete.");
        }
    }
}
