using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Controllers;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free temporary Seeker character source checks when explicitly enabled.
/// </summary>
public partial class TemporarySeekerCharacterSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SEEKER_CHARACTER_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string SeekerDisplayName = "SEEKER";

    private TemporarySeekerCharacterSource source = null!;
    private FocusedSearchActivityController controller = null!;
    private TemporarySeekerPerceptionSource perceptionSource = null!;
    private CharacterBody2D seeker = null!;
    private Line2D searchIndicator = null!;
    private Label focusStatus = null!;
    private Label focusedSearchResult = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        source = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        controller = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        perceptionSource = GetNode<TemporarySeekerPerceptionSource>(
            "../TemporarySeekerPerceptionSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        searchIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        FocusedSearchActivitySnapshot expectedActivities = controller.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = controller.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = controller.Perception;
        CharacterPerceptionSnapshot expectedSourcePerception = perceptionSource.Perception;
        Vector2 expectedSeekerPosition = seeker.Position;
        Vector2 expectedSeekerVelocity = seeker.Velocity;
        bool expectedSearchIndicatorVisibility = searchIndicator.Visible;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;
        Character first = source.Character;
        Character second = source.Character;

        if (!HasExpectedPropertySurface()
            || !ReferenceEquals(first, second)
            || !ReferenceEquals(first.Identity.Id, controller.SearcherCharacterId)
            || !ReferenceEquals(
                first.ControllerAssignment.CharacterId,
                controller.SearcherCharacterId)
            || !ReferenceEquals(
                first.ControllerAssignment.CharacterId,
                first.Identity.Id)
            || !ReferenceEquals(first.ControllerAssignment.ControllerType, ControllerType.Human)
            || !string.Equals(
                first.Identity.DisplayName,
                SeekerDisplayName,
                StringComparison.Ordinal))
        {
            Fail("Temporary Seeker character source failed exact character retention.");
            return;
        }

        if (first.PublicBackground.Text.Length != 0
            || first.PrivateBackground.Text.Length != 0
            || first.Capabilities.Entries.Count != 0
            || first.Limitations.Entries.Count != 0
            || first.Conditions.Entries.Count != 0
            || first.Inventory.ObjectReferences.Count != 0
            || first.Knowledge.KnowledgeReferences.Count != 0
            || first.Beliefs.Entries.Count != 0
            || first.Relationships.Entries.Count != 0
            || first.Goals.Entries.Count != 0)
        {
            Fail("Temporary Seeker character source authored non-minimal components.");
            return;
        }

        CharacterId independentCharacterId = new("independent-seeker");
        Character independent = CreateCharacter(independentCharacterId);
        if (!ReferenceEquals(independent.Identity.Id, independentCharacterId)
            || !ReferenceEquals(
                independent.ControllerAssignment.CharacterId,
                independentCharacterId)
            || independent.Identity.Id == controller.SearcherCharacterId
            || ReferenceEquals(independent.Identity.Id, controller.SearcherCharacterId)
            || !ReferenceEquals(
                independent.ControllerAssignment.ControllerType,
                ControllerType.Human))
        {
            Fail("Temporary Seeker character source failed independent identifier retention.");
            return;
        }

        if (!ReferenceEquals(controller.Activities, expectedActivities)
            || !ReferenceEquals(controller.Outcomes, expectedOutcomes)
            || !ReferenceEquals(controller.Perception, expectedPerception)
            || !ReferenceEquals(perceptionSource.Perception, expectedSourcePerception)
            || seeker.Position != expectedSeekerPosition
            || seeker.Velocity != expectedSeekerVelocity
            || searchIndicator.Visible != expectedSearchIndicatorVisibility
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail(
                "Temporary Seeker character source changed focused-search, perception, "
                + "movement, or presentation state.");
            return;
        }

        GD.Print(
            "Temporary Seeker character source smoke passed: one stable complete character; "
            + "exact controller-owned identifier retention in identity and human assignment; "
            + "minimal empty authored components; independent construction; and unchanged "
            + "focused-search, perception, movement, and presentation state were correct.");
        GetTree().Quit();
    }

    private Character CreateCharacter(CharacterId characterId)
    {
        System.Reflection.MethodInfo? method = typeof(TemporarySeekerCharacterSource).GetMethod(
            "CreateCharacter",
            System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.NonPublic);
        if (method is null)
        {
            Fail("Temporary Seeker character source smoke found no character factory.");
            return null!;
        }

        return (Character)(method.Invoke(null, [characterId])
            ?? throw new InvalidOperationException(
                "The temporary Seeker character source returned no character."));
    }

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(TemporarySeekerCharacterSource)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        return properties.Length == 1
            && string.Equals(properties[0].Name, "Character", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(Character)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
