using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact concealment-owned visibility projection for one scene body.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_CONCEALMENT_PRESENTATION_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterObjectSabotageConcealmentPresentationController controller = new();
        Node2D body = new();
        ObjectId targetObjectId = new("presented-water-cask");
        ObjectId unrelatedObjectId = new("unrelated-object");
        WorldObjectConcealmentSnapshot visible = new([unrelatedObjectId]);
        WorldObjectConcealmentSnapshot concealed =
            new([unrelatedObjectId, targetObjectId]);
        WorldObjectConcealmentSnapshot recovered = new([unrelatedObjectId]);
        if (!RejectsInitializationNulls(controller, targetObjectId, body, visible)
            || !controller.TryInitialize(targetObjectId, body, visible)
            || !ReferenceEquals(controller.TargetObjectId, targetObjectId)
            || !ReferenceEquals(controller.LastPresentedConcealment, visible)
            || !body.Visible
            || controller.TryInitialize(targetObjectId, body, visible))
        {
            Fail("Concealment presentation did not initialize exact visible target state.");
            return;
        }

        WorldObjectConcealmentSnapshot stale = new(visible.ConcealedObjectIds);
        if (controller.TryPresent(stale, concealed)
            || !RejectsPresentationNulls(controller, visible, concealed)
            || !ReferenceEquals(controller.LastPresentedConcealment, visible)
            || !body.Visible)
        {
            Fail("Stale or null concealment presentation input changed body visibility.");
            return;
        }

        if (!controller.TryPresent(visible, concealed)
            || !ReferenceEquals(controller.LastPresentedConcealment, concealed)
            || body.Visible
            || controller.TryPresent(visible, concealed)
            || !ReferenceEquals(controller.LastPresentedConcealment, concealed)
            || body.Visible)
        {
            Fail("Exact concealment did not hide the target body once.");
            return;
        }

        if (!controller.TryPresent(concealed, recovered)
            || !ReferenceEquals(controller.LastPresentedConcealment, recovered)
            || !body.Visible
            || !HasClosedSurface())
        {
            Fail("Exact recovery did not restore the target body with a bounded surface.");
            return;
        }

        GD.Print(
            "Object sabotage concealment presentation smoke passed: unrelated state preserved "
            + "visibility, exact concealment hid one body, exact recovery restored it, and stale, "
            + "repeated, and null input preserved presentation state.");
        body.Free();
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsInitializationNulls(
        CharacterObjectSabotageConcealmentPresentationController controller,
        ObjectId targetObjectId,
        CanvasItem body,
        WorldObjectConcealmentSnapshot concealment)
    {
        CharacterObjectSabotageConcealmentPresentationController first = new();
        CharacterObjectSabotageConcealmentPresentationController second = new();
        CharacterObjectSabotageConcealmentPresentationController third = new();
        bool rejected = ThrowsArgumentNull(() => first.TryInitialize(null!, body, concealment))
            && ThrowsArgumentNull(
                () => second.TryInitialize(targetObjectId, null!, concealment))
            && ThrowsArgumentNull(() => third.TryInitialize(targetObjectId, body, null!));
        first.Free();
        second.Free();
        third.Free();
        return rejected
            && controller.TargetObjectId is null
            && controller.LastPresentedConcealment is null;
    }

    private static bool RejectsPresentationNulls(
        CharacterObjectSabotageConcealmentPresentationController controller,
        WorldObjectConcealmentSnapshot expectedConcealment,
        WorldObjectConcealmentSnapshot currentConcealment) =>
        ThrowsArgumentNull(() => controller.TryPresent(null!, currentConcealment))
        && ThrowsArgumentNull(() => controller.TryPresent(expectedConcealment, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageConcealmentPresentationController);
        string[] propertyNames = type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return propertyNames.SequenceEqual(["LastPresentedConcealment", "TargetObjectId"])
            && methodNames.SequenceEqual(["TryInitialize", "TryPresent"]);
    }

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
