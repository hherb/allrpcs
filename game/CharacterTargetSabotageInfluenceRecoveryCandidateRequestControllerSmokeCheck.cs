using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one consideration request for the exact current recovery candidate.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateRequestControllerSmokeCheck
    : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterTargetSabotageInfluenceController influence =
            GetNode<CharacterTargetSabotageInfluenceController>(
                "../CharacterTargetSabotageInfluenceController");
        CharacterTargetSabotageInfluenceRecoveryCandidateController candidates =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        CharacterTargetSabotageInfluenceRecoveryCandidateRequestController requests =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceResult staleResult =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);
        CharacterRelationshipChange recoveryChange = RecoveryChange(input);
        CharacterTargetSabotageInfluenceRecoveryCandidate staleCandidate = new(
            staleResult,
            recoveryChange);

        bool unavailableRejected = !requests.TryCreate(staleCandidate)
            && requests.Request is null;
        CharacterTargetSabotageInfluenceConsiderationResult selection = new(
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                [input.Candidate]),
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                input.Candidate));
        bool applied = influence.TryInitialize(input.Relationship)
            && influence.TryResolveConsideration(selection);
        CharacterTargetSabotageInfluenceResult? currentResult = influence.LatestAppliedResult;
        bool candidateCreated = currentResult is not null
            && candidates.TryCreate(currentResult, recoveryChange)
            && candidates.Candidate is not null;
        CharacterTargetSabotageInfluenceResult retainedResult = currentResult!;
        CharacterTargetSabotageInfluenceRecoveryCandidate retainedCandidate =
            candidates.Candidate!;
        CharacterTargetSabotageInfluenceRecoveryCandidate rebuiltCandidate = new(
            retainedResult,
            recoveryChange);
        CharacterTargetSabotageInfluenceRecoveryCandidate foreignCandidate = ForeignCandidate();

        bool invalidAndNullRejected = !requests.TryCreate(staleCandidate)
            && !requests.TryCreate(rebuiltCandidate)
            && !requests.TryCreate(foreignCandidate)
            && ThrowsArgumentNull(() => requests.TryCreate(null!))
            && requests.Request is null;
        bool created = requests.TryCreate(retainedCandidate)
            && requests.Request is { } request
            && ReferenceEquals(request.Actor, input.Target)
            && ReferenceEquals(request.Relationship, retainedResult.Relationship)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates.Single(), retainedCandidate)
            && CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                request.Relationship,
                retainedCandidate)
            && CharacterTargetSabotageInfluenceRecoveryDecisionPolicy.CanAccept(
                request,
                new CharacterTargetSabotageInfluenceRecoveryDecision(
                    input.Target.Identity.Id,
                    null));

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest retained = requests.Request!;
        bool repeatedIsInert = !requests.TryCreate(retainedCandidate)
            && !requests.TryCreate(rebuiltCandidate)
            && ReferenceEquals(requests.Request, retained)
            && ReferenceEquals(candidates.Candidate, retainedCandidate)
            && ReferenceEquals(influence.LatestAppliedResult, retainedResult)
            && ReferenceEquals(influence.Relationship, retainedResult.Relationship)
            && ReferenceEquals(retainedResult.Candidate, input.Candidate)
            && ReferenceEquals(retainedResult.Relationship.Reasons.Single(),
                input.Candidate.RelationshipChange.Reason)
            && ReferenceEquals(retainedCandidate.RecoveryChange, recoveryChange)
            && retainedResult.Relationship.Dimensions.Single().Value == 11;

        if (!unavailableRejected
            || !applied
            || !candidateCreated
            || !invalidAndNullRejected
            || !created
            || !repeatedIsInert
            || !HasClosedSurface())
        {
            GD.PushError("Influence recovery request did not retain the exact current candidate.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Influence recovery candidate request smoke passed: unavailable, stale, rebuilt, "
            + "foreign, null, and repeated inputs stayed inert; one request retained the exact "
            + "target, relationship, and current candidate without starting recovery work.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(
            CharacterTargetSabotageInfluenceRecoveryCandidateRequestController);
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
            && properties.Length == 1
            && properties[0].Name == "Request"
            && properties[0].PropertyType
                == typeof(CharacterTargetSabotageInfluenceRecoveryConsiderationRequest)
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

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

    private static CharacterTargetSabotageInfluenceRecoveryCandidate ForeignCandidate()
    {
        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceResult result =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);
        return new CharacterTargetSabotageInfluenceRecoveryCandidate(
            result,
            RecoveryChange(input));
    }

    private static CharacterRelationshipChange RecoveryChange(InfluenceInput input) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("influence-recovery"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("influence-recovery"),
                "The target reconsidered the enemy agent's influence."));

    private static InfluenceInput CreateInput()
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
                    new RelationshipReasonId("influence-attempt"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("enemy-agent-influence"),
                    "The enemy agent attempted to gain influence.")));
        return new InfluenceInput(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(
                    CharacterRelationshipDimension.Trust,
                    10)],
                []),
            candidate);
    }

    private sealed record InfluenceInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}
