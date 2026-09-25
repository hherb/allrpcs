namespace Allrpcs.Domain.Characters;

/// <summary>Requires exact recovered Delay history and ordinary access to its target.</summary>
public static class CharacterTargetSabotageDelayInvestigationPolicy
{
    /// <summary>Returns whether one investigator may expose the candidate's authored evidence.</summary>
    public static bool CanInvestigate(
        Character investigator,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageDelayRecoveryResult currentRecovery,
        CharacterTargetSabotageDelayInvestigationCandidate currentCandidate,
        CharacterTargetSabotageDelayInvestigationCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(investigator);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(currentRecovery);
        ArgumentNullException.ThrowIfNull(currentCandidate);
        ArgumentNullException.ThrowIfNull(candidate);

        CharacterTargetSabotageDelayRecoveryResult recovery = candidate.RecoveryResult;
        CharacterTargetSabotageDelayRecoveryCandidate recoveryCandidate = recovery.Candidate;
        CharacterTargetSabotageDelayResult delay = recoveryCandidate.DelayResult;
        CharacterTargetSabotageDelayCandidate delayCandidate = delay.Candidate;

        return ReferenceEquals(candidate, currentCandidate)
            && ReferenceEquals(recovery, currentRecovery)
            && ReferenceEquals(interactionAccess.ActorId, investigator.Identity.Id)
            && interactionAccess.ReachableTargetCharacterIds.Any(targetId =>
                ReferenceEquals(targetId, delayCandidate.Occurrence.Target.Identity.Id))
            && ReferenceEquals(
                delayCandidate.Occurrence.Proposal.ActionKind,
                CharacterSabotageActionKind.Delay)
            && delay.EventLog.Entries.Count > 0
            && ReferenceEquals(delay.EventLog.Entries[^1], delayCandidate.WorldEvent.EventId)
            && ReferenceEquals(delay.WorldTime.CurrentEventId, delayCandidate.WorldEvent.EventId)
            && recovery.EventLog.Entries.Count == delay.EventLog.Entries.Count + 1
            && delay.EventLog.Entries.Select((eventId, index) =>
                ReferenceEquals(eventId, recovery.EventLog.Entries[index])).All(value => value)
            && ReferenceEquals(
                recovery.EventLog.Entries[^1],
                recoveryCandidate.RecoveryEvent.EventId)
            && ReferenceEquals(
                recovery.WorldTime.CurrentEventId,
                recoveryCandidate.RecoveryEvent.EventId);
    }
}
