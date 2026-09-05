using System.Globalization;
using Allrpcs.Application.Movement;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
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
    private const int MaximumSynchronizationFrames = 60;
    private const string StartChangedText = "Focused search START: CHANGED";
    private const string StartUnchangedText = "Focused search START: UNCHANGED";
    private const string CancelChangedText = "Focused search CANCEL: CHANGED";
    private const string CancelUnchangedText = "Focused search CANCEL: UNCHANGED";
    private const string CompleteChangedText = "Focused search COMPLETE: CHANGED";
    private const string CompleteUnchangedText = "Focused search COMPLETE: UNCHANGED";
    private const string ResolveChangedText = "Focused search RESOLVE: CHANGED";
    private const string ResolveUnchangedText = "Focused search RESOLVE: UNCHANGED";
    private const string TemporaryInstructionsText =
        "TEMP · F START · C CANCEL · E COMPLETE · R RESOLVE · X WAIT · M MOVE";
    private const string UnrelatedAction = "interaction_focus_smoke_unrelated";
    private static readonly string[] ExpectedObjectIdValues =
    [
        "herb-patch",
        "stone-marker",
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
    private Label focusedSearchResultStatus = null!;
    private Label instructions = null!;
    private Healer healer = null!;
    private Main main = null!;
    private CharacterBody2D seeker = null!;
    private ObjectId[] authoredObjectIds = [];
    private DiscoveryConditionSet authoredConditions = null!;
    private CharacterCapabilitySet authoredCapabilities = null!;
    private CharacterInventory authoredInventory = null!;
    private NavigationRegion2D navigationRegion = null!;
    private DiscoveryCapabilityRequirementSet authoredCapabilityRequirements = null!;
    private DiscoveryToolRequirementSet authoredToolRequirements = null!;
    private DiscoveryCapabilityRequirement authoredCapabilityRequirement = null!;
    private DiscoveryToolRequirement authoredToolRequirement = null!;
    private CharacterCapability authoredCapability = null!;
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
        focusedSearchResultStatus = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
        instructions = GetNode<Label>("../CommandOverlay/Panel/Instructions");
        healer = GetNode<Healer>("../Healer");
        main = GetNode<Main>("..");
        navigationRegion = GetNode<NavigationRegion2D>("../GreyboxNavigation");
        seeker = GetNode<CharacterBody2D>("../Seeker");

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
            && NavigationServer2D.MapGetIterationId(navigationMap) > 0;
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

        GreyboxInteractionTarget hiddenTarget = controller.GetTarget(2);
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
            + "stable request-identifier retention; stable capability-only and tool-only "
            + "focused-search context authoring; mapped start, cancellation, completion, and "
            + "resolution inputs; exact temporary control text; exact retained active requests; "
            + "both eligible starts and focus-independent cancellations and outcome-aware "
            + "completions; exact outcome "
            + "recording and resolution; target-only perception replacement; exact unrelated "
            + "entry retention; unchanged inputs; failure-state identity; activity presentation; "
            + "and initial, changed, and unchanged lifecycle-result presentation were correct.");
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
            || !ReferenceEquals(controller.FocusedTargetObjectId, authoredObjectIds[expectedIndex]))
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the focused object identifier "
                + "was not the exact authored target instance.");
            return false;
        }

        FocusedSearchRequest? request = controller.CreateFocusedSearchRequest(
            focusedSearchController.SearcherCharacterId);
        if (request is null
            || !ReferenceEquals(
                request.SearcherCharacterId,
                focusedSearchController.SearcherCharacterId)
            || !ReferenceEquals(request.TargetObjectId, authoredObjectIds[expectedIndex]))
        {
            Fail(
                $"Interaction-focus smoke failed {scenario}: the request did not retain the "
                + "exact searcher and focused target identifier instances.");
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

        if (controller.CreateFocusedSearchRequest(focusedSearchController.SearcherCharacterId)
            is not null)
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

    private bool AssertEligibleFocusedSearchStart(int expectedTargetIndex)
    {
        ObjectId targetObjectId = authoredObjectIds[expectedTargetIndex];

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
            || !AssertFocusedSearchResult(CancelUnchangedText, "no-active cancellation"))
        {
            Fail("Interaction-focus smoke failed no-active-search cancellation identity.");
            return false;
        }

        PressFocusedSearchComplete();
        if (!ReferenceEquals(focusedSearchController.Activities, initialSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
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
            || !AssertFocusedSearchResult(CancelChangedText, "successful cancellation"))
        {
            Fail("Interaction-focus smoke failed focused-search input cancellation.");
            return false;
        }

        PressFocusedSearchCancel();
        if (!ReferenceEquals(focusedSearchController.Activities, cancelledSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, initialOutcomes)
            || focusedSearchIndicator.Visible
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
            || !AssertFocusedSearchResult(CompleteChangedText, "successful completion"))
        {
            Fail("Interaction-focus smoke failed focused-search input completion.");
            return false;
        }

        PressFocusedSearchComplete();
        if (!ReferenceEquals(focusedSearchController.Activities, completedSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, completedOutcomes)
            || focusedSearchIndicator.Visible
            || !AssertFocusedSearchResult(CompleteUnchangedText, "repeated completion"))
        {
            Fail("Interaction-focus smoke failed repeated completion identity.");
            return false;
        }

        FocusedSearchOutcome recordedOutcome = completedOutcomes.Entries[^1];
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
            || focusedSearchIndicator.Visible
            || !AssertFocusedSearchResult(ResolveChangedText, "successful resolution"))
        {
            Fail("Interaction-focus smoke failed focused-search input resolution.");
            return false;
        }

        if (!ReferenceEquals(completedOutcomes.Entries[^1], recordedOutcome)
            || !ReferenceEquals(
                completedPerception.Entries.Single(
                    entry => entry.ObjectId == targetObjectId).VisibilityState,
                VisibilityState.Noticed))
        {
            Fail("Interaction-focus smoke changed completed resolution inputs.");
            return false;
        }

        PressFocusedSearchResolve();
        if (!ReferenceEquals(focusedSearchController.Activities, completedSnapshot)
            || !ReferenceEquals(focusedSearchController.Outcomes, resolvedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, resolvedPerception)
            || focusedSearchIndicator.Visible
            || !AssertFocusedSearchResult(ResolveUnchangedText, "repeated resolution"))
        {
            Fail("Interaction-focus smoke failed repeated resolution identity.");
            return false;
        }

        seeker.GlobalPosition = focusedPosition;
        controller.RefreshFocus();

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
            || focusedSearchIndicator.Visible)
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
        authoredConditions = focusedSearchContextSource.Conditions;
        authoredCapabilities = focusedSearchContextSource.Capabilities;
        authoredInventory = focusedSearchContextSource.Inventory;
        authoredCapabilityRequirements = authoredConditions.CapabilityRequirements;
        authoredToolRequirements = authoredConditions.ToolRequirements;
        if (authoredCapabilityRequirements.Entries.Count != 1
            || authoredToolRequirements.Entries.Count != 1
            || authoredCapabilities.Entries.Count != 1
            || authoredInventory.ObjectReferences.Count != 1)
        {
            Fail(
                "Interaction-focus smoke failed focused-search context authoring: expected one "
                + "capability requirement, tool requirement, capability, and inventory tool.");
            return false;
        }

        authoredCapabilityRequirement = authoredCapabilityRequirements.Entries[0];
        authoredToolRequirement = authoredToolRequirements.Entries[0];
        authoredCapability = authoredCapabilities.Entries[0];
        authoredToolObjectId = authoredInventory.ObjectReferences[0];

        bool isStable = ReferenceEquals(
                authoredCapabilityRequirement.ObjectId,
                authoredObjectIds[0])
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
            && ReferenceEquals(authoredToolRequirement.ObjectId, authoredObjectIds[1])
            && ReferenceEquals(
                authoredToolRequirement.RequiredToolObjectId,
                authoredToolObjectId)
            && string.Equals(authoredToolObjectId.Value, "field-lens", StringComparison.Ordinal);
        if (!isStable)
        {
            Fail(
                "Interaction-focus smoke failed focused-search context authoring: target, "
                + "capability, or tool instances were not retained exactly.");
            return false;
        }

        return true;
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
            && conditions.CapabilityRequirements.Entries.Count == 1
            && ReferenceEquals(
                conditions.CapabilityRequirements.Entries[0],
                authoredCapabilityRequirement)
            && conditions.ToolRequirements.Entries.Count == 1
            && ReferenceEquals(
                conditions.ToolRequirements.Entries[0],
                authoredToolRequirement)
            && capabilities.Entries.Count == 1
            && ReferenceEquals(capabilities.Entries[0], authoredCapability)
            && inventory.ObjectReferences.Count == 1
            && ReferenceEquals(inventory.ObjectReferences[0], authoredToolObjectId)
            && ReferenceEquals(authoredCapabilityRequirement.ObjectId, authoredObjectIds[0])
            && ReferenceEquals(authoredToolRequirement.ObjectId, authoredObjectIds[1])
            && ReferenceEquals(
                authoredToolRequirement.RequiredToolObjectId,
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
