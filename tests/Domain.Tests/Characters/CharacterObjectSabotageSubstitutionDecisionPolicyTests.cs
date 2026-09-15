using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageSubstitutionDecisionPolicyTests
{
    [Fact]
    public void AcceptsExactAbstentionAndExactEligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageSubstitutionDecision abstention = new(
            input.Character.Identity.Id,
            null);
        CharacterObjectSabotageSubstitutionDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.True(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                input.Request,
                abstention));
        Assert.True(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                input.Request,
                selection));
    }

    [Fact]
    public void RejectsDistinctValueEqualCharacterOrEvidenceIdentifiers()
    {
        PolicyInput input = Input();

        Assert.False(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                input.Request,
                new CharacterObjectSabotageSubstitutionDecision(
                    new CharacterId(input.Character.Identity.Id.Value),
                    null)));
        Assert.False(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                input.Request,
                new CharacterObjectSabotageSubstitutionDecision(
                    input.Character.Identity.Id,
                    new EvidenceId(
                        input.Candidate.Evidence.Evidence.EvidenceId.Value))));
    }

    [Fact]
    public void RejectsUnknownOrCurrentlyIneligibleSelection()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageSubstitutionDecision unknown = new(
            input.Character.Identity.Id,
            new EvidenceId("unknown-evidence"));
        CharacterObjectSabotageSubstitutionConsiderationRequest inaccessible = Request(
            input,
            new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
            input.Substitutions);
        CharacterObjectSabotageSubstitutionConsiderationRequest alreadySubstituted = Request(
            input,
            input.PhysicalAccess,
            new CharacterObjectSabotageSubstitutionSnapshot(
                [input.Candidate.Substitution]));
        CharacterObjectSabotageSubstitutionDecision selection = new(
            input.Character.Identity.Id,
            input.Candidate.Evidence.Evidence.EvidenceId);

        Assert.False(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                input.Request,
                unknown));
        Assert.False(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                inaccessible,
                selection));
        Assert.False(
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(
                alreadySubstituted,
                selection));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePureAcceptanceQuery()
    {
        PolicyInput input = Input();
        CharacterObjectSabotageSubstitutionDecision decision = new(
            input.Character.Identity.Id,
            null);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterObjectSabotageSubstitutionDecisionPolicy.CanAccept(input.Request, null!));

        Type policyType = typeof(CharacterObjectSabotageSubstitutionDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterObjectSabotageSubstitutionConsiderationRequest Request(
        PolicyInput input,
        CharacterPhysicalAccessSnapshot access,
        CharacterObjectSabotageSubstitutionSnapshot substitutions) =>
        new(
            input.Character,
            access,
            input.WorldObjects,
            substitutions,
            input.Evidence,
            [input.Candidate]);

    private static PolicyInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject("village-water-cask");
        WorldObject replacement = WorldObject("tainted-water-cask");
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        CharacterObjectSabotageSubstitutionCandidate candidate = new(
            new CharacterObjectSabotageSubstitution(occurrence, replacement),
            new CharacterObjectSabotageEvidence(
                occurrence,
                new DiscoveryEvidence(
                    new EvidenceId("water-cask-substitution-signs"),
                    target.Identity.Id,
                    VisibilityState.Noticed,
                    false)));
        CharacterPhysicalAccessSnapshot access = new(
            character.Identity.Id,
            [target.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([target]);
        CharacterObjectSabotageSubstitutionSnapshot substitutions = new([]);
        DiscoveryEvidenceSnapshot evidence = new([]);
        CharacterObjectSabotageSubstitutionConsiderationRequest request = new(
            character,
            access,
            worldObjects,
            substitutions,
            evidence,
            [candidate]);

        return new PolicyInput(
            character,
            access,
            worldObjects,
            substitutions,
            evidence,
            candidate,
            request);
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), id),
            new WorldObjectDescription($"The authored {id} object."));

    private sealed record PolicyInput(
        Character Character,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        CharacterObjectSabotageSubstitutionSnapshot Substitutions,
        DiscoveryEvidenceSnapshot Evidence,
        CharacterObjectSabotageSubstitutionCandidate Candidate,
        CharacterObjectSabotageSubstitutionConsiderationRequest Request);
}
