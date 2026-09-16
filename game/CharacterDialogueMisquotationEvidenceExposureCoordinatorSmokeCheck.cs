using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed-result coordination into exposed misquotation evidence.</summary>
public partial class CharacterDialogueMisquotationEvidenceExposureCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 180;

    private TemporaryCharacterDialogueMisquotationEvidenceSource primary = null!;
    private TemporaryCharacterDialogueMisquotationEvidenceSource alternate = null!;
    private MisquotationEvidenceExposureScenario? scenario;
    private CharacterDialogueMisquotationEvidenceExposureSnapshot? selectedExposure;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        primary = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>("../PrimarySource");
        alternate = GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>("../AlternateSource");
        SetProcess(true);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Misquotation-evidence exposure coordination exceeded its bounded duration.");
            return;
        }

        switch (stage)
        {
            case 0:
                StartSelection();
                break;
            case 1:
                ObserveSelection();
                break;
            case 2:
                AcknowledgeSelectionAndStartAbstention();
                break;
            case 3:
                ObserveAbstentionAndStartCancellation();
                break;
            case 4:
                ObserveCancellationAndStartFailure();
                break;
            case 5:
                ObserveFailureAndStartDuplicate();
                break;
            case 6:
                ObserveDuplicateAndStartFull();
                break;
            case 7:
                ObserveFullAndStartStaleRequest();
                break;
            case 8:
                ObserveStaleRequestAndStartStaleAcknowledgement();
                break;
            case 9:
                ObserveStaleAcknowledgement();
                break;
            default:
                Fail("Misquotation-evidence exposure coordination entered an unknown stage.");
                break;
        }
    }

    private void StartSelection()
    {
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));
        CharacterDialogueMisquotationEvidenceDecision decision = Decision(primary, primary.Candidate);
        bool inert = !scenario.Coordinator.TryExposeCompletedResult()
            && !scenario.Coordinator.TryAcknowledgeCompletedResult()
            && scenario.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            && scenario.HasExposure([]);
        if (!inert
            || !scenario.Start(primary, new ExposureCoordinatorImmediateModel(decision))
            || scenario.Coordinator.TryExposeCompletedResult()
            || !HasClosedSurface())
        {
            Fail("Selection did not begin from inert initialized exposure state.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelection()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryExposeCompletedResult())
        {
            Fail("A fresh exact selection was not exposed exactly once before acknowledgement.");
            return;
        }

        selectedExposure = scenario.Exposure.Exposure;
        if (selectedExposure is null
            || selectedExposure.Entries.Count != 1
            || !ReferenceEquals(selectedExposure.Entries.Single(), primary.Candidate)
            || !scenario.HasView(selectedExposure, [primary.Candidate])
            || !ReferenceEquals(
                selectedExposure.Entries.Single().Occurrence.DeliveredEvent,
                primary.DeliveredEvent))
        {
            Fail("Selection did not install the fresh exact exposure snapshot.");
            return;
        }

        stage = 2;
    }

    private void AcknowledgeSelectionAndStartAbstention()
    {
        if (!scenario!.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Idle
            || scenario.Task.Result is not null
            || !ReferenceEquals(scenario.Exposure.Exposure, selectedExposure)
            || !scenario.HasView(selectedExposure!, [primary.Candidate])
            || !scenario.Start(
                primary,
                new ExposureCoordinatorImmediateModel(Decision(primary, null))))
        {
            Fail("Exact selection acknowledgement changed exposure or blocked a later decision.");
            return;
        }

        stage = 3;
    }

    private void ObserveAbstentionAndStartCancellation()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (!scenario.Coordinator.TryExposeCompletedResult()
            || !ReferenceEquals(scenario.Exposure.Exposure, selectedExposure)
            || !scenario.HasView(selectedExposure!, [primary.Candidate])
            || !scenario.Coordinator.TryAcknowledgeCompletedResult())
        {
            Fail("Abstention did not retain and acknowledge the exact current snapshot.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));
        if (!scenario.Start(primary, new ExposureCoordinatorWaitingModel())
            || scenario.Coordinator.TryExposeCompletedResult()
            || !scenario.Task.TryCancel())
        {
            Fail("Running or cancellation setup changed exposure state.");
            return;
        }

        stage = 4;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Cancelled
            || scenario.Task.Result is not null
            || scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !scenario.HasExposure([]))
        {
            Fail("Cancellation changed exposure or became observable as a result.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));
        if (!scenario.Start(primary, new ExposureCoordinatorFailingModel()))
        {
            Fail("Failure scenario could not start.");
            return;
        }

        stage = 5;
    }

    private void ObserveFailureAndStartDuplicate()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Failed
            || scenario.Task.Result is not null
            || scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !scenario.HasExposure([]))
        {
            Fail("Model failure changed exposure or became observable as a result.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot(
            [primary.Candidate]));
        if (!scenario.Start(
            primary,
            new ExposureCoordinatorImmediateModel(Decision(primary, primary.Candidate))))
        {
            Fail("Duplicate-evidence scenario could not start.");
            return;
        }

        stage = 6;
    }

    private void ObserveDuplicateAndStartFull()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        CharacterDialogueMisquotationEvidenceExposureSnapshot duplicate = scenario.Exposure.Exposure!;
        if (scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(scenario.Exposure.Exposure, duplicate))
        {
            Fail("Duplicate completed evidence was not inert and one-shot observed.");
            return;
        }

        DisposeScenario();
        CharacterDialogueMisquotationEvidence[] entries = Enumerable.Range(
                0,
                CharacterDialogueMisquotationEvidenceExposureSnapshot.MaximumEvidenceCount)
            .Select(_ => CreateDistinctEvidence())
            .ToArray();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot(entries));
        if (!scenario.Start(
            primary,
            new ExposureCoordinatorImmediateModel(Decision(primary, primary.Candidate))))
        {
            Fail("Full-exposure scenario could not start.");
            return;
        }

        stage = 7;
    }

    private void ObserveFullAndStartStaleRequest()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        CharacterDialogueMisquotationEvidenceExposureSnapshot full = scenario.Exposure.Exposure!;
        if (scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(scenario.Exposure.Exposure, full))
        {
            Fail("Capacity exhaustion changed exposure or permitted acknowledgement.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));
        CharacterDialogueMisquotationEvidenceConsiderationResult preResolved = Result(
            primary,
            primary.Candidate);
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial = scenario.Exposure.Exposure!;
        if (!scenario.Exposure.TryExpose(initial, preResolved)
            || !scenario.Start(
                primary,
                new ExposureCoordinatorImmediateModel(Decision(primary, primary.Candidate))))
        {
            Fail("Stale-request scenario could not be prepared.");
            return;
        }

        selectedExposure = scenario.Exposure.Exposure;
        stage = 8;
    }

    private void ObserveStaleRequestAndStartStaleAcknowledgement()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryExposeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(scenario.Exposure.Exposure, selectedExposure))
        {
            Fail("An already-resolved request changed exposure or became acknowledgeable.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario(new CharacterDialogueMisquotationEvidenceExposureSnapshot([]));
        if (!scenario.Start(
            primary,
            new ExposureCoordinatorImmediateModel(Decision(primary, primary.Candidate))))
        {
            Fail("Stale-acknowledgement scenario could not start.");
            return;
        }

        stage = 9;
    }

    private void ObserveStaleAcknowledgement()
    {
        if (scenario!.Task.Status == CharacterDialogueMisquotationEvidenceTaskStatus.Running)
        {
            return;
        }

        if (!scenario.Coordinator.TryExposeCompletedResult())
        {
            Fail("Stale-acknowledgement selection could not resolve.");
            return;
        }

        CharacterDialogueMisquotationEvidenceExposureSnapshot selected = scenario.Exposure.Exposure!;
        CharacterDialogueMisquotationEvidenceConsiderationResult next = Result(
            alternate,
            alternate.Candidate);
        if (!scenario.Exposure.TryExpose(selected, next)
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Task.Status != CharacterDialogueMisquotationEvidenceTaskStatus.Completed
            || scenario.Task.Result is null
            || !HasTrailingAlternateEvidence())
        {
            Fail("Stale acknowledgement reset a result after exposure advanced.");
            return;
        }

        GD.Print(
            "Misquotation-evidence exposure coordinator smoke passed: exact one-shot selection, "
            + "abstention, acknowledgement, stale, duplicate, full, running, cancelled and "
            + "failed inertness, closed surface, and unchanged dialogue evidence.");
        DisposeScenario();
        GetTree().Quit();
    }

    private bool HasTrailingAlternateEvidence()
    {
        IReadOnlyList<CharacterDialogueMisquotationEvidence> entries =
            scenario!.Exposure.Exposure!.Entries;
        return ReferenceEquals(entries[entries.Count - 1], alternate.Candidate);
    }

    private MisquotationEvidenceExposureScenario CreateScenario(
        CharacterDialogueMisquotationEvidenceExposureSnapshot initial)
    {
        MisquotationEvidenceExposureScenario created = new(
            initial,
            primary.WitnessCharacterId);
        AddChild(created.Container);
        created.Coordinator.SetProcess(false);
        return created;
    }

    private void DisposeScenario()
    {
        scenario!.Container.Free();
        scenario = null;
    }

    private CharacterDialogueMisquotationEvidence CreateDistinctEvidence()
    {
        CharacterDialogueMisquotationProposal proposal = alternate.Occurrence.Proposal;
        Allrpcs.Domain.Dialogue.DialogueEvent delivered = new(
            new Allrpcs.Domain.Dialogue.DialogueDelivery(
                new Allrpcs.Domain.Dialogue.DialogueUtterance(
                    proposal.ActorId,
                    proposal.MisquotedText),
                Allrpcs.Domain.Dialogue.DialogueDeliveryMode.OrdinarySpeech),
            new Allrpcs.Domain.Dialogue.DialogueAudienceSnapshot(
                proposal.ActorId,
                [alternate.WitnessCharacterId]));
        return new(
            new CharacterDialogueMisquotationOccurrence(proposal, delivered),
            alternate.WitnessCharacterId);
    }

    private static CharacterDialogueMisquotationEvidenceDecision Decision(
        TemporaryCharacterDialogueMisquotationEvidenceSource source,
        CharacterDialogueMisquotationEvidence? selected) =>
        new(source.Actor.Identity.Id, selected);

    private static CharacterDialogueMisquotationEvidenceConsiderationResult Result(
        TemporaryCharacterDialogueMisquotationEvidenceSource source,
        CharacterDialogueMisquotationEvidence? selected)
    {
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            source.Actor,
            [source.Candidate]);
        return new(request, Decision(source, selected));
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterDialogueMisquotationEvidenceExposureCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(
                ["TryAcknowledgeCompletedResult", "TryExposeCompletedResult", "_Process", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        scenario?.Container.Free();
        scenario = null;
        GetTree().Quit(1);
    }

}
