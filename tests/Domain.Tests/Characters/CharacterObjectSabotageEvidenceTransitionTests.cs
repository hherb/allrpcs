using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageEvidenceTransitionTests
{
    [Fact]
    public void LeaveAppendsExactEvidenceToFreshSnapshotAndRetainsCandidate()
    {
        TransitionInput input = Input();
        DiscoveryEvidence retained = new(
            new EvidenceId("retained-evidence"),
            input.Target.Identity.Id,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot existing = new([retained]);

        CharacterObjectSabotageEvidenceResult result =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                existing,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.NotSame(existing, result.Evidence);
        Assert.Collection(
            result.Evidence.Entries,
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(input.Candidate.Evidence, entry));
        Assert.Same(retained, Assert.Single(existing.Entries));
        Assert.All(
            typeof(CharacterObjectSabotageEvidenceResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void LeaveRejectsIneligibleCandidateWithoutChangingInputs()
    {
        TransitionInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);
        DiscoveryEvidenceSnapshot existing = new([]);
        CharacterObjectSabotageOccurrence exactOccurrence = input.Candidate.Occurrence;
        DiscoveryEvidence exactEvidence = input.Candidate.Evidence;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                inaccessible,
                input.WorldObjects,
                existing,
                input.Candidate));

        Assert.Empty(existing.Entries);
        Assert.Empty(inaccessible.AccessibleObjectIds);
        Assert.Same(exactOccurrence, input.Candidate.Occurrence);
        Assert.Same(exactEvidence, input.Candidate.Evidence);
    }

    [Fact]
    public void ResultAndTransitionRejectNullInputs()
    {
        TransitionInput input = Input();
        DiscoveryEvidenceSnapshot existing = new([]);
        CharacterObjectSabotageEvidenceResult result =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                existing,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageEvidenceResult(null!, result.Evidence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageEvidenceResult(input.Candidate, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                null!, input.PhysicalAccess, input.WorldObjects, existing, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character, null!, input.WorldObjects, existing, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character, input.PhysicalAccess, null!, existing, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character, input.PhysicalAccess, input.WorldObjects, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character, input.PhysicalAccess, input.WorldObjects, existing, null!));
    }

    [Fact]
    public void TransitionExposesOnlyOneStaticOperation()
    {
        Type transitionType = typeof(CharacterObjectSabotageEvidenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(transitionType.GetMethods().Where(
            candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Leave", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(transitionType.GetConstructors());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static TransitionInput Input()
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
                CharacterSabotageActionKind.Conceal),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId("disturbed-cask-cover"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new TransitionInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageEvidence(occurrence, evidence));
    }

    private sealed record TransitionInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageEvidence Candidate);
}
