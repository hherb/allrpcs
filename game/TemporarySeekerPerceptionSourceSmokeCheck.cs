using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary Seeker perception source checks when explicitly enabled.
/// </summary>
public partial class TemporarySeekerPerceptionSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_PERCEPTION_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporarySeekerPerceptionSource source = null!;
    private FocusedSearchActivityController controller = null!;
    private GreyboxInteractionTarget herbPatch = null!;
    private GreyboxInteractionTarget stoneMarker = null!;
    private Line2D indicator = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporarySeekerPerceptionSource>("../TemporarySeekerPerceptionSource");
        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        herbPatch = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch");
        stoneMarker = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/StoneMarker");
        indicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        bool expectedIndicatorVisibility = indicator.Visible;
        CharacterPerceptionSnapshot first = source.Perception;
        CharacterPerceptionSnapshot second = source.Perception;

        if (expectedActivities.Entries.Count != 0
            || expectedOutcomes.Entries.Count != 0
            || expectedIndicatorVisibility)
        {
            Fail(
                "Temporary Seeker perception source smoke found changed initial controller or "
                + "presentation state.");
            return;
        }

        if (!ReferenceEquals(first, second)
            || !ReferenceEquals(first.CharacterId, controller.SearcherCharacterId))
        {
            Fail("Temporary Seeker perception source smoke failed stable snapshot retention.");
            return;
        }

        if (first.Entries.Count != 2
            || !ReferenceEquals(first.Entries[0].ObjectId, herbPatch.ObjectId)
            || !ReferenceEquals(first.Entries[0].VisibilityState, VisibilityState.Noticed)
            || !ReferenceEquals(first.Entries[1].ObjectId, stoneMarker.ObjectId)
            || !ReferenceEquals(first.Entries[1].VisibilityState, VisibilityState.Noticed))
        {
            Fail(
                "Temporary Seeker perception source smoke failed exact ordered perception "
                + "retention.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || indicator.Visible != expectedIndicatorVisibility)
        {
            Fail(
                "Temporary Seeker perception source smoke changed controller state or "
                + "presentation.");
            return;
        }

        GD.Print(
            "Temporary Seeker perception source smoke passed: exact Seeker, authored marker, "
            + "order, noticed-state, stable snapshot, and unchanged controller and presentation "
            + "boundaries were correct.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
