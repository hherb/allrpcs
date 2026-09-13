using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageConcealmentSelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrder()
    {
        SelectionInput input = Input();
        WorldObject firstTarget = Target("first-cask");
        WorldObject secondTarget = Target("second-cask");
        CharacterObjectSabotageEvidence first = Candidate(
            input.Character,
            firstTarget,
            "first-evidence");
        CharacterObjectSabotageEvidence second = Candidate(
            input.Character,
            secondTarget,
            "second-evidence");
        CharacterPhysicalAccessSnapshot access = new(
            input.Character.Identity.Id,
            [firstTarget.Identity.Id, secondTarget.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([firstTarget, secondTarget]);
        CharacterObjectSabotageEvidence[] candidates = [first, second];

        CharacterObjectSabotageEvidence? selected =
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                access,
                worldObjects,
                input.Concealment,
                input.ExistingEvidence,
                candidates);

        Assert.Same(first, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(first, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Collection(
            worldObjects.Entries,
            entry => Assert.Same(firstTarget, entry),
            entry => Assert.Same(secondTarget, entry));
        Assert.Collection(
            access.AccessibleObjectIds,
            objectId => Assert.Same(firstTarget.Identity.Id, objectId),
            objectId => Assert.Same(secondTarget.Identity.Id, objectId));
    }

    [Fact]
    public void SkipsEveryIneligibleKindBeforeLaterEligibleCandidate()
    {
        SelectionInput input = Input();
        WorldObject inaccessibleTarget = Target("inaccessible-cask");
        WorldObject incapableTarget = Target("incapable-cask");
        WorldObject unknownTarget = Target("unknown-cask");
        WorldObject missingTarget = Target("missing-cask");
        WorldObject valueEqualMissingTarget = Target("missing-cask");
        WorldObject concealedTarget = Target("concealed-cask");
        WorldObject staleEvidenceTarget = Target("stale-evidence-cask");
        WorldObject movedTarget = Target("moved-cask");
        WorldObject eligibleTarget = Target("eligible-cask");
        CharacterObjectSabotageEvidence inaccessible = Candidate(
            input.Character,
            inaccessibleTarget,
            "inaccessible-evidence");
        CharacterObjectSabotageEvidence incapable = Candidate(
            input.Character,
            incapableTarget,
            "incapable-evidence",
            requiredCapabilityKey: "missing-capability");
        CharacterObjectSabotageEvidence unknown = Candidate(
            input.Character,
            unknownTarget,
            "unknown-evidence",
            requiredKnowledgeId: new KnowledgeId("missing-knowledge"));
        CharacterObjectSabotageEvidence missing = Candidate(
            input.Character,
            missingTarget,
            "missing-target-evidence");
        CharacterObjectSabotageEvidence alreadyConcealed = Candidate(
            input.Character,
            concealedTarget,
            "concealed-evidence");
        CharacterObjectSabotageEvidence staleEvidence = Candidate(
            input.Character,
            staleEvidenceTarget,
            "stale-evidence");
        CharacterObjectSabotageEvidence nonConceal = Candidate(
            input.Character,
            movedTarget,
            "move-evidence",
            CharacterSabotageActionKind.Move);
        CharacterObjectSabotageEvidence eligible = Candidate(
            input.Character,
            eligibleTarget,
            "eligible-evidence");
        CharacterPhysicalAccessSnapshot access = new(
            input.Character.Identity.Id,
            [
                incapableTarget.Identity.Id,
                unknownTarget.Identity.Id,
                missingTarget.Identity.Id,
                concealedTarget.Identity.Id,
                staleEvidenceTarget.Identity.Id,
                movedTarget.Identity.Id,
                eligibleTarget.Identity.Id,
            ]);
        WorldObjectSnapshot worldObjects = new(
            [
                inaccessibleTarget,
                incapableTarget,
                unknownTarget,
                valueEqualMissingTarget,
                concealedTarget,
                staleEvidenceTarget,
                movedTarget,
                eligibleTarget,
            ]);
        WorldObjectConcealmentSnapshot concealment = new(
            [new ObjectId(concealedTarget.Identity.Id.Value)]);
        DiscoveryEvidenceSnapshot evidence = new(
            [
                new DiscoveryEvidence(
                    new EvidenceId(staleEvidence.Evidence.EvidenceId.Value),
                    staleEvidenceTarget.Identity.Id,
                    VisibilityState.Noticed,
                    false),
            ]);

        CharacterObjectSabotageEvidence? selected =
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                access,
                worldObjects,
                concealment,
                evidence,
                [
                    inaccessible,
                    incapable,
                    unknown,
                    missing,
                    alreadyConcealed,
                    staleEvidence,
                    nonConceal,
                    eligible,
                ]);

        Assert.Same(eligible, selected);
        Assert.Same(missingTarget.Identity.Id, missing.Occurrence.Target.Identity.Id);
        Assert.Same(valueEqualMissingTarget, worldObjects.Entries[3]);
        Assert.NotSame(missingTarget, valueEqualMissingTarget);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligible()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence inaccessible = Candidate(
            input.Character,
            input.Target,
            "inaccessible-evidence");

        CharacterObjectSabotageEvidence? selected =
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                new CharacterPhysicalAccessSnapshot(input.Character.Identity.Id, []),
                input.WorldObjects,
                input.Concealment,
                input.ExistingEvidence,
                [inaccessible]);

        Assert.Null(selected);
        Assert.Same(input.Target, Assert.Single(input.WorldObjects.Entries));
        Assert.Empty(input.Concealment.ConcealedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence first = Candidate(
            input.Character,
            input.Target,
            "duplicate-evidence");
        CharacterObjectSabotageEvidence duplicate = new(
            first.Occurrence,
            new DiscoveryEvidence(
                new EvidenceId(first.Evidence.EvidenceId.Value),
                input.Target.Identity.Id,
                VisibilityState.Noticed,
                false));

        Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Concealment,
                input.ExistingEvidence,
                [first, duplicate]));
        Assert.Same(first.Occurrence, duplicate.Occurrence);
        Assert.NotSame(first.Evidence, duplicate.Evidence);
    }

    [Fact]
    public void AcceptsMaximumCountAndRejectsInvalidCollections()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageConcealmentSelection.MaximumCandidateCount)
            .Select(index => Candidate(
                input.Character,
                input.Target,
                $"candidate-{index}"))
            .ToArray();
        CharacterObjectSabotageEvidence[] excessive = maximum
            .Append(Candidate(input.Character, input.Target, "excessive-candidate"))
            .ToArray();

        Assert.Same(
            maximum[0],
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Concealment,
                input.ExistingEvidence,
                maximum));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Concealment,
                input.ExistingEvidence,
                excessive));
        Assert.Throws<ArgumentException>(() =>
            CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
                input.Character,
                input.PhysicalAccess,
                input.WorldObjects,
                input.Concealment,
                input.ExistingEvidence,
                [null!]));
    }

    [Fact]
    public void RejectsEveryNullInputAndExposesOnePureOperation()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence[] candidates = [input.Candidate];

        Assert.Throws<ArgumentNullException>(() => Select(
            null!, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, null!, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, null!, input.Concealment,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            null!, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Concealment,
            input.ExistingEvidence, null!));

        Type selectionType = typeof(CharacterObjectSabotageConcealmentSelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstEligible", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterObjectSabotageEvidence? Select(
        Character character,
        CharacterPhysicalAccessSnapshot access,
        WorldObjectSnapshot worldObjects,
        WorldObjectConcealmentSnapshot concealment,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        CharacterObjectSabotageConcealmentSelection.SelectFirstEligible(
            character,
            access,
            worldObjects,
            concealment,
            evidence,
            candidates);

    private static SelectionInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target("village-water-cask");
        CharacterObjectSabotageEvidence candidate = Candidate(
            character,
            target,
            "disturbed-cask-cover");

        return new SelectionInput(
            character,
            target,
            new CharacterPhysicalAccessSnapshot(character.Identity.Id, [target.Identity.Id]),
            new WorldObjectSnapshot([target]),
            new WorldObjectConcealmentSnapshot([]),
            new DiscoveryEvidenceSnapshot([]),
            candidate);
    }

    private static CharacterObjectSabotageEvidence Candidate(
        Character character,
        WorldObject target,
        string evidenceId,
        CharacterSabotageActionKind? actionKind = null,
        string requiredCapabilityKey = "assess-local-influence",
        KnowledgeId? requiredKnowledgeId = null)
    {
        CharacterActionAccessRequirement requirement = new(
            target.Identity.Id,
            requiredCapabilityKey,
            requiredKnowledgeId ?? character.Knowledge.KnowledgeReferences.Single());
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                requirement,
                actionKind ?? CharacterSabotageActionKind.Conceal),
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

    private sealed record SelectionInput(
        Character Character,
        WorldObject Target,
        CharacterPhysicalAccessSnapshot PhysicalAccess,
        WorldObjectSnapshot WorldObjects,
        WorldObjectConcealmentSnapshot Concealment,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
