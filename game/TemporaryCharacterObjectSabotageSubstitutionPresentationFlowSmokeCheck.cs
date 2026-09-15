using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks explicit main-scene substitution and recovery presentation.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionPresentationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OBJECT_SABOTAGE_SUBSTITUTION_PRESENTATION_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";

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
        WorldObjectController worldState =
            GetNode<WorldObjectController>("../WorldObjectController");
        TemporaryCharacterObjectSabotageSubstitutionSource source =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        CharacterObjectSabotageSubstitutionController state =
            GetNode<CharacterObjectSabotageSubstitutionController>(
                "../CharacterObjectSabotageSubstitutionController");
        CharacterObjectSabotageSubstitutionPresentationController presentation =
            GetNode<CharacterObjectSabotageSubstitutionPresentationController>(
                "../CharacterObjectSabotageSubstitutionPresentationController");
        TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController>(
                "../TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController");
        CanvasItem originalBody = GetNode<CanvasItem>("../InteractionCandidates/WaterCask/Body");
        CanvasItem replacementBody =
            GetNode<CanvasItem>("../InteractionCandidates/WaterCask/SubstituteBody");
        Node2D waterCask = GetNode<Node2D>("../InteractionCandidates/WaterCask");
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
        Vector2 expectedWaterCaskPosition = waterCask.Position;
        Vector2 expectedSeekerPosition = seeker.GlobalPosition;
        Vector2 expectedHealerPosition = healer.GlobalPosition;
        WorldObjectSnapshot expectedWorldObjects = source.WorldObjects;
        CharacterObjectSabotageSubstitutionSnapshot initialSubstitutions = source.Substitutions;
        DiscoveryEvidenceSnapshot initialEvidence = source.Evidence;

        if (!originalBody.Visible
            || replacementBody.Visible
            || !ReferenceEquals(presentation.Substitution, source.Candidate.Substitution)
            || !ReferenceEquals(presentation.LastPresentedSubstitutions, initialSubstitutions)
            || !ReferenceEquals(state.Substitutions, initialSubstitutions)
            || !ReferenceEquals(state.Evidence, initialEvidence))
        {
            Fail("The main-scene substitution presentation did not initialize exact source state.");
            return;
        }

        if (!flow.TryApply()
            || state.Substitutions is not { } appliedSubstitutions
            || state.Evidence is not { } appliedEvidence
            || originalBody.Visible
            || !replacementBody.Visible
            || !ReferenceEquals(
                presentation.LastPresentedSubstitutions,
                appliedSubstitutions)
            || !ReferenceEquals(
                appliedSubstitutions.Entries.Single(),
                source.Candidate.Substitution)
            || !ReferenceEquals(
                appliedEvidence.Entries.Single(),
                source.Candidate.Evidence.Evidence)
            || flow.TryApply()
            || originalBody.Visible
            || !replacementBody.Visible)
        {
            Fail("Explicit substitution did not present the exact replacement once.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(new CharacterId("seeker"), []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || originalBody.Visible
            || !replacementBody.Visible
            || !ReferenceEquals(state.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !ReferenceEquals(
                presentation.LastPresentedSubstitutions,
                appliedSubstitutions))
        {
            Fail("Unseen or null recovery input changed substitution presentation state.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            unseen.CharacterId,
            [new WorldObjectVisibility(
                source.Candidate.Evidence.Evidence.ObjectId,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed)
            || state.Substitutions is not { } recoveredSubstitutions
            || recoveredSubstitutions.Entries.Count != 0
            || !originalBody.Visible
            || replacementBody.Visible
            || !ReferenceEquals(
                presentation.LastPresentedSubstitutions,
                recoveredSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || flow.TryRecover(noticed)
            || !originalBody.Visible
            || replacementBody.Visible
            || waterCask.Position != expectedWaterCaskPosition
            || seeker.GlobalPosition != expectedSeekerPosition
            || healer.GlobalPosition != expectedHealerPosition
            || !ReferenceEquals(source.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(worldSource.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(worldState.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(source.Substitutions, initialSubstitutions)
            || !ReferenceEquals(source.Evidence, initialEvidence)
            || !HasClosedFlowSurface())
        {
            Fail("Evidence-authorized recovery did not restore exact substitution presentation.");
            return;
        }

        GD.Print(
            "Temporary substitution presentation flow smoke passed: inert original presentation, "
            + "one explicit replacement, repeat and unseen rejection, exact noticed-evidence "
            + "recovery, stable actors and world-object truth, and closed behavior.");
        GetTree().Quit();
    }

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSubstitutionPresentationFlowController);
        string[] methodNames = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        System.Reflection.PropertyInfo[] properties = type.GetProperties(
            System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.DeclaredOnly);

        return methodNames.SequenceEqual(["TryApply", "TryRecover", "_Ready"])
            && properties.Length == 0;
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
