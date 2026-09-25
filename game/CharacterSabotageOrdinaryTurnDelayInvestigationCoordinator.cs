using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exposes authored evidence from only the exact released ordinary-play Delay history.</summary>
public partial class CharacterSabotageOrdinaryTurnDelayInvestigationCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recovery = null!;
    private CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator resolution = null!;
    private CharacterSabotageOrdinaryTurnBoundaryCoordinator boundary = null!;
    private CharacterTurnActivityController turns = null!;
    private TemporaryCharacterTargetSabotageInvestigationSource source = null!;

    /// <summary>Gets the exact investigation candidate retained after successful exposure.</summary>
    public CharacterTargetSabotageDelayInvestigationCandidate? Candidate { get; private set; }

    /// <summary>Gets the exact investigator-owned authored evidence view.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        recovery = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator");
        resolution = GetNode<CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator>(
            "../CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator");
        boundary = GetNode<CharacterSabotageOrdinaryTurnBoundaryCoordinator>(
            "../CharacterSabotageOrdinaryTurnBoundaryCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        source = GetNode<TemporaryCharacterTargetSabotageInvestigationSource>(
            "../TemporaryCharacterTargetSabotageInvestigationSource");
    }

    /// <summary>Investigates only the exact recovered Delay after its shared slot was released.</summary>
    public bool TryInvestigate(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayRecoveryResult? expectedRecovery,
        Character? expectedInvestigator,
        CharacterInteractionAccessSnapshot? expectedAccess,
        EvidenceId? expectedEvidenceId)
    {
        if (Candidate is not null
            || EvidenceView is not null
            || expectedReservation is null
            || expectedRecovery is null
            || expectedInvestigator is null
            || expectedAccess is null
            || expectedEvidenceId is null
            || turns.SabotageReservation is not null
            || turns.ReservedSabotageTurn is not null
            || !ReferenceEquals(boundary.Reservation, expectedReservation)
            || !ReferenceEquals(resolution.Reservation, expectedReservation)
            || !ReferenceEquals(resolution.DelayRecovery, expectedRecovery)
            || !ReferenceEquals(recovery.Reservation, expectedReservation)
            || !ReferenceEquals(recovery.AppliedResult, expectedRecovery)
            || !ReferenceEquals(source.Investigator, expectedInvestigator)
            || !ReferenceEquals(source.InteractionAccess, expectedAccess)
            || !ReferenceEquals(source.DelayEvidenceId, expectedEvidenceId))
        {
            return false;
        }

        CharacterTargetSabotageDelayInvestigationCandidate candidate = new(
            expectedRecovery,
            expectedEvidenceId);
        if (!CharacterTargetSabotageDelayInvestigationPolicy.CanInvestigate(
                expectedInvestigator,
                expectedAccess,
                expectedRecovery,
                candidate,
                candidate))
        {
            return false;
        }

        CharacterEvidenceViewSnapshot view =
            CharacterTargetSabotageDelayInvestigationTransition.Investigate(
                expectedInvestigator,
                expectedAccess,
                expectedRecovery,
                candidate,
                candidate);
        Candidate = candidate;
        EvidenceView = view;
        return true;
    }
}
