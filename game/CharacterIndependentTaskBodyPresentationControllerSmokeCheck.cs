using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact party-owned visibility projection for one independent-task body.</summary>
public partial class CharacterIndependentTaskBodyPresentationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_BODY_PRESENTATION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterIndependentTaskBodyPresentationController controller = new();
        Node2D body = new();
        CharacterId characterId = new("presented-healer");
        CharacterPartySnapshot separatedParty =
            new([new CharacterId("remaining-seeker")]);
        CharacterPartySnapshot rejoinedParty =
            new([separatedParty.MemberCharacterIds[0], characterId]);
        if (!RejectsInitializationNulls(controller, characterId, body, separatedParty)
            || !controller.TryInitialize(characterId, body, separatedParty)
            || !ReferenceEquals(controller.CharacterId, characterId)
            || !ReferenceEquals(controller.LastPresentedParty, separatedParty)
            || body.Visible
            || controller.TryInitialize(characterId, body, separatedParty))
        {
            Fail("Body presentation did not initialize from exact separated party state.");
            return;
        }

        CharacterPartySnapshot staleParty =
            new(separatedParty.MemberCharacterIds);
        if (controller.TryPresent(staleParty, rejoinedParty)
            || !RejectsPresentationNulls(controller, separatedParty, rejoinedParty)
            || !ReferenceEquals(controller.LastPresentedParty, separatedParty)
            || body.Visible)
        {
            Fail("Stale or null party presentation input changed body visibility.");
            return;
        }

        if (!controller.TryPresent(separatedParty, rejoinedParty)
            || !ReferenceEquals(controller.LastPresentedParty, rejoinedParty)
            || !body.Visible
            || controller.TryPresent(separatedParty, rejoinedParty)
            || !ReferenceEquals(controller.LastPresentedParty, rejoinedParty)
            || !body.Visible)
        {
            Fail("Exact party rejoin did not restore body visibility once.");
            return;
        }

        GD.Print(
            "Independent-task body presentation controller smoke passed: exact separated party "
            + "state hid one body, exact rejoin restored it, and stale, repeated, and null input "
            + "preserved presentation state.");
        body.Free();
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsInitializationNulls(
        CharacterIndependentTaskBodyPresentationController controller,
        CharacterId characterId,
        CanvasItem body,
        CharacterPartySnapshot party)
    {
        CharacterIndependentTaskBodyPresentationController first = new();
        CharacterIndependentTaskBodyPresentationController second = new();
        CharacterIndependentTaskBodyPresentationController third = new();
        bool rejected = ThrowsArgumentNull(() => first.TryInitialize(null!, body, party))
            && ThrowsArgumentNull(() => second.TryInitialize(characterId, null!, party))
            && ThrowsArgumentNull(() => third.TryInitialize(characterId, body, null!));
        first.Free();
        second.Free();
        third.Free();
        return rejected
            && controller.CharacterId is null
            && controller.LastPresentedParty is null;
    }

    private static bool RejectsPresentationNulls(
        CharacterIndependentTaskBodyPresentationController controller,
        CharacterPartySnapshot expectedParty,
        CharacterPartySnapshot currentParty) =>
        ThrowsArgumentNull(() => controller.TryPresent(null!, currentParty))
        && ThrowsArgumentNull(() => controller.TryPresent(expectedParty, null!));

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
