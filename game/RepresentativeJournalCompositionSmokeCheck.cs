using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit representative safe-journal composition when enabled.</summary>
public partial class RepresentativeJournalCompositionSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_REPRESENTATIVE_JOURNAL_COMPOSITION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        RepresentativeJournalCompositionCoordinator coordinator =
            GetNode<RepresentativeJournalCompositionCoordinator>(
                "../RepresentativeJournalCompositionCoordinator");
        JournalController journalController = GetNode<JournalController>("../JournalController");
        FocusedSearchActivityController focusedSearch =
            GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        WorldObjectController worldObjects = GetNode<WorldObjectController>(
            "../WorldObjectController");
        DialogueEventController dialogue = GetNode<DialogueEventController>(
            "../DialogueEventController");
        FirstPatientStateController firstPatient = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        TemporaryFirstPatientTreatmentContextSource treatment =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource");
        Label focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");

        CharacterPerceptionSnapshot perception = focusedSearch.Perception
            ?? throw new InvalidOperationException("Missing initialized Seeker perception.");
        WorldObjectSnapshot objects = worldObjects.WorldObjects
            ?? throw new InvalidOperationException("Missing initialized world objects.");
        DialogueEvent? lastDialogue = dialogue.LastOccurredEvent;
        ExperimentJournalSnapshot experiments = firstPatient.Experiments
            ?? throw new InvalidOperationException("Missing initialized experiment journal.");
        FirstPatientTreatmentContext context = treatment.Context;
        string focusText = focusStatus.Text;

        if (journalController.Journal is not null
            || !coordinator.TryComposeAndInitialize())
        {
            Fail("Representative journal did not remain inert before explicit composition.");
            return;
        }

        JournalSnapshot journal = journalController.Journal
            ?? throw new InvalidOperationException("Representative journal was not installed.");
        int identifiedCount = perception.Entries.Count(
            entry => ReferenceEquals(entry.VisibilityState, VisibilityState.Identified));
        int heardCount = lastDialogue is not null
            && lastDialogue.Audience.ListenerCharacterIds.Contains(perception.CharacterId)
                ? 1
                : 0;
        IReadOnlyList<InvestigationConclusionEvidencePlan> plans =
            context.Scenario.Investigation.Plans.Plans;
        int openQuestionCount = plans.Count(
            plan => !context.Conclusions.Conclusions.Any(
                conclusion => ReferenceEquals(conclusion, plan.Conclusion)));

        if (!ReferenceEquals(journal.SeekerId, perception.CharacterId)
            || journal.Observations.Entries.Count != identifiedCount
            || journal.Testimony?.Entries.Count != heardCount
            || journal.Hypotheses?.Entries.Count != plans.Count
            || journal.Experiments?.Entries.Count != experiments.Records.Count
            || journal.OpenQuestions?.Entries.Count != openQuestionCount
            || journal.PendingSections.Count != 0
            || coordinator.TryComposeAndInitialize()
            || !ReferenceEquals(journalController.Journal, journal)
            || !ReferenceEquals(focusedSearch.Perception, perception)
            || !ReferenceEquals(worldObjects.WorldObjects, objects)
            || !ReferenceEquals(dialogue.LastOccurredEvent, lastDialogue)
            || !ReferenceEquals(firstPatient.Experiments, experiments)
            || !ReferenceEquals(treatment.Context, context)
            || !string.Equals(focusStatus.Text, focusText, StringComparison.Ordinal)
            || !HasClosedSurface())
        {
            Fail("Representative journal composition changed sources or produced unsafe content.");
            return;
        }

        GD.Print(
            "Representative journal composition smoke passed: explicit scene-source composition "
            + "installed one complete safe journal, retained exact source provenance, copied only "
            + "safe authored content, rejected repetition, and left ordinary UI and sources intact.");
        GetTree().Quit();
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(RepresentativeJournalCompositionCoordinator);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);
        return methods.Length == 2
            && methods.Any(method => method.Name == nameof(Node._Ready))
            && methods.Any(method =>
                method.Name == nameof(
                    RepresentativeJournalCompositionCoordinator.TryComposeAndInitialize)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 0);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
