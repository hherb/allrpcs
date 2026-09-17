using System.Collections;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceRecoverySelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrderWithoutMutation()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate second = EquivalentCandidate(input);
        CharacterTargetSabotageInfluenceRecoveryCandidate[] candidates = [input.Candidate, second];

        CharacterTargetSabotageInfluenceRecoveryCandidate? selected = Select(input, candidates);

        Assert.Same(input.Candidate, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(input.Candidate, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Same(input.InfluenceResult, input.Candidate.InfluenceResult);
        Assert.Same(input.RecoveryChange, input.Candidate.RecoveryChange);
        Assert.Equal(11, input.CurrentRelationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceChange.Reason, input.CurrentRelationship.Reasons.Single());
    }

    [Fact]
    public void SkipsIneligibleCandidateBeforeLaterEligibleCandidate()
    {
        SelectionInput input = Input();
        CharacterRelationshipChange wrongDelta = new(
            input.RecoveryChange.CharacterId,
            input.RecoveryChange.RelatedCharacterId,
            input.InfluenceChange.Delta,
            CharacterRelationshipChangeTests.Reason("wrong-delta", "wrong-delta-turn"));
        CharacterTargetSabotageInfluenceRecoveryCandidate ineligible = new(
            input.InfluenceResult,
            wrongDelta);
        CharacterTargetSabotageInfluenceRecoveryCandidate eligible = EquivalentCandidate(input);

        CharacterTargetSabotageInfluenceRecoveryCandidate? selected =
            Select(input, [ineligible, eligible]);

        Assert.Same(eligible, selected);
        Assert.Same(input.InfluenceResult, eligible.InfluenceResult);
        Assert.Same(input.RecoveryChange, eligible.RecoveryChange);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SelectionInput input = Input();
        CharacterRelationshipMemory staleRelationship = input.PriorRelationship;

        CharacterTargetSabotageInfluenceRecoveryCandidate? selected =
            CharacterTargetSabotageInfluenceRecoverySelection.SelectFirstEligible(
                staleRelationship,
                [input.Candidate]);

        Assert.Null(selected);
        Assert.Equal(10, staleRelationship.Dimensions.Single().Value);
        Assert.Empty(staleRelationship.Reasons);
        Assert.Equal(11, input.CurrentRelationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceResult, input.Candidate.InfluenceResult);
        Assert.Same(input.RecoveryChange, input.Candidate.RecoveryChange);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentException>(() => Select(input, [input.Candidate, null!]));
        Assert.Equal(11, input.CurrentRelationship.Dimensions.Single().Value);
        Assert.Same(input.InfluenceChange.Reason, input.CurrentRelationship.Reasons.Single());
    }

    [Fact]
    public void RejectsDuplicateExactReferencesButAcceptsDistinctEquivalentCandidates()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate equivalent = EquivalentCandidate(input);

        Assert.Throws<ArgumentException>(() =>
            Select(input, [input.Candidate, input.Candidate]));
        Assert.Same(input.Candidate, Select(input, [input.Candidate, equivalent]));
        Assert.NotSame(input.Candidate, equivalent);
        Assert.Same(input.InfluenceResult, equivalent.InfluenceResult);
        Assert.Same(input.RecoveryChange, equivalent.RecoveryChange);
    }

    [Fact]
    public void MaterializesOnePassEnumerableOnceAndEnforcesMaximumCount()
    {
        SelectionInput input = Input();
        CharacterTargetSabotageInfluenceRecoveryCandidate[] maximum = Enumerable
            .Range(0, CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount)
            .Select(_ => EquivalentCandidate(input))
            .ToArray();
        CharacterTargetSabotageInfluenceRecoveryCandidate[] excessive = maximum
            .Append(EquivalentCandidate(input))
            .ToArray();
        SingleUseEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> onePass =
            new(maximum);

        Assert.Same(maximum[0], Select(input, onePass));
        Assert.Equal(1, onePass.EnumerationCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(input, excessive));
        Assert.Equal(
            CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount,
            maximum.Length);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOneClosedPureOperation()
    {
        SelectionInput input = Input();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoverySelection.SelectFirstEligible(
                null!, [input.Candidate]));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceRecoverySelection.SelectFirstEligible(
                input.CurrentRelationship, null!));

        Type selectionType = typeof(CharacterTargetSabotageInfluenceRecoverySelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstEligible", method.Name);
        Assert.Equal(typeof(CharacterTargetSabotageInfluenceRecoveryCandidate), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterRelationshipMemory),
                typeof(IEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate>),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterTargetSabotageInfluenceRecoveryCandidate? Select(
        SelectionInput input,
        IEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> candidates) =>
        CharacterTargetSabotageInfluenceRecoverySelection.SelectFirstEligible(
            input.CurrentRelationship,
            candidates);

    private static CharacterTargetSabotageInfluenceRecoveryCandidate EquivalentCandidate(
        SelectionInput input) =>
        new(input.InfluenceResult, input.RecoveryChange);

    private static SelectionInput Input()
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
        CharacterRelationshipChange influenceChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            CharacterRelationshipChangeTests.Reason(
                "influence-attempt",
                "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipMemory currentRelationship =
            CharacterRelationshipChangeTransition.Apply(priorRelationship, influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            currentRelationship);
        CharacterRelationshipChange recoveryChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            -1,
            CharacterRelationshipChangeTests.Reason(
                "influence-recovered",
                "seeker-recovery"));

        return new SelectionInput(
            priorRelationship,
            currentRelationship,
            influenceChange,
            influenceResult,
            recoveryChange,
            new CharacterTargetSabotageInfluenceRecoveryCandidate(
                influenceResult,
                recoveryChange));
    }

    private sealed record SelectionInput(
        CharacterRelationshipMemory PriorRelationship,
        CharacterRelationshipMemory CurrentRelationship,
        CharacterRelationshipChange InfluenceChange,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterRelationshipChange RecoveryChange,
        CharacterTargetSabotageInfluenceRecoveryCandidate Candidate);

    private sealed class SingleUseEnumerable<T>(IEnumerable<T> values) : IEnumerable<T>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException(
                    "The candidate source was enumerated more than once.");
            }

            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
