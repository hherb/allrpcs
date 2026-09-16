using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks isolated truth-neutral rendering of one exact witness presentation.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidencePresentationPanelSmokeCheck
    : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryPresent", "_Process", "_Ready"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController view =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        CharacterDialogueMisquotationEvidencePresentationSelectionController selections =
            GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "../CharacterDialogueMisquotationEvidencePresentationSelectionController");
        CharacterDialogueMisquotationEvidencePresentationController presentations =
            GetNode<CharacterDialogueMisquotationEvidencePresentationController>(
                "../CharacterDialogueMisquotationEvidencePresentationController");
        TemporaryCharacterDialogueMisquotationEvidencePresentationPanel panel =
            GetNode<TemporaryCharacterDialogueMisquotationEvidencePresentationPanel>(
                "../TemporaryCharacterDialogueMisquotationEvidencePresentationPanel");
        Label sourceText = panel.GetNode<Label>("Panel/SourceText");
        Label deliveredText = panel.GetNode<Label>("Panel/DeliveredText");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);

        bool inert = !panel.Visible
            && panel.Presentation is null
            && sourceText.Text.Length == 0
            && deliveredText.Text.Length == 0;
        if (!view.TryInitialize(initialExposure, source.WitnessCharacterId)
            || !view.TryRefresh(initialExposure, selectedExposure)
            || view.View is not { } selectedView
            || !selections.TryChoose(selectedView, source.Candidate)
            || selections.Selection is not { } selection
            || !presentations.TryPresent(selectedView, selection)
            || presentations.Presentation is not { } presentation)
        {
            Fail("Misquotation-evidence temporary UI could not arrange one presentation.");
            return;
        }

        CharacterDialogueMisquotationEvidencePresentationSelection distinctSelection = new(
            selectedView,
            source.Candidate);
        CharacterDialogueMisquotationEvidencePresentationSnapshot distinctPresentation = new(
            distinctSelection);
        bool rejected = !panel.TryPresent(distinctPresentation)
            && !panel.Visible
            && panel.Presentation is null
            && sourceText.Text.Length == 0
            && deliveredText.Text.Length == 0
            && ThrowsArgumentNull(() => panel.TryPresent(null!));
        bool presented = panel.TryPresent(presentation)
            && panel.Visible
            && ReferenceEquals(panel.Presentation, presentation)
            && sourceText.Text == source.SourceEvent.Utterance.Text
            && deliveredText.Text == source.DeliveredEvent.Utterance.Text
            && !panel.TryPresent(presentation)
            && ReferenceEquals(view.View, selectedView)
            && ReferenceEquals(selections.Selection, selection)
            && ReferenceEquals(presentations.Presentation, presentation)
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent);

        CharacterDialogueMisquotationEvidenceExposureSnapshot laterExposure = new(
            [source.Candidate]);
        bool staleHidden = view.TryRefresh(selectedExposure, laterExposure);
        panel._Process(0);
        staleHidden = staleHidden
            && !panel.Visible
            && panel.Presentation is null
            && sourceText.Text.Length == 0
            && deliveredText.Text.Length == 0
            && !panel.TryPresent(presentation)
            && view.View is { } laterView
            && selections.TryChoose(laterView, source.Candidate)
            && selections.Selection is { } laterSelection
            && presentations.TryPresent(laterView, laterSelection)
            && presentations.Presentation is { } laterPresentation
            && panel.TryPresent(laterPresentation);

        CharacterDialogueMisquotationEvidenceViewSnapshot currentView = view.View!;
        CharacterDialogueMisquotationEvidencePresentationSelection currentSelection =
            selections.Selection!;
        CharacterDialogueMisquotationEvidencePresentationSnapshot currentPresentation =
            presentations.Presentation!;
        bool acknowledged = presentations.TryAcknowledge(
            currentView,
            currentSelection,
            currentPresentation);
        panel._Process(0);
        acknowledged = acknowledged
            && !panel.Visible
            && panel.Presentation is null
            && sourceText.Text.Length == 0
            && deliveredText.Text.Length == 0
            && ReferenceEquals(view.View, currentView)
            && ReferenceEquals(selections.Selection, currentSelection)
            && ReferenceEquals(currentView.Entries.Single(), source.Candidate)
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent);

        if (!inert
            || !rejected
            || !presented
            || !staleHidden
            || !acknowledged
            || !HasNeutralLabels(panel)
            || !HasClosedSurface())
        {
            Fail("Misquotation-evidence temporary UI failed exact truth-neutral rendering.");
            return;
        }

        GD.Print(
            "Misquotation-evidence temporary UI smoke passed: hidden inert startup, exact "
            + "snapshot and text retention, neutral separate labels, stale and distinct "
            + "equivalent rejection, acknowledgement hiding, unchanged source state, null "
            + "guard, and closed surface.");
        GetTree().Quit();
    }

    private static bool HasNeutralLabels(
        TemporaryCharacterDialogueMisquotationEvidencePresentationPanel panel)
    {
        return panel.GetNode<Label>("Panel/SourceLabel").Text == "Source statement"
            && panel.GetNode<Label>("Panel/DeliveredLabel").Text == "Delivered statement";
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidencePresentationPanel);
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

        return methods.SequenceEqual(ExpectedMethodNames)
            && properties.Length == 1
            && properties[0].Name == "Presentation"
            && properties[0].PropertyType
                == typeof(CharacterDialogueMisquotationEvidencePresentationSnapshot)
            && !properties[0].CanWrite;
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
