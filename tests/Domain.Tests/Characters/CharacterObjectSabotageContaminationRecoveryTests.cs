using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationRecoveryTests
{
    [Fact]
    public void PolicyRequiresContaminatedTargetAndExactAuthoritativeVisibleEvidence()
    {
        RecoveryInput input = Input();
        DiscoveryEvidence valueEqualEvidence = new(
            new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
            new ObjectId(input.Target.Identity.Id.Value),
            input.Candidate.Evidence.MinimumVisibilityState,
            input.Candidate.Evidence.IsCritical);

        Assert.True(CanRecover(input));
        Assert.False(CanRecover(input with { Contamination = new WorldObjectContaminationSnapshot([]) }));
        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([valueEqualEvidence]),
        }));
        Assert.False(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(input.EvidenceView.CharacterId, []),
        }));
        Assert.True(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                [new EvidenceId(input.Candidate.Evidence.EvidenceId.Value)]),
        }));
    }

    [Fact]
    public void PolicyRejectsWrongActionKindAndMisdirectedEvidence()
    {
        RecoveryInput wrongKind = Input(CharacterSabotageActionKind.Conceal);
        RecoveryInput input = Input();
        DiscoveryEvidence misdirectedEvidence = new(
            new EvidenceId("misdirected-evidence"),
            new ObjectId("healers-mortar"),
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageEvidence misdirectedCandidate = new(
            input.Candidate.Occurrence,
            misdirectedEvidence);

        Assert.False(CanRecover(wrongKind));
        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([misdirectedEvidence]),
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                [misdirectedEvidence.EvidenceId]),
            Candidate = misdirectedCandidate,
        }));
    }

    [Fact]
    public void RecoverRemovesOnlyTargetAndRetainsExactAuthorityWithoutConsumingEvidence()
    {
        RecoveryInput input = Input();
        ObjectId retainedId = new("healers-mortar");
        WorldObjectContaminationSnapshot contamination = new(
            [retainedId, input.Target.Identity.Id]);

        CharacterObjectSabotageContaminationRecoveryResult result =
            CharacterObjectSabotageContaminationRecoveryTransition.Recover(
                contamination,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.NotSame(contamination, result.Contamination);
        Assert.Same(retainedId, Assert.Single(result.Contamination.ContaminatedObjectIds));
        Assert.Same(input.EvidenceView, result.EvidenceView);
        Assert.Same(input.Candidate, result.Candidate);
        Assert.Collection(
            contamination.ContaminatedObjectIds,
            objectId => Assert.Same(retainedId, objectId),
            objectId => Assert.Same(input.Target.Identity.Id, objectId));
        Assert.Same(input.Candidate.Evidence, Assert.Single(input.Evidence.Entries));
    }

    [Fact]
    public void RecoverRejectsIneligibleInputWithoutChangingAnything()
    {
        RecoveryInput input = Input();
        CharacterEvidenceViewSnapshot unseen = new(input.EvidenceView.CharacterId, []);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageContaminationRecoveryTransition.Recover(
                input.Contamination,
                input.Evidence,
                unseen,
                input.Candidate));

        Assert.Same(input.Target.Identity.Id, Assert.Single(input.Contamination.ContaminatedObjectIds));
        Assert.Same(input.Candidate.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.Empty(unseen.EvidenceIds);
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOneClosedOperationEach()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageContaminationRecoveryResult result =
            CharacterObjectSabotageContaminationRecoveryTransition.Recover(
                input.Contamination,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationRecoveryResult(
                null!, result.EvidenceView, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationRecoveryResult(
                result.Contamination, null!, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageContaminationRecoveryResult(
                result.Contamination, result.EvidenceView, null!));
        AssertNullRejection(
            input,
            (contamination, evidence, evidenceView, candidate) =>
                CharacterObjectSabotageContaminationRecoveryPolicy.CanRecover(
                    contamination, evidence, evidenceView, candidate));
        AssertNullRejection(
            input,
            (contamination, evidence, evidenceView, candidate) =>
                CharacterObjectSabotageContaminationRecoveryTransition.Recover(
                    contamination, evidence, evidenceView, candidate));

        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageContaminationRecoveryPolicy),
            "CanRecover");
        AssertSingleStaticOperation(
            typeof(CharacterObjectSabotageContaminationRecoveryTransition),
            "Recover");
        Assert.All(
            typeof(CharacterObjectSabotageContaminationRecoveryResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    private static void AssertNullRejection(
        RecoveryInput input,
        Func<WorldObjectContaminationSnapshot, DiscoveryEvidenceSnapshot,
            CharacterEvidenceViewSnapshot, CharacterObjectSabotageEvidence, object> operation)
    {
        Assert.Throws<ArgumentNullException>(() => operation(
            null!, input.Evidence, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Contamination, null!, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Contamination, input.Evidence, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Contamination, input.Evidence, input.EvidenceView, null!));
    }

    private static void AssertSingleStaticOperation(Type type, string name)
    {
        System.Reflection.MethodInfo method = Assert.Single(type.GetMethods().Where(
            candidate => candidate.DeclaringType == type));
        Assert.Equal(name, method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(type.GetConstructors());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static bool CanRecover(RecoveryInput input) =>
        CharacterObjectSabotageContaminationRecoveryPolicy.CanRecover(
            input.Contamination,
            input.Evidence,
            input.EvidenceView,
            input.Candidate);

    private static RecoveryInput Input(CharacterSabotageActionKind? actionKind = null)
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
                actionKind ?? CharacterSabotageActionKind.Contaminate),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId("bitter-cask-residue"),
            target.Identity.Id,
            VisibilityState.Noticed,
            false);

        return new RecoveryInput(
            target,
            new WorldObjectContaminationSnapshot([target.Identity.Id]),
            new DiscoveryEvidenceSnapshot([evidence]),
            new CharacterEvidenceViewSnapshot(new CharacterId("seeker"), [evidence.EvidenceId]),
            new CharacterObjectSabotageEvidence(occurrence, evidence));
    }

    private sealed record RecoveryInput(
        WorldObject Target,
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterEvidenceViewSnapshot EvidenceView,
        CharacterObjectSabotageEvidence Candidate);
}
