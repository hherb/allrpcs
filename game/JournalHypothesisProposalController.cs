using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Domain.Investigations;
using Godot;

namespace Allrpcs.Game;

/// <summary>Sends the player's written hypothesis to the interpreter and notes a match in the journal.</summary>
public partial class JournalHypothesisProposalController : Node
{
    internal const string ConsideringStatusText = "Considering that idea...";
    internal const string AddedStatusText = "Noted under Hypotheses. Test it on the patient.";
    internal const string AlreadyNotedStatusText = "That hypothesis is already in your journal.";
    internal const string NoMatchStatusText = "That does not connect to anything you have learned yet.";
    internal const string NothingOpenStatusText = "Every open question already has an answer.";
    internal const string UnavailableStatusText =
        "The journal could not reach the character model. Check Settings and try again.";

    private RepresentativeJournalCompositionCoordinator composition = null!;
    private JournalController journal = null!;
    private JournalSectionPresentationController presentation = null!;
    private JournalSectionPanel panel = null!;
    private LineEdit input = null!;
    private Button propose = null!;
    private Label status = null!;
    private IInvestigationHypothesisInterpreter? interpreter;
    private Task<InvestigationHypothesisRecognition?>? pending;
    private CancellationTokenSource? cancellation;

    /// <summary>Gets whether an interpretation is in flight.</summary>
    public bool Pending => pending is not null;

    /// <summary>Gets the status line last shown under the hypothesis input.</summary>
    public string Status => status.Text;

    /// <inheritdoc />
    public override void _Ready()
    {
        composition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
        journal = GetNode<JournalController>("../JournalController");
        presentation = GetNode<JournalSectionPresentationController>("../JournalSectionPresentationController");
        panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        input = GetNode<LineEdit>("../JournalSectionPanel/Panel/HypothesisInput");
        propose = GetNode<Button>("../JournalSectionPanel/Panel/Propose");
        status = GetNode<Label>("../JournalSectionPanel/Panel/HypothesisStatus");
        input.TextSubmitted += _ => TryPropose();
        propose.Pressed += () => TryPropose();
    }

    /// <summary>Uses the provider-composed (or a check's scripted) interpreter for later proposals.</summary>
    internal void UseInterpreter(IInvestigationHypothesisInterpreter value)
    {
        ArgumentNullException.ThrowIfNull(value);
        interpreter = value;
    }

    /// <summary>Starts interpreting the input text against the open questions; false when nothing starts.</summary>
    public bool TryPropose()
    {
        if (pending is not null || journal.Journal is null)
        {
            return false;
        }

        _ = composition.TryRefresh();
        JournalSnapshot current = journal.Journal;
        IReadOnlyList<FirstPatientHypothesisTest> tests = composition.HypothesisTests;
        InvestigationHypothesisRecognitionRequest? request =
            JournalHypothesisProposalFlow.CreateRequest(current, tests, input.Text);
        if (request is null)
        {
            if (JournalInvestigationTransition.OpenQuestions(current, tests).Count == 0)
            {
                status.Text = NothingOpenStatusText;
            }

            return false;
        }

        if (interpreter is null)
        {
            status.Text = UnavailableStatusText;
            return false;
        }

        cancellation = new CancellationTokenSource();
        pending = InvestigationHypothesisRecognitionController.TryRecognizeAsync(
            interpreter, request, cancellation.Token);
        propose.Disabled = true;
        status.Text = ConsideringStatusText;
        return true;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (pending is not { IsCompleted: true } finished)
        {
            return;
        }

        Clear();
        if (!finished.IsCompletedSuccessfully)
        {
            GD.PrintErr("Hypothesis interpretation failed: " + finished.Exception?.GetBaseException().GetType().Name);
            status.Text = UnavailableStatusText;
            return;
        }

        if (journal.Journal is not { } current)
        {
            return;
        }

        (JournalHypothesisProposalResult result, JournalSnapshot updated) =
            JournalHypothesisProposalFlow.Apply(current, finished.Result);
        status.Text = result switch
        {
            JournalHypothesisProposalResult.Added => AddedStatusText,
            JournalHypothesisProposalResult.AlreadyNoted => AlreadyNotedStatusText,
            _ => NoMatchStatusText,
        };
        if (result != JournalHypothesisProposalResult.Added || !journal.TryReplace(current, updated))
        {
            return;
        }

        input.Text = string.Empty;
        // An earlier dose may already have tested it.
        _ = composition.TryRefresh();
        // Show the new entry at once when the journal is open.
        if (panel.Visible && presentation.TryRefresh() && presentation.Presentation is { } refreshed)
        {
            _ = panel.TryPresent(refreshed);
        }
    }

    /// <summary>Drops a pending interpretation after a load so its stale answer never lands.</summary>
    public void ResetSession()
    {
        cancellation?.Cancel();
        Clear();
        input.Text = string.Empty;
        status.Text = string.Empty;
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        cancellation?.Cancel();
        Clear();
    }

    private void Clear()
    {
        // Observe a faulted task so it never surfaces as unobserved.
        _ = pending?.Exception;
        pending = null;
        cancellation?.Dispose();
        cancellation = null;
        propose.Disabled = false;
    }
}
