using System.Collections;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceSelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrder()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceCandidate second = Candidate(input.Occurrence, 2);
        CharacterTargetSabotageInfluenceCandidate[] candidates = [input.Candidate, second];

        CharacterTargetSabotageInfluenceCandidate? selected = Select(input, candidates);

        Assert.Same(input.Candidate, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.Change, input.Candidate.RelationshipChange);
        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void SkipsIneligibleCandidatesBeforeLaterEligibleCandidate()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceCandidate delay = Candidate(
            Occurrence(input.Actor, input.Target, CharacterSabotageActionKind.Delay));
        CharacterTargetSabotageInfluenceCandidate wrongDirection = Candidate(
            input.Occurrence,
            characterId: RepresentativeHermitCharacterSource.Create()[0].Identity.Id);
        CharacterTargetSabotageInfluenceCandidate inapplicable = new(
            input.Occurrence,
            Change(
                input.Occurrence,
                dimension: CharacterRelationshipDimension.Affection));
        CharacterTargetSabotageInfluenceCandidate eligible = Candidate(input.Occurrence, 2);

        CharacterTargetSabotageInfluenceCandidate? selected = Select(
            input,
            [delay, wrongDirection, inapplicable, eligible]);

        Assert.Same(eligible, selected);
        Assert.Same(input.Occurrence, eligible.Occurrence);
        Assert.Same(input.Target.Identity.Id, eligible.RelationshipChange.CharacterId);
        Assert.Same(input.Actor.Identity.Id, eligible.RelationshipChange.RelatedCharacterId);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SelectionInput input = Input();
        CharacterInteractionAccessSnapshot inaccessible = new(input.Actor.Identity.Id, []);

        CharacterTargetSabotageInfluenceCandidate? selected =
            CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
                input.Actor,
                inaccessible,
                input.Relationship,
                [input.Candidate]);

        Assert.Null(selected);
        Assert.Empty(inaccessible.ReachableTargetCharacterIds);
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.Change, input.Candidate.RelationshipChange);
        Assert.Equal(10, input.Relationship.Dimensions.Single().Value);
        Assert.Empty(input.Relationship.Reasons);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentException>(() => Select(input, [input.Candidate, null!]));
        Assert.Same(input.Occurrence, input.Candidate.Occurrence);
        Assert.Same(input.Change, input.Candidate.RelationshipChange);
    }

    [Fact]
    public void RejectsDuplicateExactReferencesButAcceptsDistinctEquivalentCandidates()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceCandidate equivalent = new(
            input.Occurrence,
            input.Change);

        Assert.Throws<ArgumentException>(() => Select(
            input,
            [input.Candidate, input.Candidate]));
        Assert.Same(input.Candidate, Select(input, [input.Candidate, equivalent]));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.Occurrence, equivalent.Occurrence);
        Assert.Same(input.Change, equivalent.RelationshipChange);
    }

    [Fact]
    public void MaterializesOnePassEnumerableOnceAndEnforcesMaximumCount()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Occurrence, index + 1))
            .ToArray();
        CharacterTargetSabotageInfluenceCandidate[] excessive = maximum
            .Append(Candidate(input.Occurrence, maximum.Length + 1))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageInfluenceCandidate> onePass = new(maximum);

        Assert.Same(maximum[0], Select(input, onePass));
        Assert.Equal(1, onePass.EnumerationCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(input, excessive));
        Assert.Equal(
            CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount,
            maximum.Length);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOneClosedPureOperation()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
                null!, input.InteractionAccess, input.Relationship, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
                input.Actor, null!, input.Relationship, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
                input.Actor, input.InteractionAccess, null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
                input.Actor, input.InteractionAccess, input.Relationship, null!));

        Type selectionType = typeof(CharacterTargetSabotageInfluenceSelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstEligible", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceCandidate), method.ReturnType);
        Assert.Equal(
            [
                typeof(Character),
                typeof(CharacterInteractionAccessSnapshot),
                typeof(CharacterRelationshipMemory),
                typeof(IEnumerable<CharacterTargetSabotageInfluenceCandidate>),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterTargetSabotageInfluenceCandidate? Select(
        SelectionInput input,
        IEnumerable<CharacterTargetSabotageInfluenceCandidate> candidates) =>
        CharacterTargetSabotageInfluenceSelection.SelectFirstEligible(
            input.Actor,
            input.InteractionAccess,
            input.Relationship,
            candidates);

    private static SelectionInput Input()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = Occurrence(actor, target);
        CharacterRelationshipChange change = Change(occurrence);
        return new(
            actor,
            target,
            new CharacterInteractionAccessSnapshot(actor.Identity.Id, [target.Identity.Id]),
            new CharacterRelationshipMemory(
                target.Identity.Id,
                actor.Identity.Id,
                [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
                []),
            occurrence,
            change,
            new CharacterTargetSabotageInfluenceCandidate(occurrence, change));
    }

    private static CharacterTargetSabotageInfluenceCandidate Candidate(
        CharacterTargetSabotageOccurrence occurrence,
        int ordinal = 1,
        CharacterId? characterId = null) =>
        new(occurrence, Change(occurrence, 1, characterId, ordinal: ordinal));

    private static CharacterRelationshipChange Change(
        CharacterTargetSabotageOccurrence occurrence,
        int delta = 1,
        CharacterId? characterId = null,
        CharacterRelationshipDimension? dimension = null,
        int ordinal = 1) =>
        new(
            characterId ?? occurrence.Target.Identity.Id,
            occurrence.Proposal.ActorId,
            delta,
            CharacterRelationshipChangeTests.Reason(
                $"influence-attempt-{ordinal}",
                $"enemy-agent-influence-{ordinal}",
                dimension));

    private static CharacterTargetSabotageOccurrence Occurrence(
        Character actor,
        Character target,
        CharacterSabotageActionKind? actionKind = null) =>
        new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                actionKind ?? CharacterSabotageActionKind.Influence),
            target);

    private sealed record SelectionInput(
        Character Actor,
        Character Target,
        CharacterInteractionAccessSnapshot InteractionAccess,
        CharacterRelationshipMemory Relationship,
        CharacterTargetSabotageOccurrence Occurrence,
        CharacterRelationshipChange Change,
        CharacterTargetSabotageInfluenceCandidate Candidate);

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException("The candidate source was enumerated more than once.");
            }

            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
