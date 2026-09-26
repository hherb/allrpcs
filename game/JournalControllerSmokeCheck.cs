using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned safe-journal checks when explicitly enabled.</summary>
public partial class JournalControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_JOURNAL_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        JournalController controller = GetNode<JournalController>("../JournalController");
        CharacterMemoryController unrelatedController =
            GetNode<CharacterMemoryController>("../CharacterMemoryController");
        object unrelatedMemory = unrelatedController.Memory;
        JournalSnapshot incomplete = ComposeObservationsOnlyJournal();

        if (controller.Journal is not null
            || controller.TryInitialize(incomplete)
            || controller.Journal is not null
            || !ThrowsArgumentNull(() => controller.TryInitialize(null!))
            || controller.Journal is not null)
        {
            Fail("Journal controller accepted incomplete or null initialization.");
            return;
        }

        JournalSnapshot complete = ComposeCompleteEmptyJournal();
        JournalSnapshot replacement = ComposeCompleteEmptyJournal();
        if (!controller.TryInitialize(complete)
            || !ReferenceEquals(controller.Journal, complete)
            || controller.TryInitialize(complete)
            || controller.TryInitialize(replacement)
            || !ReferenceEquals(controller.Journal, complete)
            || !ReferenceEquals(unrelatedController.Memory, unrelatedMemory)
            || !HasClosedSurface())
        {
            Fail("Journal controller failed exact one-shot ownership or state isolation.");
            return;
        }

        GD.Print(
            "Journal controller smoke passed: inert startup, complete exact one-shot ownership, "
            + "incomplete, repeated, replacement, and null rejection, closed public surface, "
            + "and unrelated scene-state isolation were correct.");
        GetTree().Quit();
    }

    private static JournalSnapshot ComposeObservationsOnlyJournal()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        return JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
    }

    private static JournalSnapshot ComposeCompleteEmptyJournal()
    {
        JournalSnapshot journal = ComposeObservationsOnlyJournal();
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(journal, []));
        journal = JournalTestimonySnapshotProjection.Compose(journal, testimony);
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot([])));
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        journal = JournalExperimentSnapshotProjection.Compose(journal, experiments);
        JournalOpenQuestionSectionSnapshot openQuestions =
            JournalOpenQuestionSectionSnapshotProjection.Project(
                JournalOpenQuestionSectionProjection.Bind(
                    journal,
                    new OpenInvestigationQuestionSnapshot([])));
        return JournalOpenQuestionSnapshotProjection.Compose(journal, openQuestions);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(JournalController);
        System.Reflection.PropertyInfo property = type.GetProperty(nameof(JournalController.Journal))
            ?? throw new InvalidOperationException("Missing journal property.");
        System.Reflection.MethodInfo method = type.GetMethod(nameof(JournalController.TryInitialize))
            ?? throw new InvalidOperationException("Missing journal initialization method.");
        return property.PropertyType == typeof(JournalSnapshot)
            && property.CanRead
            && property.SetMethod?.IsPrivate == true
            && method.ReturnType == typeof(bool)
            && method.GetParameters() is [{ ParameterType: var parameterType }]
            && parameterType == typeof(JournalSnapshot);
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
