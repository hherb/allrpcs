using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterObjectSabotageContaminationSelectionTests
{
    [Fact]
    public void SelectsExactFirstEligibleCandidateInStableOrder()
    {
        SelectionInput input = Input();
        WorldObject firstTarget = Target("first-cask");
        WorldObject secondTarget = Target("second-cask");
        CharacterObjectSabotageEvidence first = Candidate(input.Character, firstTarget, "first-evidence");
        CharacterObjectSabotageEvidence second = Candidate(input.Character, secondTarget, "second-evidence");
        CharacterPhysicalAccessSnapshot access = new(
            input.Character.Identity.Id,
            [firstTarget.Identity.Id, secondTarget.Identity.Id]);
        WorldObjectSnapshot worldObjects = new([firstTarget, secondTarget]);
        CharacterObjectSabotageEvidence[] candidates = [first, second];

        CharacterObjectSabotageEvidence? selected = Select(
            input.Character,
            access,
            worldObjects,
            input.Contamination,
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
        WorldObject wrongActorTarget = Target("wrong-actor-cask");
        WorldObject wrongRequirementTarget = Target("wrong-requirement-cask");
        WorldObject misdirectedEvidenceTarget = Target("misdirected-evidence-cask");
        WorldObject contaminatedTarget = Target("contaminated-cask");
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
        CharacterObjectSabotageEvidence wrongActor = Candidate(
            input.Character,
            wrongActorTarget,
            "wrong-actor-evidence",
            actorId: new CharacterId("another-character"));
        CharacterObjectSabotageEvidence wrongRequirement = Candidate(
            input.Character,
            wrongRequirementTarget,
            "wrong-requirement-evidence",
            requiredTargetId: new ObjectId("another-target"));
        CharacterObjectSabotageEvidence misdirectedEvidence = Candidate(
            input.Character,
            misdirectedEvidenceTarget,
            "misdirected-evidence",
            evidenceTargetId: new ObjectId("another-evidence-target"));
        CharacterObjectSabotageEvidence alreadyContaminated = Candidate(
            input.Character,
            contaminatedTarget,
            "contaminated-evidence");
        CharacterObjectSabotageEvidence staleEvidence = Candidate(
            input.Character,
            staleEvidenceTarget,
            "stale-evidence");
        CharacterObjectSabotageEvidence nonContaminate = Candidate(
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
                wrongActorTarget.Identity.Id,
                wrongRequirementTarget.Identity.Id,
                misdirectedEvidenceTarget.Identity.Id,
                contaminatedTarget.Identity.Id,
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
                wrongActorTarget,
                wrongRequirementTarget,
                misdirectedEvidenceTarget,
                contaminatedTarget,
                staleEvidenceTarget,
                movedTarget,
                eligibleTarget,
            ]);
        WorldObjectContaminationSnapshot contamination = new(
            [new ObjectId(contaminatedTarget.Identity.Id.Value)]);
        DiscoveryEvidenceSnapshot evidence = new(
            [
                new DiscoveryEvidence(
                    new EvidenceId(staleEvidence.Evidence.EvidenceId.Value),
                    staleEvidenceTarget.Identity.Id,
                    VisibilityState.Noticed,
                    false),
            ]);

        CharacterObjectSabotageEvidence? selected = Select(
            input.Character,
            access,
            worldObjects,
            contamination,
            evidence,
            [
                inaccessible,
                incapable,
                unknown,
                missing,
                wrongActor,
                wrongRequirement,
                misdirectedEvidence,
                alreadyContaminated,
                staleEvidence,
                nonContaminate,
                eligible,
            ]);

        Assert.Same(eligible, selected);
        Assert.Same(missingTarget.Identity.Id, missing.Occurrence.Target.Identity.Id);
        Assert.Same(valueEqualMissingTarget, worldObjects.Entries[3]);
        Assert.NotSame(missingTarget, valueEqualMissingTarget);
    }

    [Fact]
    public void ReturnsNullWhenNoCandidateIsEligibleWithoutChangingInputs()
    {
        SelectionInput input = Input();
        CharacterPhysicalAccessSnapshot inaccessible = new(input.Character.Identity.Id, []);

        CharacterObjectSabotageEvidence? selected = Select(
            input.Character,
            inaccessible,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            [input.Candidate]);

        Assert.Null(selected);
        Assert.Null(Select(
            input.Character,
            new CharacterPhysicalAccessSnapshot(new CharacterId("another-character"), [input.Target.Identity.Id]),
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            [input.Candidate]));
        Assert.Same(input.Target, Assert.Single(input.WorldObjects.Entries));
        Assert.Empty(inaccessible.AccessibleObjectIds);
        Assert.Empty(input.Contamination.ContaminatedObjectIds);
        Assert.Empty(input.ExistingEvidence.Entries);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence duplicate = new(
            input.Candidate.Occurrence,
            new DiscoveryEvidence(
                new EvidenceId(input.Candidate.Evidence.EvidenceId.Value),
                input.Target.Identity.Id,
                VisibilityState.Noticed,
                false));

        Assert.Throws<ArgumentException>(() => Select(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            [input.Candidate, duplicate]));
        Assert.Same(input.Candidate.Occurrence, duplicate.Occurrence);
        Assert.NotSame(input.Candidate.Evidence, duplicate.Evidence);
    }

    [Fact]
    public void AcceptsMaximumCountAndRejectsInvalidCollections()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence[] maximum = Enumerable
            .Range(0, CharacterObjectSabotageContaminationSelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Character, input.Target, $"candidate-{index}"))
            .ToArray();
        CharacterObjectSabotageEvidence[] excessive = maximum
            .Append(Candidate(input.Character, input.Target, "excessive-candidate"))
            .ToArray();

        Assert.Same(maximum[0], Select(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            maximum));
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            excessive));
        Assert.Throws<ArgumentException>(() => Select(
            input.Character,
            input.PhysicalAccess,
            input.WorldObjects,
            input.Contamination,
            input.ExistingEvidence,
            [null!]));
    }

    [Fact]
    public void RejectsEveryNullInputAndExposesOnePureOperation()
    {
        SelectionInput input = Input();
        CharacterObjectSabotageEvidence[] candidates = [input.Candidate];

        Assert.Throws<ArgumentNullException>(() => Select(
            null!, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, null!, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, null!, input.Contamination,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, null!,
            input.ExistingEvidence, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            null!, candidates));
        Assert.Throws<ArgumentNullException>(() => Select(
            input.Character, input.PhysicalAccess, input.WorldObjects, input.Contamination,
            input.ExistingEvidence, null!));

        Type selectionType = typeof(CharacterObjectSabotageContaminationSelection);
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
        WorldObjectContaminationSnapshot contamination,
        DiscoveryEvidenceSnapshot evidence,
        IEnumerable<CharacterObjectSabotageEvidence> candidates) =>
        CharacterObjectSabotageContaminationSelection.SelectFirstEligible(
            character,
            access,
            worldObjects,
            contamination,
            evidence,
            candidates);

    private static SelectionInput Input()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = Target("village-water-cask");
        CharacterObjectSabotageEvidence candidate = Candidate(
            character,
            target,
            "bitter-cask-residue");

        return new SelectionInput(
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
        string evidenceId,
        CharacterSabotageActionKind? actionKind = null,
        string requiredCapabilityKey = "assess-local-influence",
        KnowledgeId? requiredKnowledgeId = null,
        CharacterId? actorId = null,
        ObjectId? requiredTargetId = null,
        ObjectId? evidenceTargetId = null)
    {
        CharacterActionAccessRequirement requirement = new(
            requiredTargetId ?? target.Identity.Id,
            requiredCapabilityKey,
            requiredKnowledgeId ?? character.Knowledge.KnowledgeReferences.Single());
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                actorId ?? character.Identity.Id,
                requirement,
                actionKind ?? CharacterSabotageActionKind.Contaminate),
            target);
        DiscoveryEvidence evidence = new(
            new EvidenceId(evidenceId),
            evidenceTargetId ?? target.Identity.Id,
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
        WorldObjectContaminationSnapshot Contamination,
        DiscoveryEvidenceSnapshot ExistingEvidence,
        CharacterObjectSabotageEvidence Candidate);
}
