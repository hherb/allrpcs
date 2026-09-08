using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs focused temporary deterministic character-turn source checks when enabled.</summary>
public partial class TemporaryCharacterTurnSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_TURN_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryCharacterTurnSource source = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporaryCharacterTurnSource>("../TemporaryCharacterTurnSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterTurnActivity activity = source.Activity;
        CharacterTurnResponse response = source.Response;
        if (!ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            || activity.Response is not null
            || !ReferenceEquals(response.Request, activity.Request)
            || !ReferenceEquals(response.Disposition, CharacterTurnDisposition.Accept)
            || !ReferenceEquals(response.ImmediateIntention.Type, CharacterIntentionType.Wait)
            || response.ImmediateIntention.TargetObjectId is not null
            || response.Commitments.Count != 0
            || response.StateProposals.Count != 0
            || response.Speech is null
            || !ReferenceEquals(
                response.Speech.SpeakerCharacterId,
                activity.Request.RespondingCharacterId)
            || !HasExpectedSourceSurface())
        {
            Fail("Temporary character-turn source failed exact deterministic activity composition.");
            return;
        }

        GD.Print(
            "Temporary character-turn source smoke passed: exact awaiting activity, exact "
            + "deterministic response, fixed wait intention, and bounded source surface were correct.");
        GetTree().Quit();
    }

    private bool HasExpectedSourceSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(TemporaryCharacterTurnSource)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        System.Reflection.MethodInfo[] methods = typeof(TemporaryCharacterTurnSource)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        bool matches = properties.Length == 2
            && IsPrivateSetProperty(properties[0], "Activity", typeof(CharacterTurnActivity))
            && IsPrivateSetProperty(properties[1], "Response", typeof(CharacterTurnResponse))
            && methods.Length == 1
            && methods[0].Name == nameof(TemporaryCharacterTurnSource._Ready);
        if (!matches)
        {
            Fail("Temporary character-turn source exposed an unexpected public surface.");
        }

        return matches;
    }

    private static bool IsPrivateSetProperty(
        System.Reflection.PropertyInfo property,
        string name,
        Type type) =>
        property.Name == name
        && property.PropertyType == type
        && property.CanRead
        && property.SetMethod is not null
        && property.SetMethod.IsPrivate;

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
