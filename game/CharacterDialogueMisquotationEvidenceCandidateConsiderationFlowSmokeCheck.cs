using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Checks one explicit decision for the exact live-candidate request.</summary>
public partial class CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController flow = null!;
    private CharacterDialogueMisquotationEvidenceCandidateRequestController requests = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;
    private CharacterDialogueMisquotationEvidenceConsiderationRequest request = null!;
    private CharacterDialogueMisquotationEvidence candidate = null!;
    private WaitingModel waitingModel = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel repeatedModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        flow = GetNode<CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController>(
            "../CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController");
        requests = GetNode<CharacterDialogueMisquotationEvidenceCandidateRequestController>(
            "../CharacterDialogueMisquotationEvidenceCandidateRequestController");
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        waitingModel = new WaitingModel();
        selectionModel = new RecordingModel(input => new(
            input.Actor.Identity.Id,
            input.Candidates.Single()));
        repeatedModel = new RecordingModel(input => new(input.Actor.Identity.Id, null));

        StartAndCancelExactLiveRequest();
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Live-candidate consideration flow exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 1:
                ObserveCancellationAndStartSelection();
                break;
            case 2:
                ObserveSelectionAndRepeatedStart();
                break;
            default:
                Fail("Live-candidate consideration flow entered an unknown smoke stage.");
                break;
        }
    }

    private void StartAndCancelExactLiveRequest()
    {
        CharacterDialogueMisquotationEvidenceConsiderationRequest staleRequest = new(
            source.Actor,
            [source.Candidate]);
        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        bool unavailableAndNullRejected = modelSource.TrySupplyModel(waitingModel)
            && !flow.TryStart(staleRequest)
            && ThrowsArgumentNull(() => flow.TryStart(null!))
            && !modelSource.TrySupplyModel(preservationProbe)
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle;

        CharacterDialogueMisquotationOccurrenceCoordinator occurrences =
            GetNode<CharacterDialogueMisquotationOccurrenceCoordinator>(
                "../CharacterDialogueMisquotationOccurrenceCoordinator");
        CharacterDialogueMisquotationEvidenceCandidateController candidates =
            GetNode<CharacterDialogueMisquotationEvidenceCandidateController>(
                "../CharacterDialogueMisquotationEvidenceCandidateController");
        CharacterDialogueMisquotationProposal proposal = source.Occurrence.Proposal;
        DialogueDelivery delivery = new(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            proposal.SourceEvent.Delivery.Mode);
        bool liveRequestCreated = occurrences.TryOccur(
                proposal,
                delivery,
                NumericsVector2.Zero,
                192.0f,
                48.0f,
                [
                    new DialogueHearingCandidate(
                        source.WitnessCharacterId,
                        new NumericsVector2(32.0f, 0.0f)),
                ])
            && candidates.TryCreate(
                source.Actor,
                occurrences.LastOccurrence!,
                source.WitnessCharacterId)
            && requests.TryCreate(source.Actor, candidates.Candidate!);
        request = requests.Request!;
        candidate = candidates.Candidate!;

        bool startedAndCancelled = !flow.TryStart(staleRequest)
            && flow.TryStart(request)
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running
            && modelSource.TrySupplyModel(selectionModel)
            && !flow.TryStart(request)
            && !modelSource.TrySupplyModel(preservationProbe)
            && task.TryCancel();

        if (!unavailableAndNullRejected
            || !liveRequestCreated
            || !startedAndCancelled
            || !HasClosedSurface()
            || !LiveRequestIsUnchanged())
        {
            Fail("Live-candidate consideration did not fail closed before exact cancellation.");
            return;
        }

        stage = 1;
    }

    private void ObserveCancellationAndStartSelection()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Cancelled
            || task.Result is not null
            || waitingModel.CallCount != 1
            || !ReferenceEquals(waitingModel.Request, request)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(preservationProbe)
            || !ReferenceEquals(requests.Request, request)
            || !task.TryReset()
            || !flow.TryStart(request))
        {
            Fail("Live-candidate consideration consumed a queued model after cancellation.");
            return;
        }

        stage = 2;
    }

    private void ObserveSelectionAndRepeatedStart()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        RecordingModel preservationProbe = new(input => new(input.Actor.Identity.Id, null));
        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedEvidence, candidate)
            || selectionModel.CallCount != 1
            || !ReferenceEquals(selectionModel.Request, request)
            || !modelSource.TrySupplyModel(repeatedModel)
            || flow.TryStart(request)
            || modelSource.TrySupplyModel(preservationProbe)
            || repeatedModel.CallCount != 0
            || !LiveRequestIsUnchanged())
        {
            Fail("Live-candidate consideration changed state after its exact completed result.");
            return;
        }

        GD.Print(
            "Live-candidate consideration flow smoke passed: unavailable, null, stale, overlap, "
            + "cancelled, and repeated starts preserved the queued model; one reset start "
            + "completed for the exact request and live candidate.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryStart", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private bool LiveRequestIsUnchanged() =>
        ReferenceEquals(requests.Request, request)
        && ReferenceEquals(request.Actor, source.Actor)
        && request.Candidates.Count == 1
        && ReferenceEquals(request.Candidates.Single(), candidate);

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class WaitingModel : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        private readonly TaskCompletionSource<CharacterDialogueMisquotationEvidenceDecision> source =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

        public async ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            using CancellationTokenRegistration registration = cancellationToken.Register(
                () => source.TrySetCanceled(cancellationToken));
            return await source.Task.ConfigureAwait(false);
        }
    }

    private sealed class RecordingModel(
        Func<CharacterDialogueMisquotationEvidenceConsiderationRequest,
            CharacterDialogueMisquotationEvidenceDecision> decide)
        : ICharacterDialogueMisquotationEvidenceDecisionModel
    {
        public int CallCount { get; private set; }

        public CharacterDialogueMisquotationEvidenceConsiderationRequest? Request { get; private set; }

        public ValueTask<CharacterDialogueMisquotationEvidenceDecision> DecideAsync(
            CharacterDialogueMisquotationEvidenceConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            return ValueTask.FromResult(decide(request));
        }
    }
}
