using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact completed ordinary-turn Delay recovery.</summary>
public partial class CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator start = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator request = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact generic author request retained at recovery completion.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact recovery request retained at completion.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <summary>Gets the exact completed recovery decision result.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationResult? Result { get; private set; }

    /// <summary>Gets the exact applied Delay recovery result.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? AppliedResult { get; private set; }

    /// <summary>Gets the exact authoritative event log produced by recovery.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the exact authoritative world time produced by recovery.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        start = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator");
        request = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
    }

    /// <summary>Retains and acknowledges only the exact completed ordinary-turn recovery.</summary>
    public bool TryRetain(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedAuthorRequest is null
            || expectedRequest is null
            || AuthorRequest is not null
            || Request is not null
            || Result is not null
            || AppliedResult is not null
            || EventLog is not null
            || WorldTime is not null
            || !ReferenceEquals(request.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(request.Request, expectedRequest)
            || !ReferenceEquals(start.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(start.Request, expectedRequest)
            || !ReferenceEquals(recovery.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(recovery.Request, expectedRequest)
            || !ReferenceEquals(recovery.StartedRequest, expectedRequest))
        {
            return false;
        }

        if (!recovery.TryComplete(expectedAuthorRequest, expectedRequest)
            || recovery.CompletedResult is not { } result
            || recovery.RecoveredResult is not { } appliedResult
            || !ReferenceEquals(recovery.RecoveredEventLog, appliedResult.EventLog)
            || !ReferenceEquals(recovery.RecoveredWorldTime, appliedResult.WorldTime))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay recovery must complete and acknowledge once.");
        }

        AuthorRequest = expectedAuthorRequest;
        Request = expectedRequest;
        Result = result;
        AppliedResult = appliedResult;
        EventLog = appliedResult.EventLog;
        WorldTime = appliedResult.WorldTime;
        return true;
    }
}
