using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one influence-gated sabotage-action request from exact relationship state.</summary>
public partial class CharacterSabotageActionInfluenceRequestControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        SceneGraph unavailable = Graph("Unavailable", "unavailable", false);
        SceneGraph ineligible = Graph("Ineligible", "ineligible", true);
        SceneGraph eligible = Graph("Eligible", "eligible", false);

        bool unavailableRejected = !TryCreate(unavailable)
            && unavailable.Requests.Request is null
            && unavailable.State.Relationship is null;
        bool ineligibleRejected = ineligible.State.TryInitialize(
                ineligible.Input.Request.Relationship)
            && !TryCreate(ineligible)
            && ineligible.Requests.Request is null
            && ReferenceEquals(
                ineligible.State.Relationship,
                ineligible.Input.Request.Relationship);

        bool initialized = eligible.State.TryInitialize(
            eligible.Input.Request.Relationship);
        Character replacementActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterInteractionAccessSnapshot replacementAccess = new(
            eligible.Input.Request.Actor.Identity.Id,
            eligible.Input.Request.InteractionAccess.ReachableTargetCharacterIds);
        CharacterRelationshipMemory replacementRelationship = new(
            eligible.Input.Request.Relationship.CharacterId,
            eligible.Input.Request.Relationship.RelatedCharacterId,
            eligible.Input.Request.Relationship.Dimensions,
            eligible.Input.Request.Relationship.Reasons);
        CharacterTargetSabotageInfluenceCandidate replacementCandidate = new(
            eligible.Input.Candidate.Occurrence,
            eligible.Input.Candidate.RelationshipChange);
        bool staleAndReplacementRejected = initialized
            && !TryCreate(eligible, actor: replacementActor)
            && !TryCreate(eligible, interactionAccess: replacementAccess)
            && !TryCreate(eligible, relationship: replacementRelationship)
            && !TryCreate(eligible, candidate: replacementCandidate)
            && eligible.Requests.Request is null;
        bool nullsRejected = RejectsNulls(eligible);
        bool created = TryCreate(eligible)
            && eligible.Requests.Request is { } request
            && ReferenceEquals(request.Actor, eligible.Input.Request.Actor)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(
                request.ActionKinds.Single(),
                CharacterSabotageActionKind.Influence)
            && eligible.Requests.CandidateBinding
                is CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> binding
            && ReferenceEquals(binding.Request, request)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Influence)
            && ReferenceEquals(binding.Candidate, eligible.Input.Candidate);

        CharacterSabotageActionConsiderationRequest retained = eligible.Requests.Request!;
        bool repeatedIsInert = !TryCreate(eligible)
            && ReferenceEquals(eligible.Requests.Request, retained)
            && ReferenceEquals(
                eligible.State.Relationship,
                eligible.Input.Request.Relationship)
            && eligible.State.LatestAppliedResult is null
            && CharacterTargetSabotageInfluencePolicy.CanApply(
                eligible.Input.Request.Actor,
                eligible.Input.Request.InteractionAccess,
                eligible.Input.Request.Relationship,
                eligible.Input.Candidate);

        if (!unavailableRejected
            || !ineligibleRejected
            || !staleAndReplacementRejected
            || !nullsRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            Fail("Influence-gated sabotage-action request did not preserve exact relationship state.");
            return;
        }

        GD.Print(
            "Sabotage-action influence request smoke passed: unavailable, ineligible, stale, "
            + "replacement, null, and repeated inputs stayed inert; one request retained the "
            + "exact enemy actor and canonical influence action without changing a relationship.");
        GetTree().Quit();
    }

    private SceneGraph Graph(string parentName, string suffix, bool atMaximum)
    {
        InfluenceInput input = CreateInput(suffix, atMaximum);
        return new SceneGraph(
            input,
            GetNode<CharacterTargetSabotageInfluenceController>(
                $"../{parentName}/CharacterTargetSabotageInfluenceController"),
            GetNode<CharacterSabotageActionInfluenceRequestController>(
                $"../{parentName}/CharacterSabotageActionInfluenceRequestController"));
    }

    private static bool TryCreate(
        SceneGraph graph,
        Character? actor = null,
        CharacterInteractionAccessSnapshot? interactionAccess = null,
        CharacterRelationshipMemory? relationship = null,
        CharacterTargetSabotageInfluenceCandidate? candidate = null) =>
        graph.Requests.TryCreate(
            graph.Input.Request,
            actor ?? graph.Input.Request.Actor,
            interactionAccess ?? graph.Input.Request.InteractionAccess,
            relationship ?? graph.Input.Request.Relationship,
            candidate ?? graph.Input.Candidate);

    private static bool RejectsNulls(SceneGraph graph) =>
        ThrowsArgumentNull(() => graph.Requests.TryCreate(
            null!, graph.Input.Request.Actor, graph.Input.Request.InteractionAccess,
            graph.Input.Request.Relationship, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, null!, graph.Input.Request.InteractionAccess,
            graph.Input.Request.Relationship, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor, null!,
            graph.Input.Request.Relationship, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, null!, graph.Input.Candidate))
        && ThrowsArgumentNull(() => graph.Requests.TryCreate(
            graph.Input.Request, graph.Input.Request.Actor,
            graph.Input.Request.InteractionAccess, graph.Input.Request.Relationship, null!));

    private static InfluenceInput CreateInput(string suffix, bool atMaximum)
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            target);
        CharacterTargetSabotageInfluenceCandidate candidate = new(
            occurrence,
            new CharacterRelationshipChange(
                target.Identity.Id,
                actor.Identity.Id,
                1,
                new CharacterRelationshipReason(
                    new RelationshipReasonId($"influence-attempt-{suffix}"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId($"enemy-agent-influence-{suffix}"),
                    "The enemy agent attempted to gain influence.")));
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(
                CharacterRelationshipDimension.Trust,
                atMaximum ? CharacterRelationshipDimensionValue.MaximumValue : 10)],
            []);
        CharacterTargetSabotageInfluenceConsiderationRequest request = new(
            actor,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            relationship,
            [candidate]);
        return new InfluenceInput(request, candidate);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterSabotageActionInfluenceRequestController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methods.SequenceEqual(["TryCreate", "_Ready"])
            && HasExpectedProperties(properties);
    }

    private static bool HasExpectedProperties(System.Reflection.PropertyInfo[] properties) =>
        properties.Length == 2
        && properties.Single(property => property.Name == "CandidateBinding").PropertyType
            == typeof(CharacterSabotageActionCandidateBinding)
        && properties.Single(property => property.Name == "Request").PropertyType
            == typeof(CharacterSabotageActionConsiderationRequest)
        && properties.All(property => property.SetMethod is { IsPrivate: true });

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

    private sealed record InfluenceInput(
        CharacterTargetSabotageInfluenceConsiderationRequest Request,
        CharacterTargetSabotageInfluenceCandidate Candidate);

    private sealed record SceneGraph(
        InfluenceInput Input,
        CharacterTargetSabotageInfluenceController State,
        CharacterSabotageActionInfluenceRequestController Requests);
}
