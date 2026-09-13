using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one authored independent-task departure through exact scene state.</summary>
public partial class TemporaryCharacterIndependentTaskSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_INDEPENDENT_TASK_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterIndependentTaskSource source =
            GetNode<TemporaryCharacterIndependentTaskSource>(
                "../TemporaryCharacterIndependentTaskSource");
        CharacterIndependentTaskDepartureController controller =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        IReadOnlyList<Character> participants =
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "../TemporaryOrdinaryCharacterParticipantSource").Participants;
        WorldObject herbPatch = GetNode<TemporaryWorldObjectSource>(
            "../TemporaryWorldObjectSource").WorldObjects.Entries[0];

        CharacterIndependentTaskActivity authored =
            AssertSingle(source.Activities.Activities);
        if (!ReferenceEquals(source.DepartingCharacterId, participants[0].Identity.Id)
            || !ReferenceEquals(authored.Assignment.CharacterId, source.DepartingCharacterId)
            || !ReferenceEquals(
                authored.Assignment.Intention.Type,
                CharacterIntentionType.ExamineObject)
            || !ReferenceEquals(
                authored.Assignment.Intention.TargetObjectId,
                herbPatch.Identity.Id)
            || !ReferenceEquals(authored.State, CharacterIndependentTaskActivityState.Assigned)
            || source.Party.MemberCharacterIds.Count != 2
            || !ReferenceEquals(source.Party.MemberCharacterIds[0], participants[0].Identity.Id)
            || !ReferenceEquals(source.Party.MemberCharacterIds[1], participants[1].Identity.Id))
        {
            Fail("The temporary source did not retain one exact authored task and party.");
            return;
        }

        CharacterIndependentTaskActivity active = controller.ActiveActivity!;
        if (controller.Activities is not { } resultingActivities
            || controller.Party is not { } resultingParty
            || !ReferenceEquals(active.Assignment, authored.Assignment)
            || !ReferenceEquals(active.State, CharacterIndependentTaskActivityState.Active)
            || !ReferenceEquals(AssertSingle(resultingActivities.Activities), active)
            || resultingParty.MemberCharacterIds.Count != 1
            || !ReferenceEquals(resultingParty.MemberCharacterIds[0], participants[1].Identity.Id)
            || !ReferenceEquals(AssertSingle(source.Activities.Activities), authored)
            || source.Party.MemberCharacterIds.Count != 2
            || !HasClosedSourceSurface())
        {
            Fail("The authored scene departure did not start and separate atomically.");
            return;
        }

        GD.Print(
            "Temporary independent-task source smoke passed: the exact authored healer task "
            + "targets the scene herb patch, initializes exact party state, starts active, and "
            + "separates the same member without mutating source snapshots.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterIndependentTaskSource);
        string[] propertyNames = type.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        return propertyNames.Length == 3
            && propertyNames[0] == "Activities"
            && propertyNames[1] == "DepartingCharacterId"
            && propertyNames[2] == "Party"
            && methods.Length == 1
            && methods[0].Name == nameof(_Ready);
    }

    private static T AssertSingle<T>(IEnumerable<T> values)
    {
        T[] retained = values.ToArray();
        return retained.Length == 1
            ? retained[0]
            : throw new InvalidOperationException("Expected exactly one authored value.");
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
