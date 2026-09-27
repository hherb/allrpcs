using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact inert substitution-sabotage composition in the main scene.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionMainSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_MAIN_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string WaterCaskObjectId = "village-water-cask";

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryWorldObjectSource worldSource =
            GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource");
        TemporaryCharacterObjectSabotageSubstitutionSource source =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        CharacterObjectSabotageSubstitutionController state =
            GetNode<CharacterObjectSabotageSubstitutionController>(
                "../CharacterObjectSabotageSubstitutionController");
        TemporaryCharacterObjectSabotageSubstitutionFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionFlowController>(
                "../TemporaryCharacterObjectSabotageSubstitutionFlowController");
        WorldObject target = worldSource.WorldObjects.Entries[2];

        if (!ReferenceEquals(source.WorldObjects, worldSource.WorldObjects)
            || worldSource.WorldObjects.Entries.Count != 4
            || !ReferenceEquals(source.Candidate.Substitution.Occurrence.Target, target)
            || target.Identity.Id.Value != WaterCaskObjectId
            || ReferenceEquals(source.Candidate.Substitution.Replacement, target)
            || source.Candidate.Substitution.Replacement.Identity.Id == target.Identity.Id
            || !ReferenceEquals(state.Substitutions, source.Substitutions)
            || !ReferenceEquals(state.Evidence, source.Evidence)
            || state.Substitutions.Entries.Count != 0
            || state.Evidence.Entries.Count != 0
            || !CharacterObjectSabotageSubstitutionPolicy.CanApply(
                source.Character,
                source.PhysicalAccess,
                source.WorldObjects,
                state.Substitutions,
                state.Evidence,
                source.Candidate)
            || !HasExpectedFlowSurface(flow))
        {
            Fail("The main scene did not retain one exact inert eligible substitution candidate.");
            return;
        }

        GD.Print(
            "Temporary substitution sabotage main-scene smoke passed: exact water-cask "
            + "source, distinct replacement, exact empty scene-owned state, explicit flow, and "
            + "no automatic substitution.");
        GetTree().Quit();
    }

    private static bool HasExpectedFlowSurface(
        TemporaryCharacterObjectSabotageSubstitutionFlowController flow) =>
        flow.GetType().GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .SequenceEqual(["TryApply", "TryRecover", "_Ready"]);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
