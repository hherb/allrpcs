using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageDelayInvestigationTests
{
    [Fact]
    public void ExactReachableRecoveredDelayProducesOnlyItsAuthoredEvidence()
    {
        InvestigationInput input = CreateInput();

        Assert.True(CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            input.Recovery,
            input.Candidate,
            input.Candidate));

        CharacterEvidenceViewSnapshot view =
            CharacterTargetSabotageDelayInvestigationTransition.Investigate(
                input.Investigator,
                input.Access,
                input.Recovery,
                input.Candidate,
                input.Candidate);

        Assert.Same(input.Investigator.Identity.Id, view.CharacterId);
        Assert.Collection(view.EvidenceIds, evidence => Assert.Same(input.EvidenceId, evidence));
    }

    [Fact]
    public void RejectsInaccessibleRebuiltStaleAndIncompleteHistory()
    {
        InvestigationInput input = CreateInput();
        CharacterInteractionAccessSnapshot inaccessible = new(
            input.Investigator.Identity.Id,
            []);
        CharacterTargetSabotageDelayRecoveryResult rebuiltRecovery = new(
            input.Recovery.Candidate,
            input.Recovery.EventLog,
            input.Recovery.WorldTime);
        CharacterTargetSabotageDelayInvestigationCandidate rebuiltCandidate = new(
            input.Recovery,
            input.EvidenceId);
        CharacterTargetSabotageDelayRecoveryResult incomplete = new(
            input.Recovery.Candidate,
            input.Recovery.Candidate.DelayResult.EventLog,
            input.Recovery.Candidate.DelayResult.WorldTime);
        CharacterTargetSabotageDelayInvestigationCandidate incompleteCandidate = new(
            incomplete,
            input.EvidenceId);

        Assert.False(CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
            input.Investigator,
            inaccessible,
            input.Recovery,
            input.Candidate,
            input.Candidate));
        Assert.False(CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            rebuiltRecovery,
            input.Candidate,
            input.Candidate));
        Assert.False(CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            input.Recovery,
            input.Candidate,
            rebuiltCandidate));
        Assert.False(CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            incomplete,
            incompleteCandidate,
            incompleteCandidate));
    }

    [Fact]
    public void ContractsRejectNullAndFailedInvestigationWithoutPartialOutput()
    {
        InvestigationInput input = CreateInput();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayInvestigationCandidate(null!, input.EvidenceId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageDelayInvestigationCandidate(input.Recovery, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
                null!, input.Access, input.Recovery, input.Candidate, input.Candidate));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageDelayInvestigationTransition.Investigate(
                input.Investigator,
                new CharacterInteractionAccessSnapshot(input.Investigator.Identity.Id, []),
                input.Recovery,
                input.Candidate,
                input.Candidate));
    }

    private static InvestigationInput CreateInput()
    {
        Character investigator = RepresentativeHermitCharacterSource.Create()[0];
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Delay),
            target);
        WorldEvent delayEvent = new(
            new WorldEventId("investigated-delay"),
            WorldEventTimeEffect.Advance);
        WorldEvent recoveryEvent = new(
            new WorldEventId("investigated-delay-recovery"),
            WorldEventTimeEffect.Advance);
        CharacterTargetSabotageDelayResult delay = new(
            new CharacterTargetSabotageDelayCandidate(occurrence, delayEvent),
            new WorldEventLogSnapshot([delayEvent.EventId]),
            new WorldTimeSnapshot(delayEvent.EventId));
        CharacterTargetSabotageDelayRecoveryCandidate recoveryCandidate = new(
            delay,
            recoveryEvent);
        CharacterTargetSabotageDelayRecoveryResult recovery = new(
            recoveryCandidate,
            new WorldEventLogSnapshot([delayEvent.EventId, recoveryEvent.EventId]),
            new WorldTimeSnapshot(recoveryEvent.EventId));
        EvidenceId evidenceId = new("sage-delay-history");

        return new InvestigationInput(
            investigator,
            new CharacterInteractionAccessSnapshot(
                investigator.Identity.Id,
                [target.Identity.Id]),
            recovery,
            evidenceId,
            new CharacterTargetSabotageDelayInvestigationCandidate(recovery, evidenceId));
    }

    private sealed record InvestigationInput(
        Character Investigator,
        CharacterInteractionAccessSnapshot Access,
        CharacterTargetSabotageDelayRecoveryResult Recovery,
        EvidenceId EvidenceId,
        CharacterTargetSabotageDelayInvestigationCandidate Candidate);
}
