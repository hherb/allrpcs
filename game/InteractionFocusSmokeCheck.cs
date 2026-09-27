using System.Globalization;
using Allrpcs.Application.Movement;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free interaction-focus presentation checks when explicitly enabled.
/// </summary>
public partial class InteractionFocusSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_INTERACTION_FOCUS_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string FocusedSearchCancelAction = "focused_search_cancel";
    private const string FocusedSearchCompleteAction = "focused_search_complete";
    private const string FocusedSearchResolveAction = "focused_search_resolve";
    private const string FocusedSearchStartAction = "focused_search_start";
    private const string HealerMoveAction = "healer_move";
    private const string HealerToggleAction = "healer_toggle_follow_wait";
    private const string InitialFocusedSearchResultText = "Focused search: NO ATTEMPT";
    private const int FocusedWorldObjectCount = 2;
    private const int MaximumSynchronizationFrames = 60;
    private const float NavigationSynchronizationTolerancePixels = 1.0f;
    private const string StartChangedText = "Focused search START: CHANGED";
    private const string StartUnchangedText = "Focused search START: UNCHANGED";
    private const string CancelChangedText = "Focused search CANCEL: CHANGED";
    private const string CancelUnchangedText = "Focused search CANCEL: UNCHANGED";
    private const string CompleteChangedText = "Focused search COMPLETE: CHANGED";
    private const string CompleteUnchangedText = "Focused search COMPLETE: UNCHANGED";
    private const string ResolveChangedText = "Focused search RESOLVE: CHANGED";
    private const string ResolveUnchangedText = "Focused search RESOLVE: UNCHANGED";
    private const string TemporaryInstructionsText =
        "TEMP · TAB PANELS · U CUSTOMIZE · J JOURNAL · M MAP · I SHRINE · P PREPARE · T TREAT · K SAGE "
        + "· F START · C CANCEL · E COMPLETE · R RESOLVE · V CANCEL TURN · X WAIT · H MOVE";
    private const string UnrelatedAction = "interaction_focus_smoke_unrelated";
    private static readonly string[] ExpectedObjectIdValues =
    [
        "herb-patch",
        "stone-marker",
        "village-water-cask",
        "wagon-clue",
        "hidden-marker",
        "excluded-marker",
    ];
    private static readonly Vector2 FirstTargetPosition = new(720.0f, 520.0f);
    private static readonly Vector2 EqualDistancePosition = new(800.0f, 520.0f);
    private static readonly Vector2 SecondTargetPosition = new(880.0f, 520.0f);
    private static readonly Vector2 OutOfRangePosition = new(500.0f, 800.0f);
    private static readonly Vector2 HiddenTargetPosition = new(800.0f, 280.0f);
    private static readonly Vector2 DisabledTargetPosition = new(1040.0f, 440.0f);

    private InteractionFocusController controller = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private TemporaryFocusedSearchContextSource focusedSearchContextSource = null!;
    private Line2D focusedSearchIndicator = null!;
    private TemporaryResolvedPerceptionIndicator[] resolvedPerceptionIndicators = [];
    private Label focusedSearchResultStatus = null!;
    private Label instructions = null!;
    private Healer healer = null!;
    private Marker2D healerCommandDestination = null!;
    private Main main = null!;
    private CharacterBody2D seeker = null!;
    private TemporaryWorldObjectSource worldObjectSource = null!;
    private WorldObjectController worldObjectController = null!;
    private ObjectId[] authoredObjectIds = [];
    private ObjectId[] sourceObjectIds = [];
    private DiscoveryConditionSet authoredConditions = null!;
    private CharacterCapabilitySet authoredCapabilities = null!;
    private CharacterInventory authoredInventory = null!;
    private NavigationRegion2D navigationRegion = null!;
    private DiscoveryCapabilityRequirementSet authoredCapabilityRequirements = null!;
    private DiscoveryToolRequirementSet authoredToolRequirements = null!;
    private DiscoveryCapabilityRequirement authoredCapabilityRequirement = null!;
    private DiscoveryCapabilityRequirement authoredWaterCaskCapabilityRequirement = null!;
    private DiscoveryToolRequirement authoredToolRequirement = null!;
    private CharacterCapability authoredCapability = null!;
    private CharacterCapability authoredWaterCaskCapability = null!;
    private ObjectId authoredToolObjectId = null!;
    private int synchronizationFrames;
    private SmokeStage stage;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<InteractionFocusController>("../InteractionFocusController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        focusedSearchContextSource = GetNode<TemporaryFocusedSearchContextSource>(
            "../TemporaryFocusedSearchContextSource");
        focusedSearchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        resolvedPerceptionIndicators =
        [
            GetNode<TemporaryResolvedPerceptionIndicator>(
                "../InteractionCandidates/HerbPatch/TemporaryResolvedPerceptionIndicator"),
            GetNode<TemporaryResolvedPerceptionIndicator>(
                "../InteractionCandidates/StoneMarker/TemporaryResolvedPerceptionIndicator"),
        ];
        focusedSearchResultStatus = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
        instructions = GetNode<Label>("../CommandOverlay/Panel/Instructions");
        healer = GetNode<Healer>("../Healer");
        healerCommandDestination = GetNode<Marker2D>("../HealerCommandDestination");
        main = GetNode<Main>("..");
        navigationRegion = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        worldObjectSource = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");

        if (!AssertFocusedSearchResult(InitialFocusedSearchResultText, "initial presentation")
            || !AssertFocusedSearchInputActions()
            || !AssertTemporaryInstructions()
            || !AssertAuthoredObjectIdentifiers()
            || !AssertAuthoredFocusedSearchContext())
        {
            SetProcess(false);
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;

        switch (stage)
        {
            case SmokeStage.Synchronizing:
                CheckNavigationSynchronization();
                break;
            case SmokeStage.RepresentativeSelection:
                CheckRepresentativeSelection();
                break;
            case SmokeStage.EqualDistanceTie:
                CheckEqualDistanceTie();
                break;
            case SmokeStage.FocusChange:
                CheckFocusChange();
                break;
            case SmokeStage.OutOfRangeClearing:
                CheckOutOfRangeClearing();
                break;
            case SmokeStage.HiddenTarget:
                CheckHiddenTarget();
                break;
            case SmokeStage.DisabledTarget:
                CheckDisabledTarget();
                break;
            default:
                Fail("Interaction-focus smoke entered an unknown stage.");
                break;
        }
    }

    private void CheckNavigationSynchronization()
    {
        synchronizationFrames++;
        Rid navigationMap = NavigationServer2D.RegionGetMap(navigationRegion.GetRid());
        bool isSynchronized = NavigationServer2D.RegionGetIterationId(navigationRegion.GetRid()) > 0
            && NavigationServer2D.MapGetIterationId(navigationMap) > 0
            && NavigationServer2D.MapGetClosestPoint(
                navigationMap,
                healerCommandDestination.GlobalPosition)
                .DistanceTo(healerCommandDestination.GlobalPosition)
                <= NavigationSynchronizationTolerancePixels;
        if (isSynchronized)
        {
            stage = SmokeStage.RepresentativeSelection;
            return;
        }

        if (synchronizationFrames >= MaximumSynchronizationFrames)
        {
            Fail("Interaction-focus smoke could not synchronize the navigation map.");
        }
    }

    private void CheckRepresentativeSelection()
    {
        if (!AssertResolvedPerceptionPresentationContract())
        {
            return;
        }

        seeker.GlobalPosition = FirstTargetPosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 0, "representative nearest selection"))
        {
            return;
        }

        if (!AssertEligibleFocusedSearchStart(expectedTargetIndex: 0))
        {
            return;
        }

        stage = SmokeStage.EqualDistanceTie;
    }

    private void CheckEqualDistanceTie()
    {
        seeker.GlobalPosition = EqualDistancePosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 0, "authored-order equal-distance tie"))
        {
            return;
        }

        stage = SmokeStage.FocusChange;
    }

    private void CheckFocusChange()
    {
        seeker.GlobalPosition = SecondTargetPosition;
        controller.RefreshFocus();
        if (!AssertOnlyTargetFocused(expectedIndex: 1, "deterministic focus change"))
        {
            return;
        }

        if (!AssertEligibleFocusedSearchStart(expectedTargetIndex: 1))
        {
            return;
        }

        stage = SmokeStage.OutOfRangeClearing;
    }

    private void CheckOutOfRangeClearing()
    {
        seeker.GlobalPosition = OutOfRangePosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("out-of-range clearing"))
        {
            return;
        }

        stage = SmokeStage.HiddenTarget;
    }

    private void CheckHiddenTarget()
    {
        seeker.GlobalPosition = HiddenTargetPosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("hidden-target exclusion"))
        {
            return;
        }

        GreyboxInteractionTarget hiddenTarget = controller.GetTarget(4);
        if (hiddenTarget.Visible)
        {
            Fail("The hidden interaction target remained visible in the scene.");
            return;
        }

        stage = SmokeStage.DisabledTarget;
    }

    private void CheckDisabledTarget()
    {
        seeker.GlobalPosition = DisabledTargetPosition;
        controller.RefreshFocus();
        if (!AssertNoTargetFocused("disabled-target exclusion"))
        {
            return;
        }

        GD.Print(
            "Interaction-focus smoke passed: nearest selection, authored-order tie-breaking, "
            + "focus change, out-of-range clearing, hidden and disabled exclusions, and exact "
            + "stable request-identifier retention; world-object-gated request creation and "
            + "starts with exact value-equal identifier retention; world-object-derived "
            + "capability-only and tool-only focused-search context authoring with independent "
            + "inputs and explicit undersized-snapshot failure; mapped start, cancellation, "
            + "completion, and resolution inputs; exact temporary control text; exact retained "
            + "active requests; "
            + "both eligible starts, missing-object start rejection, focus-independent "
            + "cancellations, and world-object-gated "
            + "outcome-aware completions; exact outcome recording and exact world-object-bound "
            + "resolution; missing- and mismatched-object rejection; "
            + "target-only perception replacement and exact world-object description presentation "
            + "only for identified targets, including value-equal replacement and hide-again "
            + "behavior; missing-object hiding; exact unrelated entry retention; unchanged inputs; "
            + "failure-state identity; activity presentation; and initial, changed, and unchanged "
            + "lifecycle-result presentation were correct.");
        SetProcess(false);
        GetTree().Quit();
    }

    private bool AssertOnlyTargetFocused(int expectedIndex, string scenario)
    {
        if (controller.FocusedTargetIndex != expectedIndex)
        {
            string actualIndex = controller.FocusedTargetIndex is int value
                ? value.ToString(CultureInfo.InvariantCulture)
                : "none";
            Fail(
                $"Interaction-focus smoke failed {scenario}: expected {expectedIndex}, "
                + $"received {actualIndex}.");
            return false;
        }

        if (!ReferenceEquals(controller.GetTarget(expectedIndex).ObjectId, authoredObjectIds[expectedIndex])
            || !ReferenceEquals(controller.FocusedTargetObjectId, sourceObjectIds[expectedIndex])
            || ReferenceEquals(sourceObjectIds[expectedIndex], authoredObjectIds[expectedIndex])
            || sourceObjectIds[expectedIndex] != authoredObjectIds[expectedIndex])
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the focused object identifier "
                + "did not retain the exact independently authored world-object instance.");
            return false;
        }

        if (!AssertWorldObjectGatedRequestCreation(expectedIndex))
        {
            return false;
        }

        for (int index = 0; index < controller.TargetCount; index++)
        {
            bool expectedFocus = index == expectedIndex;
            if (controller.GetTarget(index).IsFocused != expectedFocus)
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} indicator "
                    + $"state was {controller.GetTarget(index).IsFocused}.");
                return false;
            }
        }

        return true;
    }

    private bool AssertWorldObjectGatedRequestCreation(int expectedTargetIndex)
    {
        ObjectId focusedTargetObjectId = sourceObjectIds[expectedTargetIndex];
        WorldObject exactWorldObject = worldObjectController.FindWorldObject(focusedTargetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the focused world object.");
        WorldObject mismatchedWorldObject = worldObjectSource.WorldObjects.Entries
            .Take(FocusedWorldObjectCount)
            .Single(worldObject => worldObject.Identity.Id != focusedTargetObjectId);
        ObjectId valueEqualObjectId = new(focusedTargetObjectId.Value);
        WorldObject valueEqualWorldObject = CreateWorldObject(
            valueEqualObjectId,
            "Value-equal focused object");
        WorldObjectIdentity exactIdentity = exactWorldObject.Identity;
        WorldObjectDescription exactDescription = exactWorldObject.Description;
        WorldObjectIdentity mismatchedIdentity = mismatchedWorldObject.Identity;
        WorldObjectDescription mismatchedDescription = mismatchedWorldObject.Description;
        WorldObjectIdentity valueEqualIdentity = valueEqualWorldObject.Identity;
        WorldObjectDescription valueEqualDescription = valueEqualWorldObject.Description;

        FocusedSearchRequest? exactRequest = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId,
            exactWorldObject);
        FocusedSearchRequest? valueEqualRequest = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId,
            valueEqualWorldObject);
        FocusedSearchRequest? absentRequest = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId,
            null);
        FocusedSearchRequest? mismatchedRequest = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId,
            mismatchedWorldObject);

        if (exactRequest is null
            || valueEqualRequest is null
            || absentRequest is not null
            || mismatchedRequest is not null
            || !ReferenceEquals(
                exactRequest.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(exactRequest.TargetObjectId, exactIdentity.Id)
            || !ReferenceEquals(
                valueEqualRequest.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(valueEqualRequest.TargetObjectId, valueEqualObjectId)
            || ReferenceEquals(valueEqualObjectId, focusedTargetObjectId)
            || valueEqualObjectId != focusedTargetObjectId
            || !ReferenceEquals(exactWorldObject.Identity, exactIdentity)
            || !ReferenceEquals(exactWorldObject.Description, exactDescription)
            || !ReferenceEquals(mismatchedWorldObject.Identity, mismatchedIdentity)
            || !ReferenceEquals(mismatchedWorldObject.Description, mismatchedDescription)
            || !ReferenceEquals(valueEqualWorldObject.Identity, valueEqualIdentity)
            || !ReferenceEquals(valueEqualWorldObject.Description, valueEqualDescription)
            || !ReferenceEquals(valueEqualIdentity.Id, valueEqualObjectId))
        {
            Fail(
                "Interaction-focus smoke failed null, mismatched, exact, or value-equal "
                + "world-object-gated request creation and input retention.");
            return false;
        }

        return true;
    }

    private bool AssertNoTargetFocused(string scenario)
    {
        if (controller.FocusedTargetIndex is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: unexpected index "
                + $"{controller.FocusedTargetIndex}.");
            return false;
        }

        if (controller.FocusedTargetObjectId is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: unexpected object identifier "
                + $"{controller.FocusedTargetObjectId}.");
            return false;
        }

        if (controller.CreateFocusedSearchRequest(
                focusedSearchController.SearcherCharacterId,
                worldObjectSource.WorldObjects.Entries[0]) is not null)
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: a request was created without "
                + "a focused target.");
            return false;
        }

        FocusedSearchActivitySnapshot expectedSnapshot = focusedSearchController.Activities;
        PressFocusedSearchStart();
        if (!ReferenceEquals(focusedSearchController.Activities, expectedSnapshot)
            || focusedSearchIndicator.Visible
            || !AssertFocusedSearchResult(StartUnchangedText, scenario)
            || !AssertFocusedSearchContextUnchanged())
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the absent-focus start attempt "
                + "invoked or changed focused-search state.");
            return false;
        }

        for (int index = 0; index < controller.TargetCount; index++)
        {
            if (!ReferenceEquals(controller.GetTarget(index).ObjectId, authoredObjectIds[index]))
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} replaced its "
                    + "authored object identifier.");
                return false;
            }

            if (controller.GetTarget(index).IsFocused)
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} still showed "
                    + "its focus indicator.");
                return false;
            }
        }

        return true;
    }

    private bool AssertWorldObjectGatedStartDelegation(int expectedTargetIndex)
    {
        ObjectId focusedTargetObjectId = sourceObjectIds[expectedTargetIndex];
        WorldObject mismatchedWorldObject = worldObjectSource.WorldObjects.Entries
            .Take(FocusedWorldObjectCount)
            .Single(worldObject => worldObject.Identity.Id != focusedTargetObjectId);
        ObjectId valueEqualObjectId = new(focusedTargetObjectId.Value);
        WorldObject valueEqualWorldObject = CreateWorldObject(
            valueEqualObjectId,
            "Value-equal start object");
        WorldObjectIdentity mismatchedIdentity = mismatchedWorldObject.Identity;
        WorldObjectDescription mismatchedDescription = mismatchedWorldObject.Description;
        WorldObjectIdentity valueEqualIdentity = valueEqualWorldObject.Identity;
        WorldObjectDescription valueEqualDescription = valueEqualWorldObject.Description;
        WorldObjectSnapshot expectedWorldObjects = worldObjectSource.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        int? expectedFocusedIndex = controller.FocusedTargetIndex;
        ObjectId? expectedFocusedObjectId = controller.FocusedTargetObjectId;

        bool nullChanged = controller.TryStartFocusedSearch(
            null,
            authoredConditions,
            authoredCapabilities,
            authoredInventory);
        bool mismatchChanged = controller.TryStartFocusedSearch(
            mismatchedWorldObject,
            authoredConditions,
            authoredCapabilities,
            authoredInventory);
        if (nullChanged
            || mismatchChanged
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || focusedSearchIndicator.Visible)
        {
            Fail(
                "Interaction-focus smoke changed focused-search state for a null or "
                + "mismatched start object.");
            return false;
        }

        bool changed = controller.TryStartFocusedSearch(
            valueEqualWorldObject,
            authoredConditions,
            authoredCapabilities,
            authoredInventory);
        FocusedSearchActivitySnapshot startedActivities = focusedSearchController.Activities;
        FocusedSearchRequest? startedRequest = startedActivities.Entries.SingleOrDefault()?.Request;
        bool cancelled = startedRequest is not null
            && focusedSearchController.TryCancelSearch(startedRequest);

        if (!changed
            || startedRequest is null
            || !cancelled
            || ReferenceEquals(startedActivities, expectedActivities)
            || !ReferenceEquals(
                startedRequest.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(startedRequest.TargetObjectId, valueEqualObjectId)
            || focusedSearchController.Activities.Entries.Count != 0
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || focusedSearchIndicator.Visible
            || !ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || controller.FocusedTargetIndex != expectedFocusedIndex
            || !ReferenceEquals(controller.FocusedTargetObjectId, expectedFocusedObjectId)
            || !ReferenceEquals(mismatchedWorldObject.Identity, mismatchedIdentity)
            || !ReferenceEquals(mismatchedWorldObject.Description, mismatchedDescription)
            || !ReferenceEquals(valueEqualWorldObject.Identity, valueEqualIdentity)
            || !ReferenceEquals(valueEqualWorldObject.Description, valueEqualDescription)
            || !ReferenceEquals(valueEqualIdentity.Id, valueEqualObjectId)
            || !AssertFocusedSearchContextUnchanged())
        {
            Fail(
                "Interaction-focus smoke failed value-equal world-object start delegation or "
                + "input and surrounding-state retention.");
            return false;
        }

        return true;
    }

    private bool AssertEligibleFocusedSearchStart(int expectedTargetIndex)
    {
        ObjectId targetObjectId = sourceObjectIds[expectedTargetIndex];

        if (!AssertWorldObjectGatedStartDelegation(expectedTargetIndex))
        {
            return false;
        }

        FocusedSearchActivitySnapshot initialSnapshot = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot initialOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot initialPerception =
            focusedSearchController.Perception
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires initialized Seeker perception.");
        if (initialSnapshot.Entries.Count != 0 || focusedSearchIndicator.Visible)
        {
            Fail("Interaction-focus smoke failed eligible start setup: state was not empty.");
            return false;
        }

        if (!AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "eligible start setup"))
        {
            return false;
        }

        if (!AssertAbsentWorldObjectStartIsGated(
                targetObjectId,
                initialSnapshot,
                initialOutcomes,
                initialPerception,
                expectedTargetIndex))
        {
            return false;
        }

        string expectedUnchangedResultText = focusedSearchResultStatus.Text;
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(FocusedSearchCancelAction),
                Pressed = false,
            });
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(FocusedSearchCompleteAction),
                Pressed = false,
            });
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(FocusedSearchResolveAction),
                Pressed = false,
            });
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(FocusedSearchStartAction),
                Pressed = false,
            });
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(UnrelatedAction),
                Pressed = true,
            });
        if (!ReferenceEquals(focusedSearchController.Activities, initialSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, initialPerception)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "released and unrelated input preservation")
            || !AssertFocusedSearchResult(
                expectedUnchangedResultText,
                "released and unrelated input preservation")
            || !AssertNonSearchInputsPreserveResult(
                expectedUnchangedResultText,
                initialSnapshot,
                initialOutcomes,
                initialPerception))
        {
            Fail("Interaction-focus smoke failed input-only start gating.");
            return false;
        }

        PressFocusedSearchCancel();
        if (!ReferenceEquals(focusedSearchController.Activities, initialSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "no-active cancellation")
            || !AssertFocusedSearchResult(CancelUnchangedText, "no-active cancellation"))
        {
            Fail("Interaction-focus smoke failed no-active-search cancellation identity.");
            return false;
        }

        PressFocusedSearchComplete();
        if (!ReferenceEquals(focusedSearchController.Activities, initialSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "no-active completion")
            || !AssertFocusedSearchResult(CompleteUnchangedText, "no-active completion"))
        {
            Fail("Interaction-focus smoke failed no-active-search completion identity.");
            return false;
        }

        PressFocusedSearchResolve();
        if (!ReferenceEquals(focusedSearchController.Activities, initialSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, initialPerception)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "no-outcome resolution")
            || !AssertFocusedSearchResult(ResolveUnchangedText, "no-outcome resolution"))
        {
            Fail("Interaction-focus smoke failed no-outcome resolution identity.");
            return false;
        }

        PressFocusedSearchStart();
        if (ReferenceEquals(focusedSearchController.Activities, initialSnapshot))
        {
            Fail("Interaction-focus smoke failed eligible focused-search input start.");
            return false;
        }

        FocusedSearchActivitySnapshot startedSnapshot = focusedSearchController.Activities;
        if (startedSnapshot.Entries.Count != 1
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "eligible start")
            || !AssertFocusedSearchResult(StartChangedText, "eligible start"))
        {
            Fail("Interaction-focus smoke failed eligible start state and presentation.");
            return false;
        }

        FocusedSearchRequest request = startedSnapshot.Entries[0].Request;
        if (!ReferenceEquals(request.SearcherCharacterId, focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, targetObjectId)
            || !AssertFocusedSearchContextUnchanged())
        {
            return false;
        }

        PressFocusedSearchStart();
        if (!ReferenceEquals(focusedSearchController.Activities, startedSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "already-active start")
            || !AssertFocusedSearchResult(StartUnchangedText, "already-active start"))
        {
            Fail("Interaction-focus smoke failed already-active start state identity.");
            return false;
        }

        Vector2 focusedPosition = seeker.GlobalPosition;
        seeker.GlobalPosition = OutOfRangePosition;
        controller.RefreshFocus();
        if (controller.FocusedTargetIndex is not null)
        {
            Fail("Interaction-focus smoke failed cancellation focus-independence setup.");
            return false;
        }

        PressFocusedSearchCancel();
        FocusedSearchActivitySnapshot cancelledSnapshot = focusedSearchController.Activities;
        if (ReferenceEquals(cancelledSnapshot, startedSnapshot)
            || cancelledSnapshot.Entries.Count != 0
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "successful cancellation")
            || !AssertFocusedSearchResult(CancelChangedText, "successful cancellation"))
        {
            Fail("Interaction-focus smoke failed focused-search input cancellation.");
            return false;
        }

        PressFocusedSearchCancel();
        if (!ReferenceEquals(focusedSearchController.Activities, cancelledSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "repeated cancellation")
            || !AssertFocusedSearchResult(CancelUnchangedText, "repeated cancellation"))
        {
            Fail("Interaction-focus smoke failed repeated cancellation identity.");
            return false;
        }

        seeker.GlobalPosition = focusedPosition;
        controller.RefreshFocus();

        PressFocusedSearchStart();
        FocusedSearchActivitySnapshot completionStartedSnapshot =
            focusedSearchController.Activities;
        if (ReferenceEquals(completionStartedSnapshot, cancelledSnapshot)
            || completionStartedSnapshot.Entries.Count != 1
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || !focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "completion setup start")
            || !AssertFocusedSearchResult(StartChangedText, "completion setup start"))
        {
            Fail("Interaction-focus smoke failed completion setup start.");
            return false;
        }

        FocusedSearchRequest completionRequest = completionStartedSnapshot.Entries[0].Request;
        if (!ReferenceEquals(
                completionRequest.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(completionRequest.TargetObjectId, targetObjectId)
            || !AssertFocusedSearchContextUnchanged())
        {
            Fail("Interaction-focus smoke failed completion setup request retention.");
            return false;
        }

        seeker.GlobalPosition = OutOfRangePosition;
        controller.RefreshFocus();
        if (controller.FocusedTargetIndex is not null)
        {
            Fail("Interaction-focus smoke failed completion focus-independence setup.");
            return false;
        }

        if (!AssertAbsentWorldObjectCompletionIsGated(
                targetObjectId,
                completionStartedSnapshot,
                initialOutcomes,
                initialPerception,
                expectedTargetIndex))
        {
            return false;
        }

        PressFocusedSearchComplete();
        FocusedSearchActivitySnapshot completedSnapshot = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot completedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot completedPerception =
            focusedSearchController.Perception
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke lost initialized Seeker perception.");
        if (ReferenceEquals(completedSnapshot, completionStartedSnapshot)
            || completedSnapshot.Entries.Count != 0
            || !AssertCompletedOutcome(initialOutcomes, completedOutcomes, completionRequest)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "successful completion")
            || !AssertFocusedSearchResult(CompleteChangedText, "successful completion"))
        {
            Fail("Interaction-focus smoke failed focused-search input completion.");
            return false;
        }

        PressFocusedSearchComplete();
        if (!ReferenceEquals(focusedSearchController.Activities, completedSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, completedOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex,
                "repeated completion")
            || !AssertFocusedSearchResult(CompleteUnchangedText, "repeated completion"))
        {
            Fail("Interaction-focus smoke failed repeated completion identity.");
            return false;
        }

        FocusedSearchOutcome recordedOutcome = completedOutcomes.Entries[^1];
        if (!AssertAbsentWorldObjectResolutionIsGated(
                targetObjectId,
                completedSnapshot,
                completedOutcomes,
                completedPerception,
                expectedTargetIndex))
        {
            return false;
        }

        if (!AssertMismatchedWorldObjectResolutionIsRejected(
                recordedOutcome,
                targetObjectId,
                completedSnapshot,
                completedOutcomes,
                completedPerception,
                expectedTargetIndex))
        {
            return false;
        }

        WorldObjectSnapshot resolvedWorldObjects = worldObjectSource.WorldObjects;
        WorldObject resolvedWorldObject =
            worldObjectController.FindWorldObject(targetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the target world object.");
        WorldObjectIdentity resolvedWorldObjectIdentity = resolvedWorldObject.Identity;
        WorldObjectDescription resolvedWorldObjectDescription = resolvedWorldObject.Description;
        GreyboxInteractionTarget resolvedTarget = controller.GetTarget(expectedTargetIndex);
        ObjectId resolvedTargetObjectId = resolvedTarget.ObjectId;
        bool resolvedTargetFocusState = resolvedTarget.IsFocused;

        PressFocusedSearchResolve();
        FocusedSearchOutcomeSnapshot resolvedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot resolvedPerception =
            focusedSearchController.Perception
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke lost resolved Seeker perception.");
        if (!ReferenceEquals(focusedSearchController.Activities, completedSnapshot)
            || ReferenceEquals(resolvedOutcomes, completedOutcomes)
            || resolvedOutcomes.Entries.Count != initialOutcomes.Entries.Count
            || ReferenceEquals(resolvedPerception, completedPerception)
            || !AssertResolvedPerception(
                completedPerception,
                resolvedPerception,
                targetObjectId)
            || !ReferenceEquals(worldObjectController.WorldObjects, resolvedWorldObjects)
            || !ReferenceEquals(worldObjectSource.WorldObjects, resolvedWorldObjects)
            || !ReferenceEquals(
                worldObjectController.FindWorldObject(targetObjectId),
                resolvedWorldObject)
            || !ReferenceEquals(resolvedWorldObject.Identity, resolvedWorldObjectIdentity)
            || !ReferenceEquals(resolvedWorldObject.Description, resolvedWorldObjectDescription)
            || !ReferenceEquals(resolvedWorldObject.Identity.Id, targetObjectId)
            || !ReferenceEquals(controller.GetTarget(expectedTargetIndex), resolvedTarget)
            || !ReferenceEquals(resolvedTarget.ObjectId, resolvedTargetObjectId)
            || resolvedTarget.IsFocused != resolvedTargetFocusState
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex + 1,
                "successful resolution")
            || !AssertFocusedSearchResult(ResolveChangedText, "successful resolution"))
        {
            Fail("Interaction-focus smoke failed focused-search input resolution.");
            return false;
        }

        if (!ReferenceEquals(completedOutcomes.Entries[^1], recordedOutcome)
            || !ReferenceEquals(
                completedPerception.Entries.Single(
                    entry => entry.ObjectId == targetObjectId).VisibilityState,
                initialPerception.Entries.Single(
                    entry => entry.ObjectId == targetObjectId).VisibilityState))
        {
            Fail("Interaction-focus smoke changed completed resolution inputs.");
            return false;
        }

        PressFocusedSearchResolve();
        if (!ReferenceEquals(focusedSearchController.Activities, completedSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, resolvedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount: expectedTargetIndex + 1,
                "repeated resolution")
            || !AssertFocusedSearchResult(ResolveUnchangedText, "repeated resolution"))
        {
            Fail("Interaction-focus smoke failed repeated resolution identity.");
            return false;
        }

        if (!AssertMissingWorldObjectRefreshHidesOnlyTarget(
                targetObjectId,
                completedSnapshot,
                resolvedOutcomes,
                resolvedPerception,
                expectedTargetIndex))
        {
            return false;
        }

        seeker.GlobalPosition = focusedPosition;
        controller.RefreshFocus();

        return true;
    }

    private bool AssertAbsentWorldObjectStartIsGated(
        ObjectId targetObjectId,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception,
        int expectedTargetIndex)
    {
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        WorldObject targetWorldObject = worldObjectController.FindWorldObject(targetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the target world object.");
        WorldObjectIdentity targetIdentity = targetWorldObject.Identity;
        WorldObjectDescription targetDescription = targetWorldObject.Description;
        WorldObject[] retainedWorldObjects = sourceWorldObjects.Entries
            .Where(worldObject => worldObject.Identity.Id != targetObjectId)
            .ToArray();
        WorldObjectSnapshot controlledWorldObjects = new(retainedWorldObjects);
        GreyboxInteractionTarget target = controller.GetTarget(expectedTargetIndex);
        ObjectId expectedTargetObjectId = target.ObjectId;
        bool expectedTargetFocusState = target.IsFocused;

        SetControlledWorldObjects(controlledWorldObjects);
        PressFocusedSearchStart();

        bool unchanged = retainedWorldObjects.Length == sourceWorldObjects.Entries.Count - 1
            && !controlledWorldObjects.Entries.Any(
                worldObject => worldObject.Identity.Id == targetObjectId)
            && ReferenceEquals(worldObjectController.WorldObjects, controlledWorldObjects)
            && ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            && ReferenceEquals(targetWorldObject.Identity, targetIdentity)
            && ReferenceEquals(targetWorldObject.Description, targetDescription)
            && ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            && ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            && ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            && !focusedSearchIndicator.Visible
            && ReferenceEquals(controller.GetTarget(expectedTargetIndex), target)
            && ReferenceEquals(target.ObjectId, expectedTargetObjectId)
            && target.IsFocused == expectedTargetFocusState
            && AssertResolvedPerceptionIndicators(
                expectedTargetIndex,
                "absent-world-object start")
            && AssertFocusedSearchResult(StartUnchangedText, "absent-world-object start")
            && AssertFocusedSearchContextUnchanged();

        SetControlledWorldObjects(sourceWorldObjects);
        if (!unchanged
            || !ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects))
        {
            Fail(
                "Interaction-focus smoke changed focused-search, world-object, target, context, "
                + "or presentation state while the focused object was absent.");
            return false;
        }

        return true;
    }

    private bool AssertAbsentWorldObjectCompletionIsGated(
        ObjectId targetObjectId,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception,
        int expectedTargetIndex)
    {
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        WorldObject targetWorldObject = worldObjectController.FindWorldObject(targetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the target world object.");
        WorldObjectIdentity targetIdentity = targetWorldObject.Identity;
        WorldObjectDescription targetDescription = targetWorldObject.Description;
        WorldObject[] retainedWorldObjects = sourceWorldObjects.Entries
            .Where(worldObject => worldObject.Identity.Id != targetObjectId)
            .ToArray();
        WorldObjectSnapshot controlledWorldObjects = new(retainedWorldObjects);
        GreyboxInteractionTarget target = controller.GetTarget(expectedTargetIndex);
        ObjectId expectedTargetObjectId = target.ObjectId;
        bool expectedTargetFocusState = target.IsFocused;

        SetControlledWorldObjects(controlledWorldObjects);
        PressFocusedSearchComplete();

        bool unchanged = retainedWorldObjects.Length == sourceWorldObjects.Entries.Count - 1
            && !controlledWorldObjects.Entries.Any(
                worldObject => worldObject.Identity.Id == targetObjectId)
            && ReferenceEquals(worldObjectController.WorldObjects, controlledWorldObjects)
            && ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            && ReferenceEquals(targetWorldObject.Identity, targetIdentity)
            && ReferenceEquals(targetWorldObject.Description, targetDescription)
            && ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            && ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            && ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            && focusedSearchIndicator.Visible
            && ReferenceEquals(controller.GetTarget(expectedTargetIndex), target)
            && ReferenceEquals(target.ObjectId, expectedTargetObjectId)
            && target.IsFocused == expectedTargetFocusState
            && AssertResolvedPerceptionIndicators(
                expectedTargetIndex,
                "absent-world-object completion")
            && AssertFocusedSearchResult(
                CompleteUnchangedText,
                "absent-world-object completion");

        SetControlledWorldObjects(sourceWorldObjects);
        if (!unchanged
            || !ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects))
        {
            Fail(
                "Interaction-focus smoke changed focused-search, world-object, target, or "
                + "presentation state while the active target object was absent.");
            return false;
        }

        return true;
    }

    private bool AssertMismatchedWorldObjectResolutionIsRejected(
        FocusedSearchOutcome outcome,
        ObjectId targetObjectId,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception,
        int expectedIdentifiedCount)
    {
        WorldObjectSnapshot expectedWorldObjects = worldObjectSource.WorldObjects;
        WorldObject targetWorldObject =
            worldObjectController.FindWorldObject(targetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the target world object.");
        WorldObject mismatchedWorldObject = expectedWorldObjects.Entries
            .Take(FocusedWorldObjectCount)
            .Single(worldObject => worldObject.Identity.Id != targetObjectId);
        WorldObjectIdentity targetIdentity = targetWorldObject.Identity;
        WorldObjectDescription targetDescription = targetWorldObject.Description;
        WorldObjectIdentity mismatchedIdentity = mismatchedWorldObject.Identity;
        WorldObjectDescription mismatchedDescription = mismatchedWorldObject.Description;
        GreyboxInteractionTarget[] expectedTargets = Enumerable.Range(0, controller.TargetCount)
            .Select(controller.GetTarget)
            .ToArray();
        ObjectId[] expectedTargetObjectIds = expectedTargets
            .Select(target => target.ObjectId)
            .ToArray();
        bool[] expectedTargetFocusStates = expectedTargets
            .Select(target => target.IsFocused)
            .ToArray();
        string expectedResultText = focusedSearchResultStatus.Text;

        bool changed = focusedSearchController.TryResolveOutcome(
            outcome,
            mismatchedWorldObject);

        bool unchanged = !changed
            && ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(worldObjectSource.WorldObjects, expectedWorldObjects)
            && ReferenceEquals(
                worldObjectController.FindWorldObject(targetObjectId),
                targetWorldObject)
            && ReferenceEquals(
                worldObjectController.FindWorldObject(mismatchedIdentity.Id),
                mismatchedWorldObject)
            && ReferenceEquals(targetWorldObject.Identity, targetIdentity)
            && ReferenceEquals(targetWorldObject.Description, targetDescription)
            && ReferenceEquals(mismatchedWorldObject.Identity, mismatchedIdentity)
            && ReferenceEquals(mismatchedWorldObject.Description, mismatchedDescription)
            && ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            && ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            && ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            && expectedTargets.Select(
                    (target, index) =>
                        ReferenceEquals(controller.GetTarget(index), target)
                        && ReferenceEquals(target.ObjectId, expectedTargetObjectIds[index])
                        && target.IsFocused == expectedTargetFocusStates[index])
                .All(isRetained => isRetained)
            && !focusedSearchIndicator.Visible
            && AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount,
                "target-mismatched world-object resolution")
            && AssertFocusedSearchResult(
                expectedResultText,
                "target-mismatched world-object resolution");
        if (!unchanged)
        {
            Fail(
                "Interaction-focus smoke changed world-object, focused-search, target, or "
                + "presentation state for a target-mismatched world object.");
            return false;
        }

        return true;
    }

    private bool AssertAbsentWorldObjectResolutionIsGated(
        ObjectId targetObjectId,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception,
        int expectedIdentifiedCount)
    {
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        if (!ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects))
        {
            Fail("Interaction-focus smoke lost the scene world-object snapshot.");
            return false;
        }

        WorldObject[] retainedWorldObjects = sourceWorldObjects.Entries
            .Where(worldObject => worldObject.Identity.Id != targetObjectId)
            .ToArray();
        WorldObjectSnapshot controlledWorldObjects = new(retainedWorldObjects);
        GreyboxInteractionTarget[] retainedTargets = Enumerable.Range(0, controller.TargetCount)
            .Select(controller.GetTarget)
            .ToArray();
        ObjectId[] retainedTargetObjectIds = retainedTargets
            .Select(target => target.ObjectId)
            .ToArray();
        bool[] retainedTargetFocusStates = retainedTargets
            .Select(target => target.IsFocused)
            .ToArray();

        SetControlledWorldObjects(controlledWorldObjects);
        PressFocusedSearchResolve();

        bool unchanged =
            retainedWorldObjects.Length == sourceWorldObjects.Entries.Count - 1
            && !controlledWorldObjects.Entries.Any(
                worldObject => worldObject.Identity.Id == targetObjectId)
            && retainedWorldObjects.All(
                worldObject => sourceWorldObjects.Entries.Contains(worldObject))
            && ReferenceEquals(worldObjectController.WorldObjects, controlledWorldObjects)
            && ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            && ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            && ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            && ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            && !focusedSearchIndicator.Visible
            && retainedTargets.Select(
                    (target, index) =>
                        ReferenceEquals(controller.GetTarget(index), target)
                        && ReferenceEquals(target.ObjectId, retainedTargetObjectIds[index])
                        && target.IsFocused == retainedTargetFocusStates[index])
                .All(isRetained => isRetained)
            && AssertResolvedPerceptionIndicators(
                expectedIdentifiedCount,
                "absent-world-object resolution")
            && AssertFocusedSearchResult(
                ResolveUnchangedText,
                "absent-world-object resolution");

        SetControlledWorldObjects(sourceWorldObjects);
        if (!unchanged
            || !ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects))
        {
            Fail(
                "Interaction-focus smoke changed state while the recorded outcome target was "
                + "absent from the controlled world-object snapshot.");
            return false;
        }

        return true;
    }

    private void SetControlledWorldObjects(WorldObjectSnapshot worldObjects)
    {
        System.Reflection.MethodInfo setter = typeof(WorldObjectController)
            .GetProperty(nameof(WorldObjectController.WorldObjects))?
            .GetSetMethod(nonPublic: true)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the private world-object snapshot setter.");

        setter.Invoke(worldObjectController, [worldObjects]);
    }

    private bool AssertMissingWorldObjectRefreshHidesOnlyTarget(
        ObjectId targetObjectId,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception,
        int expectedTargetIndex)
    {
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        WorldObject[] retainedWorldObjects = sourceWorldObjects.Entries
            .Where(worldObject => worldObject.Identity.Id != targetObjectId)
            .ToArray();
        WorldObjectSnapshot controlledWorldObjects = new(retainedWorldObjects);
        GreyboxInteractionTarget[] expectedTargets = Enumerable.Range(0, controller.TargetCount)
            .Select(controller.GetTarget)
            .ToArray();
        ObjectId[] expectedTargetObjectIds = expectedTargets
            .Select(target => target.ObjectId)
            .ToArray();
        bool[] expectedTargetFocusStates = expectedTargets
            .Select(target => target.IsFocused)
            .ToArray();
        string expectedResultText = focusedSearchResultStatus.Text;

        SetControlledWorldObjects(controlledWorldObjects);
        RefreshResolvedPerceptionPresentation();
        bool hiddenWithoutStateChange =
            controlledWorldObjects.Entries.Count == retainedWorldObjects.Length
            && retainedWorldObjects.Select(
                    (worldObject, index) => ReferenceEquals(
                        controlledWorldObjects.Entries[index],
                        worldObject))
                .All(isRetained => isRetained)
            && !controlledWorldObjects.Entries.Any(
                worldObject => worldObject.Identity.Id == targetObjectId)
            && ReferenceEquals(worldObjectController.WorldObjects, controlledWorldObjects)
            && ReferenceEquals(worldObjectSource.WorldObjects, sourceWorldObjects)
            && ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            && ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            && ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            && expectedTargets.Select(
                    (target, index) =>
                        ReferenceEquals(controller.GetTarget(index), target)
                        && ReferenceEquals(target.ObjectId, expectedTargetObjectIds[index])
                        && target.IsFocused == expectedTargetFocusStates[index])
                .All(isRetained => isRetained)
            && AssertResolvedPerceptionIndicators(
                expectedTargetIndex,
                "missing-world-object presentation refresh")
            && AssertFocusedSearchResult(
                expectedResultText,
                "missing-world-object presentation refresh");

        SetControlledWorldObjects(sourceWorldObjects);
        RefreshResolvedPerceptionPresentation();
        if (!hiddenWithoutStateChange
            || !ReferenceEquals(worldObjectController.WorldObjects, sourceWorldObjects)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || !AssertResolvedPerceptionIndicators(
                expectedTargetIndex + 1,
                "restored-world-object presentation refresh"))
        {
            Fail(
                "Interaction-focus smoke changed state or failed target-only presentation while "
                + "refreshing a missing world object.");
            return false;
        }

        return true;
    }

    private void RefreshResolvedPerceptionPresentation()
    {
        System.Reflection.MethodInfo method = typeof(Main).GetMethod(
            "RefreshResolvedPerceptionPresentation",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the private perception refresh method.");

        method.Invoke(main, null);
    }

    private bool AssertResolvedPerceptionPresentationContract()
    {
        CharacterPerceptionSnapshot initialPerception =
            focusedSearchController.Perception
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires initialized Seeker perception.");
        TemporaryResolvedPerceptionIndicator indicator = resolvedPerceptionIndicators[0];
        ObjectId targetObjectId = sourceObjectIds[0];
        ObjectId unrelatedObjectId = sourceObjectIds[1];
        WorldObject targetWorldObject =
            worldObjectController.FindWorldObject(targetObjectId)
            ?? throw new InvalidOperationException(
                "The interaction-focus smoke requires the target world object.");
        WorldObjectIdentity targetIdentity = targetWorldObject.Identity;
        WorldObjectDescription targetDescription = targetWorldObject.Description;
        WorldObject absentWorldObject = new(
            new WorldObjectIdentity(new ObjectId("absent-object"), "Absent object"),
            new WorldObjectDescription(string.Empty));
        ObjectId valueEqualTargetObjectId = new(targetObjectId.Value);
        WorldObject valueEqualTargetWorldObject = new(
            new WorldObjectIdentity(valueEqualTargetObjectId, "Value-equal target"),
            new WorldObjectDescription("Replacement target description."));
        CharacterPerceptionSnapshot identifiedPerception = new(
            initialPerception.CharacterId,
            [new WorldObjectVisibility(targetObjectId, VisibilityState.Identified)]);

        if (!AssertResolvedPerceptionIndicatorStates(
                [false, true],
                "initial observation-reinterpretation presentation"))
        {
            return false;
        }

        if (!AssertResolvedDescriptionClears(
                indicator,
                targetWorldObject,
                identifiedPerception,
                null,
                initialPerception,
                "null target"))
        {
            return false;
        }

        if (!AssertResolvedDescriptionClears(
                indicator,
                targetWorldObject,
                identifiedPerception,
                targetWorldObject,
                null,
                "null perception"))
        {
            return false;
        }

        CharacterPerceptionSnapshot unrelatedPerception = new(
            initialPerception.CharacterId,
            [new WorldObjectVisibility(unrelatedObjectId, VisibilityState.Identified)]);
        if (!AssertResolvedDescriptionClears(
                indicator,
                targetWorldObject,
                identifiedPerception,
                targetWorldObject,
                unrelatedPerception,
                "unrelated perception entry"))
        {
            return false;
        }

        if (!AssertResolvedDescriptionClears(
                indicator,
                targetWorldObject,
                identifiedPerception,
                absentWorldObject,
                initialPerception,
                "absent object"))
        {
            return false;
        }

        foreach (VisibilityState nonIdentifiedState in VisibilityState.All.Where(
                     state => !ReferenceEquals(state, VisibilityState.Identified)))
        {
            CharacterPerceptionSnapshot nonIdentifiedPerception = new(
                initialPerception.CharacterId,
                [new WorldObjectVisibility(targetObjectId, nonIdentifiedState)]);
            if (!AssertResolvedDescriptionClears(
                    indicator,
                    targetWorldObject,
                    identifiedPerception,
                    targetWorldObject,
                    nonIdentifiedPerception,
                    $"{nonIdentifiedState.Value} visibility"))
            {
                return false;
            }
        }

        indicator.Present(targetWorldObject, identifiedPerception);
        if (!indicator.IsIndicatorVisible
            || !string.Equals(
                GetResolvedPerceptionIndicatorText(0),
                targetDescription.Text,
                StringComparison.Ordinal))
        {
            Fail("Interaction-focus smoke failed an exact identified target description.");
            return false;
        }

        WorldObjectIdentity valueEqualIdentity = valueEqualTargetWorldObject.Identity;
        WorldObjectDescription valueEqualDescription = valueEqualTargetWorldObject.Description;
        indicator.Present(valueEqualTargetWorldObject, identifiedPerception);
        if (!indicator.IsIndicatorVisible
            || !string.Equals(
                GetResolvedPerceptionIndicatorText(0),
                valueEqualDescription.Text,
                StringComparison.Ordinal)
            || ReferenceEquals(valueEqualTargetObjectId, targetObjectId)
            || valueEqualTargetObjectId != targetObjectId
            || !ReferenceEquals(valueEqualTargetWorldObject.Identity, valueEqualIdentity)
            || !ReferenceEquals(valueEqualTargetWorldObject.Description, valueEqualDescription)
            || !ReferenceEquals(valueEqualIdentity.Id, valueEqualTargetObjectId))
        {
            Fail(
                "Interaction-focus smoke failed replacement description presentation for a "
                + "world object with a distinct value-equal identifier.");
            return false;
        }

        if (!ReferenceEquals(targetWorldObject.Identity, targetIdentity)
            || !ReferenceEquals(targetWorldObject.Description, targetDescription)
            || !ReferenceEquals(targetWorldObject.Identity.Id, targetObjectId)
            || !ReferenceEquals(identifiedPerception.Entries[0].ObjectId, targetObjectId)
            || !ReferenceEquals(
                identifiedPerception.Entries[0].VisibilityState,
                VisibilityState.Identified))
        {
            Fail("Interaction-focus smoke changed presentation world-object or perception input.");
            return false;
        }

        CharacterPerceptionSnapshot noticedBaseline = new(
            initialPerception.CharacterId,
            initialPerception.Entries.Select(
                entry => new WorldObjectVisibility(
                    entry.ObjectId,
                    VisibilityState.Noticed)));
        SetFocusedSearchControllerPerception(noticedBaseline);
        RefreshResolvedPerceptionPresentation();
        return AssertResolvedPerceptionIndicators(0, "presentation-contract restoration");
    }

    private bool AssertResolvedPerceptionIndicatorStates(
        IReadOnlyList<bool> expectedVisibility,
        string scenario)
    {
        if (expectedVisibility.Count != resolvedPerceptionIndicators.Length)
        {
            throw new InvalidOperationException(
                "Resolved-perception visibility expectations must cover every indicator.");
        }

        for (int index = 0; index < resolvedPerceptionIndicators.Length; index++)
        {
            bool expectedVisible = expectedVisibility[index];
            string expectedDescription = expectedVisible
                ? worldObjectController.FindWorldObject(sourceObjectIds[index])?.Description.Text
                    ?? throw new InvalidOperationException(
                        "Identified presentation requires an existing world object.")
                : string.Empty;
            string actualDescription = GetResolvedPerceptionIndicatorText(index);
            if (resolvedPerceptionIndicators[index].IsIndicatorVisible != expectedVisible
                || !string.Equals(
                    actualDescription,
                    expectedDescription,
                    StringComparison.Ordinal))
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} identified "
                    + $"presentation was "
                    + $"{resolvedPerceptionIndicators[index].IsIndicatorVisible} with text "
                    + $"'{actualDescription}'.");
                return false;
            }
        }

        return true;
    }

    private bool AssertResolvedPerceptionIndicators(
        int expectedIdentifiedCount,
        string scenario)
    {
        for (int index = 0; index < resolvedPerceptionIndicators.Length; index++)
        {
            bool expectedVisible = index < expectedIdentifiedCount;
            string expectedDescription = expectedVisible
                ? worldObjectController.FindWorldObject(sourceObjectIds[index])?.Description.Text
                    ?? throw new InvalidOperationException(
                        "Identified presentation requires an existing world object.")
                : string.Empty;
            string actualDescription = GetResolvedPerceptionIndicatorText(index);
            if (resolvedPerceptionIndicators[index].IsIndicatorVisible != expectedVisible
                || !string.Equals(
                    actualDescription,
                    expectedDescription,
                    StringComparison.Ordinal))
            {
                Fail(
                    $"Interaction-focus smoke failed {scenario}: target {index} identified "
                    + $"presentation was "
                    + $"{resolvedPerceptionIndicators[index].IsIndicatorVisible} with text "
                    + $"'{actualDescription}'.");
                return false;
            }
        }

        return true;
    }

    private string GetResolvedPerceptionIndicatorText(int index) =>
        resolvedPerceptionIndicators[index].GetNode<Label>("Indicator").Text;

    private void SetFocusedSearchControllerPerception(CharacterPerceptionSnapshot perception)
    {
        System.Reflection.PropertyInfo property = typeof(FocusedSearchActivityController)
            .GetProperty(nameof(FocusedSearchActivityController.Perception))
            ?? throw new InvalidOperationException(
                "The focused-search controller perception property was not found.");
        property.SetValue(focusedSearchController, perception);
    }

    private bool AssertResolvedDescriptionClears(
        TemporaryResolvedPerceptionIndicator indicator,
        WorldObject identifiedTarget,
        CharacterPerceptionSnapshot identifiedPerception,
        WorldObject? nextTarget,
        CharacterPerceptionSnapshot? nextPerception,
        string scenario)
    {
        indicator.Present(identifiedTarget, identifiedPerception);
        if (!indicator.IsIndicatorVisible
            || !string.Equals(
                GetResolvedPerceptionIndicatorText(0),
                identifiedTarget.Description.Text,
                StringComparison.Ordinal))
        {
            Fail($"Interaction-focus smoke failed {scenario} clearing setup.");
            return false;
        }

        indicator.Present(nextTarget, nextPerception);
        if (indicator.IsIndicatorVisible
            || !string.IsNullOrEmpty(GetResolvedPerceptionIndicatorText(0)))
        {
            Fail($"Interaction-focus smoke retained description presentation for {scenario}.");
            return false;
        }

        return true;
    }

    private bool AssertCompletedOutcome(
        FocusedSearchOutcomeSnapshot previousOutcomes,
        FocusedSearchOutcomeSnapshot completedOutcomes,
        FocusedSearchRequest completedRequest)
    {
        if (ReferenceEquals(completedOutcomes, previousOutcomes)
            || completedOutcomes.Entries.Count != previousOutcomes.Entries.Count + 1)
        {
            Fail("Interaction-focus smoke failed focused-search outcome recording.");
            return false;
        }

        for (int index = 0; index < previousOutcomes.Entries.Count; index++)
        {
            if (!ReferenceEquals(
                    completedOutcomes.Entries[index],
                    previousOutcomes.Entries[index]))
            {
                Fail("Interaction-focus smoke replaced a previously recorded outcome.");
                return false;
            }
        }

        FocusedSearchOutcome outcome = completedOutcomes.Entries[^1];
        if (!ReferenceEquals(outcome.Request, completedRequest)
            || !ReferenceEquals(outcome.ResultingVisibilityState, VisibilityState.Identified))
        {
            Fail(
                "Interaction-focus smoke failed exact request and identified-state outcome "
                + "retention.");
            return false;
        }

        return true;
    }

    private static bool AssertResolvedPerception(
        CharacterPerceptionSnapshot previousPerception,
        CharacterPerceptionSnapshot resolvedPerception,
        ObjectId targetObjectId)
    {
        if (!ReferenceEquals(resolvedPerception.CharacterId, previousPerception.CharacterId)
            || resolvedPerception.Entries.Count != previousPerception.Entries.Count)
        {
            return false;
        }

        for (int index = 0; index < previousPerception.Entries.Count; index++)
        {
            WorldObjectVisibility previousEntry = previousPerception.Entries[index];
            WorldObjectVisibility resolvedEntry = resolvedPerception.Entries[index];
            if (previousEntry.ObjectId == targetObjectId)
            {
                if (ReferenceEquals(resolvedEntry, previousEntry)
                    || !ReferenceEquals(resolvedEntry.ObjectId, previousEntry.ObjectId)
                    || !ReferenceEquals(
                        resolvedEntry.VisibilityState,
                        VisibilityState.Identified))
                {
                    return false;
                }
            }
            else if (!ReferenceEquals(resolvedEntry, previousEntry))
            {
                return false;
            }
        }

        return true;
    }

    private bool AssertNonSearchInputsPreserveResult(
        string expectedText,
        FocusedSearchActivitySnapshot expectedActivities,
        FocusedSearchOutcomeSnapshot expectedOutcomes,
        CharacterPerceptionSnapshot expectedPerception)
    {
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName("move_left"),
                Pressed = true,
            });
        if (!AssertFocusedSearchResult(expectedText, "movement input preservation"))
        {
            return false;
        }

        PressAction(HealerToggleAction);
        if (healer.CommandMode != HealerCommandMode.Wait
            || !AssertFocusedSearchResult(expectedText, "healer wait input preservation"))
        {
            Fail("Interaction-focus smoke failed unchanged healer wait behavior.");
            return false;
        }

        PressAction(HealerToggleAction);
        if (healer.CommandMode != HealerCommandMode.Follow
            || !AssertFocusedSearchResult(expectedText, "healer follow input preservation"))
        {
            Fail("Interaction-focus smoke failed unchanged healer follow behavior.");
            return false;
        }

        PressAction(HealerMoveAction);
        if (healer.CommandMode != HealerCommandMode.MoveToDestination
            || !AssertFocusedSearchResult(expectedText, "healer move input preservation"))
        {
            Fail("Interaction-focus smoke failed unchanged healer move behavior.");
            return false;
        }

        PressAction(HealerToggleAction);
        if (healer.CommandMode != HealerCommandMode.Follow
            || !AssertFocusedSearchResult(expectedText, "healer move reset preservation"))
        {
            Fail("Interaction-focus smoke failed unchanged healer move reset behavior.");
            return false;
        }

        if (!ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || focusedSearchIndicator.Visible
            || !AssertResolvedPerceptionIndicators(
                expectedPerception.Entries.Count(
                    entry => entry.VisibilityState == VisibilityState.Identified),
                "non-search input preservation"))
        {
            Fail("Interaction-focus smoke failed non-search input state preservation.");
            return false;
        }

        return true;
    }

    private bool AssertFocusedSearchResult(string expectedText, string scenario)
    {
        if (string.Equals(focusedSearchResultStatus.Text, expectedText, StringComparison.Ordinal))
        {
            return true;
        }

        Fail(
            $"Interaction-focus smoke failed {scenario}: expected focused-search result "
            + $"'{expectedText}', received '{focusedSearchResultStatus.Text}'.");
        return false;
    }

    private bool AssertFocusedSearchInputActions()
    {
        if (!HasOnlyPhysicalKey(FocusedSearchStartAction, Key.F)
            || !HasOnlyPhysicalKey(FocusedSearchCancelAction, Key.C)
            || !HasOnlyPhysicalKey(FocusedSearchCompleteAction, Key.E)
            || !HasOnlyPhysicalKey(FocusedSearchResolveAction, Key.R))
        {
            Fail(
                "Interaction-focus smoke failed focused-search input mappings: expected only "
                + "physical F for start, physical C for cancellation, and physical E for "
                + "completion, and physical R for resolution.");
            return false;
        }

        return true;
    }

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private bool AssertTemporaryInstructions()
    {
        if (string.Equals(instructions.Text, TemporaryInstructionsText, StringComparison.Ordinal))
        {
            return true;
        }

        Fail(
            "Interaction-focus smoke failed temporary instructions: expected "
            + $"'{TemporaryInstructionsText}', received '{instructions.Text}'.");
        return false;
    }

    private void PressFocusedSearchCancel()
    {
        PressAction(FocusedSearchCancelAction);
    }

    private void PressFocusedSearchComplete()
    {
        PressAction(FocusedSearchCompleteAction);
    }

    private void PressFocusedSearchStart()
    {
        PressAction(FocusedSearchStartAction);
    }

    private void PressFocusedSearchResolve()
    {
        PressAction(FocusedSearchResolveAction);
    }

    private void PressAction(string action)
    {
        main._UnhandledInput(
            new InputEventAction
            {
                Action = new StringName(action),
                Pressed = true,
            });
    }

    private bool AssertAuthoredFocusedSearchContext()
    {
        WorldObjectSnapshot sourceWorldObjects = worldObjectSource.WorldObjects;
        WorldObject sourceHerbPatch = sourceWorldObjects.Entries[0];
        WorldObject sourceStoneMarker = sourceWorldObjects.Entries[1];
        WorldObject sourceWaterCask = sourceWorldObjects.Entries[2];
        WorldObject sourceWagonClue = sourceWorldObjects.Entries[3];
        authoredConditions = focusedSearchContextSource.Conditions;
        authoredCapabilities = focusedSearchContextSource.Capabilities;
        authoredInventory = focusedSearchContextSource.Inventory;
        authoredCapabilityRequirements = authoredConditions.CapabilityRequirements;
        authoredToolRequirements = authoredConditions.ToolRequirements;
        if (authoredCapabilityRequirements.Entries.Count != 2
            || authoredToolRequirements.Entries.Count != 2
            || authoredCapabilities.Entries.Count != 2
            || authoredInventory.ObjectReferences.Count != 1)
        {
            Fail(
                "Interaction-focus smoke failed focused-search context authoring: expected two "
                + "capability requirements and capabilities plus two tool requirements and tool.");
            return false;
        }

        authoredCapabilityRequirement = authoredCapabilityRequirements.Entries[0];
        authoredWaterCaskCapabilityRequirement = authoredCapabilityRequirements.Entries[1];
        authoredToolRequirement = authoredToolRequirements.Entries[0];
        authoredCapability = authoredCapabilities.Entries[0];
        authoredWaterCaskCapability = authoredCapabilities.Entries[1];
        authoredToolObjectId = authoredInventory.ObjectReferences[0];

        bool isStable = ReferenceEquals(
                authoredCapabilityRequirement.ObjectId,
                sourceHerbPatch.Identity.Id)
            && string.Equals(
                authoredCapabilityRequirement.RequiredCapabilityKey,
                authoredCapability.Key,
                StringComparison.Ordinal)
            && string.Equals(
                authoredCapability.Key,
                "identify-healing-plants",
                StringComparison.Ordinal)
            && string.Equals(
                authoredCapability.Description,
                "Identifies healing plants.",
                StringComparison.Ordinal)
            && ReferenceEquals(
                authoredWaterCaskCapabilityRequirement.ObjectId,
                sourceWaterCask.Identity.Id)
            && string.Equals(
                authoredWaterCaskCapabilityRequirement.RequiredCapabilityKey,
                authoredWaterCaskCapability.Key,
                StringComparison.Ordinal)
            && string.Equals(
                authoredWaterCaskCapability.Key,
                "inspect-water-sources",
                StringComparison.Ordinal)
            && string.Equals(
                authoredWaterCaskCapability.Description,
                "Inspects water sources for tampering.",
                StringComparison.Ordinal)
            && ReferenceEquals(authoredToolRequirement.ObjectId, sourceStoneMarker.Identity.Id)
            && ReferenceEquals(
                authoredToolRequirement.RequiredToolObjectId,
                authoredToolObjectId)
            && ReferenceEquals(
                authoredToolRequirements.Entries[1].ObjectId,
                sourceWagonClue.Identity.Id)
            && ReferenceEquals(
                authoredToolRequirements.Entries[1].RequiredToolObjectId,
                authoredToolObjectId)
            && string.Equals(authoredToolObjectId.Value, "field-lens", StringComparison.Ordinal);
        if (!isStable)
        {
            Fail(
                "Interaction-focus smoke failed focused-search context authoring: target, "
                + "capability, or tool instances were not retained exactly.");
            return false;
        }

        ObjectId independentFirstObjectId = new("independent-first-object");
        ObjectId independentSecondObjectId = new("independent-second-object");
        ObjectId independentUnrelatedObjectId = new("independent-unrelated-object");
        WorldObject independentFirstObject = CreateWorldObject(
            independentFirstObjectId,
            "Independent first object");
        WorldObject independentSecondObject = CreateWorldObject(
            independentSecondObjectId,
            "Independent second object");
        WorldObject independentUnrelatedObject = CreateWorldObject(
            independentUnrelatedObjectId,
            "Independent unrelated object");
        WorldObjectIdentity independentFirstIdentity = independentFirstObject.Identity;
        WorldObjectDescription independentFirstDescription = independentFirstObject.Description;
        WorldObjectIdentity independentSecondIdentity = independentSecondObject.Identity;
        WorldObjectDescription independentSecondDescription = independentSecondObject.Description;
        WorldObjectIdentity independentUnrelatedIdentity = independentUnrelatedObject.Identity;
        WorldObjectDescription independentUnrelatedDescription =
            independentUnrelatedObject.Description;
        WorldObjectSnapshot independentWorldObjects = new(
            [independentFirstObject, independentSecondObject, independentUnrelatedObject]);
        (
            DiscoveryConditionSet independentConditions,
            CharacterCapabilitySet independentCapabilities,
            CharacterInventory independentInventory) = CreateFocusedSearchContext(
                independentWorldObjects);

        DiscoveryCapabilityRequirement independentCapabilityRequirement =
            independentConditions.CapabilityRequirements.Entries[0];
        DiscoveryCapabilityRequirement independentWaterCaskRequirement =
            independentConditions.CapabilityRequirements.Entries[1];
        DiscoveryToolRequirement independentToolRequirement =
            independentConditions.ToolRequirements.Entries.Single();
        CharacterCapability independentCapability = independentCapabilities.Entries[0];
        CharacterCapability independentWaterCaskCapability = independentCapabilities.Entries[1];
        ObjectId independentToolObjectId = independentInventory.ObjectReferences.Single();
        bool isWorldObjectDerived = ReferenceEquals(
                independentCapabilityRequirement.ObjectId,
                independentFirstObjectId)
            && ReferenceEquals(independentToolRequirement.ObjectId, independentSecondObjectId)
            && string.Equals(
                independentCapabilityRequirement.RequiredCapabilityKey,
                independentCapability.Key,
                StringComparison.Ordinal)
            && string.Equals(
                independentCapability.Key,
                "identify-healing-plants",
                StringComparison.Ordinal)
            && string.Equals(
                independentCapability.Description,
                "Identifies healing plants.",
                StringComparison.Ordinal)
            && ReferenceEquals(
                independentWaterCaskRequirement.ObjectId,
                independentUnrelatedObjectId)
            && string.Equals(
                independentWaterCaskRequirement.RequiredCapabilityKey,
                independentWaterCaskCapability.Key,
                StringComparison.Ordinal)
            && string.Equals(
                independentWaterCaskCapability.Key,
                "inspect-water-sources",
                StringComparison.Ordinal)
            && string.Equals(
                independentWaterCaskCapability.Description,
                "Inspects water sources for tampering.",
                StringComparison.Ordinal)
            && ReferenceEquals(
                independentToolRequirement.RequiredToolObjectId,
                independentToolObjectId)
            && string.Equals(
                independentToolObjectId.Value,
                "field-lens",
                StringComparison.Ordinal)
            && ReferenceEquals(
                independentConditions.CapabilityRequirements.Entries[0],
                independentCapabilityRequirement)
            && ReferenceEquals(
                independentConditions.ToolRequirements.Entries[0],
                independentToolRequirement)
            && ReferenceEquals(
                independentConditions.CapabilityRequirements.Entries[1],
                independentWaterCaskRequirement)
            && ReferenceEquals(independentCapabilities.Entries[0], independentCapability)
            && ReferenceEquals(
                independentCapabilities.Entries[1],
                independentWaterCaskCapability)
            && ReferenceEquals(
                independentInventory.ObjectReferences[0],
                independentToolObjectId)
            && ReferenceEquals(independentWorldObjects.Entries[0], independentFirstObject)
            && ReferenceEquals(independentWorldObjects.Entries[1], independentSecondObject)
            && ReferenceEquals(independentWorldObjects.Entries[2], independentUnrelatedObject)
            && ReferenceEquals(independentFirstObject.Identity, independentFirstIdentity)
            && ReferenceEquals(independentFirstObject.Description, independentFirstDescription)
            && ReferenceEquals(independentSecondObject.Identity, independentSecondIdentity)
            && ReferenceEquals(independentSecondObject.Description, independentSecondDescription)
            && ReferenceEquals(independentUnrelatedObject.Identity, independentUnrelatedIdentity)
            && ReferenceEquals(
                independentUnrelatedObject.Description,
                independentUnrelatedDescription)
            && ReferenceEquals(independentFirstObject.Identity.Id, independentFirstObjectId)
            && ReferenceEquals(independentSecondObject.Identity.Id, independentSecondObjectId)
            && ReferenceEquals(
                independentUnrelatedObject.Identity.Id,
                independentUnrelatedObjectId)
            && ThrowsForInsufficientWorldObjects(
                new WorldObjectSnapshot([independentFirstObject, independentSecondObject]));
        if (!isWorldObjectDerived)
        {
            Fail(
                "Interaction-focus smoke failed focused-search context authoring from an "
                + "independent world-object snapshot.");
            return false;
        }

        return true;
    }

    private (
        DiscoveryConditionSet Conditions,
        CharacterCapabilitySet Capabilities,
        CharacterInventory Inventory) CreateFocusedSearchContext(
            WorldObjectSnapshot worldObjects)
    {
        System.Reflection.MethodInfo? method = typeof(TemporaryFocusedSearchContextSource)
            .GetMethod(
                "CreateContext",
                System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.NonPublic);
        if (method is null)
        {
            Fail("Interaction-focus smoke found no world-object context derivation method.");
            return default;
        }

        return ((
            DiscoveryConditionSet Conditions,
            CharacterCapabilitySet Capabilities,
            CharacterInventory Inventory))(method.Invoke(null, [worldObjects])
                ?? throw new InvalidOperationException(
                    "The temporary focused-search source returned no derived context."));
    }

    private bool ThrowsForInsufficientWorldObjects(WorldObjectSnapshot worldObjects)
    {
        try
        {
            _ = CreateFocusedSearchContext(worldObjects);
        }
        catch (System.Reflection.TargetInvocationException exception)
            when (exception.InnerException is InvalidOperationException)
        {
            return true;
        }

        return false;
    }

    private static WorldObject CreateWorldObject(ObjectId objectId, string displayName)
    {
        return new WorldObject(
            new WorldObjectIdentity(objectId, displayName),
            new WorldObjectDescription($"Description for {displayName}."));
    }

    private bool AssertFocusedSearchContextUnchanged()
    {
        DiscoveryConditionSet conditions = focusedSearchContextSource.Conditions;
        CharacterCapabilitySet capabilities = focusedSearchContextSource.Capabilities;
        CharacterInventory inventory = focusedSearchContextSource.Inventory;

        bool isUnchanged = ReferenceEquals(conditions, authoredConditions)
            && ReferenceEquals(capabilities, authoredCapabilities)
            && ReferenceEquals(inventory, authoredInventory)
            && ReferenceEquals(
                conditions.CapabilityRequirements,
                authoredCapabilityRequirements)
            && ReferenceEquals(conditions.ToolRequirements, authoredToolRequirements)
            && conditions.CapabilityRequirements.Entries.Count == 2
            && ReferenceEquals(
                conditions.CapabilityRequirements.Entries[0],
                authoredCapabilityRequirement)
            && ReferenceEquals(
                conditions.CapabilityRequirements.Entries[1],
                authoredWaterCaskCapabilityRequirement)
            && conditions.ToolRequirements.Entries.Count == 2
            && ReferenceEquals(
                conditions.ToolRequirements.Entries[0],
                authoredToolRequirement)
            && ReferenceEquals(
                conditions.ToolRequirements.Entries[1],
                authoredToolRequirements.Entries[1])
            && capabilities.Entries.Count == 2
            && ReferenceEquals(capabilities.Entries[0], authoredCapability)
            && ReferenceEquals(capabilities.Entries[1], authoredWaterCaskCapability)
            && inventory.ObjectReferences.Count == 1
            && ReferenceEquals(inventory.ObjectReferences[0], authoredToolObjectId)
            && ReferenceEquals(authoredCapabilityRequirement.ObjectId, sourceObjectIds[0])
            && ReferenceEquals(
                authoredWaterCaskCapabilityRequirement.ObjectId,
                worldObjectSource.WorldObjects.Entries[2].Identity.Id)
            && ReferenceEquals(authoredToolRequirement.ObjectId, sourceObjectIds[1])
            && ReferenceEquals(
                authoredToolRequirement.RequiredToolObjectId,
                authoredToolObjectId)
            && ReferenceEquals(
                authoredToolRequirements.Entries[1].ObjectId,
                worldObjectSource.WorldObjects.Entries[3].Identity.Id)
            && ReferenceEquals(
                authoredToolRequirements.Entries[1].RequiredToolObjectId,
                authoredToolObjectId);
        if (!isUnchanged)
        {
            Fail("Interaction-focus smoke failed eligible start: a supplied context input changed.");
        }

        return isUnchanged;
    }

    private bool AssertAuthoredObjectIdentifiers()
    {
        if (controller.TargetCount != ExpectedObjectIdValues.Length)
        {
            Fail(
                "Interaction-focus smoke failed stable identifier authoring: expected "
                + $"{ExpectedObjectIdValues.Length} targets, received {controller.TargetCount}.");
            return false;
        }

        authoredObjectIds = new ObjectId[controller.TargetCount];
        HashSet<string> distinctValues = new(StringComparer.Ordinal);
        for (int index = 0; index < controller.TargetCount; index++)
        {
            GreyboxInteractionTarget target = controller.GetTarget(index);
            authoredObjectIds[index] = target.ObjectId;
            if (!string.Equals(
                    target.ObjectId.Value,
                    ExpectedObjectIdValues[index],
                    StringComparison.Ordinal))
            {
                Fail(
                    $"Interaction-focus smoke failed stable identifier authoring: target {index} "
                    + $"exposed {target.ObjectId}.");
                return false;
            }

            if (!distinctValues.Add(target.ObjectId.Value))
            {
                Fail(
                    $"Interaction-focus smoke failed stable identifier authoring: target {index} "
                    + "duplicated an existing identifier.");
                return false;
            }
        }

        sourceObjectIds = worldObjectSource.WorldObjects.Entries
            .Take(FocusedWorldObjectCount)
            .Select(worldObject => worldObject.Identity.Id)
            .ToArray();
        if (sourceObjectIds.Length != FocusedWorldObjectCount)
        {
            Fail("Interaction-focus smoke found an unexpected temporary world-object count.");
            return false;
        }

        for (int index = 0; index < sourceObjectIds.Length; index++)
        {
            if (ReferenceEquals(sourceObjectIds[index], authoredObjectIds[index])
                || sourceObjectIds[index] != authoredObjectIds[index])
            {
                Fail(
                    $"Interaction-focus smoke failed independent identifier authoring at {index}.");
                return false;
            }
        }

        return true;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private enum SmokeStage
    {
        Synchronizing,
        RepresentativeSelection,
        EqualDistanceTie,
        FocusChange,
        OutOfRangeClearing,
        HiddenTarget,
        DisabledTarget,
    }
}
