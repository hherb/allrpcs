using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact completed concealment-result presentation coordination.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 120;

    private Scenario? scenario;
    private WorldObjectConcealmentSnapshot? retainedConcealment;
    private DiscoveryEvidenceSnapshot? retainedEvidence;
    private WorldObjectConcealmentSnapshot? retainedPresentation;
    private int processFrames;
    private int stage;

    /// <inheritdoc />
    public override void _Ready() => SetProcess(true);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Concealment result presentation exceeded its bounded smoke duration.");
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
                ObserveRepeatedSelectionAndStartAbstention();
                break;
            case 3:
                ObserveAbstentionAndStartCancellation();
                break;
            case 4:
                ObserveCancellationAndStartFailure();
                break;
            case 5:
                ObserveFailureAndStartStalePresentation();
                break;
            case 6:
                ObserveStalePresentation();
                break;
            case 7:
                ObserveStaleAbstention();
                break;
            default:
                Fail("Concealment result presentation entered an unknown smoke stage.");
                break;
        }
    }

    private void StartSelection()
    {
        scenario = CreateScenario();
        bool inert = !scenario.Coordinator.TryPresentCompletedResult()
            && scenario.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Idle
            && scenario.Task.Result is null
            && scenario.Body.Visible;
        if (!inert
            || !scenario.Start(new ImmediateModel(scenario.SelectionDecision))
            || scenario.Coordinator.TryPresentCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !HasClosedSurface())
        {
            Fail("Selection presentation did not begin from an inert null-result task.");
            return;
        }

        stage = 1;
    }

    private void ObserveSelection()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || scenario.Task.Result is not { SelectedCandidate: not null } result
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || !scenario.Coordinator.TryPresentCompletedResult())
        {
            Fail("An unobserved selection was acknowledged or could not be presented.");
            return;
        }

        retainedConcealment = scenario.State.Concealment;
        retainedEvidence = scenario.State.Evidence;
        retainedPresentation = scenario.Presentation.LastPresentedConcealment;
        if (scenario.Body.Visible
            || !ReferenceEquals(retainedPresentation, retainedConcealment)
            || !ReferenceEquals(
                retainedConcealment!.ConcealedObjectIds.Single(),
                scenario.Input.Candidate.Evidence.ObjectId)
            || !ReferenceEquals(retainedEvidence!.Entries.Single(), scenario.Input.Candidate.Evidence)
            || !ReferenceEquals(result.Request.Concealment, scenario.Input.Concealment))
        {
            Fail("The exact completed selection did not hide its body once.");
            return;
        }

        stage = 2;
    }

    private void ObserveRepeatedSelectionAndStartAbstention()
    {
        if (!scenario!.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Coordinator.TryPresentCompletedResult()
            || scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || scenario.Task.Result is not null
            || !ReferenceEquals(scenario.State.Concealment, retainedConcealment)
            || !ReferenceEquals(scenario.State.Evidence, retainedEvidence)
            || !ReferenceEquals(scenario.Presentation.LastPresentedConcealment, retainedPresentation)
            || scenario.Body.Visible)
        {
            Fail("Exact selection acknowledgement or its repetition changed scene state.");
            return;
        }

        CharacterObjectSabotageConcealmentDecision abstention = new(
            scenario.Input.Character.Identity.Id,
            null);
        if (!scenario.StartCurrent(new ImmediateModel(abstention)))
        {
            Fail("A later explicit decision could not start after acknowledgement.");
            return;
        }

        stage = 3;
    }

    private void ObserveAbstentionAndStartCancellation()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || scenario.Task.Result is not { SelectedCandidate: null }
            || scenario.Body.Visible
            || !ReferenceEquals(scenario.State.Concealment, retainedConcealment)
            || !ReferenceEquals(scenario.State.Evidence, retainedEvidence)
            || !ReferenceEquals(scenario.Presentation.LastPresentedConcealment, retainedPresentation)
            || scenario.Coordinator.TryPresentCompletedResult()
            || !scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Idle
            || scenario.Task.Result is not null)
        {
            Fail("Exact abstention acknowledgement changed concealment presentation or state.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario();
        if (!scenario.Start(new WaitingModel())
            || scenario.Coordinator.TryPresentCompletedResult()
            || !scenario.Task.TryCancel())
        {
            Fail("Running or cancellation setup changed concealment presentation.");
            return;
        }

        stage = 4;
    }

    private void ObserveCancellationAndStartFailure()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Cancelled
            || scenario.Task.Result is not null
            || !scenario.Body.Visible
            || !scenario.HasInitialState())
        {
            Fail("Cancelled null-result presentation changed task or scene state.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario();
        if (!scenario.Start(new FailingModel()))
        {
            Fail("The failing presentation scenario could not start.");
            return;
        }

        stage = 5;
    }

    private void ObserveFailureAndStartStalePresentation()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Failed
            || scenario.Task.Result is not null
            || !scenario.Body.Visible
            || !scenario.HasInitialState())
        {
            Fail("Failed null-result presentation changed task or scene state.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario();
        WorldObjectConcealmentSnapshot stalePresentation = new([new ObjectId("other-object")]);
        if (!scenario.Presentation.TryPresent(scenario.Input.Concealment, stalePresentation)
            || !scenario.Body.Visible
            || !scenario.Start(new ImmediateModel(scenario.SelectionDecision)))
        {
            Fail("The stale presentation scenario could not start.");
            return;
        }

        retainedPresentation = stalePresentation;
        stage = 6;
    }

    private void ObserveStalePresentation()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || scenario.Task.Result is not { SelectedCandidate: not null }
            || scenario.Body.Visible is false
            || !ReferenceEquals(
                scenario.Presentation.LastPresentedConcealment,
                retainedPresentation)
            || scenario.Coordinator.TryPresentCompletedResult()
            || scenario.Coordinator.TryAcknowledgeCompletedResult())
        {
            Fail("A stale completed selection changed presentation or terminal task state.");
            return;
        }

        DisposeScenario();
        scenario = CreateScenario();
        CharacterObjectSabotageConcealmentDecision abstention = new(
            scenario.Input.Character.Identity.Id,
            null);
        if (!scenario.Start(new ImmediateModel(abstention)))
        {
            Fail("The stale abstention scenario could not start.");
            return;
        }

        stage = 7;
    }

    private Scenario CreateScenario()
    {
        Scenario created = new(CreateInput());
        AddChild(created.Container);
        created.Coordinator.SetProcess(false);
        return created;
    }

    private void DisposeScenario()
    {
        scenario!.Container.Free();
        scenario = null;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageConcealmentPresentationCoordinator);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(
                ["TryAcknowledgeCompletedResult", "TryPresentCompletedResult", "_Process", "_Ready"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("presented-water-cask"), "Water cask"),
            new WorldObjectDescription("A covered village water cask."));
        CharacterPhysicalAccessSnapshot access = new(character.Identity.Id, [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        WorldObjectConcealmentSnapshot concealment = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageEvidence candidate = new(
            new CharacterObjectSabotageOccurrence(
                new CharacterObjectSabotageProposal(
                    character.Identity.Id,
                    new CharacterActionAccessRequirement(
                        target.Identity.Id,
                        "assess-local-influence",
                        character.Knowledge.KnowledgeReferences.Single()),
                    CharacterSabotageActionKind.Conceal),
                target),
            new DiscoveryEvidence(
                new EvidenceId("presented-water-cask-concealment"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));
        CharacterObjectSabotageConcealmentConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            concealment,
            evidence,
            [candidate]);
        return new(character, concealment, evidence, candidate, request);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        scenario?.Container.Free();
        scenario = null;
        GetTree().Quit(1);
    }

    private sealed class Scenario
    {
        public Scenario(Input input)
        {
            Input = input;
            Container = new Node();
            State = new CharacterObjectSabotageConcealmentController
            {
                Name = "CharacterObjectSabotageConcealmentController",
            };
            Presentation = new CharacterObjectSabotageConcealmentPresentationController
            {
                Name = "CharacterObjectSabotageConcealmentPresentationController",
            };
            Task = new CharacterObjectSabotageConcealmentTaskController
            {
                Name = "CharacterObjectSabotageConcealmentTaskController",
            };
            Coordinator = new CharacterObjectSabotageConcealmentPresentationCoordinator();
            Body = new Node2D();
            Container.AddChild(State);
            Container.AddChild(Presentation);
            Container.AddChild(Task);
            Container.AddChild(Coordinator);
            Container.AddChild(Body);
            if (!State.TryInitialize(input.Concealment, input.Evidence)
                || !Presentation.TryInitialize(
                    input.Candidate.Evidence.ObjectId,
                    Body,
                    input.Concealment))
            {
                throw new InvalidOperationException("The presentation scenario could not initialize.");
            }
        }

        public Input Input { get; }

        public Node Container { get; }

        public CharacterObjectSabotageConcealmentController State { get; }

        public CharacterObjectSabotageConcealmentPresentationController Presentation { get; }

        public CharacterObjectSabotageConcealmentTaskController Task { get; }

        public CharacterObjectSabotageConcealmentPresentationCoordinator Coordinator { get; }

        public Node2D Body { get; }

        public CharacterObjectSabotageConcealmentDecision SelectionDecision =>
            new(Input.Character.Identity.Id, Input.Candidate.Evidence.EvidenceId);

        public bool Start(ICharacterObjectSabotageConcealmentDecisionModel model) =>
            Task.TryStart(model, Input.Request, State);

        public bool StartCurrent(ICharacterObjectSabotageConcealmentDecisionModel model) =>
            Task.TryStart(
                model,
                new CharacterObjectSabotageConcealmentConsiderationRequest(
                    Input.Character,
                    Input.Request.PhysicalAccess,
                    Input.Request.WorldObjects,
                    State.Concealment!,
                    State.Evidence!,
                    [Input.Candidate]),
                State);

        public bool HasInitialState() =>
            ReferenceEquals(State.Concealment, Input.Concealment)
            && ReferenceEquals(State.Evidence, Input.Evidence)
            && ReferenceEquals(Presentation.LastPresentedConcealment, Input.Concealment);
    }

    private sealed class ImmediateModel(CharacterObjectSabotageConcealmentDecision decision)
        : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(decision);
        }
    }

    private sealed class WaitingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public async ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("The cancellation wait completed unexpectedly.");
        }
    }

    private sealed class FailingModel : ICharacterObjectSabotageConcealmentDecisionModel
    {
        public ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
            CharacterObjectSabotageConcealmentConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            _ = cancellationToken;
            return ValueTask.FromException<CharacterObjectSabotageConcealmentDecision>(
                new InvalidOperationException("Expected presentation smoke failure."));
        }
    }

    private sealed record Input(
        Character Character,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageEvidence Candidate,
        CharacterObjectSabotageConcealmentConsiderationRequest Request);
}
