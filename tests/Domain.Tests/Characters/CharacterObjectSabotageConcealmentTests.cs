using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentTests
{
    [Fact]
    public void PolicyAllowsEligibleConcealmentWhenTargetIsNotConcealed()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Conceal);

        Assert.True(CanApply(input));
        Assert.Empty(input.Concealment.ConcealedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void PolicyRejectsValueEqualRepeatedConcealmentAndOtherObjectKinds()
    {
        SabotageInput conceal = Input(CharacterSabotageActionKind.Conceal);
        WorldObjectConcealmentSnapshot alreadyConcealed = new(
            [new ObjectId(conceal.Target.Identity.Id.Value)]);

        Assert.False(CanApply(conceal with { Concealment = alreadyConcealed }));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Move)));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Substitute)));
        Assert.False(CanApply(Input(CharacterSabotageActionKind.Contaminate)));
    }

    [Fact]
    public void PolicyRejectsStaleEvidenceIdentityAndMissingAccess()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Conceal);
        DiscoveryEvidence staleEvidence = new(
            new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
            input.Target.Identity.Id,
            VisibilityState.Noticed,
            false);
        CharacterPhysicalAccessSnapshot missingAccess = new(input.Character.Identity.Id, []);

        Assert.False(CanApply(input with
        {
            ExistingEvidence = new DiscoveryEvidenceSnapshot([staleEvidence]),
        }));
        Assert.False(CanApply(input with { PhysicalAccess = missingAccess }));
    }

    [Fact]
    public void ApplyReturnsFreshStableConcealmentAndExactFreshEvidenceResult()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Conceal);
        ObjectId retainedId = new("storehouse-ledger");
        WorldObjectConcealmentSnapshot concealment = new([retainedId]);
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retainedId,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot evidence = new([retainedEvidence]);

        CharacterObjectSabotageConcealmentResult result =
            CharacterObjectSabotageConcealmentTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                concealment,
                evidence,
                input.Candidate);

        Assert.NotSame(concealment, result.Concealment);
        Assert.Collection(
            result.Concealment.ConcealedObjectIds,
            objectId => Assert.Same(retainedId, objectId),
            objectId => Assert.Same(input.Target.Identity.Id, objectId));
        Assert.Same(input.Candidate, result.EvidenceResult.Candidate);
        Assert.NotSame(evidence, result.EvidenceResult.Evidence);
        Assert.Collection(
            result.EvidenceResult.Evidence.Entries,
            entry => Assert.Same(retainedEvidence, entry),
            entry => Assert.Same(input.Candidate.Evidence, entry));
        Assert.Same(retainedId, Assert.Single(concealment.ConcealedObjectIds));
        Assert.Same(retainedEvidence, Assert.Single(evidence.Entries));
        Assert.All(
            typeof(CharacterObjectSabotageConcealmentResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void ApplyRejectsRepeatedConcealmentWithoutChangingInputs()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Conceal);
        ObjectId retainedExactId = new(input.Target.Identity.Id.Value);
        WorldObjectConcealmentSnapshot concealment = new([retainedExactId]);
        CharacterObjectSabotageOccurrence exactOccurrence = input.Candidate.Occurrence;
        DiscoveryEvidence exactEvidence = input.Candidate.Evidence;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageConcealmentTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                concealment,
                input.ExistingEvidence,
                input.Candidate));

        Assert.Same(retainedExactId, Assert.Single(concealment.ConcealedObjectIds));
        Assert.Empty(input.ExistingEvidence.Entries);
        Assert.Same(exactOccurrence, input.Candidate.Occurrence);
        Assert.Same(exactEvidence, input.Candidate.Evidence);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyOnePureOperationEach()
    {
        SabotageInput input = Input(CharacterSabotageActionKind.Conceal);
        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.ExistingEvidence,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentResult(null!, evidenceResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentResult(input.Concealment, null!));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            null!, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character, null!, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character, input.PhysicalAccess, null!, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, null!));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            null!, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            input.Character, null!, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            input.Character, input.PhysicalAccess, null!, input.Concealment,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => CharacterObjectSabotageConcealmentTransition.Apply(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, null!));

        AssertSingleStaticOperation(typeof(CharacterObjectSabotageConcealmentPolicy), "CanApply");
        AssertSingleStaticOperation(typeof(CharacterObjectSabotageConcealmentTransition), "Apply");
    }

    private static bool CanApply(SabotageInput input) =>
        CharacterObjectSabotageConcealmentPolicy.CanApply(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Concealment,
            input.ExistingEvidence,
            input.Candidate);

    private static void AssertSingleStaticOperation(Type type, string expectedName)
    {
        System.Reflection.MethodInfo method = Assert.Single(type.GetMethods().Where(
            candidate => candidate.DeclaringType == type));

        Assert.Equal(expectedName, method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(type.GetConstructors());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static SabotageInput Input(CharacterSabotageActionKind actionKind)
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
                actionKind),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId("disturbed-cask-cover"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new SabotageInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectConcealmentSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            new CharacterObjectSabotageEvidence(occurrence, evidence));
    }

    private sealed record SabotageInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
