using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentRecoveryTests
{
    [Fact]
    public void PolicyAllowsConcealedTargetWithExactAuthoritativeVisibleEvidence()
    {
        RecoveryInput input = Input();

        Assert.True(CanRecover(input));
        Assert.Same(input.Target.Identity.Id, Assert.Single(input.Concealment.ConcealedObjectIds));
        Assert.Same(input.Candidate.Evidence, Assert.Single(input.Evidence.Entries));
    }

    [Fact]
    public void PolicyRejectsUnconcealedTargetUnseenEvidenceAndWrongKind()
    {
        RecoveryInput input = Input();

        Assert.False(CanRecover(input with { Concealment = new WorldObjectConcealmentSnapshot([]) }));
        Assert.False(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(input.EvidenceView.CharacterId, []),
        }));
        Assert.False(CanRecover(Input(CharacterSabotageActionKind.Move)));
    }

    [Fact]
    public void PolicyRequiresExactAuthoritativeEvidenceButAcceptsValueEqualViewIdentity()
    {
        RecoveryInput input = Input();
        DiscoveryEvidence valueEqualEvidence = new(
            new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
            new ObjectId(input.Target.Identity.Id.Value),
            input.Candidate.Evidence.MinimumVisibilityState,
            input.Candidate.Evidence.IsCritical);
        CharacterEvidenceViewSnapshot valueEqualView = new(
            input.EvidenceView.CharacterId,
            [new EvidenceId(input.Candidate.Evidence.EvidenceId.Value)]);

        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([valueEqualEvidence]),
        }));
        Assert.True(CanRecover(input with { EvidenceView = valueEqualView }));
    }

    [Fact]
    public void RecoverRemovesOnlyTargetIntoFreshSnapshotAndRetainsExactInputs()
    {
        RecoveryInput input = Input();
        ObjectId retainedId = new("storehouse-ledger");
        WorldObjectConcealmentSnapshot concealment = new(
            [retainedId, input.Target.Identity.Id]);

        CharacterObjectSabotageConcealmentRecoveryResult result =
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                concealment,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.Same(input.Candidate, result.Candidate);
        Assert.Same(input.EvidenceView, result.EvidenceView);
        Assert.NotSame(concealment, result.Concealment);
        Assert.Same(retainedId, Assert.Single(result.Concealment.ConcealedObjectIds));
        Assert.Collection(
            concealment.ConcealedObjectIds,
            objectId => Assert.Same(retainedId, objectId),
            objectId => Assert.Same(input.Target.Identity.Id, objectId));
        Assert.Same(input.Candidate.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.All(
            typeof(CharacterObjectSabotageConcealmentRecoveryResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void RecoverRejectsIneligibleInputWithoutChangingAnything()
    {
        RecoveryInput input = Input();
        CharacterEvidenceViewSnapshot unseen = new(input.EvidenceView.CharacterId, []);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                input.Concealment,
                input.Evidence,
                unseen,
                input.Candidate));

        Assert.Same(input.Target.Identity.Id, Assert.Single(input.Concealment.ConcealedObjectIds));
        Assert.Same(input.Candidate.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.Empty(unseen.EvidenceIds);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnePureOperationEach()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageConcealmentRecoveryResult result =
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                input.Concealment,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentRecoveryResult(
                null!, result.EvidenceView, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentRecoveryResult(
                result.Concealment, null!, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageConcealmentRecoveryResult(
                result.Concealment, result.EvidenceView, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                null!, input.Evidence, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                input.Concealment, null!, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                input.Concealment, input.Evidence, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
                input.Concealment, input.Evidence, input.EvidenceView, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                null!, input.Evidence, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                input.Concealment, null!, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                input.Concealment, input.Evidence, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageConcealmentRecoveryTransition.Recover(
                input.Concealment, input.Evidence, input.EvidenceView, null!));

        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageConcealmentRecoveryPolicy),
            "CanRecover");
        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageConcealmentRecoveryTransition),
            "Recover");
    }

    private static bool CanRecover(RecoveryInput input) =>
        CharacterObjectSabotageConcealmentRecoveryPolicy.CanRecover(
            input.Concealment,
            input.Evidence,
            input.EvidenceView,
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

    private static RecoveryInput Input(
        CharacterSabotageActionKind? actionKind = null)
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
                actionKind ?? CharacterSabotageActionKind.Conceal),
            target);
        DiscoveryEvidence sabotageEvidence = new(
            new EvidenceId("disturbed-cask-cover"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageEvidence candidate = new(occurrence, sabotageEvidence);

        return new RecoveryInput(
            target,
            new WorldObjectConcealmentSnapshot([target.Identity.Id]),
            new DiscoveryEvidenceSnapshot([sabotageEvidence]),
            new CharacterEvidenceViewSnapshot(
                new CharacterId("seeker"),
                [sabotageEvidence.EvidenceId]),
            candidate);
    }

    private sealed record RecoveryInput(
        WorldObject Target,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterEvidenceViewSnapshot EvidenceView,
        CharacterObjectSabotageEvidence Candidate);
}
