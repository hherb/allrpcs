using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves inert startup and one exact Seeker customization selection install.</summary>
public partial class SeekerCustomizationSelectionSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SEEKER_CUSTOMIZATION_SELECTION_CHECK";
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
        RunChecks();
    }

    private void RunChecks()
    {
        SeekerCustomizationSelectionSceneSource source =
            GetNode<SeekerCustomizationSelectionSceneSource>(
                "../SeekerCustomizationSelectionSceneSource");
        SeekerCustomizationOptions options = source.Options;
        SeekerCustomizationSelectionState initial = source.CurrentState;
        SeekerCustomizationSelectionState rebuiltInitial =
            SeekerCustomizationSelectionStateTransition.Initialize(options);
        SeekerCustomizationOptions foreignOptions =
            RepresentativeSeekerCustomizationOptionsSource.Create();
        SeekerCustomizationSelectionState foreignInitial =
            SeekerCustomizationSelectionStateTransition.Initialize(foreignOptions);
        CharacterIdentity identity = new(new CharacterId("seeker"), "Mara");
        SeekerPronouns pronouns = new("they", "them", "their", "theirs", "themself");
        SeekerAppearanceChoice appearance = options.AppearanceChoices[1];
        SeekerBackgroundChoice background = options.BackgroundChoices[2];
        SeekerCustomizationSelectionState selected =
            SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                pronouns,
                appearance,
                background);
        SeekerCustomizationSelectionState rebuiltSelected =
            SeekerCustomizationSelectionStateTransition.Select(
                rebuiltInitial,
                identity,
                pronouns,
                appearance,
                background);
        SeekerCustomizationSelectionState foreignSelected =
            SeekerCustomizationSelectionStateTransition.Select(
                foreignInitial,
                identity,
                pronouns,
                foreignOptions.AppearanceChoices[1],
                foreignOptions.BackgroundChoices[2]);

        if (initial.SelectedProfile is not null
            || !ReferenceEquals(initial.Options, options)
            || !HasExpectedRepresentativeOrder(options)
            || source.TryInstallSelection(null, selected)
            || source.TryInstallSelection(rebuiltInitial, selected)
            || source.TryInstallSelection(initial, null)
            || source.TryInstallSelection(initial, rebuiltSelected)
            || source.TryInstallSelection(initial, foreignSelected)
            || !ReferenceEquals(source.CurrentState, initial)
            || !RejectsInvalidTransitions(initial, identity, pronouns, appearance, background))
        {
            Fail("Ordinary startup or rejected customization input changed selection state.");
            return;
        }

        if (!source.TryInstallSelection(initial, selected)
            || !ReferenceEquals(source.CurrentState, selected)
            || !ReferenceEquals(selected.Options, options)
            || !ReferenceEquals(selected.PreviousState, initial)
            || !ReferenceEquals(selected.SelectedProfile?.Identity, identity)
            || !ReferenceEquals(selected.SelectedProfile?.Pronouns, pronouns)
            || !ReferenceEquals(selected.SelectedProfile?.Appearance, appearance)
            || !ReferenceEquals(selected.SelectedProfile?.Background, background)
            || source.TryInstallSelection(initial, selected)
            || source.TryInstallSelection(selected, selected)
            || !ReferenceEquals(source.CurrentState, selected))
        {
            Fail("Exact Seeker customization selection installation was not one-shot.");
            return;
        }

        GD.Print(
            "Seeker customization selection smoke passed: inert empty startup, stable "
            + "representative order, null/rebuilt/foreign/wrong-character/incomplete rejection, "
            + "one exact selection/install, repeated-path preservation, and exact option, "
            + "identity, pronoun, appearance, background, and profile retention.");
        GetTree().Quit();
    }

    private static bool HasExpectedRepresentativeOrder(SeekerCustomizationOptions options) =>
        options.AppearanceChoices.Select(choice => choice.Key).SequenceEqual(
            ["plain-tunic", "travelling-cloak", "work-apron"])
        && options.BackgroundChoices.Select(choice => choice.Key).SequenceEqual(
            ["forester", "scribe", "apothecarys-assistant", "traveller"]);

    private static bool RejectsInvalidTransitions(
        SeekerCustomizationSelectionState initial,
        CharacterIdentity identity,
        SeekerPronouns pronouns,
        SeekerAppearanceChoice appearance,
        SeekerBackgroundChoice background)
    {
        try
        {
            _ = SeekerCustomizationSelectionStateTransition.Select(
                initial,
                new CharacterIdentity(new CharacterId("healer"), identity.DisplayName),
                pronouns,
                appearance,
                background);
            return false;
        }
        catch (ArgumentException)
        {
        }

        try
        {
            _ = SeekerCustomizationSelectionStateTransition.Select(
                initial,
                identity,
                null!,
                appearance,
                background);
            return false;
        }
        catch (ArgumentNullException)
        {
            return initial.SelectedProfile is null;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
