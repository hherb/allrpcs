using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Resolves the exact sabotage-owning character turn after admitted route completion.</summary>
public partial class CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnAuthoringCoordinator authoring = null!;
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator admission = null!;
    private CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact awaiting activity replaced by successful resolution.</summary>
    public CharacterTurnActivity? OriginalTurn { get; private set; }

    /// <summary>Gets the exact terminal activity installed by successful resolution.</summary>
    public CharacterTurnActivity? ResolvedTurn { get; private set; }

    /// <summary>Gets the exact admitted sabotage completion bound to resolution.</summary>
    public CharacterSabotageTurnCompletion? Completion { get; private set; }

    /// <summary>Gets the exact recovered Delay result current at successful resolution.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? DelayRecovery { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authoring = GetNode<CharacterSabotageOrdinaryTurnAuthoringCoordinator>(
            "../CharacterSabotageOrdinaryTurnAuthoringCoordinator");
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        admission = GetNode<CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator>(
            "../CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator");
        terminalReset = GetNode<CharacterSabotageOrdinaryTurnTerminalResetCoordinator>(
            "../CharacterSabotageOrdinaryTurnTerminalResetCoordinator");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Installs one response-free terminal successor for the exact admitted Delay turn.</summary>
    public bool TryResolve(
        CharacterTurnActivity? expectedTurn,
        CharacterSabotageTurnCompletion? expectedCompletion,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery)
    {
        if (OriginalTurn is not null
            || ResolvedTurn is not null
            || Completion is not null
            || DelayRecovery is not null
            || expectedTurn is null
            || expectedCompletion is null
            || expectedDelayRecovery is null
            || !ReferenceEquals(turns.Activity, expectedTurn)
            || !CharacterTurnActionResolutionPolicy.CanResolve(expectedTurn)
            || !ReferenceEquals(admission.Admission, expectedCompletion)
            || !ReferenceEquals(expectedCompletion.Actor, authoring.Actor)
            || !ReferenceEquals(authoring.Turn, expectedTurn)
            || !ReferenceEquals(expectedCompletion.ActionKind, CharacterSabotageActionKind.Delay)
            || !binding.IsRetainedAcknowledgedBinding(
                binding.AcknowledgedResult,
                expectedCompletion.AuthorRequest,
                binding.SelectedCandidateBinding)
            || !ReferenceEquals(
                binding.SelectedCandidateBinding?.ActionKind,
                expectedCompletion.ActionKind)
            || !ReferenceEquals(terminalReset.ResetAuthorRequest, expectedCompletion.AuthorRequest)
            || !ReferenceEquals(terminalReset.ResetActionKind, expectedCompletion.ActionKind)
            || !ReferenceEquals(terminalReset.DelayRecovery, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.LatestAppliedResult, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.EventLog, expectedDelayRecovery.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, expectedDelayRecovery.WorldTime))
        {
            return false;
        }

        CharacterTurnActivity resolved =
            CharacterTurnActionResolutionTransition.Resolve(expectedTurn);
        if (!turns.TryInstallActionResolution(expectedTurn, resolved)
            || !ReferenceEquals(turns.Activity, resolved))
        {
            throw new InvalidOperationException(
                "A preflighted sabotage-owning turn must install its exact terminal successor.");
        }

        OriginalTurn = expectedTurn;
        ResolvedTurn = resolved;
        Completion = expectedCompletion;
        DelayRecovery = expectedDelayRecovery;
        return true;
    }
}
