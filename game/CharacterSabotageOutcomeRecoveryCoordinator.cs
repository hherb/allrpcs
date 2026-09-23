using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Routes one exact eligible sabotage outcome through its existing follow-up.</summary>
public partial class CharacterSabotageOutcomeRecoveryCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterObjectSabotageConcealmentController concealmentState = null!;
    private CharacterObjectSabotageConcealmentPresentationCoordinator concealmentResults = null!;
    private TemporaryCharacterObjectSabotageMoveSource moveSource = null!;
    private CharacterObjectSabotageMoveController moveState = null!;
    private CharacterObjectSabotageMovePresentationCoordinator moveResults = null!;
    private CharacterObjectSabotageSubstitutionController substitutionState = null!;
    private CharacterObjectSabotageSubstitutionPresentationCoordinator substitutionResults = null!;
    private CharacterObjectSabotageContaminationController contaminationState = null!;
    private CharacterObjectSabotageContaminationPresentationCoordinator contaminationResults = null!;
    private CharacterDialogueMisquotationEvidenceExposureController misquotationState = null!;
    private CharacterDialogueMisquotationEvidenceExposureCoordinator misquotationResults = null!;
    private CharacterDialogueMisquotationEvidenceViewController misquotationViewState = null!;
    private CharacterDialogueMisquotationEvidencePresentationSelectionController
        misquotationSelections = null!;
    private CharacterDialogueMisquotationEvidencePresentationController
        misquotationPresentations = null!;
    private TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController
        misquotationPresentationFlow = null!;

    /// <summary>Gets the exact generic author request whose concealment was recovered.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact evidence view used by the existing recovery gate.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <summary>Gets the exact recovered concealment snapshot installed by the existing owner.</summary>
    public WorldObjectConcealmentSnapshot? RecoveredConcealment { get; private set; }

    /// <summary>Gets the exact recovered placement snapshot installed by the existing owner.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <summary>Gets the exact recovered substitution snapshot installed by the existing owner.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot? RecoveredSubstitutions { get; private set; }

    /// <summary>Gets the exact recovered contamination snapshot installed by the existing owner.</summary>
    public WorldObjectContaminationSnapshot? RecoveredContamination { get; private set; }

    /// <summary>Gets the exact eligible exposure routed through its witness view.</summary>
    public CharacterDialogueMisquotationEvidenceExposureSnapshot? RoutedMisquotationExposure
    {
        get;
        private set;
    }

    /// <summary>Gets the exact current witness view used by the misquotation route.</summary>
    public CharacterDialogueMisquotationEvidenceViewSnapshot? RoutedMisquotationView
    {
        get;
        private set;
    }

    /// <summary>Gets the exact truth-neutral selection created by the existing view route.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSelection? MisquotationSelection
    {
        get;
        private set;
    }

    /// <summary>Gets the exact truth-neutral presentation created by the existing view route.</summary>
    public CharacterDialogueMisquotationEvidencePresentationSnapshot? MisquotationPresentation
    {
        get;
        private set;
    }

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
        moveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>(
            "../TemporaryCharacterObjectSabotageMoveSource");
        moveState = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
        moveResults = GetNode<CharacterObjectSabotageMovePresentationCoordinator>(
            "../CharacterObjectSabotageMovePresentationCoordinator");
        substitutionState = GetNode<CharacterObjectSabotageSubstitutionController>(
            "../CharacterObjectSabotageSubstitutionController");
        substitutionResults = GetNode<CharacterObjectSabotageSubstitutionPresentationCoordinator>(
            "../CharacterObjectSabotageSubstitutionPresentationCoordinator");
        contaminationState = GetNode<CharacterObjectSabotageContaminationController>(
            "../CharacterObjectSabotageContaminationController");
        contaminationResults = GetNode<CharacterObjectSabotageContaminationPresentationCoordinator>(
            "../CharacterObjectSabotageContaminationPresentationCoordinator");
        misquotationState = GetNode<CharacterDialogueMisquotationEvidenceExposureController>(
            "../CharacterDialogueMisquotationEvidenceExposureController");
        misquotationResults = GetNode<CharacterDialogueMisquotationEvidenceExposureCoordinator>(
            "../CharacterDialogueMisquotationEvidenceExposureCoordinator");
        misquotationViewState = GetNode<CharacterDialogueMisquotationEvidenceViewController>(
            "../CharacterDialogueMisquotationEvidenceViewController");
        misquotationSelections =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        misquotationPresentations =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        misquotationPresentationFlow =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController");
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

    /// <summary>Recovers only the exact eligible move branch once.</summary>
    public bool TryRecoverMove(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || expectedEvidenceView is null
            || AuthorRequest is not null
            || EvidenceView is not null
            || RecoveredConcealment is not null
            || RecoveredPlacements is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.MoveCandidate, expectedCandidate)
            || !ReferenceEquals(eligibility.EvidenceView, expectedEvidenceView)
            || !ReferenceEquals(moveSource.RecoveryCandidate, expectedCandidate)
            || !ReferenceEquals(
                moveSource.RecoveryCandidate.SourcePlacement,
                moveSource.Placements.Entries.Single())
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Move)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || results.MoveResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate, expectedCandidate.Candidate)
            || !ReferenceEquals(moveResults.AcknowledgedResult, result)
            || outcomes.Placements is not { } placements
            || outcomes.MoveEvidence is not { } evidence
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(moveResults.AcknowledgedPlacements, placements)
            || !ReferenceEquals(moveResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(moveState.Placements, placements)
            || !ReferenceEquals(moveState.Evidence, evidence)
            || !moveState.TryRecover(
                placements,
                evidence,
                expectedEvidenceView,
                expectedCandidate))
        {
            return false;
        }

        WorldObjectPlacementSnapshot recovered = moveState.Placements
            ?? throw new InvalidOperationException(
                "Successful move recovery must install an exact snapshot.");
        if (!ReferenceEquals(moveState.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Move recovery must preserve authoritative evidence.");
        }

        AuthorRequest = expectedAuthorRequest;
        EvidenceView = expectedEvidenceView;
        RecoveredPlacements = recovered;
        return true;
    }

    /// <summary>Recovers only the exact eligible substitution branch once.</summary>
    public bool TryRecoverSubstitution(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterObjectSabotageSubstitutionCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || expectedEvidenceView is null
            || AuthorRequest is not null
            || EvidenceView is not null
            || RecoveredConcealment is not null
            || RecoveredPlacements is not null
            || RecoveredSubstitutions is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.SubstitutionCandidate, expectedCandidate)
            || !ReferenceEquals(eligibility.EvidenceView, expectedEvidenceView)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Substitute)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || results.SubstitutionResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate, expectedCandidate)
            || !ReferenceEquals(substitutionResults.AcknowledgedResult, result)
            || outcomes.Substitutions is not { } substitutions
            || outcomes.SubstitutionEvidence is not { } evidence
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(
                substitutionResults.AcknowledgedSubstitutions,
                substitutions)
            || !ReferenceEquals(substitutionResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(substitutionState.Substitutions, substitutions)
            || !ReferenceEquals(substitutionState.Evidence, evidence)
            || !substitutionState.TryRecover(
                substitutions,
                evidence,
                expectedEvidenceView,
                expectedCandidate))
        {
            return false;
        }

        CharacterObjectSabotageSubstitutionSnapshot recovered =
            substitutionState.Substitutions
            ?? throw new InvalidOperationException(
                "Successful substitution recovery must install an exact snapshot.");
        if (!ReferenceEquals(substitutionState.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Substitution recovery must preserve authoritative evidence.");
        }

        AuthorRequest = expectedAuthorRequest;
        EvidenceView = expectedEvidenceView;
        RecoveredSubstitutions = recovered;
        return true;
    }

    /// <summary>Recovers only the exact eligible contamination branch once.</summary>
    public bool TryRecoverContamination(
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
            || RecoveredPlacements is not null
            || RecoveredSubstitutions is not null
            || RecoveredContamination is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.ContaminationCandidate, expectedCandidate)
            || !ReferenceEquals(eligibility.EvidenceView, expectedEvidenceView)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Contaminate)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || results.ContaminationResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate, expectedCandidate)
            || !ReferenceEquals(contaminationResults.AcknowledgedResult, result)
            || outcomes.Contamination is not { } contamination
            || outcomes.ContaminationEvidence is not { } evidence
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(
                contaminationResults.AcknowledgedContamination,
                contamination)
            || !ReferenceEquals(contaminationResults.AcknowledgedEvidence, evidence)
            || !ReferenceEquals(contaminationState.Contamination, contamination)
            || !ReferenceEquals(contaminationState.Evidence, evidence)
            || !contaminationState.TryRecover(
                contamination,
                evidence,
                expectedEvidenceView,
                expectedCandidate))
        {
            return false;
        }

        WorldObjectContaminationSnapshot recovered = contaminationState.Contamination
            ?? throw new InvalidOperationException(
                "Successful contamination recovery must install an exact snapshot.");
        if (!ReferenceEquals(contaminationState.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Contamination recovery must preserve authoritative evidence.");
        }

        AuthorRequest = expectedAuthorRequest;
        EvidenceView = expectedEvidenceView;
        RecoveredContamination = recovered;
        return true;
    }

    /// <summary>Routes only the exact eligible misquotation branch through its witness view once.</summary>
    public bool TryRouteMisquotation(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterDialogueMisquotationEvidenceExposureSnapshot? expectedExposure,
        CharacterDialogueMisquotationEvidenceViewSnapshot? expectedView)
    {
        if (expectedAuthorRequest is null
            || expectedExposure is null
            || expectedView is null
            || AuthorRequest is not null
            || EvidenceView is not null
            || RecoveredConcealment is not null
            || RecoveredPlacements is not null
            || RecoveredSubstitutions is not null
            || RecoveredContamination is not null
            || RoutedMisquotationExposure is not null
            || RoutedMisquotationView is not null
            || MisquotationSelection is not null
            || MisquotationPresentation is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.MisquotationExposure, expectedExposure)
            || !ReferenceEquals(eligibility.MisquotationView, expectedView)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Misquote)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || results.MisquotationResult is not { SelectedEvidence: { } candidate } result
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterDialogueMisquotationEvidence> selected
            || !ReferenceEquals(selected.Request, expectedAuthorRequest)
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(misquotationResults.AcknowledgedResult, result)
            || !ReferenceEquals(misquotationResults.AcknowledgedExposure, expectedExposure)
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(outcomes.MisquotationExposure, expectedExposure)
            || !ReferenceEquals(misquotationState.Exposure, expectedExposure)
            || !ReferenceEquals(misquotationViewState.SourceExposure, expectedExposure)
            || !ReferenceEquals(misquotationViewState.View, expectedView)
            || !expectedView.Entries.Any(entry => ReferenceEquals(entry, candidate))
            || !misquotationPresentationFlow.TryChoose(expectedView, candidate)
            || misquotationSelections.Selection is not { } selection
            || !ReferenceEquals(selection.WitnessView, expectedView)
            || !ReferenceEquals(selection.SelectedEvidence, candidate)
            || misquotationPresentations.Presentation is not { } presentation
            || !ReferenceEquals(presentation.Selection, selection))
        {
            return false;
        }

        AuthorRequest = expectedAuthorRequest;
        RoutedMisquotationExposure = expectedExposure;
        RoutedMisquotationView = expectedView;
        MisquotationSelection = selection;
        MisquotationPresentation = presentation;
        return true;
    }
}
