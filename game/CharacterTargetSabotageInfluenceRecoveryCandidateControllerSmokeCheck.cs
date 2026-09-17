using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one-shot production of an exact-current influence recovery candidate.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryCandidateControllerSmokeCheck : Node
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
        InfluenceInput input = CreateInput();
        CharacterTargetSabotageInfluenceResult fabricatedResult =
            CharacterTargetSabotageInfluenceTransition.Apply(
                input.Actor,
                input.InteractionAccess,
                input.Relationship,
                input.Candidate);
        CharacterRelationshipChange recoveryChange = RecoveryChange(input, -1);
        bool uninitializedRejected = !candidates.TryCreate(fabricatedResult, recoveryChange)
            && candidates.Candidate is null;
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
        if (!applied || currentResult is null)
        {
            GD.PushError("Influence recovery candidate source setup did not apply influence.");
            GetTree().Quit(1);
            return;
        }

        CharacterTargetSabotageInfluenceResult staleResult = new(
            input.Candidate,
            currentResult.Relationship);
        CharacterRelationshipMemory rebuiltRelationship = new(
            currentResult.Relationship.CharacterId,
            currentResult.Relationship.RelatedCharacterId,
            currentResult.Relationship.Dimensions,
            currentResult.Relationship.Reasons);
        CharacterTargetSabotageInfluenceResult rebuiltResult = new(
            input.Candidate,
            rebuiltRelationship);

        bool rejectedBeforeCreation = uninitializedRejected
            && candidates.Candidate is null
            && !candidates.TryCreate(staleResult, recoveryChange)
            && !candidates.TryCreate(rebuiltResult, recoveryChange)
            && !candidates.TryCreate(currentResult, MisdirectedChange(input))
            && !candidates.TryCreate(currentResult, WrongDimensionChange(input))
            && !candidates.TryCreate(currentResult, RecoveryChange(input, -2))
            && candidates.Candidate is null;
        bool created = candidates.TryCreate(currentResult, recoveryChange)
            && candidates.Candidate is { } candidate
            && ReferenceEquals(candidate.InfluenceResult, currentResult)
            && ReferenceEquals(candidate.RecoveryChange, recoveryChange)
            && CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                currentResult.Relationship,
                candidate);

        CharacterTargetSabotageInfluenceRecoveryCandidate retained = candidates.Candidate!;
        bool repeatedAndNullAreInert = !candidates.TryCreate(currentResult, recoveryChange)
            && ThrowsArgumentNull(() => candidates.TryCreate(null!, recoveryChange))
            && ThrowsArgumentNull(() => candidates.TryCreate(currentResult, null!))
            && ReferenceEquals(candidates.Candidate, retained);
        bool sourcesUnchanged = ReferenceEquals(influence.LatestAppliedResult, currentResult)
            && ReferenceEquals(influence.Relationship, currentResult.Relationship)
            && ReferenceEquals(selection.Request.Relationship, input.Relationship)
            && ReferenceEquals(selection.Request.Candidates.Single(), input.Candidate)
            && ReferenceEquals(currentResult.Candidate, input.Candidate)
            && currentResult.Relationship.Dimensions.Single().Value == 11
            && ReferenceEquals(
                currentResult.Relationship.Reasons.Single(),
                input.Candidate.RelationshipChange.Reason)
            && rebuiltRelationship.Dimensions.Single().Value == 11
            && rebuiltRelationship.Reasons.Count == 1;

        if (!rejectedBeforeCreation
            || !created
            || !repeatedAndNullAreInert
            || !sourcesUnchanged
            || !HasClosedSurface())
        {
            GD.PushError("Influence recovery candidate source failed exact-current ownership.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Influence recovery candidate source smoke passed: stale, rebuilt, misdirected, "
            + "wrong-dimension, wrong-delta, null, and repeated input stayed inert; one exact "
            + "current influence result retained the caller-authored inverse change.");
        GetTree().Quit();
    }

    private static CharacterRelationshipChange RecoveryChange(InfluenceInput input, int delta) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            delta,
            new CharacterRelationshipReason(
                new RelationshipReasonId($"influence-recovery-{Math.Abs(delta)}"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId($"influence-recovery-{Math.Abs(delta)}"),
                "The target reconsidered the enemy agent's influence."));

    private static CharacterRelationshipChange MisdirectedChange(InfluenceInput input) =>
        new(
            input.Actor.Identity.Id,
            input.Target.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("misdirected-influence-recovery"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("misdirected-influence-recovery"),
                "The change points in the wrong direction."));

    private static CharacterRelationshipChange WrongDimensionChange(InfluenceInput input) =>
        new(
            input.Target.Identity.Id,
            input.Actor.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("wrong-dimension-influence-recovery"),
                CharacterRelationshipDimension.Fear,
                new CharacterTurnId("wrong-dimension-influence-recovery"),
                "The change targets the wrong dimension."));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceRecoveryCandidateController);
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
            && properties[0].Name == "Candidate"
            && properties[0].PropertyType
                == typeof(CharacterTargetSabotageInfluenceRecoveryCandidate)
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
        CharacterInteractionAccessSnapshot access = new(
            actor.Identity.Id,
            [target.Identity.Id]);
        CharacterRelationshipMemory relationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);

        return new InfluenceInput(actor, target, access, relationship, candidate);
    }

    private sealed record InfluenceInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}
