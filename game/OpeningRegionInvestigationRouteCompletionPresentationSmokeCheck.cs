using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the ordinary exact route-completion acknowledgement flow when enabled.</summary>
public partial class OpeningRegionInvestigationRouteCompletionPresentationSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_INVESTIGATION_ROUTE_COMPLETION_PRESENTATION_CHECK";
    private const string FailedMode = "failed";
    private const string RestoredMode = "restored";
    private const string SuccessMode = "success";

    private OpeningRegionInvestigationRouteCompletion? firstCompletion;
    private OpeningRegionInvestigationRouteCompletion? secondCompletion;
    private OpeningRegionInvestigationRouteCompletionCoordinator completions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator consumption = null!;
    private OpeningRegionInvestigationRouteCompletionPresentationCoordinator coordinator = null!;
    private OpeningRegionInvestigationRouteCompletionPanel panel = null!;
    private OpeningRegionInvestigationRouteProgressController progress = null!;
    private CharacterPerceptionSnapshot? retainedPerception;
    private JournalSnapshot? retainedJournal;
    private SeekerMapSnapshot? retainedMap;
    private WorldObjectSnapshot? retainedWorldObjects;
    private string mode = string.Empty;
    private int stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        mode = OS.GetEnvironment(CheckEnvironmentVariable);
        SetProcess(mode is SuccessMode or FailedMode or RestoredMode);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);
        if (stage == 0)
        {
            ArrangeAcceptedCompletions();
            stage = 1;
            SetProcess(true);
            return;
        }

        VerifyMode();
    }

    private void ArrangeAcceptedCompletions()
    {
        progress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        completions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
        consumption = GetNode<OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
        coordinator = GetNode<OpeningRegionInvestigationRouteCompletionPresentationCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionPresentationCoordinator");
        panel = GetNode<OpeningRegionInvestigationRouteCompletionPanel>(
            "../OpeningRegionInvestigationRouteCompletionPanel");
        retainedJournal = GetNode<JournalController>("../JournalController").Journal;
        retainedMap = GetNode<SeekerMapController>("../SeekerMapController").Map;
        retainedPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
        retainedWorldObjects = GetNode<WorldObjectController>(
            "../WorldObjectController").WorldObjects;
        Label title = panel.GetNode<Label>("Panel/Title");
        Label content = panel.GetNode<Label>("Panel/Content");
        if (progress.Progress is not null
            || completions.AcceptedCompletions.Count != 0
            || consumption.ConsumedCompletion is not null
            || coordinator.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || title.Text.Length != 0
            || content.Text.Length != 0
            || !HasClosedSurfaces())
        {
            Fail("Route-completion presentation changed state during ordinary startup.");
            return;
        }

        OpeningRegionMapLocationCatalogue locations =
            RepresentativeOpeningRegionMapLocationSource.Create();
        DiscoveryScenario scenario = RepresentativeDiscoveryScenarioSource.Create();
        OpeningRegionInvestigationRouteCatalogue routes =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        OpeningRegionInvestigationRoute first = routes.Routes[0];
        OpeningRegionInvestigationRoute second = routes.Routes[1];
        if (!progress.TryInitialize(routes, locations, scenario))
        {
            Fail("Route-completion presentation could not initialize exact route progress.");
            return;
        }

        Complete(first);
        OpeningRegionInvestigationRouteProgressSnapshot firstComplete = progress.Progress!;
        OpeningRegionInvestigationRouteCompletion unaccepted =
            OpeningRegionInvestigationRouteCompletionPolicy.Authorize(firstComplete, first)!;
        bool nullPresentationRejected = ThrowsArgumentNull(() =>
            coordinator.TryPresent(null!));
        if (!nullPresentationRejected
            || coordinator.TryPresent(unaccepted)
            || !completions.TryObserve(firstComplete, first))
        {
            Fail("Route-completion presentation accepted null or unaccepted provenance.");
            return;
        }

        firstCompletion = completions.AcceptedCompletions.Single();
        Complete(second);
        OpeningRegionInvestigationRouteProgressSnapshot secondComplete = progress.Progress!;
        if (!completions.TryObserve(secondComplete, second)
            || completions.AcceptedCompletions is not { Count: 2 } accepted
            || !ReferenceEquals(accepted[0], firstCompletion))
        {
            Fail("Route-completion presentation could not arrange ordered accepted tokens.");
            return;
        }

        secondCompletion = accepted[1];
        if (mode == RestoredMode)
        {
            consumption.InstallRestored(firstCompletion);
        }
    }

    private void VerifyMode()
    {
        if (mode == RestoredMode)
        {
            VerifyRestoredConsumed();
            return;
        }

        OpeningRegionInvestigationRouteCompletionPresentationSnapshot? presentation =
            coordinator.Presentation;
        if (presentation is null
            || !ReferenceEquals(panel.Presentation, presentation)
            || !panel.Visible
            || panel.GetNode<Label>("Panel/Title").Text != presentation.Title
            || panel.GetNode<Label>("Panel/Content").Text != presentation.Content
            || presentation.Title
                != OpeningRegionInvestigationRouteCompletionPresentationProjection.Title
            || presentation.Content != "Stream and abandoned laundry route complete.")
        {
            Fail("The first exact accepted route completion was not presented safely.");
            return;
        }

        OpeningRegionInvestigationRouteCompletionPresentationSnapshot distinct =
            OpeningRegionInvestigationRouteCompletionPresentationProjection.Project(
                firstCompletion!);
        bool rejected = !coordinator.TryPresent(secondCompletion!)
            && !coordinator.TryPresent(firstCompletion!)
            && !coordinator.TryAcknowledge(distinct)
            && !panel.TryPresent(distinct)
            && ThrowsArgumentNull(() => coordinator.TryAcknowledge(null!))
            && ThrowsArgumentNull(() => panel.TryPresent(null!))
            && ReferenceEquals(coordinator.Presentation, presentation)
            && ReferenceEquals(panel.Presentation, presentation)
            && consumption.ConsumedCompletion is null;
        if (!rejected)
        {
            Fail("Route-completion presentation accepted later, distinct, repeated, or null state.");
            return;
        }

        if (mode == FailedMode)
        {
            VerifyFailedConsumption(presentation);
        }
        else
        {
            VerifySuccessfulAcknowledgement(presentation);
        }
    }

    private void VerifySuccessfulAcknowledgement(
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot presentation)
    {
        Button acknowledge = panel.GetNode<Button>("Panel/Acknowledge");
        acknowledge.EmitSignal(BaseButton.SignalName.Pressed);
        acknowledge.EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(consumption.ConsumedCompletion, firstCompletion)
            || coordinator.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || panel.GetNode<Label>("Panel/Title").Text.Length != 0
            || panel.GetNode<Label>("Panel/Content").Text.Length != 0
            || coordinator.TryAcknowledge(presentation)
            || coordinator.TryPresent(secondCompletion!)
            || !HasRetainedState())
        {
            Fail("Exact route-completion acknowledgement was not atomic and one-shot.");
            return;
        }

        Pass("successful exact first-token presentation and one-shot consumption");
    }

    private void VerifyFailedConsumption(
        OpeningRegionInvestigationRouteCompletionPresentationSnapshot presentation)
    {
        if (!consumption.TryConsume(firstCompletion!))
        {
            Fail("Failed-consumption smoke setup could not consume the exact token.");
            return;
        }

        panel.GetNode<Button>("Panel/Acknowledge").EmitSignal(BaseButton.SignalName.Pressed);
        if (!ReferenceEquals(consumption.ConsumedCompletion, firstCompletion)
            || !ReferenceEquals(coordinator.Presentation, presentation)
            || !ReferenceEquals(panel.Presentation, presentation)
            || !panel.Visible
            || !HasRetainedState())
        {
            Fail("Failed route-completion consumption did not retain the visible presentation.");
            return;
        }

        Pass("failed consumption retained the exact visible presentation and state");
    }

    private void VerifyRestoredConsumed()
    {
        if (!ReferenceEquals(consumption.ConsumedCompletion, firstCompletion)
            || coordinator.Presentation is not null
            || panel.Presentation is not null
            || panel.Visible
            || coordinator.TryPresent(firstCompletion!)
            || coordinator.TryPresent(secondCompletion!)
            || !HasRetainedState())
        {
            Fail("Restored consumed completion was presented or replaced by a later token.");
            return;
        }

        Pass("restored consumed state stayed inert and retained later accepted tokens");
    }

    private bool HasRetainedState() =>
        completions.AcceptedCompletions is { Count: 2 } accepted
        && ReferenceEquals(accepted[0], firstCompletion)
        && ReferenceEquals(accepted[1], secondCompletion)
        && GetNode<JournalController>("../JournalController").Journal == retainedJournal
        && GetNode<SeekerMapController>("../SeekerMapController").Map == retainedMap
        && ReferenceEquals(
            GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception,
            retainedPerception)
        && ReferenceEquals(
            GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
            retainedWorldObjects);

    private void Complete(OpeningRegionInvestigationRoute route)
    {
        foreach (Allrpcs.Domain.Identifiers.LocationId locationId in route.LocationIds)
        {
            if (!progress.TryRecordLocation(progress.Progress!, route, locationId))
            {
                throw new InvalidOperationException("Could not arrange complete route locations.");
            }
        }

        foreach (Allrpcs.Domain.Identifiers.EvidenceId evidenceId in route.EvidenceIds)
        {
            if (!progress.TryRecordEvidence(progress.Progress!, route, evidenceId))
            {
                throw new InvalidOperationException("Could not arrange complete route evidence.");
            }
        }
    }

    private static bool HasClosedSurfaces()
    {
        return HasSurface(
                typeof(OpeningRegionInvestigationRouteCompletionPresentationCoordinator),
                ["TryAcknowledge", "TryPresent", "_Process", "_Ready"])
            && HasSurface(
                typeof(OpeningRegionInvestigationRouteCompletionPanel),
                ["TryPresent", "_Process", "_Ready"]);
    }

    private static bool HasSurface(Type type, string[] expectedMethods)
    {
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo property = AssertSingle(type.GetProperties(
            System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly));
        return methods.SequenceEqual(expectedMethods.Order(StringComparer.Ordinal))
            && property.Name == "Presentation"
            && property.PropertyType
                == typeof(OpeningRegionInvestigationRouteCompletionPresentationSnapshot)
            && property.SetMethod?.IsPublic != true;
    }

    private static T AssertSingle<T>(IReadOnlyList<T> values) =>
        values.Count == 1
            ? values[0]
            : throw new InvalidOperationException("Expected one closed-surface member.");

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

    private void Pass(string detail)
    {
        GD.Print(
            "Opening-region route-completion presentation smoke passed: inert startup, "
            + detail
            + ", first-over-later ordering, unaccepted, distinct, repeated and null rejection, "
            + "fixed copy, closed surfaces, and unrelated-state isolation were correct.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
