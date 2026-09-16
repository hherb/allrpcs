using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit orchestration of one exact witness presentation choice.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidencePresentationFlowSmokeCheck
    : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryChoose", "_Ready"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController viewController =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController selectionController =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidencePresentationController presentationController =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController flow =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);
        CharacterDialogueMisquotationEvidenceViewSnapshot unownedView = new(
            source.WitnessCharacterId,
            []);

        bool inert = selectionController.Selection is null
            && presentationController.Presentation is null
            && !flow.TryChoose(unownedView, null)
            && selectionController.Selection is null
            && presentationController.Presentation is null;
        if (!viewController.TryInitialize(initialExposure, source.WitnessCharacterId)
            || viewController.View is not { } initialView
            || !flow.TryChoose(initialView, null)
            || selectionController.Selection is not { } abstention
            || !ReferenceEquals(abstention.WitnessView, initialView)
            || abstention.SelectedEvidence is not null
            || presentationController.Presentation is not null
            || flow.TryChoose(initialView, null))
        {
            Fail("Misquotation-evidence presentation flow did not retain one abstention.");
            return;
        }

        if (!viewController.TryRefresh(initialExposure, selectedExposure)
            || viewController.View is not { } selectedView)
        {
            Fail("Misquotation-evidence presentation flow could not arrange a fresh view.");
            return;
        }

        CharacterDialogueMisquotationEvidence replacement = new(
            source.Candidate.Occurrence,
            new CharacterId(source.WitnessCharacterId.Value));
        CharacterDialogueMisquotationEvidenceViewSnapshot distinctEquivalentView = new(
            source.WitnessCharacterId,
            [source.Candidate]);
        bool rejected = !flow.TryChoose(initialView, null)
            && !flow.TryChoose(distinctEquivalentView, source.Candidate)
            && !flow.TryChoose(selectedView, replacement)
            && selectionController.Selection is null
            && presentationController.Presentation is null
            && ThrowsArgumentNull(() => flow.TryChoose(null!, source.Candidate));
        bool selected = flow.TryChoose(selectedView, source.Candidate)
            && selectionController.Selection is { } selection
            && presentationController.Presentation is { } presentation
            && ReferenceEquals(selection.WitnessView, selectedView)
            && ReferenceEquals(selection.SelectedEvidence, source.Candidate)
            && ReferenceEquals(presentation.Selection, selection)
            && ReferenceEquals(presentation.WitnessCharacterId, source.WitnessCharacterId)
            && ReferenceEquals(presentation.SourceEvent, source.SourceEvent)
            && ReferenceEquals(presentation.DeliveredEvent, source.DeliveredEvent)
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent)
            && !flow.TryChoose(selectedView, source.Candidate);

        if (!inert || !rejected || !selected || !HasClosedSurface())
        {
            Fail("Misquotation-evidence presentation flow failed exact-view orchestration.");
            return;
        }

        GD.Print(
            "Misquotation-evidence presentation flow smoke passed: inert startup, explicit "
            + "selection and abstention, stale, value-equal, non-member, and repeated rejection, "
            + "exact nested identity retention, unchanged source state, null guards, and closed "
            + "surface.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methods.SequenceEqual(ExpectedMethodNames) && properties.Length == 0;
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
