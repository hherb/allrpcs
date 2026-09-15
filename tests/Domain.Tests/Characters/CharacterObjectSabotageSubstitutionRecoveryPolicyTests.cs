using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionRecoveryPolicyTests
{
    [Fact]
    public void AllowsExactActiveSubstitutionWithAuthoritativeVisibleEvidence()
    {
        RecoveryInput input = Input();

        Assert.True(CanRecover(input));
        Assert.Same(input.Candidate.Substitution, Assert.Single(input.Substitutions.Entries));
        Assert.Same(input.Candidate.Evidence.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.Same(
            input.Candidate.Evidence.Evidence.EvidenceId,
            Assert.Single(input.EvidenceView.EvidenceIds));
    }

    [Fact]
    public void RejectsMissingOrDistinctValueEqualActiveSubstitution()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageSubstitution valueEqualSubstitution = Substitution(
            input.Candidate.Substitution.Occurrence,
            WorldObject(input.Candidate.Substitution.Replacement.Identity.Id.Value));

        Assert.False(CanRecover(input with
        {
            Substitutions = new CharacterObjectSabotageSubstitutionSnapshot([]),
        }));
        Assert.False(CanRecover(input with
        {
            Substitutions = new CharacterObjectSabotageSubstitutionSnapshot(
                [valueEqualSubstitution]),
        }));
        Assert.NotSame(input.Candidate.Substitution, valueEqualSubstitution);
        Assert.Equal(
            input.Candidate.Substitution.Replacement.Identity,
            valueEqualSubstitution.Replacement.Identity);
    }

    [Fact]
    public void RequiresExactAuthoritativeEvidenceAndVisibleEvidenceIdentity()
    {
        RecoveryInput input = Input();
        DiscoveryEvidence exactEvidence = input.Candidate.Evidence.Evidence;
        DiscoveryEvidence valueEqualEvidence = new(
            new EvidenceId(exactEvidence.EvidenceId.Value),
            new ObjectId(exactEvidence.ObjectId.Value),
            exactEvidence.MinimumVisibilityState,
            exactEvidence.IsCritical);

        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([]),
        }));
        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([valueEqualEvidence]),
        }));
        Assert.False(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                []),
        }));
        Assert.True(CanRecover(input with
        {
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                [new EvidenceId(exactEvidence.EvidenceId.Value)]),
        }));
    }

    [Fact]
    public void RejectsAuthoritativeVisibleEvidenceForAnotherObject()
    {
        RecoveryInput input = Input();
        DiscoveryEvidence mismatchedEvidence = new(
            new EvidenceId("misdirected-substitution-evidence"),
            new ObjectId("storehouse-ledger"),
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageSubstitutionCandidate mismatchedCandidate = new(
            input.Candidate.Substitution,
            new CharacterObjectSabotageEvidence(
                input.Candidate.Substitution.Occurrence,
                mismatchedEvidence));

        Assert.False(CanRecover(input with
        {
            Evidence = new DiscoveryEvidenceSnapshot([mismatchedEvidence]),
            EvidenceView = new CharacterEvidenceViewSnapshot(
                input.EvidenceView.CharacterId,
                [mismatchedEvidence.EvidenceId]),
            Candidate = mismatchedCandidate,
        }));
    }

    [Fact]
    public void RecoverRemovesOnlyExactSubstitutionAndRetainsExactAuthority()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageSubstitution retained = IndependentSubstitution(
            "storehouse-ledger",
            "false-ledger");
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new(
            [retained, input.Candidate.Substitution]);

        CharacterObjectSabotageSubstitutionRecoveryResult result =
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                substitutions,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.NotSame(substitutions, result.Substitutions);
        Assert.Same(retained, Assert.Single(result.Substitutions.Entries));
        Assert.Same(input.EvidenceView, result.EvidenceView);
        Assert.Same(input.Candidate, result.Candidate);
        Assert.Collection(
            substitutions.Entries,
            substitution => Assert.Same(retained, substitution),
            substitution => Assert.Same(input.Candidate.Substitution, substitution));
        Assert.Same(input.Candidate.Evidence.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.All(
            typeof(CharacterObjectSabotageSubstitutionRecoveryResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void RecoverRejectsIneligibleInputWithoutChangingAnything()
    {
        RecoveryInput input = Input();
        CharacterEvidenceViewSnapshot unseen = new(input.EvidenceView.CharacterId, []);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                input.Substitutions,
                input.Evidence,
                unseen,
                input.Candidate));

        Assert.Same(input.Candidate.Substitution, Assert.Single(input.Substitutions.Entries));
        Assert.Same(input.Candidate.Evidence.Evidence, Assert.Single(input.Evidence.Entries));
        Assert.Empty(unseen.EvidenceIds);
    }

    [Fact]
    public void RejectsNullsAndExposesOneClosedStaticQuery()
    {
        RecoveryInput input = Input();
        CharacterObjectSabotageSubstitutionRecoveryResult result =
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                input.Substitutions,
                input.Evidence,
                input.EvidenceView,
                input.Candidate);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                null!, input.Evidence, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                input.Substitutions, null!, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                input.Substitutions, input.Evidence, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
                input.Substitutions, input.Evidence, input.EvidenceView, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                null!, input.Evidence, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                input.Substitutions, null!, input.EvidenceView, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                input.Substitutions, input.Evidence, null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionRecoveryTransition.Recover(
                input.Substitutions, input.Evidence, input.EvidenceView, null!));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionRecoveryResult(
                null!, result.EvidenceView, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionRecoveryResult(
                result.Substitutions, null!, result.Candidate));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionRecoveryResult(
                result.Substitutions, result.EvidenceView, null!));

        System.Reflection.MethodInfo method = Assert.Single(
            typeof(CharacterObjectSabotageSubstitutionRecoveryPolicy)
                .GetMethods()
                .Where(candidate =>
                    candidate.DeclaringType
                        == typeof(CharacterObjectSabotageSubstitutionRecoveryPolicy)));

        Assert.Equal("CanRecover", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(typeof(CharacterObjectSabotageSubstitutionRecoveryPolicy).GetConstructors());
        Assert.True(typeof(CharacterObjectSabotageSubstitutionRecoveryPolicy).IsAbstract);
        Assert.True(typeof(CharacterObjectSabotageSubstitutionRecoveryPolicy).IsSealed);

        System.Reflection.MethodInfo transition = Assert.Single(
            typeof(CharacterObjectSabotageSubstitutionRecoveryTransition)
                .GetMethods()
                .Where(candidate =>
                    candidate.DeclaringType
                        == typeof(CharacterObjectSabotageSubstitutionRecoveryTransition)));
        Assert.Equal("Recover", transition.Name);
        Assert.True(transition.IsStatic);
        Assert.Empty(typeof(CharacterObjectSabotageSubstitutionRecoveryTransition).GetConstructors());
        Assert.True(typeof(CharacterObjectSabotageSubstitutionRecoveryTransition).IsAbstract);
        Assert.True(typeof(CharacterObjectSabotageSubstitutionRecoveryTransition).IsSealed);
    }

    private static bool CanRecover(RecoveryInput input) =>
        CharacterObjectSabotageSubstitutionRecoveryPolicy.CanRecover(
            input.Substitutions,
            input.Evidence,
            input.EvidenceView,
            input.Candidate);

    private static RecoveryInput Input()
    {
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                RepresentativeEnemyAgentCharacterSource.Create().Identity.Id,
                new CharacterActionAccessRequirement(
                    new ObjectId("village-water-cask"),
                    "substitute-water-source",
                    RepresentativeEnemyAgentCharacterSource
                        .Create()
                        .Knowledge
                        .KnowledgeReferences
                        .Single()),
                CharacterSabotageActionKind.Substitute),
            WorldObject("village-water-cask"));
        CharacterObjectSabotageSubstitution substitution = Substitution(
            occurrence,
            WorldObject("tainted-water-cask"));
        DiscoveryEvidence evidence = new(
            new EvidenceId("water-cask-substitution-signs"),
            occurrence.Target.Identity.Id,
            VisibilityState.Noticed,
            false);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            substitution,
            new CharacterObjectSabotageEvidence(occurrence, evidence));

        return new RecoveryInput(
            new CharacterObjectSabotageSubstitutionSnapshot([substitution]),
            new DiscoveryEvidenceSnapshot([evidence]),
            new CharacterEvidenceViewSnapshot(
                new CharacterId("seeker"),
                [evidence.EvidenceId]),
            candidate);
    }

    private static CharacterObjectSabotageSubstitution Substitution(
        CharacterObjectSabotageOccurrence occurrence,
        WorldObject replacement) =>
        new(occurrence, replacement);

    private static CharacterObjectSabotageSubstitution IndependentSubstitution(
        string targetId,
        string replacementId)
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "substitute-object",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);

        return Substitution(occurrence, WorldObject(replacementId));
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

    private sealed record RecoveryInput(
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterEvidenceViewSnapshot EvidenceView,
        CharacterObjectSabotageSubstitutionCandidate Candidate);
}
