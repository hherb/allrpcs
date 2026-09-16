using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit truth-neutral review of one exact current witness view.</summary>
public partial class TemporaryCharacterDialogueMisquotationEvidenceReviewPanelSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["_Process", "_Ready"];

    /// <inheritdoc />
    public override void _Ready()
    {
        TemporaryCharacterDialogueMisquotationEvidenceSource source =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceSource>(
                "../TemporaryCharacterDialogueMisquotationEvidenceSource");
        CharacterDialogueMisquotationEvidenceViewController view =
            GetNode<CharacterDialogueMisquotationEvidenceViewController>(
                "../CharacterDialogueMisquotationEvidenceViewController");
        TemporaryCharacterDialogueMisquotationEvidenceReviewPanel panel =
            GetNode<TemporaryCharacterDialogueMisquotationEvidenceReviewPanel>(
                "../TemporaryCharacterDialogueMisquotationEvidenceReviewPanel");
        Button review = panel.GetNode<Button>("Review");
        Button close = panel.GetNode<Button>("Close");
        ItemList statements = panel.GetNode<ItemList>("Statements");
        CharacterDialogueMisquotationEvidenceExposureSnapshot initialExposure = new([]);
        CharacterDialogueMisquotationEvidenceExposureSnapshot selectedExposure = new(
            [source.Candidate]);

        bool inert = review.Text == "Review evidence"
            && close.Text == "Close review"
            && !close.Visible
            && !statements.Visible
            && statements.ItemCount == 0;
        review.EmitSignal(BaseButton.SignalName.Pressed);
        inert = inert && !statements.Visible && statements.ItemCount == 0;
        if (!view.TryInitialize(initialExposure, source.WitnessCharacterId))
        {
            Fail("Misquotation-evidence review UI could not initialize an empty witness view.");
            return;
        }

        review.EmitSignal(BaseButton.SignalName.Pressed);
        inert = inert && !statements.Visible && statements.ItemCount == 0;
        if (!view.TryRefresh(initialExposure, selectedExposure)
            || view.View is not { } selectedView)
        {
            Fail("Misquotation-evidence review UI could not arrange an exposed witness view.");
            return;
        }

        review.EmitSignal(BaseButton.SignalName.Pressed);
        string expectedText = $"Source statement: {source.SourceEvent.Utterance.Text}\n"
            + $"Delivered statement: {source.DeliveredEvent.Utterance.Text}";
        bool reviewed = statements.Visible
            && close.Visible
            && statements.ItemCount == 1
            && statements.GetItemText(0) == expectedText
            && ReferenceEquals(selectedView.Entries.Single(), source.Candidate)
            && ReferenceEquals(source.Candidate.Occurrence.Proposal.SourceEvent, source.SourceEvent)
            && ReferenceEquals(source.Candidate.Occurrence.DeliveredEvent, source.DeliveredEvent);

        close.EmitSignal(BaseButton.SignalName.Pressed);
        bool closed = !close.Visible
            && !statements.Visible
            && statements.ItemCount == 0
            && SelectionIsAbsent(panel);
        review.EmitSignal(BaseButton.SignalName.Pressed);

        CharacterDialogueMisquotationEvidenceExposureSnapshot laterExposure = new(
            [source.Candidate]);
        bool staleHidden = view.TryRefresh(selectedExposure, laterExposure);
        panel._Process(0);
        staleHidden = staleHidden
            && !statements.Visible
            && !close.Visible
            && statements.ItemCount == 0
            && view.View is { } laterView
            && !ReferenceEquals(laterView, selectedView);
        review.EmitSignal(BaseButton.SignalName.Pressed);
        staleHidden = staleHidden
            && statements.Visible
            && statements.ItemCount == 1
            && statements.GetItemText(0) == expectedText;

        if (!inert || !reviewed || !closed || !staleHidden || !HasClosedSurface())
        {
            Fail("Misquotation-evidence review UI failed exact truth-neutral rendering.");
            return;
        }

        GD.Print(
            "Misquotation-evidence review UI smoke passed: explicit empty-view inertness, exact "
            + "source and delivered statement rendering, neutral close, stale hiding, unchanged "
            + "source state, and closed observational surface.");
        GetTree().Quit();
    }

    private static bool SelectionIsAbsent(
        TemporaryCharacterDialogueMisquotationEvidenceReviewPanel panel)
    {
        Node parent = panel.GetParent();
        return parent.GetNode<CharacterDialogueMisquotationEvidencePresentationSelectionController>(
                "CharacterDialogueMisquotationEvidencePresentationSelectionController")
            .Selection is null;
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(TemporaryCharacterDialogueMisquotationEvidenceReviewPanel);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return methods.SequenceEqual(ExpectedMethodNames)
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
