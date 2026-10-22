using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search perception initialization checks when explicitly enabled.
/// </summary>
public partial class FocusedSearchPerceptionControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_FOCUSED_SEARCH_PERCEPTION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityController controller = null!;
    private TemporarySeekerPerceptionSource source = null!;
    private Line2D indicator = null!;
    private Label focusedSearchResultStatus = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        source = GetNode<TemporarySeekerPerceptionSource>("../TemporarySeekerPerceptionSource");
        indicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        focusedSearchResultStatus = GetNode<Label>(
            "../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        // A fresh launch resolves nothing at boot: the controller holds the authored perception.
        CharacterPerceptionSnapshot? bootPerception = controller.Perception;
        if (bootPerception is null
            || !ReferenceEquals(bootPerception, source.Perception)
            || !ReferenceEquals(bootPerception.CharacterId, controller.SearcherCharacterId)
            || !ReferenceEquals(
                bootPerception.Entries.Single(entry => entry.ObjectId.Value == "herb-patch")
                    .VisibilityState,
                VisibilityState.Unnoticed)
            || bootPerception.Entries.Any(entry =>
                ReferenceEquals(entry.VisibilityState, VisibilityState.Identified)))
        {
            Fail(
                "Focused-search perception controller smoke found evidence resolved at boot.");
            return;
        }

        SetPerception(null);
        if (!controller.TryInitializePerception(source.Perception))
        {
            Fail("Focused-search perception controller smoke rejected scene initialization.");
            return;
        }

        CharacterPerceptionSnapshot initializedPerception = controller.Perception!;
        if (!ReferenceEquals(initializedPerception, source.Perception)
            || !ReferenceEquals(initializedPerception.CharacterId, controller.SearcherCharacterId))
        {
            Fail(
                "Focused-search perception controller smoke failed exact scene-wired Seeker "
                + "perception retention.");
            return;
        }

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        bool expectedIndicatorVisibility = indicator.Visible;
        string expectedResultText = focusedSearchResultStatus.Text;

        CharacterPerceptionSnapshot wrongCharacterPerception = new(
            new CharacterId("healer"),
            []);
        if (controller.TryInitializePerception(wrongCharacterPerception))
        {
            Fail("Focused-search perception controller smoke accepted a wrong character.");
            return;
        }

        if (controller.TryInitializePerception(source.Perception))
        {
            Fail("Focused-search perception controller smoke accepted repeated initialization.");
            return;
        }

        if (!RejectsNullInitialization())
        {
            return;
        }

        if (!ReferenceEquals(controller.Perception, initializedPerception)
            || !ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || indicator.Visible != expectedIndicatorVisibility
            || focusedSearchResultStatus.Text != expectedResultText)
        {
            Fail(
                "Focused-search perception controller smoke changed retained controller state "
                + "or presentation after rejected initialization.");
            return;
        }

        GD.Print(
            "Focused-search perception controller smoke passed: unresolved authored boot state; "
            + "exact isolated Seeker perception initialization; wrong-character, repeated, and "
            + "null rejection; and unchanged activity, outcome, and presentation state were "
            + "correct.");
        GetTree().Quit();
    }

    private bool RejectsNullInitialization()
    {
        try
        {
            controller.TryInitializePerception(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        Fail("Focused-search perception controller smoke failed null rejection.");
        return false;
    }

    private void SetPerception(CharacterPerceptionSnapshot? perception)
    {
        System.Reflection.PropertyInfo property = typeof(FocusedSearchActivityController)
            .GetProperty(nameof(FocusedSearchActivityController.Perception))
            ?? throw new InvalidOperationException(
                "Focused-search controller perception property was not found.");
        property.SetValue(controller, perception);
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
