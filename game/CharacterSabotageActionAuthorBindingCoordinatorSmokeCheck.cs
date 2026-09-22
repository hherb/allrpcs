using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact binding from one observed action selection to its author output.</summary>
public partial class CharacterSabotageActionAuthorBindingCoordinatorSmokeCheck : Node
{
    private const int MaximumProcessFrames = 240;

    private BindingGraph selected = null!;
    private BindingGraph abstained = null!;
    private BindingGraph invalid = null!;
    private BindingGraph stale = null!;
    private Character actor = null!;
    private int processFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        actor = RepresentativeEnemyAgentCharacterSource.Create();
        selected = CreateGraph("Selected", actor, CharacterSabotageActionKind.Move);
        abstained = CreateGraph("Abstained", actor, null);
        invalid = CreateGraph("Invalid", actor, CharacterSabotageActionKind.Move);
        stale = CreateGraph("Stale", actor, CharacterSabotageActionKind.Move, rebuildRequest: true);

        if (selected.Binding.TryBindObservedSelection()
            || invalid.Binding.TryBindObservedSelection()
            || !Starts(selected)
            || !Starts(abstained)
            || !Starts(invalid)
            || !Starts(stale)
            || selected.Binding.TryBindObservedSelection())
        {
            Fail("Binding did not remain inert before exact completed-result observation.");
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Sabotage-action author binding exceeded its bounded duration.");
            return;
        }

        if (!AllCompleted())
        {
            return;
        }

        if (!ObserveAll()
            || !BindsExactSelection()
            || !BindsAbstention()
            || !RejectsInvalidCurrentOutputs()
            || !RejectsStaleResult()
            || !RejectsResetObservation()
            || !HasClosedSurface())
        {
            Fail("Observed sabotage-action selection did not bind only its exact author output.");
            return;
        }

        GD.Print(
            "Sabotage-action author binding smoke passed: exact selection and abstention bound once; "
            + "unavailable, running, reset, stale, rebuilt, foreign, wrong-kind, duplicate, null, "
            + "and repeated states stayed inert with composer, authors, task, selection, and inputs "
            + "unchanged.");
        GetTree().Quit();
    }

    private BindingGraph CreateGraph(
        string name,
        Character exactActor,
        CharacterSabotageActionKind? selection,
        bool rebuildRequest = false)
    {
        Node parent = GetNode<Node>($"../{name}");
        CharacterSabotageActionRequestAuthorStub[] authors = GetAuthors(parent);
        authors[0].Request = Request(exactActor, CharacterSabotageActionKind.Conceal);
        authors[1].Request = Request(exactActor, CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationRequest?[] outputs = Outputs(authors);
        CharacterSabotageActionRequestComposer composer =
            parent.GetNode<CharacterSabotageActionRequestComposer>(
                "CharacterSabotageActionRequestComposer");
        if (!composer.TryCompose(exactActor, outputs) || composer.Request is null)
        {
            throw new InvalidOperationException("Expected exact smoke composition.");
        }

        CharacterSabotageActionConsiderationRequest decisionRequest = rebuildRequest
            ? new CharacterSabotageActionConsiderationRequest(
                exactActor,
                [CharacterSabotageActionKind.Conceal, CharacterSabotageActionKind.Move])
            : composer.Request;
        CharacterSabotageActionDecision decision = new(exactActor.Identity.Id, selection);
        return new BindingGraph(
            authors,
            outputs,
            composer,
            parent.GetNode<CharacterSabotageActionTaskController>(
                "CharacterSabotageActionTaskController"),
            parent.GetNode<CharacterSabotageActionSelectionCoordinator>(
                "CharacterSabotageActionSelectionCoordinator"),
            parent.GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
                "CharacterSabotageActionAuthorBindingCoordinator"),
            decisionRequest,
            decision);
    }

    private static bool Starts(BindingGraph graph) =>
        graph.Task.TryStart(new ImmediateModel(graph.Decision), graph.DecisionRequest);

    private bool AllCompleted() =>
        Graphs().All(graph => graph.Task.Status == CharacterSabotageActionTaskStatus.Completed);

    private bool ObserveAll() => Graphs().All(graph =>
        graph.Selection.TryObserveCompletedResult()
        && ReferenceEquals(graph.Selection.ObservedResult, graph.Task.Result));

    private bool BindsExactSelection()
    {
        CharacterSabotageActionConsiderationResult? result = selected.Task.Result;
        return result is not null
            && selected.Binding.TryBindObservedSelection()
            && ReferenceEquals(selected.Binding.SelectedAuthorRequest, selected.Outputs[1])
            && !selected.Binding.TryBindObservedSelection()
            && ReferenceEquals(selected.Composer.Request, result.Request)
            && ReferenceEquals(selected.Selection.ObservedResult, result)
            && GraphIsUnchanged(selected);
    }

    private bool BindsAbstention()
    {
        CharacterSabotageActionConsiderationResult? result = abstained.Task.Result;
        return result is not null
            && result.SelectedActionKind is null
            && abstained.Binding.TryBindObservedSelection()
            && abstained.Binding.SelectedAuthorRequest is null
            && !abstained.Binding.TryBindObservedSelection()
            && ReferenceEquals(abstained.Selection.ObservedResult, result)
            && GraphIsUnchanged(abstained);
    }

    private bool RejectsInvalidCurrentOutputs()
    {
        CharacterSabotageActionConsiderationResult? result = invalid.Task.Result;
        CharacterSabotageActionConsiderationRequest exactMove = invalid.Outputs[1]!;
        Character foreign = RepresentativeSageCharacterSource.Create();
        CharacterSabotageActionRequestAuthorStub move = invalid.Authors[1];

        move.Request = Request(actor, CharacterSabotageActionKind.Move);
        bool rebuiltRejected = !invalid.Binding.TryBindObservedSelection();
        move.Request = Request(foreign, CharacterSabotageActionKind.Move);
        bool foreignRejected = !invalid.Binding.TryBindObservedSelection();
        move.Request = Request(actor, CharacterSabotageActionKind.Conceal);
        bool wrongKindRejected = !invalid.Binding.TryBindObservedSelection();
        move.Request = invalid.Authors[0].Request;
        bool duplicateRejected = !invalid.Binding.TryBindObservedSelection();
        move.Request = null;
        bool nullRejected = !invalid.Binding.TryBindObservedSelection();
        move.Request = exactMove;

        return rebuiltRejected
            && foreignRejected
            && wrongKindRejected
            && duplicateRejected
            && nullRejected
            && result is not null
            && ReferenceEquals(invalid.Selection.ObservedResult, result)
            && invalid.Binding.SelectedAuthorRequest is null
            && invalid.Binding.TryBindObservedSelection()
            && ReferenceEquals(invalid.Binding.SelectedAuthorRequest, exactMove)
            && GraphIsUnchanged(invalid);
    }

    private bool RejectsStaleResult() =>
        stale.Task.Result is { } result
        && !ReferenceEquals(result.Request, stale.Composer.Request)
        && !stale.Binding.TryBindObservedSelection()
        && stale.Binding.SelectedAuthorRequest is null
        && ReferenceEquals(stale.Selection.ObservedResult, result)
        && GraphIsUnchanged(stale);

    private bool RejectsResetObservation()
    {
        BindingGraph graph = stale;
        CharacterSabotageActionConsiderationResult? observed = graph.Selection.ObservedResult;
        return observed is not null
            && graph.Task.TryReset()
            && graph.Task.Status == CharacterSabotageActionTaskStatus.Idle
            && graph.Task.Result is null
            && ReferenceEquals(graph.Selection.ObservedResult, observed)
            && !graph.Binding.TryBindObservedSelection()
            && graph.Binding.SelectedAuthorRequest is null;
    }

    private IEnumerable<BindingGraph> Graphs() => [selected, abstained, invalid, stale];

    private static CharacterSabotageActionRequestAuthorStub[] GetAuthors(Node parent) =>
    [
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionConcealmentRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionMoveRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionSubstitutionRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionContaminationRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionMisquotationRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionDelayRequestController"),
        parent.GetNode<CharacterSabotageActionRequestAuthorStub>(
            "CharacterSabotageActionInfluenceRequestController"),
    ];

    private static CharacterSabotageActionConsiderationRequest Request(
        Character exactActor,
        CharacterSabotageActionKind kind) => new(exactActor, [kind]);

    private static CharacterSabotageActionConsiderationRequest?[] Outputs(
        IEnumerable<CharacterSabotageActionRequestAuthorStub> authors) =>
        authors.Select(author => author.Request).ToArray();

    private static bool GraphIsUnchanged(BindingGraph graph) =>
        ReferenceEquals(graph.Composer.Request?.Actor, graph.DecisionRequest.Actor)
        && Outputs(graph.Authors)
            .Select((output, index) => ReferenceEquals(output, graph.Outputs[index]))
            .All(value => value)
        && ReferenceEquals(graph.Task.Result, graph.Selection.ObservedResult)
        && graph.Task.Status == CharacterSabotageActionTaskStatus.Completed;

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionAuthorBindingCoordinator);
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return properties.SequenceEqual(["SelectedAuthorRequest"])
            && methods.SequenceEqual(["TryBindObservedSelection", "_Ready"]);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }

    private sealed class ImmediateModel(CharacterSabotageActionDecision decision)
        : ICharacterSabotageActionDecisionModel
    {
        public ValueTask<CharacterSabotageActionDecision> DecideAsync(
            CharacterSabotageActionConsiderationRequest request,
            CancellationToken cancellationToken) => ValueTask.FromResult(decision);
    }

    private sealed record BindingGraph(
        CharacterSabotageActionRequestAuthorStub[] Authors,
        CharacterSabotageActionConsiderationRequest?[] Outputs,
        CharacterSabotageActionRequestComposer Composer,
        CharacterSabotageActionTaskController Task,
        CharacterSabotageActionSelectionCoordinator Selection,
        CharacterSabotageActionAuthorBindingCoordinator Binding,
        CharacterSabotageActionConsiderationRequest DecisionRequest,
        CharacterSabotageActionDecision Decision);
}
