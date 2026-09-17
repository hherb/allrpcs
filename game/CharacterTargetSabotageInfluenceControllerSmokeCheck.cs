using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact scene ownership and resolution of accepted influence results.</summary>
public partial class CharacterTargetSabotageInfluenceControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        InfluenceInput input = CreateInput();

        if (!ResolvesSelection(input)
            || !ConsumesReferentiallyInertAbstention(input)
            || !RejectsStaleUninitializedAndNull(input)
            || !HasClosedSurface())
        {
            GD.PushError("Influence controller failed exact authoritative relationship resolution.");
            GetTree().Quit(1);
            return;
        }

        GD.Print(
            "Influence controller smoke passed: exact selection installs one fresh relationship "
            + "and retains its exact result, abstention is inert but consumed, stale, value-equal, "
            + "null, uninitialized, and repeated inputs fail closed, and sources stay unchanged.");
        GetTree().Quit();
    }

    private static bool ResolvesSelection(InfluenceInput input)
    {
        CharacterTargetSabotageInfluenceController controller = new();
        CharacterTargetSabotageInfluenceConsiderationResult selection = Result(
            input,
            input.Relationship,
            input.Candidate);

        bool accepted = controller.TryInitialize(input.Relationship)
            && controller.TryResolveConsideration(selection)
            && controller.Relationship is { } changed
            && controller.LatestAppliedResult is { } applied
            && ReferenceEquals(applied.Candidate, input.Candidate)
            && ReferenceEquals(applied.Relationship, changed)
            && !ReferenceEquals(changed, input.Relationship)
            && changed.Dimensions.Single().Value == 11
            && ReferenceEquals(changed.Reasons.Single(), input.Candidate.RelationshipChange.Reason)
            && !controller.TryResolveConsideration(selection)
            && ReferenceEquals(controller.Relationship, changed)
            && ReferenceEquals(controller.LatestAppliedResult, applied)
            && HasUnchangedSources(input, selection.Request);

        controller.Free();
        return accepted;
    }

    private static bool ConsumesReferentiallyInertAbstention(InfluenceInput input)
    {
        CharacterTargetSabotageInfluenceController controller = new();
        CharacterTargetSabotageInfluenceConsiderationResult abstention = Result(
            input,
            input.Relationship,
            null);
        CharacterTargetSabotageInfluenceConsiderationResult laterAbstention = Result(
            input,
            input.Relationship,
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

    private static bool RejectsStaleUninitializedAndNull(InfluenceInput input)
    {
        CharacterTargetSabotageInfluenceController controller = new();
        CharacterTargetSabotageInfluenceConsiderationResult exact = Result(
            input,
            input.Relationship,
            input.Candidate);
        CharacterRelationshipMemory valueEqualRelationship = new(
            input.Relationship.CharacterId,
            input.Relationship.RelatedCharacterId,
            input.Relationship.Dimensions,
            input.Relationship.Reasons);
        CharacterTargetSabotageInfluenceConsiderationResult stale = Result(
            input,
            valueEqualRelationship,
            input.Candidate);

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
            && valueEqualRelationship.Dimensions.Single().Value == 10
            && valueEqualRelationship.Reasons.Count == 0
            && HasUnchangedSources(input, exact.Request)
            && HasUnchangedSources(input, stale.Request);

        controller.Free();
        return rejectedUninitialized && rejected;
    }

    private static CharacterTargetSabotageInfluenceConsiderationResult Result(
        InfluenceInput input,
        CharacterRelationshipMemory relationship,
        CharacterTargetSabotageInfluenceCandidate? selectedCandidate) =>
        new(
            new CharacterTargetSabotageInfluenceConsiderationRequest(
                input.Actor,
                input.InteractionAccess,
                relationship,
                [input.Candidate]),
            new CharacterTargetSabotageInfluenceDecision(
                input.Actor.Identity.Id,
                selectedCandidate));

    private static bool HasUnchangedSources(
        InfluenceInput input,
        CharacterTargetSabotageInfluenceConsiderationRequest request) =>
        ReferenceEquals(request.Actor, input.Actor)
        && ReferenceEquals(request.InteractionAccess, input.InteractionAccess)
        && ReferenceEquals(request.Candidates.Single(), input.Candidate)
        && input.Relationship.Dimensions.Single().Value == 10
        && input.Relationship.Reasons.Count == 0;

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterTargetSabotageInfluenceController);
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

        return new InfluenceInput(actor, access, relationship, candidate);
    }

    private sealed record InfluenceInput(
        Character Actor,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageInfluenceCandidate Candidate);
}
