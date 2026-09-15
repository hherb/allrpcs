using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks exact substitution-owned projection for original and replacement bodies.</summary>
public partial class CharacterObjectSabotageSubstitutionPresentationControllerSmokeCheck : Node
{
    /// <inheritdoc />
    public override void _Ready()
    {
        CharacterObjectSabotageSubstitutionPresentationController controller = new();
        Node2D originalBody = new() { Visible = false };
        Node2D replacementBody = new() { Visible = true };
        CharacterObjectSabotageSubstitution substitution = Substitution(
            "presented-water-cask",
            "presented-tainted-water-cask");
        CharacterObjectSabotageSubstitution unrelated = Substitution(
            "unrelated-target",
            "unrelated-replacement");
        CharacterObjectSabotageSubstitutionSnapshot original = new([unrelated]);
        CharacterObjectSabotageSubstitutionSnapshot substituted = new([unrelated, substitution]);
        CharacterObjectSabotageSubstitutionSnapshot recovered = new([unrelated]);

        if (!RejectsInitializationInvalidInputs(
                controller,
                substitution,
                originalBody,
                replacementBody,
                original)
            || !controller.TryInitialize(
                substitution,
                originalBody,
                replacementBody,
                original)
            || !ReferenceEquals(controller.Substitution, substitution)
            || !ReferenceEquals(controller.LastPresentedSubstitutions, original)
            || !originalBody.Visible
            || replacementBody.Visible
            || controller.TryInitialize(
                substitution,
                originalBody,
                replacementBody,
                original))
        {
            Fail("Substitution presentation did not initialize exact original target state.");
            return;
        }

        CharacterObjectSabotageSubstitutionSnapshot stale = new(original.Entries);
        CharacterObjectSabotageSubstitutionSnapshot mismatched = new(
            [unrelated, Substitution("presented-water-cask", "other-replacement")]);
        if (controller.TryPresent(stale, substituted)
            || controller.TryPresent(original, mismatched)
            || !RejectsPresentationNulls(controller, original, substituted)
            || !ReferenceEquals(controller.LastPresentedSubstitutions, original)
            || !originalBody.Visible
            || replacementBody.Visible)
        {
            Fail("Stale, mismatched, or null substitution presentation changed body state.");
            return;
        }

        if (!controller.TryPresent(original, substituted)
            || !ReferenceEquals(controller.LastPresentedSubstitutions, substituted)
            || originalBody.Visible
            || !replacementBody.Visible
            || controller.TryPresent(original, substituted)
            || !ReferenceEquals(controller.LastPresentedSubstitutions, substituted)
            || originalBody.Visible
            || !replacementBody.Visible)
        {
            Fail("Exact substitution did not replace the original body once.");
            return;
        }

        if (!controller.TryPresent(substituted, recovered)
            || !ReferenceEquals(controller.LastPresentedSubstitutions, recovered)
            || !originalBody.Visible
            || replacementBody.Visible
            || !ReferenceEquals(unrelated, recovered.Entries.Single())
            || !HasClosedSurface())
        {
            Fail("Exact recovery did not restore only the original body with a bounded surface.");
            return;
        }

        GD.Print(
            "Substitution presentation smoke passed: unrelated state retained the original, "
            + "exact substitution swapped body visibility, exact recovery restored it, and stale, "
            + "value-equal, repeated, null, and aliased inputs preserved presentation state.");
        originalBody.Free();
        replacementBody.Free();
        controller.Free();
        GetTree().Quit();
    }

    private static bool RejectsInitializationInvalidInputs(
        CharacterObjectSabotageSubstitutionPresentationController controller,
        CharacterObjectSabotageSubstitution substitution,
        CanvasItem originalBody,
        CanvasItem replacementBody,
        CharacterObjectSabotageSubstitutionSnapshot substitutions)
    {
        CharacterObjectSabotageSubstitutionPresentationController first = new();
        CharacterObjectSabotageSubstitutionPresentationController second = new();
        CharacterObjectSabotageSubstitutionPresentationController third = new();
        CharacterObjectSabotageSubstitutionPresentationController fourth = new();
        CharacterObjectSabotageSubstitutionPresentationController fifth = new();
        CharacterObjectSabotageSubstitutionPresentationController sixth = new();
        CharacterObjectSabotageSubstitutionSnapshot mismatched = new(
            [Substitution("presented-water-cask", "other-replacement")]);
        bool rejected = ThrowsArgumentNull(
                () => first.TryInitialize(null!, originalBody, replacementBody, substitutions))
            && ThrowsArgumentNull(
                () => second.TryInitialize(substitution, null!, replacementBody, substitutions))
            && ThrowsArgumentNull(
                () => third.TryInitialize(substitution, originalBody, null!, substitutions))
            && ThrowsArgumentNull(
                () => fourth.TryInitialize(substitution, originalBody, replacementBody, null!))
            && ThrowsArgument(
                () => fifth.TryInitialize(substitution, originalBody, originalBody, substitutions))
            && !sixth.TryInitialize(
                substitution,
                originalBody,
                replacementBody,
                mismatched);
        first.Free();
        second.Free();
        third.Free();
        fourth.Free();
        fifth.Free();
        sixth.Free();
        return rejected
            && controller.Substitution is null
            && controller.LastPresentedSubstitutions is null;
    }

    private static bool RejectsPresentationNulls(
        CharacterObjectSabotageSubstitutionPresentationController controller,
        CharacterObjectSabotageSubstitutionSnapshot expectedSubstitutions,
        CharacterObjectSabotageSubstitutionSnapshot currentSubstitutions) =>
        ThrowsArgumentNull(() => controller.TryPresent(null!, currentSubstitutions))
        && ThrowsArgumentNull(() => controller.TryPresent(expectedSubstitutions, null!));

    private static bool HasClosedSurface()
    {
        Type type = typeof(CharacterObjectSabotageSubstitutionPresentationController);
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

        return propertyNames.SequenceEqual(["LastPresentedSubstitutions", "Substitution"])
            && methodNames.SequenceEqual(["TryInitialize", "TryPresent"]);
    }

    private static CharacterObjectSabotageSubstitution Substitution(
        string targetId,
        string replacementId)
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        WorldObject target = WorldObject(targetId);
        CharacterObjectSabotageOccurrence occurrence = new(
            new CharacterObjectSabotageProposal(
                character.Identity.Id,
                new CharacterActionAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    character.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Substitute),
            target);
        return new CharacterObjectSabotageSubstitution(occurrence, WorldObject(replacementId));
    }

    private static WorldObject WorldObject(string id) =>
        new(
            new WorldObjectIdentity(new ObjectId(id), $"Object {id}"),
            new WorldObjectDescription($"Description for {id}."));

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

    private static bool ThrowsArgument(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
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
