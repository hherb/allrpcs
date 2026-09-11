using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free temporary world-event source checks when explicitly enabled.</summary>
public partial class TemporaryWorldEventSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_EVENT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string GreyboxReadyEventIdText = "greybox-ready";
    private const string HerbPatchIdentifiedEventIdText = "herb-patch-identified";
    private const string HerbPatchExaminedEventIdText = "healer-examined-herb-patch";
    private const string HerbPatchCollectedEventIdText = "healer-collected-herb-patch";
    private const string FirstPatientRemedyAdministeredEventIdText =
        "healer-administered-first-patient-remedy";

    private TemporaryWorldEventSource source = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private CharacterBody2D seeker = null!;
    private Label focusStatus = null!;
    private Label focusedSearchResult = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldEventLogSnapshot? expectedEventLog = worldEventController.EventLog;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;
        WorldEventSnapshot firstSnapshot = source.WorldEvents;
        WorldEventSnapshot secondSnapshot = source.WorldEvents;
        WorldEvent greyboxReady = firstSnapshot.Entries[0];
        WorldEvent herbPatchIdentified = firstSnapshot.Entries[1];
        WorldEvent herbPatchExamined = firstSnapshot.Entries[2];
        WorldEvent herbPatchCollected = firstSnapshot.Entries[3];
        WorldEvent firstPatientRemedyAdministered = firstSnapshot.Entries[4];

        if (!HasExpectedPropertySurface()
            || !ReferenceEquals(firstSnapshot, secondSnapshot)
            || firstSnapshot.Entries.Count != 5
            || !ReferenceEquals(source.InitialWorldEvent, greyboxReady)
            || !ReferenceEquals(
                source.HerbPatchIdentifiedWorldEvent,
                herbPatchIdentified)
            || !ReferenceEquals(source.HerbPatchExaminedWorldEvent, herbPatchExamined)
            || !ReferenceEquals(source.HerbPatchCollectedWorldEvent, herbPatchCollected)
            || !ReferenceEquals(
                source.FirstPatientRemedyAdministeredWorldEvent,
                firstPatientRemedyAdministered)
            || !string.Equals(
                greyboxReady.EventId.Value,
                GreyboxReadyEventIdText,
                StringComparison.Ordinal)
            || !ReferenceEquals(greyboxReady.TimeEffect, WorldEventTimeEffect.NoAdvance)
            || !string.Equals(
                herbPatchIdentified.EventId.Value,
                HerbPatchIdentifiedEventIdText,
                StringComparison.Ordinal)
            || !ReferenceEquals(
                herbPatchIdentified.TimeEffect,
                WorldEventTimeEffect.Advance)
            || herbPatchExamined.EventId.Value != HerbPatchExaminedEventIdText
            || !ReferenceEquals(herbPatchExamined.TimeEffect, WorldEventTimeEffect.Advance)
            || herbPatchCollected.EventId.Value != HerbPatchCollectedEventIdText
            || !ReferenceEquals(herbPatchCollected.TimeEffect, WorldEventTimeEffect.Advance)
            || firstPatientRemedyAdministered.EventId.Value
                != FirstPatientRemedyAdministeredEventIdText
            || !ReferenceEquals(
                firstPatientRemedyAdministered.TimeEffect,
                WorldEventTimeEffect.Advance))
        {
            Fail("Temporary world-event source failed stable exact event retention.");
            return;
        }

        if (expectedEventLog is null
            || !ReferenceEquals(worldEventController.EventLog, expectedEventLog)
            || expectedEventLog.Entries.Count != 1
            || !ReferenceEquals(expectedEventLog.Entries[0], greyboxReady.EventId)
            || ReferenceEquals(expectedEventLog.Entries[0], herbPatchIdentified.EventId)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Temporary world-event source changed log, world, gameplay, or presentation state.");
            return;
        }

        GD.Print(
            "Temporary world-event source smoke passed: one stable exact snapshot containing the "
            + "canonical startup, discovery, examination, collection, and first-patient "
            + "administration events in "
            + "source order, an exact initial-event reference, exact ordinary startup recording, "
            + "and unchanged event-log, "
            + "world-object, perception, focused-search, movement, input, and presentation state "
            + "were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(TemporaryWorldEventSource)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();

        return properties.Length == 6
            && IsPrivateSetProperty(
                properties[0],
                "FirstPatientRemedyAdministeredWorldEvent",
                typeof(WorldEvent))
            && IsPrivateSetProperty(
                properties[1],
                "HerbPatchCollectedWorldEvent",
                typeof(WorldEvent))
            && IsPrivateSetProperty(
                properties[2],
                "HerbPatchExaminedWorldEvent",
                typeof(WorldEvent))
            && IsPrivateSetProperty(
                properties[3],
                "HerbPatchIdentifiedWorldEvent",
                typeof(WorldEvent))
            && IsPrivateSetProperty(properties[4], "InitialWorldEvent", typeof(WorldEvent))
            && IsPrivateSetProperty(properties[5], "WorldEvents", typeof(WorldEventSnapshot));
    }

    private static bool IsPrivateSetProperty(
        System.Reflection.PropertyInfo property,
        string name,
        Type propertyType) =>
        property.Name == name
        && property.PropertyType == propertyType
        && property.CanRead
        && property.SetMethod is not null
        && property.SetMethod.IsPrivate;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
