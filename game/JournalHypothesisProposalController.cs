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
    internal const string AlreadyAnsweredStatusText = "That question already has an answer.";
    internal const string EmptyStatusText = "Write a hypothesis first.";
    internal const string UnusableStatusText =
        "The character model gave an answer the journal could not use. Try rephrasing or try again.";
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
            status.Text = JournalInvestigationTransition.OpenQuestions(current, tests).Count == 0
                ? NothingOpenStatusText
                : EmptyStatusText;
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
            Exception? error = finished.Exception?.GetBaseException();
            // Type and status only: the transport keeps provider payloads out of exceptions.
            GD.PrintErr("Hypothesis interpretation failed: " + (error is null ? "canceled" : error.GetType().Name)
                + (error is HttpRequestException { StatusCode: { } code } ? " " + (int)code : string.Empty));
            status.Text = error is InvalidDataException ? UnusableStatusText : UnavailableStatusText;
            return;
        }

        // Current outcomes decide whether the matched question is still open.
        _ = composition.TryRefresh();
        if (journal.Journal is not { } current)
        {
            status.Text = string.Empty;
            return;
        }

        (JournalHypothesisProposalResult result, JournalSnapshot updated) =
            JournalHypothesisProposalFlow.Apply(current, composition.HypothesisTests, finished.Result);
        if (result == JournalHypothesisProposalResult.Added && !journal.TryReplace(current, updated))
        {
            GD.PrintErr("Hypothesis proposal found the journal already replaced.");
            status.Text = string.Empty;
            return;
        }

        status.Text = result switch
        {
            JournalHypothesisProposalResult.Added => AddedStatusText,
            JournalHypothesisProposalResult.AlreadyNoted => AlreadyNotedStatusText,
            JournalHypothesisProposalResult.AlreadyAnswered => AlreadyAnsweredStatusText,
            _ => NoMatchStatusText,
        };
        if (result != JournalHypothesisProposalResult.Added)
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
        // Observe a fault, now or after a dropped task ends, so it never surfaces as unobserved.
        _ = pending?.ContinueWith(
            static task => task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        pending = null;
        cancellation?.Dispose();
        cancellation = null;
        propose.Disabled = false;
    }
}
