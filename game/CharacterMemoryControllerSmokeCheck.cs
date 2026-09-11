using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned character-memory checks when enabled.</summary>
public partial class CharacterMemoryControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_MEMORY_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterMemoryController controller =
            GetNode<CharacterMemoryController>("../CharacterMemoryController");
        CharacterMemorySnapshot initial = controller.Memory;
        CharacterMemorySnapshot populated = CreateMemory("healer");
        controller.Install(populated);

        if (initial.Characters.Count != 0
            || !ReferenceEquals(controller.Memory, populated)
            || !ThrowsArgumentNull(() => controller.Install(null!))
            || !ReferenceEquals(controller.Memory, populated)
            || !HasClosedSurface())
        {
            Fail("Character-memory controller smoke failed exact ownership or closed surface.");
            return;
        }

        CharacterMemorySnapshot replacement = new([]);
        controller.Install(replacement);
        if (!ReferenceEquals(controller.Memory, replacement))
        {
            Fail("Character-memory controller smoke failed exact replacement installation.");
            return;
        }

        GD.Print(
            "Character-memory controller smoke passed: explicit empty initialization, exact "
            + "immutable replacement, null rejection preservation, and closed ownership surface "
            + "were correct.");
        GetTree().Quit();
    }

    internal static CharacterMemorySnapshot CreateMemory(string characterIdValue)
    {
        CharacterId characterId = new(characterIdValue);
        CharacterCommitmentMemory promise = new(
            new CharacterCommitmentId("inspect-moonwort"),
            characterId,
            CharacterCommitmentKind.Promise,
            new CharacterId("seeker"),
            new CharacterTurnId("turn-one"),
            "Inspect the moonwort before collection.");
        CharacterMemoryAggregate aggregate = new(
            characterId,
            new CharacterRecentConversationWindow(characterId, []),
            new CharacterConversationSummarySnapshot(characterId, []),
            new CharacterEpisodicMemorySnapshot(characterId, []),
            new CharacterEpistemicMemorySnapshot(characterId, []),
            new CharacterBeliefAssessmentSnapshot(characterId, []),
            new CharacterRelationshipMemorySnapshot(characterId, []),
            new CharacterCommitmentMemorySnapshot(characterId, [promise]),
            new CharacterMemoryIndex(characterId, []));
        return new CharacterMemorySnapshot([aggregate]);
    }

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterMemoryController);
        System.Reflection.PropertyInfo property = type.GetProperty(
            nameof(CharacterMemoryController.Memory))
            ?? throw new InvalidOperationException("Missing memory property.");
        System.Reflection.MethodInfo method = type.GetMethod(
            nameof(CharacterMemoryController.Install))
            ?? throw new InvalidOperationException("Missing install method.");
        return property.PropertyType == typeof(CharacterMemorySnapshot)
            && property.CanRead
            && property.SetMethod?.IsPrivate == true
            && method.ReturnType == typeof(void)
            && method.GetParameters() is [{ ParameterType: var parameterType }]
            && parameterType == typeof(CharacterMemorySnapshot);
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
