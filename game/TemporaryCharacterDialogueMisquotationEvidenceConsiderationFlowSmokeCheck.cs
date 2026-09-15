using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit misquotation-evidence consideration through one isolated scene flow.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowSmokeCheck
    : Node
{
    private const int MaximumProcessFrames = 120;

    private TemporaryCharacterDialogueMisquotationEvidenceModelSource modelSource = null!;
    private CharacterDialogueMisquotationEvidenceTaskController task = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController flow = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceSource source = null!;
    private RecordingModel selectionModel = null!;
    private RecordingModel overlapModel = null!;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        modelSource = GetNode<TemporaryCharacterDialogueMisquotationEvidenceModelSource>(
            "../TemporaryCharacterDialogueMisquotationEvidenceModelSource");
        task = GetNode<CharacterDialogueMisquotationEvidenceTaskController>(
            "../CharacterDialogueMisquotationEvidenceTaskController");
        flow = GetNode<TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController>(
            "../TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController");
        selectionModel = new RecordingModel(request => new(
            request.Actor.Identity.Id,
            request.Candidates.Single()));
        overlapModel = new RecordingModel(request => new(request.Actor.Identity.Id, null));
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Misquotation-evidence consideration flow exceeded its bounded smoke duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelectionAndStartAbstention();
                break;
            case 2:
                ObserveAbstentionAndStartOfflineFailure();
                break;
            case 3:
                ObserveOfflineFailure();
                break;
            default:
                Fail("Misquotation-evidence consideration flow entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        bool inertStartup = task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && task.Result is null
            && selectionModel.CallCount == 0
            && overlapModel.CallCount == 0;
        bool started = modelSource.TrySupplyModel(selectionModel)
            && flow.TryStart()
            && task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running;
        bool overlapPreserved = modelSource.TrySupplyModel(overlapModel)
            && !flow.TryStart();

        if (!inertStartup
            || !started
            || !overlapPreserved
            || !HasClosedSurface()
            || !InputsAreUnchanged())
        {
            Fail("Misquotation-evidence consideration failed inert startup or explicit start.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelectionAndStartAbstention()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { } result
            || selectionModel.CallCount != 1
            || selectionModel.Request is not { } selectionRequest
            || !RequestRetainsExactSource(selectionRequest)
            || !ReferenceEquals(result.Request, selectionRequest)
            || !ReferenceEquals(result.SelectedEvidence, source.Candidate)
            || overlapModel.CallCount != 0
            || !task.TryReset()
            || !flow.TryStart())
        {
            Fail("Misquotation-evidence consideration failed its exact selected result.");
            return;
        }

        stage = 2;
    }

    private void ObserveAbstentionAndStartOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || task.Result is not { SelectedEvidence: null }
            || overlapModel.CallCount != 1
            || overlapModel.Request is not { } abstentionRequest
            || !RequestRetainsExactSource(abstentionRequest)
            || !task.TryReset()
            || !flow.TryStart())
        {
            Fail("Misquotation-evidence consideration failed its preserved one-shot abstention.");
            return;
        }

        stage = 3;
    }

    private void ObserveOfflineFailure()
    {
        if (task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || task.Result is not null
            || selectionModel.CallCount != 1
            || overlapModel.CallCount != 1
            || !InputsAreUnchanged())
        {
            Fail("Misquotation-evidence consideration changed state after offline failure.");
            return;
        }

        GD.Print(
            "Misquotation-evidence consideration flow smoke passed: inert startup, exact request "
            + "and one-shot selection, overlap preservation, explicit reset, stable offline "
            + "failure, closed surface, and unchanged dialogue evidence.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController);
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

    private bool InputsAreUnchanged() =>
        ReferenceEquals(source.Candidate.Occurrence, source.Occurrence)
        && ReferenceEquals(source.Occurrence.Proposal.SourceEvent, source.SourceEvent)
        && ReferenceEquals(source.Occurrence.DeliveredEvent, source.DeliveredEvent)
        && ReferenceEquals(source.Candidate.WitnessCharacterId, source.WitnessCharacterId);

    private bool RequestRetainsExactSource(
        CharacterDialogueMisquotationEvidenceConsiderationRequest request) =>
        ReferenceEquals(request.Actor, source.Actor)
        && ReferenceEquals(request.Candidates.Single(), source.Candidate);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
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
