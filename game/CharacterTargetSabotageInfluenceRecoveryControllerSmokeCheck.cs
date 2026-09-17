using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership and resolution of accepted influence recovery.</summary>
public partial class CharacterTargetSabotageInfluenceRecoveryControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        RecoveryInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ConsumesReferentiallyInertAbstention(input)
            || !RejectsStaleUninitializedAndNull(input)
            || !HasClosedSurface())
        {
            GD.PushError("Influence-recovery controller failed exact relationship resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Influence-recovery controller smoke passed: exact selection installs one fresh "
            + "restored relationship and exact result, abstention is inert but consumed, stale, "
            + "value-equal, null, uninitialized, and repeated inputs fail closed, and sources "
            + "stay unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(RecoveryInput input)
    {
        CharacterTargetSabotageInfluenceRecoveryController controller = new();
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult selection = Result(
            input.Actor,
            input.Relationship,
            input.Candidate,
            input.Candidate);

        bool accepted = controller.TryInitialize(input.Relationship)
            && controller.TryResolveConsideration(selection)
            && controller.Relationship is { } restored
            && controller.LatestAppliedResult is { } applied
            && ReferenceEquals(applied.Candidate, input.Candidate)
            && ReferenceEquals(applied.Relationship, restored)
            && !ReferenceEquals(restored, input.Relationship)
            && restored.Dimensions.Single().Value == 10
            && restored.Reasons.Count == 2
            && ReferenceEquals(restored.Reasons[0], input.InfluenceReason)
            && ReferenceEquals(restored.Reasons[1], input.RecoveryReason)
            && !controller.TryResolveConsideration(selection)
            && ReferenceEquals(controller.Relationship, restored)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && HasUnchangedSources(input, selection.Request);

        controller.Free();
        return accepted;
    }

    private static bool ConsumesReferentiallyInertAbstention(RecoveryInput input)
    {
        CharacterTargetSabotageInfluenceRecoveryController controller = new();
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult abstention = Result(
            input.Actor,
            input.Relationship,
            input.Candidate,
            null);
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult laterAbstention = Result(
            input.Actor,
            input.Relationship,
            input.Candidate,
            null);

        bool accepted = controller.TryInitialize(input.Relationship)
            && controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.Relationship, input.Relationship)
            && controller.LatestAppliedResult is null
            && !controller.TryResolveConsideration(abstention)
            && controller.TryResolveConsideration(laterAbstention)
            && !controller.TryResolveConsideration(abstention)
            && ReferenceEquals(controller.Relationship, input.Relationship)
            && controller.LatestAppliedResult is null
            && HasUnchangedSources(input, abstention.Request)
            && HasUnchangedSources(input, laterAbstention.Request);

        controller.Free();
        return accepted;
    }

    private static bool RejectsStaleUninitializedAndNull(RecoveryInput input)
    {
        CharacterTargetSabotageInfluenceRecoveryController controller = new();
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult exact = Result(
            input.Actor,
            input.Relationship,
            input.Candidate,
            input.Candidate);
        CharacterRelationshipMemory valueEqualRelationship = new(
            input.Relationship.CharacterId,
            input.Relationship.RelatedCharacterId,
            input.Relationship.Dimensions,
            input.Relationship.Reasons);
        CharacterTargetSabotageInfluenceRecoveryCandidate valueEqualCandidate = Candidate(
            valueEqualRelationship,
            input.Candidate.InfluenceResult.Candidate,
            input.RecoveryReason);
        CharacterTargetSabotageInfluenceRecoveryConsiderationResult stale = Result(
            input.Actor,
            valueEqualRelationship,
            valueEqualCandidate,
            valueEqualCandidate);

        bool rejectedUninitialized = !controller.TryResolveConsideration(exact)
            && controller.Relationship is null
            && controller.LatestAppliedResult is null;
        bool rejected = controller.TryInitialize(input.Relationship)
            && !controller.TryInitialize(valueEqualRelationship)
            && !controller.TryResolveConsideration(stale)
            && ThrowsArgumentNull(() => controller.TryInitialize(null!))
            && ThrowsArgumentNull(() => controller.TryResolveConsideration(null!))
            && ReferenceEquals(controller.Relationship, input.Relationship)
            && controller.LatestAppliedResult is null
            && HasUnchangedSources(input, exact.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterTargetSabotageInfluenceRecoveryConsiderationResult Result(
        Character actor,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate,
        CharacterTargetSabotageInfluenceRecoveryCandidate? selectedCandidate) =>
        new(
            new CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
                actor,
                relationship,
                [candidate]),
            new CharacterTargetSabotageInfluenceRecoveryDecision(
                actor.Identity.Id,
                selectedCandidate));

    private static bool HasUnchangedSources(
        RecoveryInput input,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request) =>
        ReferenceEquals(request.Actor, input.Actor)
        && ReferenceEquals(request.Relationship, input.Relationship)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && input.Relationship.Dimensions.Single().Value == 11
        && ReferenceEquals(input.Relationship.Reasons.Single(), input.InfluenceReason);

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceRecoveryController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] properties = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(["TryInitialize", "TryResolveConsideration"])
            && properties.SequenceEqual(["LatestAppliedResult", "Relationship"]);
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

    private static RecoveryInput CreateInput()
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            actor);
        CharacterRelationshipReason influenceReason = Reason(
            "influence-attempt",
            "enemy-agent-influence");
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            influenceReason);
        CharacterRelationshipMemory relationship = CharacterRelationshipChangeTransition.Apply(
            new CharacterRelationshipMemory(
                actor.Identity.Id,
                enemy.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            influenceChange);
        CharacterRelationshipReason recoveryReason = Reason(
            "influence-recovered",
            "sage-recovery");
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate = new(
            new CharacterTargetSabotageInfluenceResult(
                new CharacterTargetSabotageInfluenceCandidate(occurrence, influenceChange),
                relationship),
            new CharacterRelationshipChange(
                actor.Identity.Id,
                enemy.Identity.Id,
                -1,
                recoveryReason));

        return new RecoveryInput(
            actor,
            relationship,
            influenceReason,
            recoveryReason,
            candidate);
    }

    private static CharacterTargetSabotageInfluenceRecoveryCandidate Candidate(
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate influenceCandidate,
        CharacterRelationshipReason recoveryReason)
        => new(
            new CharacterTargetSabotageInfluenceResult(
                influenceCandidate,
                relationship),
            new CharacterRelationshipChange(
                relationship.CharacterId,
                relationship.RelatedCharacterId,
                -1,
                recoveryReason));

    private static CharacterRelationshipReason Reason(string id, string turnId) =>
        new(
            new RelationshipReasonId(id),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId(turnId),
            id);

    private sealed record RecoveryInput(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipReason InfluenceReason,
        CharacterRelationshipReason RecoveryReason,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate);
}
