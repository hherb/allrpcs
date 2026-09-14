using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionTransitionTests
{
    [Fact]
    public void PolicyAllowsEligibleFreshSubstitutionAndRejectsRepeatedTarget()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageSubstitution repeated = IndependentSubstitution(
            input.Target.Identity.Id.Value,
            "other-replacement");

        Assert.True(CanApply(input));
        Assert.False(CanApply(input with
        {
            Substitutions = new CharacterObjectSabotageSubstitutionSnapshot([repeated]),
        }));
        Assert.NotSame(
            input.Candidate.Substitution.Occurrence.Target,
            repeated.Occurrence.Target);
        Assert.Equal(
            input.Candidate.Substitution.Occurrence.Target.Identity.Id,
            repeated.Occurrence.Target.Identity.Id);
    }

    [Fact]
    public void PolicyDelegatesExactOccurrenceAccessAndFreshEvidenceEligibility()
    {
        SabotageInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(
            input.Character.Identity.Id,
            []);
        DiscoveryEvidence staleEvidence = new(
            new EvidenceId(input.Candidate.Evidence.Evidence.EvidenceId.Value),
            input.Target.Identity.Id,
            VisibilityState.Unnoticed,
            false);
        WorldObject valueEqualTarget = WorldObject(input.Target.Identity.Id.Value);

        Assert.False(CanApply(input with { PhysicalAccess = inaccessible }));
        Assert.False(CanApply(input with
        {
            ExistingEvidence = new DiscoveryEvidenceSnapshot([staleEvidence]),
        }));
        Assert.False(CanApply(input with
        {
            WorldObjects = new WorldObjectSnapshot([valueEqualTarget]),
        }));
        Assert.NotSame(input.Target, valueEqualTarget);
        Assert.Equal(input.Target.Identity, valueEqualTarget.Identity);
    }

    [Fact]
    public void ApplyReturnsFreshStableSubstitutionAndExactEvidenceState()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageSubstitution retained = IndependentSubstitution(
            "herb-bundle",
            "lookalike-bundle");
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([retained]);
        DiscoveryEvidence retainedEvidence = new(
            new EvidenceId("retained-evidence"),
            retained.Occurrence.Target.Identity.Id,
            VisibilityState.Identified,
            false);
        DiscoveryEvidenceSnapshot evidence = new([retainedEvidence]);
        WorldObjectIdentity targetIdentity = input.Target.Identity;
        WorldObjectDescription targetDescription = input.Target.Description;
        WorldObject replacement = input.Candidate.Substitution.Replacement;
        WorldObjectIdentity replacementIdentity = replacement.Identity;
        WorldObjectDescription replacementDescription = replacement.Description;

        CharacterObjectSabotageSubstitutionResult result =
            CharacterObjectSabotageSubstitutionTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                substitutions,
                evidence,
                input.Candidate);

        Assert.NotSame(substitutions, result.Substitutions);
        Assert.Collection(
            result.Substitutions.Entries,
            entry => Assert.Same(retained, entry),
            entry => Assert.Same(input.Candidate.Substitution, entry));
        Assert.Same(input.Candidate.Evidence, result.EvidenceResult.Candidate);
        Assert.NotSame(evidence, result.EvidenceResult.Evidence);
        Assert.Collection(
            result.EvidenceResult.Evidence.Entries,
            entry => Assert.Same(retainedEvidence, entry),
            entry => Assert.Same(input.Candidate.Evidence.Evidence, entry));
        Assert.Same(retained, Assert.Single(substitutions.Entries));
        Assert.Same(retainedEvidence, Assert.Single(evidence.Entries));
        Assert.Same(input.Target, Assert.Single(input.WorldObjects.Entries));
        Assert.Same(targetIdentity, input.Target.Identity);
        Assert.Same(targetDescription, input.Target.Description);
        Assert.Same(replacementIdentity, replacement.Identity);
        Assert.Same(replacementDescription, replacement.Description);
        Assert.All(
            typeof(CharacterObjectSabotageSubstitutionResult).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void ApplyRejectsRepeatedTargetWithoutChangingInputs()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageSubstitution retained = IndependentSubstitution(
            input.Target.Identity.Id.Value,
            "other-replacement");
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([retained]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterObjectSabotageSubstitutionTransition.Apply(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                substitutions,
                input.ExistingEvidence,
                input.Candidate));
        Assert.Same(retained, Assert.Single(substitutions.Entries));
        Assert.Empty(input.ExistingEvidence.Entries);
        Assert.Same(input.Target, Assert.Single(input.WorldObjects.Entries));
    }

    [Fact]
    public void ContractsRejectNullsAndExposeOnlyOnePureOperationEach()
    {
        SabotageInput input = Input();
        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.ExistingEvidence,
                input.Candidate.Evidence);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionResult(null!, evidenceResult));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterObjectSabotageSubstitutionResult(input.Substitutions, null!));
        AssertNullRejection(
            input,
            (character, access, worldObjects, substitutions, evidence, candidate) =>
                CharacterObjectSabotageSubstitutionPolicy.CanApply(
                    character,
                    access,
                    worldObjects,
                    substitutions,
                    evidence,
                    candidate));
        AssertNullRejection(
            input,
            (character, access, worldObjects, substitutions, evidence, candidate) =>
                CharacterObjectSabotageSubstitutionTransition.Apply(
                    character,
                    access,
                    worldObjects,
                    substitutions,
                    evidence,
                    candidate));

        AssertSingleStaticOperation(typeof(CharacterObjectSabotageSubstitutionPolicy), "CanApply");
        AssertSingleStaticOperation(typeof(CharacterObjectSabotageSubstitutionTransition), "Apply");
    }

    private static bool CanApply(SabotageInput input) =>
        CharacterObjectSabotageSubstitutionPolicy.CanApply(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Substitutions,
            input.ExistingEvidence,
            input.Candidate);

    private static void AssertNullRejection(
        SabotageInput input,
        Func<Character, CharacterPhysicalAccessSnapshot, WorldObjectSnapshot,
            CharacterObjectSabotageSubstitutionSnapshot, DiscoveryEvidenceSnapshot,
            CharacterObjectSabotageSubstitutionCandidate, object> operation)
    {
        Assert.Throws<ArgumentNullException>(() => operation(
            null!, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, null!, input.WorldObjects, input.Substitutions,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, null!, input.Substitutions,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
            null!, input.Candidate));
        Assert.Throws<ArgumentNullException>(() => operation(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Substitutions,
            input.ExistingEvidence, null!));
    }

    private static void AssertSingleStaticOperation(Type type, string expectedName)
    {
        System.Reflection.MethodInfo method = Assert.Single(type.GetMethods().Where(
            candidate => candidate.DeclaringType == type));

        Assert.Equal(expectedName, method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(type.GetConstructors());
        Assert.True(type.IsAbstract && type.IsSealed);
    }

    private static SabotageInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitution substitution = new(
            occurrence,
            WorldObject("tainted-water-cask"));
        CharacterObjectSabotageEvidence evidence = new(
            occurrence,
            new DiscoveryEvidence(
                new EvidenceId("mismatched-cask-hoops"),
                target.Identity.Id,
                VisibilityState.Noticed,
                false));

        return new SabotageInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new CharacterObjectSabotageSubstitutionSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            new CharacterObjectSabotageSubstitutionCandidate(substitution, evidence));
    }

    private static CharacterObjectSabotageSubstitution IndependentSubstitution(
        string targetId,
        string replacementId)
    {
        WorldObject target = WorldObject(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                new CharacterId("enemy-agent"),
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    new KnowledgeId("water-cask-contents")),
                CharacterSabotageActionKind.Substitute),
            target);

        return new CharacterObjectSabotageSubstitution(occurrence, WorldObject(replacementId));
    }

    private static WorldObject WorldObject(string objectId) =>
        new(
            new WorldObjectIdentity(new ObjectId(objectId), $"Object {objectId}"),
            new WorldObjectDescription($"Description for {objectId}."));

    private sealed record SabotageInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate);
}
