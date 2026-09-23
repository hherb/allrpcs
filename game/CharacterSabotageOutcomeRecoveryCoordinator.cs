using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Routes one exact eligible sabotage outcome through an existing recovery transition.</summary>
public partial class CharacterSabotageOutcomeRecoveryCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator concealmentResults = null!;

    /// <summary>Gets the exact generic author request whose concealment was recovered.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact evidence view used by the existing recovery gate.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <summary>Gets the exact recovered concealment snapshot installed by the existing owner.</summary>
    public WorldObjectConcealmentSnapshot? RecoveredConcealment { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        outcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        concealmentState = GetNode<CharacterObjectSabotageConcealmentController>(
            "../CharacterObjectSabotageConcealmentController");
        concealmentResults = GetNode<CharacterObjectSabotageConcealmentPresentationCoordinator>(
            "../CharacterObjectSabotageConcealmentPresentationCoordinator");
    }

    /// <summary>Recovers only the exact eligible concealment branch once.</summary>
    public bool TryRecoverConcealment(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterObjectSabotageEvidence? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || expectedEvidenceView is null
            || AuthorRequest is not null
            || EvidenceView is not null
            || RecoveredConcealment is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.ConcealmentCandidate, expectedCandidate)
            || !ReferenceEquals(eligibility.EvidenceView, expectedEvidenceView)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Conceal)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || results.ConcealmentResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate, expectedCandidate)
            || !ReferenceEquals(concealmentResults.AcknowledgedResult, result)
            || outcomes.Concealment is not { } concealment
            || outcomes.ConcealmentEvidence is not { } evidence
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(concealmentResults.AcknowledgedConcealment, concealment)
            || !ReferenceEquals(concealmentResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(concealmentState.Concealment, concealment)
            || !ReferenceEquals(concealmentState.Evidence, evidence)
            || !concealmentState.TryRecover(
                concealment,
                evidence,
                expectedEvidenceView,
                expectedCandidate))
        {
            return false;
        }

        WorldObjectConcealmentSnapshot recovered = concealmentState.Concealment
            ?? throw new InvalidOperationException(
                "Successful concealment recovery must install an exact snapshot.");
        if (!ReferenceEquals(concealmentState.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Concealment recovery must preserve authoritative evidence.");
        }

        AuthorRequest = expectedAuthorRequest;
        EvidenceView = expectedEvidenceView;
        RecoveredConcealment = recovered;
        return true;
    }
}
