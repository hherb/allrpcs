using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationTransitionTests
{
    [Fact]
    public void ApplyReturnsFreshStableContaminationAndExactFreshEvidence()
    {
        SabotageInput input = Input();
        ObjectId retainedId = new("healers-mortar");
        WorldObjectContaminationSnapshot contamination = new([retainedId]);
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedId,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot evidence = new([retainedEvidence]);

        CharacterObjectSabotageContaminationResult result =
            CharacterObjectSabotageContaminationTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                contamination,
                evidence,
                input.Candidate);

        Assert.NotSame(contamination, result.Contamination);
        Assert.Collection(
            result.Contamination.ContaminatedObjectIds,
            objectId => Assert.Same(retainedId, objectId),
            objectId => Assert.Same(input.Target.Identity.Id, objectId));
        Assert.Same(input.Candidate, result.EvidenceResult.Candidate);
        Assert.NotSame(evidence, result.EvidenceResult.Evidence);
        Assert.Collection(
            result.EvidenceResult.Evidence.Entries,
            entry => Assert.Same(retainedEvidence, entry),
            entry => Assert.Same(input.Candidate.Evidence, entry));
        Assert.Same(retainedId, Assert.Single(contamination.ContaminatedObjectIds));
        Assert.Same(retainedEvidence, Assert.Single(evidence.Entries));
    }

    [Fact]
    public void ApplyRejectsRepeatedContaminationWithoutChangingInputs()
    {
        SabotageInput input = Input();
        ObjectId retainedId = new(input.Target.Identity.Id.Value);
        WorldObjectContaminationSnapshot contamination = new([retainedId]);
        CharacterObjectSabotageOccurrence occurrence = input.Candidate.Occurrence;
        DiscoveryEvidence candidateEvidence = input.Candidate.Evidence;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageContaminationTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                contamination,
                input.ExistingEvidence,
                input.Candidate));

        Assert.Same(retainedId, Assert.Single(contamination.ContaminatedObjectIds));
        Assert.Empty(input.ExistingEvidence.Entries);
        Assert.Same(occurrence, input.Candidate.Occurrence);
        Assert.Same(candidateEvidence, input.Candidate.Evidence);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeClosedReadOnlyOperations()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.ExistingEvidence,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationResult(null!, evidenceResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationResult(input.Contamination, null!));
        AssertNullRejection(
            input,
            (character, physicalAccess, worldObjects, contamination, evidence, candidate) =>
                CharacterObjectSabotageContaminationTransition.Apply(
                    character,
                    physicalAccess,
                    worldObjects,
                    contamination,
                    evidence,
                    candidate));

        Assert.All(
            typeof(CharacterObjectSabotageContaminationResult).GetProperties(),
            property => Assert.False(property.CanWrite));
        System.Reflection.MethodInfo method = Assert.Single(
            typeof(CharacterObjectSabotageContaminationTransition).GetMethods().Where(
                candidate => candidate.DeclaringType == typeof(CharacterObjectSabotageContaminationTransition)));
        Assert.Equal("Apply", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(typeof(CharacterObjectSabotageContaminationTransition).GetConstructors());
    }

    private static void AssertNullRejection(
        SabotageInput input,
        Func<Character, CharacterPhysicalAccessSnapshot, WorldObjectSnapshot,
            WorldObjectContaminationSnapshot, DiscoveryEvidenceSnapshot,
            CharacterObjectSabotageEvidence, object> operation)
    {
        Assert.Throws<ArgumentNullException>(() => operation(
            null!, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, null!, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, null!, input.Contamination,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, null!));
    }

    private static SabotageInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = new(
            new WorldObjectIdentity(new ObjectId("village-water-cask"), "Village water cask"),
            new WorldObjectDescription("A covered cask beside the village well."));
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Contaminate),
            target);
        CharacterObjectSabotageEvidence candidate = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("bitter-cask-residue"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

        return new SabotageInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectContaminationSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private sealed record SabotageInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
