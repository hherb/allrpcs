using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationSequenceTests
{
    [Fact]
    public void AppliesExactFirstEligibleCandidateAfterSkippingIneligibleCandidate()
    {
        SequenceInput input = Input();
        WorldObject inaccessibleTarget = Target("inaccessible-cask");
        WorldObject eligibleTarget = Target("eligible-cask");
        CharacterObjectSabotageEvidence inaccessible = Candidate(
            input.Character,
            inaccessibleTarget,
            "inaccessible-evidence");
        CharacterObjectSabotageEvidence eligible = Candidate(
            input.Character,
            eligibleTarget,
            "eligible-evidence");
        WorldObjectSnapshot worldObjects = new([inaccessibleTarget, eligibleTarget]);
        CharacterPhysicalAccessSnapshot access = new(
            input.Character.Identity.Id,
            [eligibleTarget.Identity.Id]);
        CharacterObjectSabotageEvidence[] candidates = [inaccessible, eligible];

        CharacterObjectSabotageContaminationResult result =
            CharacterObjectSabotageContaminationSequence.ApplyFirstEligible(
                input.Character,
                access,
                worldObjects,
                input.Contamination,
                input.ExistingEvidence,
                candidates);

        Assert.Same(eligible, result.EvidenceResult.Candidate);
        Assert.Same(
            eligibleTarget.Identity.Id,
            Assert.Single(result.Contamination.ContaminatedObjectIds));
        Assert.Same(eligible.Evidence, Assert.Single(result.EvidenceResult.Evidence.Entries));
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(inaccessible, candidate),
            candidate => Assert.Same(eligible, candidate));
    }

    [Fact]
    public void RejectsWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SequenceInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);
        CharacterObjectSabotageEvidence[] candidates = [input.Candidate];

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageContaminationSequence.ApplyFirstEligible(
                input.Character,
                inaccessible,
                input.WorldObjects,
                input.Contamination,
                input.ExistingEvidence,
                candidates));
        Assert.Same(input.Candidate, Assert.Single(candidates));
        Assert.Same(input.Target, Assert.Single(input.WorldObjects.Entries));
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeApplyingFirstEligibleCandidate()
    {
        SequenceInput input = Input();
        CharacterObjectSabotageEvidence duplicate = new(
            input.Candidate.Occurrence,
            new DiscoveryEvidence(
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
                input.Target.Identity.Id,
                VisibilityState.Noticed,
                false));

        Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageContaminationSequence.ApplyFirstEligible(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Contamination,
                input.ExistingEvidence,
                [input.Candidate, duplicate]));
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void RejectsEveryNullInputAndExposesOnePureOperation()
    {
        SequenceInput input = Input();
        CharacterObjectSabotageEvidence[] candidates = [input.Candidate];

        Assert.Throws<ArgumentNullException>(() => Apply(
            null!, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Apply(
            input.Character, null!, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Apply(
            input.Character, input.PhysicalAccess, null!, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            null!, candidates));
        Assert.Throws<ArgumentNullException>(() => Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, null!));

        Type sequenceType = typeof(CharacterObjectSabotageContaminationSequence);
        System.Reflection.MethodInfo method = Assert.Single(sequenceType.GetMethods().Where(
            candidate => candidate.DeclaringType == sequenceType));
        Assert.Equal("ApplyFirstEligible", method.Name);
        Assert.Equal(typeof(CharacterObjectSabotageContaminationResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Empty(sequenceType.GetConstructors());
        Assert.True(sequenceType.IsAbstract && sequenceType.IsSealed);
    }

    private static CharacterObjectSabotageContaminationResult Apply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot existingEvidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        CharacterObjectSabotageContaminationSequence.ApplyFirstEligible(
            character,
            physicalAccess,
            worldObjects,
            contamination,
            existingEvidence,
            candidates);

    private static SequenceInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target("village-water-cask");
        CharacterObjectSabotageEvidence candidate = Candidate(
            character,
            target,
            "bitter-cask-residue");

        return new SequenceInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static CharacterObjectSabotageEvidence Candidate(
        Character character,
        WorldObject target,
        string evidenceId)
    {
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId(evidenceId),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new CharacterObjectSabotageEvidence(occurrence, evidence);
    }

    private static WorldObject Target(string objectId) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), "Water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));

    private sealed record SequenceInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
