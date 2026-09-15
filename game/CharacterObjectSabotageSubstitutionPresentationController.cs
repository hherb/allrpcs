using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Projects one exact substitution onto original and replacement scene bodies.</summary>
public partial class CharacterObjectSabotageSubstitutionPresentationController : Node
{
    private CanvasItem originalBody = null!;
    private CanvasItem replacementBody = null!;

    /// <summary>Gets the exact authored substitution represented by the bodies.</summary>
    public CharacterObjectSabotageSubstitution? Substitution { get; private set; }

    /// <summary>Gets the exact most recently presented substitution snapshot.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot? LastPresentedSubstitutions { get; private set; }

    /// <summary>Initializes exact body projection and presents the supplied substitution state once.</summary>
    public bool TryInitialize(
        CharacterObjectSabotageSubstitution substitution,
        CanvasItem targetBody,
        CanvasItem authoredReplacementBody,
        CharacterObjectSabotageSubstitutionSnapshot substitutions)
    {
        ArgumentNullException.ThrowIfNull(substitution);
        ArgumentNullException.ThrowIfNull(targetBody);
        ArgumentNullException.ThrowIfNull(authoredReplacementBody);
        ArgumentNullException.ThrowIfNull(substitutions);

        if (ReferenceEquals(targetBody, authoredReplacementBody))
        {
            throw new ArgumentException(
                "Original and replacement bodies must be distinct.",
                nameof(authoredReplacementBody));
        }

        if (Substitution is not null || LastPresentedSubstitutions is not null)
        {
            return false;
        }

        if (!TryResolveSubstituted(substitution, substitutions, out bool isSubstituted))
        {
            return false;
        }

        Substitution = substitution;
        originalBody = targetBody;
        replacementBody = authoredReplacementBody;
        Present(isSubstituted);
        LastPresentedSubstitutions = substitutions;
        return true;
    }

    /// <summary>Projects one fresh exact snapshot from the last presented substitution state.</summary>
    public bool TryPresent(
        CharacterObjectSabotageSubstitutionSnapshot expectedSubstitutions,
        CharacterObjectSabotageSubstitutionSnapshot currentSubstitutions)
    {
        ArgumentNullException.ThrowIfNull(expectedSubstitutions);
        ArgumentNullException.ThrowIfNull(currentSubstitutions);

        if (Substitution is not { } substitution
            || !ReferenceEquals(LastPresentedSubstitutions, expectedSubstitutions)
            || ReferenceEquals(expectedSubstitutions, currentSubstitutions)
            || !TryResolveSubstituted(
                substitution,
                currentSubstitutions,
                out bool isSubstituted))
        {
            return false;
        }

        Present(isSubstituted);
        LastPresentedSubstitutions = currentSubstitutions;
        return true;
    }

    private void Present(bool isSubstituted)
    {
        originalBody.Visible = !isSubstituted;
        replacementBody.Visible = isSubstituted;
    }

    private static bool TryResolveSubstituted(
        CharacterObjectSabotageSubstitution substitution,
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        out bool isSubstituted)
    {
        CharacterObjectSabotageSubstitution? active = substitutions.Entries
            .SingleOrDefault(entry =>
                entry.Occurrence.Target.Identity.Id
                == substitution.Occurrence.Target.Identity.Id);
        isSubstituted = active is not null;
        return active is null || ReferenceEquals(active, substitution);
    }
}
