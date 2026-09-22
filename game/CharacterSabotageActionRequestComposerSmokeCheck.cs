using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact canonical composition of authored sabotage-action requests.</summary>
public partial class CharacterSabotageActionRequestComposerSmokeCheck : Node
{
    private static readonly CharacterSabotageActionKind[] CanonicalKinds =
    [
        CharacterSabotageActionKind.Conceal,
        CharacterSabotageActionKind.Move,
        CharacterSabotageActionKind.Substitute,
        CharacterSabotageActionKind.Contaminate,
        CharacterSabotageActionKind.Misquote,
        CharacterSabotageActionKind.Delay,
        CharacterSabotageActionKind.Influence,
    ];

    /// <inheritdoc />
    public override void _Ready()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        ComposerGraph invalid = Graph("Invalid");
        ComposerGraph full = Graph("Full");
        ComposerGraph partial = Graph("Partial");

        bool emptyRejected = !invalid.Composer.TryCompose(actor, invalid.Outputs())
            && invalid.Composer.Request is null;
        bool invalidInputsRejected = RejectsInvalidInputs(invalid, actor);
        bool fullCreated = CreatesFullRequest(full, actor);
        bool partialCreated = CreatesPartialRequest(partial, actor);

        if (!emptyRejected
            || !invalidInputsRejected
            || !fullCreated
            || !partialCreated
            || !HasClosedSurface())
        {
            Fail("Sabotage-action request composition did not preserve exact canonical outputs.");
            return;
        }

        GD.Print(
            "Sabotage-action request composer smoke passed: empty, foreign, replacement, wrong-kind, "
            + "duplicate, stale, null, and repeated inputs stayed inert; full and partial exact author "
            + "outputs produced fresh requests in canonical order without changing author outputs.");
        GetTree().Quit();
    }

    private ComposerGraph Graph(string parentName)
    {
        Node parent = GetNode<Node>($"../{parentName}");
        return new ComposerGraph(
            GetAuthors(parent),
            parent.GetNode<CharacterSabotageActionRequestComposer>(
                "CharacterSabotageActionRequestComposer"));
    }

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

    private static bool RejectsInvalidInputs(ComposerGraph graph, Character actor)
    {
        Character foreignActor = RepresentativeSageCharacterSource.Create();
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest oldRequest = Request(
            actor,
            CharacterSabotageActionKind.Conceal);
        CharacterSabotageActionConsiderationRequest currentRequest = Request(
            actor,
            CharacterSabotageActionKind.Conceal);

        graph.Authors[0].Request = Request(foreignActor, CharacterSabotageActionKind.Conceal);
        bool foreignRejected = !graph.Composer.TryCompose(actor, graph.Outputs());
        graph.Authors[0].Request = Request(replacementActor, CharacterSabotageActionKind.Conceal);
        bool replacementRejected = !graph.Composer.TryCompose(actor, graph.Outputs());
        graph.Authors[0].Request = Request(actor, CharacterSabotageActionKind.Move);
        bool wrongKindRejected = !graph.Composer.TryCompose(actor, graph.Outputs());
        graph.Authors[0].Request = Request(actor, CharacterSabotageActionKind.Conceal);
        graph.Authors[1].Request = Request(actor, CharacterSabotageActionKind.Conceal);
        bool duplicateRejected = !graph.Composer.TryCompose(actor, graph.Outputs());
        graph.Authors[1].Request = null;
        graph.Authors[0].Request = currentRequest;
        CharacterSabotageActionConsiderationRequest?[] staleOutputs = graph.Outputs();
        staleOutputs[0] = oldRequest;
        bool staleRejected = !graph.Composer.TryCompose(actor, staleOutputs);
        bool nullsRejected = ThrowsArgumentNull(() => graph.Composer.TryCompose(null!, graph.Outputs()))
            && ThrowsArgumentNull(() => graph.Composer.TryCompose(actor, null!));

        return foreignRejected
            && replacementRejected
            && wrongKindRejected
            && duplicateRejected
            && staleRejected
            && nullsRejected
            && graph.Composer.Request is null
            && ReferenceEquals(graph.Authors[0].Request, currentRequest);
    }

    private static bool CreatesFullRequest(ComposerGraph graph, Character actor)
    {
        for (int index = 0; index < CanonicalKinds.Length; index++)
        {
            graph.Authors[index].Request = Request(actor, CanonicalKinds[index]);
        }

        CharacterSabotageActionConsiderationRequest?[] outputs = graph.Outputs();
        bool created = graph.Composer.TryCompose(actor, outputs);
        CharacterSabotageActionConsiderationRequest? composed = graph.Composer.Request;

        return created
            && composed is not null
            && ReferenceEquals(composed.Actor, actor)
            && composed.ActionKinds.SequenceEqual(CanonicalKinds, ReferenceEqualityComparer.Instance)
            && outputs.All(output => output is not null)
            && outputs.All(output => !ReferenceEquals(composed, output))
            && OutputsUnchanged(graph, outputs)
            && !graph.Composer.TryCompose(actor, outputs)
            && ReferenceEquals(graph.Composer.Request, composed)
            && OutputsUnchanged(graph, outputs);
    }

    private static bool CreatesPartialRequest(ComposerGraph graph, Character actor)
    {
        graph.Authors[1].Request = Request(actor, CharacterSabotageActionKind.Move);
        graph.Authors[4].Request = Request(actor, CharacterSabotageActionKind.Misquote);
        graph.Authors[6].Request = Request(actor, CharacterSabotageActionKind.Influence);
        CharacterSabotageActionConsiderationRequest?[] outputs = graph.Outputs();

        return graph.Composer.TryCompose(actor, outputs)
            && graph.Composer.Request is { } request
            && ReferenceEquals(request.Actor, actor)
            && request.ActionKinds.SequenceEqual(
                [
                    CharacterSabotageActionKind.Move,
                    CharacterSabotageActionKind.Misquote,
                    CharacterSabotageActionKind.Influence,
                ],
                ReferenceEqualityComparer.Instance)
            && OutputsUnchanged(graph, outputs);
    }

    private static CharacterSabotageActionConsiderationRequest Request(
        Character actor,
        CharacterSabotageActionKind kind) => new(actor, [kind]);

    private static bool OutputsUnchanged(
        ComposerGraph graph,
        IReadOnlyList<CharacterSabotageActionConsiderationRequest?> outputs) =>
        graph.Outputs().Select((output, index) => ReferenceEquals(output, outputs[index])).All(value => value);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionRequestComposer);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly));

        return methods.SequenceEqual(["TryCompose", "_Ready"])
            && property.Name == "Request"
            && property.PropertyType == typeof(CharacterSabotageActionConsiderationRequest)
            && property.SetMethod is not null
            && property.SetMethod.IsPrivate;
    }

    private static T AssertSingle<T>(IReadOnlyList<T> values) =>
        values.Count == 1 ? values[0] : throw new InvalidOperationException("Expected one value.");

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

    private sealed record ComposerGraph(
        CharacterSabotageActionRequestAuthorStub[] Authors,
        CharacterSabotageActionRequestComposer Composer)
    {
        public CharacterSabotageActionConsiderationRequest?[] Outputs() =>
            Authors.Select(author => author.Request).ToArray();
    }
}
