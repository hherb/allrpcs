using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free focused-search indicator presentation checks when explicitly enabled.
/// </summary>
public partial class FocusedSearchActivityIndicatorSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_FOCUSED_SEARCH_INDICATOR_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private FocusedSearchActivityIndicator adapter = null!;
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

        adapter = GetNode<FocusedSearchActivityIndicator>(
            "../Seeker/FocusedSearchActivityIndicator");
        indicator = adapter.GetNode<Line2D>("Indicator");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterId seekerCharacterId = new("seeker");
        CharacterId unrelatedCharacterId = new("healer");
        ObjectId firstTargetId = new("concealed-herb");
        ObjectId secondTargetId = new("partial-inscription");
        ObjectId unrelatedTargetId = new("residue");
        FocusedSearchActivity firstMatchingActivity = CreateActivity(
            seekerCharacterId,
            firstTargetId);
        FocusedSearchActivity secondMatchingActivity = CreateActivity(
            seekerCharacterId,
            secondTargetId);
        FocusedSearchActivity unrelatedActivity = CreateActivity(
            unrelatedCharacterId,
            unrelatedTargetId);
        FocusedSearchActivitySnapshot emptySnapshot = new([]);
        FocusedSearchActivitySnapshot unrelatedSnapshot = new([unrelatedActivity]);
        FocusedSearchActivitySnapshot oneMatchingSnapshot = new([firstMatchingActivity]);
        FocusedSearchActivitySnapshot multipleMatchingSnapshot = new(
            [unrelatedActivity, firstMatchingActivity, secondMatchingActivity]);

        if (!AssertIndicatorState(expectedVisible: false, "hidden-by-default state"))
        {
            return;
        }

        adapter.Present(seekerCharacterId, emptySnapshot);
        if (!AssertIndicatorState(expectedVisible: false, "empty snapshot"))
        {
            return;
        }

        adapter.Present(seekerCharacterId, unrelatedSnapshot);
        if (!AssertIndicatorState(expectedVisible: false, "unrelated snapshot"))
        {
            return;
        }

        adapter.Present(seekerCharacterId, oneMatchingSnapshot);
        if (!AssertIndicatorState(expectedVisible: true, "one matching request"))
        {
            return;
        }

        adapter.Present(seekerCharacterId, multipleMatchingSnapshot);
        if (!AssertIndicatorState(expectedVisible: true, "multiple matching requests")
            || !AssertInputsUnchanged(
                seekerCharacterId,
                unrelatedCharacterId,
                firstTargetId,
                secondTargetId,
                unrelatedTargetId,
                emptySnapshot,
                unrelatedSnapshot,
                oneMatchingSnapshot,
                multipleMatchingSnapshot,
                unrelatedActivity,
                firstMatchingActivity,
                secondMatchingActivity))
        {
            return;
        }

        adapter.Present(seekerCharacterId, emptySnapshot);
        if (!AssertIndicatorState(expectedVisible: false, "subsequent empty snapshot"))
        {
            return;
        }

        GD.Print(
            "Focused-search activity indicator smoke passed: hidden-by-default, empty and "
            + "unrelated hiding, one and multiple matching visibility, subsequent hiding, "
            + "one authored indicator, and unchanged inputs were presented correctly.");
        GetTree().Quit();
    }

    private bool AssertIndicatorState(bool expectedVisible, string scenario)
    {
        if (indicator.Visible != expectedVisible)
        {
            Fail(
                $"Focused-search indicator smoke failed {scenario}: expected visibility "
                + $"{expectedVisible}, received {indicator.Visible}.");
            return false;
        }

        int indicatorCount = adapter.GetChildren().Count(child => child is Line2D);
        if (indicatorCount != 1)
        {
            Fail(
                $"Focused-search indicator smoke failed {scenario}: expected one authored "
                + $"indicator, received {indicatorCount}.");
            return false;
        }

        return true;
    }

    private bool AssertInputsUnchanged(
        CharacterId seekerCharacterId,
        CharacterId unrelatedCharacterId,
        ObjectId firstTargetId,
        ObjectId secondTargetId,
        ObjectId unrelatedTargetId,
        FocusedSearchActivitySnapshot emptySnapshot,
        FocusedSearchActivitySnapshot unrelatedSnapshot,
        FocusedSearchActivitySnapshot oneMatchingSnapshot,
        FocusedSearchActivitySnapshot multipleMatchingSnapshot,
        FocusedSearchActivity unrelatedActivity,
        FocusedSearchActivity firstMatchingActivity,
        FocusedSearchActivity secondMatchingActivity)
    {
        bool unchanged = emptySnapshot.Entries.Count == 0
            && HasExactEntries(unrelatedSnapshot, unrelatedActivity)
            && HasExactEntries(oneMatchingSnapshot, firstMatchingActivity)
            && HasExactEntries(
                multipleMatchingSnapshot,
                unrelatedActivity,
                firstMatchingActivity,
                secondMatchingActivity)
            && ReferenceEquals(
                unrelatedActivity.Request.SearcherCharacterId,
                unrelatedCharacterId)
            && ReferenceEquals(unrelatedActivity.Request.TargetObjectId, unrelatedTargetId)
            && ReferenceEquals(firstMatchingActivity.Request.SearcherCharacterId, seekerCharacterId)
            && ReferenceEquals(firstMatchingActivity.Request.TargetObjectId, firstTargetId)
            && ReferenceEquals(secondMatchingActivity.Request.SearcherCharacterId, seekerCharacterId)
            && ReferenceEquals(secondMatchingActivity.Request.TargetObjectId, secondTargetId);
        if (!unchanged)
        {
            Fail("Focused-search indicator presentation changed a supplied input.");
        }

        return unchanged;
    }

    private static bool HasExactEntries(
        FocusedSearchActivitySnapshot snapshot,
        params FocusedSearchActivity[] expectedEntries)
    {
        return snapshot.Entries.Count == expectedEntries.Length
            && snapshot.Entries
                .Zip(expectedEntries)
                .All(pair => ReferenceEquals(pair.First, pair.Second));
    }

    private static FocusedSearchActivity CreateActivity(
        CharacterId searcherCharacterId,
        ObjectId targetObjectId)
    {
        return new FocusedSearchActivity(
            new FocusedSearchRequest(searcherCharacterId, targetObjectId));
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
