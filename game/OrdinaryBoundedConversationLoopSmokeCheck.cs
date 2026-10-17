using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one ordinary autonomous chain stops exactly at its completed-turn bound.</summary>
public partial class OrdinaryBoundedConversationLoopSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_BOUNDED_LOOP_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "What assumption should I test?";
    private const string SageText = "Name the evidence that could change your mind.";
    private const string HealerText = "Then we can compare it with what we observed.";
    private const int MaximumProcessFrames = 500;
    private static readonly Vector2 CloseSagePosition = new(2120.0f, 950.0f);

    private readonly LoopParticipationModel participationModel = new();
    private readonly LoopCharacterModel characterModel = new();
    private TemporaryCharacterTurnParticipationModelSource participationSource = null!;
    private TemporaryCharacterModelSource characterModelSource = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnParticipationRoundTaskController roundController = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private TemporaryOrdinaryCharacterParticipantSource participantSource = null!;
    private Marker2D sagePosition = null!;
    private LineEdit input = null!;
    private object? initialWorldObjects;
    private object? initialPerception;
    private Vector2 initialSagePosition;
    private int stableFrames;
    private int processFrames;
    private bool submitted;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        participationSource = GetNode<TemporaryCharacterTurnParticipationModelSource>(
            "../TemporaryCharacterTurnParticipationModelSource");
        characterModelSource = GetNode<TemporaryCharacterModelSource>(
            "../TemporaryCharacterModelSource");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        roundController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "../CharacterTurnParticipationRoundTaskController");
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        participantSource = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        sagePosition = GetNode<Marker2D>("../SageHearingPosition");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");

        if (!participationSource.TrySupplyModel(participationModel)
            || !characterModelSource.TrySupplyModel(characterModel))
        {
            Fail("The bounded-loop smoke could not supply its credential-free models.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail(
                "The ordinary autonomous chain exceeded its bounded smoke duration: "
                + $"participation requests={participationModel.Requests.Count}, "
                + $"character requests={characterModel.Requests.Count}, "
                + $"turn state={turnController.Activity?.State.Value ?? "none"}, "
                + $"completed turns={budgetController.Budget?.CompletedTurnCount ?? 0}.");
            return;
        }

        _ = participationSource.TrySupplyModel(participationModel);
        _ = characterModelSource.TrySupplyModel(characterModel);

        if (!submitted)
        {
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            initialSagePosition = sagePosition.Position;
            sagePosition.Position = CloseSagePosition;
            input.Text = PlayerText;
            GetNode<Healer>("../Healer").Visible = true; // Explicit present listener in this disposable fixture.
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            submitted = true;
            return;
        }

        CharacterTurnActivity? activity = turnController.Activity;
        if (activity is not null
            && ReferenceEquals(activity.State, CharacterTurnActivityState.Completed)
            && characterModel.Requests.Count == 2
            && budgetController.Budget?.CompletedTurnCount == 2)
        {
            stableFrames++;
        }

        if (stableFrames < 5)
        {
            return;
        }

        SetProcess(false);
        bool passed = ProvesBoundedLoop(activity!);
        sagePosition.Position = initialSagePosition;
        if (!passed)
        {
            return;
        }

        GD.Print(
            "Ordinary bounded conversation loop smoke passed: the player-rooted round selected one "
            + "motivated sage interjection, exact speech hearing started one healer interjection, "
            + "both turns generated, occurred, completed, and advanced in order, and the exhausted "
            + "two-turn budget prevented a third round without changing world or perception state.");
        GetTree().Quit();
    }

    private bool ProvesBoundedLoop(CharacterTurnActivity activity)
    {
        IReadOnlyList<Character> participants = participantSource.Participants;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        bool matches = participationModel.Requests.Count == 3
            && participationModel.Requests[0].IsDirectlyAddressed
            && !participationModel.Requests[1].IsDirectlyAddressed
            && !participationModel.Requests[2].IsDirectlyAddressed
            && ReferenceEquals(participationModel.Requests[0].ScheduledCharacter, participants[0])
            && ReferenceEquals(participationModel.Requests[1].ScheduledCharacter, participants[1])
            && ReferenceEquals(participationModel.Requests[2].ScheduledCharacter, participants[0])
            && characterModel.Requests.Count == 2
            && ReferenceEquals(
                characterModel.Requests[0].TurnRequest.RespondingCharacterId,
                participants[1].Identity.Id)
            && ReferenceEquals(
                characterModel.Requests[1].TurnRequest.RespondingCharacterId,
                participants[0].Identity.Id)
            && activity.Response?.Speech?.Text == HealerText
            && transcript is { Entries.Count: 3 }
            && transcript.Entries[0].SpokenStatement?.Text == PlayerText
            && transcript.Entries[1].SpokenStatement?.Text == SageText
            && transcript.Entries[2].SpokenStatement?.Text == HealerText
            && budgetController.Budget is
            { CompletedTurnCount: 2, MaximumTurnCount: 2, HasPlayerInterrupted: false }
            && roundController.Status == CharacterTurnParticipationRoundTaskStatus.Idle
            && roundController.Result is null
            && ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            && ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception);
        if (!matches)
        {
            Fail("The ordinary autonomous chain did not stop at exactly two completed turns.");
        }

        return matches;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        if (sagePosition is not null)
        {
            sagePosition.Position = initialSagePosition;
        }

        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class LoopParticipationModel : ICharacterTurnParticipationModel
    {
        public List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            CharacterTurnParticipationKind kind = request.IsDirectlyAddressed
                ? CharacterTurnParticipationKind.RemainSilent
                : CharacterTurnParticipationKind.Interject;
            return ValueTask.FromResult(new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                kind,
                request.ScheduledCharacter.Goals.Entries[0]));
        }
    }

    private sealed class LoopCharacterModel : ICharacterModel
    {
        public List<CharacterModelGenerationRequest> Requests { get; } = [];

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            string text = Requests.Count == 1 ? SageText : HealerText;
            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.Clarify,
                new DialogueDelivery(
                    new DialogueUtterance(request.TurnRequest.RespondingCharacterId, text),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(text);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }
}
