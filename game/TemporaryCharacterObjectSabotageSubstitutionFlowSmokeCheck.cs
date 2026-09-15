using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one explicit authored substitution and evidence-gated recovery flow.</summary>
public partial class TemporaryCharacterObjectSabotageSubstitutionFlowSmokeCheck : Node
{
    private static readonly string[] ExpectedMethodNames = ["TryApply", "TryRecover", "_Ready"];

    /// <inheritdoc />
    public override void _Ready() => SetProcess(true);

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterObjectSabotageSubstitutionSource source =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionSource>(
                "../TemporaryCharacterObjectSabotageSubstitutionSource");
        CharacterObjectSabotageSubstitutionController state =
            GetNode<CharacterObjectSabotageSubstitutionController>(
                "../CharacterObjectSabotageSubstitutionController");
        TemporaryCharacterObjectSabotageSubstitutionFlowController flow =
            GetNode<TemporaryCharacterObjectSabotageSubstitutionFlowController>(
                "../TemporaryCharacterObjectSabotageSubstitutionFlowController");
        CharacterObjectSabotageSubstitutionSnapshot initialSubstitutions = source.Substitutions;
        DiscoveryEvidenceSnapshot initialEvidence = source.Evidence;

        if (!ReferenceEquals(state.Substitutions, initialSubstitutions)
            || !ReferenceEquals(state.Evidence, initialEvidence)
            || initialSubstitutions.Entries.Count != 0
            || initialEvidence.Entries.Count != 0)
        {
            Fail("The temporary substitution flow did not initialize exact source state inertly.");
            return;
        }

        if (!flow.TryApply()
            || state.Substitutions is not { } appliedSubstitutions
            || state.Evidence is not { } appliedEvidence
            || ReferenceEquals(appliedSubstitutions, initialSubstitutions)
            || ReferenceEquals(appliedEvidence, initialEvidence)
            || !ReferenceEquals(
                appliedSubstitutions.Entries.Single(),
                source.Candidate.Substitution)
            || !ReferenceEquals(
                appliedEvidence.Entries.Single(),
                source.Candidate.Evidence.Evidence))
        {
            Fail("The temporary substitution flow did not install exact explicit state and evidence.");
            return;
        }

        if (flow.TryApply()
            || !ReferenceEquals(state.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !HasUnchangedSource(source, initialSubstitutions, initialEvidence))
        {
            Fail("The temporary substitution flow did not reject repeat application atomically.");
            return;
        }

        CharacterPerceptionSnapshot unseen = new(new CharacterId("seeker"), []);
        if (flow.TryRecover(unseen)
            || !ThrowsArgumentNull(() => flow.TryRecover(null!))
            || !ReferenceEquals(state.Substitutions, appliedSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence))
        {
            Fail("The temporary substitution flow changed state without noticed exact evidence.");
            return;
        }

        CharacterPerceptionSnapshot noticed = new(
            unseen.CharacterId,
            [new WorldObjectVisibility(
                source.Candidate.Evidence.Evidence.ObjectId,
                VisibilityState.Noticed)]);
        if (!flow.TryRecover(noticed)
            || state.Substitutions is not { } recoveredSubstitutions
            || ReferenceEquals(recoveredSubstitutions, appliedSubstitutions)
            || recoveredSubstitutions.Entries.Count != 0
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || flow.TryRecover(noticed)
            || !ReferenceEquals(state.Substitutions, recoveredSubstitutions)
            || !ReferenceEquals(state.Evidence, appliedEvidence)
            || !HasUnchangedSource(source, initialSubstitutions, initialEvidence)
            || !HasClosedFlowSurface())
        {
            Fail("The temporary substitution flow did not recover only the exact active substitution.");
            return;
        }

        GD.Print(
            "Temporary substitution sabotage flow smoke passed: inert exact initialization, "
            + "explicit atomic application, repeat and unseen rejection, perception-gated recovery, "
            + "retained evidence, unchanged source state, and closed behavior.");
        GetTree().Quit();
    }

    private static bool HasUnchangedSource(
        TemporaryCharacterObjectSabotageSubstitutionSource source,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        DiscoveryEvidenceSnapshot evidence) =>
        ReferenceEquals(source.Substitutions, substitutions)
        && ReferenceEquals(source.Evidence, evidence)
        && source.Substitutions.Entries.Count == 0
        && source.Evidence.Entries.Count == 0;

    private static bool HasClosedFlowSurface()
    {
        Type type = typeof(TemporaryCharacterObjectSabotageSubstitutionFlowController);
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

        return methodNames.SequenceEqual(ExpectedMethodNames) && properties.Length == 0;
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
