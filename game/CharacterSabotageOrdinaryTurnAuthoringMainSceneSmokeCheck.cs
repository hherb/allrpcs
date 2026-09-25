using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact ordinary-turn sabotage authoring and generic consideration start.</summary>
public partial class CharacterSabotageOrdinaryTurnAuthoringMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SABOTAGE_ORDINARY_TURN_AUTHORING_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int EarlyProcessPriority = -100;
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source = null!;
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageOrdinaryTurnConsiderationStartCoordinator start = null!;
    private CharacterSabotageActionRequestComposer composer = null!;
    private ICharacterSabotageActionRequestAuthor[] authors = null!;
    private TemporaryCharacterSabotageActionModelSource modelSource = null!;
    private CharacterSabotageActionTaskController task = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTurnActivity turn = null!;
    private RecordingModel model = null!;
    private int processFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            SetProcess(false);
            return;
        }

        ProcessPriority = EarlyProcessPriority;
        GetParent().SetProcess(false);
        source = GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
            "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        start = GetNode<CharacterSabotageOrdinaryTurnConsiderationStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnConsiderationStartCoordinator");
        composer = GetNode<CharacterSabotageActionRequestComposer>(
            "../CharacterSabotageActionRequestComposer");
        authors = GetAuthors();
        modelSource = GetNode<TemporaryCharacterSabotageActionModelSource>(
            "../TemporaryCharacterSabotageActionModelSource");
        task = GetNode<CharacterSabotageActionTaskController>(
            "../CharacterSabotageActionTaskController");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        turn = CreateTurn(source.Actor);
        model = new RecordingModel();

        RunAuthoringChecks();
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        GetParent().SetProcess(false);
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Ordinary-turn sabotage consideration exceeded its bounded duration.");
            return;
        }

        if (task.Status == CharacterSabotageActionTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, composer.Request)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || model.CallCount != 1
            || !ReferenceEquals(model.Request, composer.Request)
            || start.TryStart(turn, composer.Request)
            || !ReferenceEquals(turns.Activity, turn))
        {
            Fail("Ordinary-turn sabotage generic consideration did not remain exact and one-shot.");
            return;
        }

        GD.Print(
            "Ordinary-turn sabotage authoring smoke passed: rejected turn and actor tokens stayed "
            + "inert; one exact current enemy turn invoked all seven authors in canonical order, "
            + "retained eligible Conceal, Delay, and Influence outputs while leaving four kinds "
            + "absent, composed once, and explicitly started one provider-neutral generic decision "
            + "without observing or routing its result.");
        GetTree().Quit();
    }

    private void RunAuthoringChecks()
    {
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterTurnActivity rebuiltTurn = new(
            turn.Request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
        bool inertStartup = authoring.Turn is null
            && authoring.Actor is null
            && authoring.Request is null
            && start.Turn is null
            && start.Request is null
            && composer.Request is null
            && authors.All(author => author.Request is null)
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && turns.Activity is null
            && model.CallCount == 0;
        bool initialized = delayState.TryInitialize(
                source.DelayRequest.EventLog,
                source.DelayRequest.WorldTime)
            && influenceState.TryInitialize(source.InfluenceRequest.Relationship)
            && turns.TryInitialize(turn);
        bool invalidRejected = !authoring.TryAuthor(null, source.Actor)
            && !authoring.TryAuthor(turn, null)
            && !authoring.TryAuthor(rebuiltTurn, source.Actor)
            && !authoring.TryAuthor(turn, replacementActor)
            && AuthorsAreEmpty();
        bool authored = authoring.TryAuthor(turn, source.Actor);
        CharacterSabotageActionConsiderationRequest? request = composer.Request;
        bool outputIsExact = authored
            && request is not null
            && ReferenceEquals(authoring.Turn, turn)
            && ReferenceEquals(authoring.Actor, source.Actor)
            && ReferenceEquals(authoring.Request, request)
            && request.ActionKinds.SequenceEqual(
                [
                    CharacterSabotageActionKind.Conceal,
                    CharacterSabotageActionKind.Delay,
                    CharacterSabotageActionKind.Influence,
                ],
                ReferenceEqualityComparer.Instance)
            && authoring.AuthorOutputs is { Count: 7 } outputs
            && outputs.Select((output, index) => ReferenceEquals(output, authors[index].Request))
                .All(value => value)
            && outputs[0] is not null
            && outputs.Skip(1).Take(4).All(output => output is null)
            && outputs[5] is not null
            && outputs[6] is not null
            && !authoring.TryAuthor(turn, source.Actor);
        bool invalidStartRejected = request is not null
            && modelSource.TrySupplyModel(model)
            && !start.TryStart(rebuiltTurn, request)
            && !start.TryStart(turn, new CharacterSabotageActionConsiderationRequest(
                source.Actor,
                request.ActionKinds))
            && model.CallCount == 0
            && task.Status == CharacterSabotageActionTaskStatus.Idle;
        bool started = request is not null
            && start.TryStart(turn, request)
            && ReferenceEquals(start.Turn, turn)
            && ReferenceEquals(start.Request, request)
            && task.Status == CharacterSabotageActionTaskStatus.Running
            && !start.TryStart(turn, request);

        if (!inertStartup
            || !initialized
            || !invalidRejected
            || !outputIsExact
            || !invalidStartRejected
            || !started)
        {
            Fail("Ordinary-turn sabotage authoring or consideration start violated its boundary.");
            return;
        }

        SetProcess(true);
    }

    private bool AuthorsAreEmpty() =>
        authors.All(author => author.Request is null && author.CandidateBinding is null)
        && composer.Request is null;

    private ICharacterSabotageActionRequestAuthor[] GetAuthors() =>
    [
        GetNode<CharacterSabotageActionConcealmentRequestController>(
            "../CharacterSabotageActionConcealmentRequestController"),
        GetNode<CharacterSabotageActionMoveRequestController>(
            "../CharacterSabotageActionMoveRequestController"),
        GetNode<CharacterSabotageActionSubstitutionRequestController>(
            "../CharacterSabotageActionSubstitutionRequestController"),
        GetNode<CharacterSabotageActionContaminationRequestController>(
            "../CharacterSabotageActionContaminationRequestController"),
        GetNode<CharacterSabotageActionMisquotationRequestController>(
            "../CharacterSabotageActionMisquotationRequestController"),
        GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController"),
        GetNode<CharacterSabotageActionInfluenceRequestController>(
            "../CharacterSabotageActionInfluenceRequestController"),
    ];

    private static CharacterTurnActivity CreateTurn(Character actor)
    {
        CharacterId speakerId = new("ordinary-turn-sabotage-smoke-speaker");
        DialogueUtterance utterance = new(speakerId, "The road ahead remains uncertain.");
        CharacterTurnRequest request = new(
            actor.Identity.Id,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(speakerId, [actor.Identity.Id])),
            new DialogueAct(utterance, DialogueActType.ReportedClaim));
        return new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class RecordingModel : ICharacterSabotageActionDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterSabotageActionConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = value;
            return ValueTask.FromResult(
                new CharacterSabotageActionDecision(
                    value.Actor.Identity.Id,
                    CharacterSabotageActionKind.Delay));
        }
    }
}
