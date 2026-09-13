using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned independent-task departure checks.</summary>
public partial class CharacterIndependentTaskDepartureControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_DEPARTURE_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterIndependentTaskDepartureController controller = new();
        CharacterIndependentTaskActivity other = Activity("other", "sage");
        CharacterIndependentTaskActivity first = Activity("first", "healer");
        CharacterIndependentTaskActivity later = Activity("later", "healer");
        CharacterIndependentTaskActivitySnapshot activities = new([other, first, later]);
        CharacterId healer = first.Assignment.CharacterId;
        CharacterId sage = other.Assignment.CharacterId;
        CharacterPartySnapshot party = new([healer, sage]);

        if (!RejectsNullInitialization(controller, activities, party)
            || !controller.TryInitialize(activities, party)
            || !ReferenceEquals(controller.Activities, activities)
            || !ReferenceEquals(controller.Party, party)
            || controller.ActiveActivity is not null
            || controller.TryInitialize(new CharacterIndependentTaskActivitySnapshot([]), party))
        {
            Fail("Departure controller rejected exact one-time atomic initialization.");
            return;
        }

        CharacterIndependentTaskActivitySnapshot staleActivities = new([other, first, later]);
        CharacterPartySnapshot staleParty = new([healer, sage]);
        if (controller.TryStartAndSeparate(
                new CharacterId("healer"),
                staleActivities,
                party)
            || controller.TryStartAndSeparate(
                new CharacterId("healer"),
                activities,
                staleParty)
            || controller.TryStartAndSeparate(
                new CharacterId("enemy-agent"),
                activities,
                party)
            || !HasExactState(controller, activities, party, null))
        {
            Fail("Departure controller changed state after stale or ineligible input.");
            return;
        }

        if (!controller.TryStartAndSeparate(
                new CharacterId("healer"),
                activities,
                party)
            || controller.ActiveActivity is not { } active
            || !ReferenceEquals(active.Assignment, first.Assignment)
            || !ReferenceEquals(active.State, CharacterIndependentTaskActivityState.Active)
            || controller.Activities is not { } resultingActivities
            || controller.Party is not { } resultingParty
            || !ReferenceEquals(resultingActivities.Activities[0], other)
            || !ReferenceEquals(resultingActivities.Activities[1], active)
            || !ReferenceEquals(resultingActivities.Activities[2], later)
            || resultingParty.MemberCharacterIds.Count != 1
            || !ReferenceEquals(resultingParty.MemberCharacterIds[0], sage)
            || !ReferenceEquals(activities.Activities[1], first)
            || party.MemberCharacterIds.Count != 2)
        {
            Fail("Departure controller failed stable atomic task start and party separation.");
            return;
        }

        if (controller.TryStartAndSeparate(
                new CharacterId("sage"),
                activities,
                party)
            || controller.TryStartAndSeparate(
                new CharacterId("healer"),
                resultingActivities,
                resultingParty)
            || !HasExactState(controller, resultingActivities, resultingParty, active)
            || !RejectsNullDepartureInputs(
                controller,
                resultingActivities,
                resultingParty))
        {
            Fail("Departure controller failed closed without partial installation.");
            return;
        }

        GD.Print(
            "Independent-task departure controller smoke passed: exact scene ownership, stable "
            + "selection, same-character separation, stale rejection, and atomic failure.");
        controller.Free();
        GetTree().Quit();
    }

    private static CharacterIndependentTaskActivity Activity(string taskId, string characterId) =>
        new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId(taskId),
                new CharacterId(characterId),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId($"{taskId}-object"))),
            CharacterIndependentTaskActivityState.Assigned);

    private static bool HasExactState(
        CharacterIndependentTaskDepartureController controller,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party,
        CharacterIndependentTaskActivity? activeActivity) =>
        ReferenceEquals(controller.Activities, activities)
        && ReferenceEquals(controller.Party, party)
        && ReferenceEquals(controller.ActiveActivity, activeActivity);

    private static bool RejectsNullInitialization(
        CharacterIndependentTaskDepartureController controller,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!, party))
        && ThrowsArgumentNull(() => controller.TryInitialize(activities, null!))
        && controller.Activities is null
        && controller.Party is null;

    private static bool RejectsNullDepartureInputs(
        CharacterIndependentTaskDepartureController controller,
        CharacterIndependentTaskActivitySnapshot activities,
        CharacterPartySnapshot party) =>
        ThrowsArgumentNull(() => controller.TryStartAndSeparate(null!, activities, party))
        && ThrowsArgumentNull(() => controller.TryStartAndSeparate(
            new CharacterId("sage"), null!, party))
        && ThrowsArgumentNull(() => controller.TryStartAndSeparate(
            new CharacterId("sage"), activities, null!));

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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
