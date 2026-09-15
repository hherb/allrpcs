using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains bounded exact active substitution state without changing world objects.</summary>
public sealed class CharacterObjectSabotageSubstitutionSnapshot
{
    /// <summary>Gets the maximum number of substitutions retained in one snapshot.</summary>
    public const int MaximumSubstitutionCount = 64;

    /// <summary>Creates an immutable stable copy of exact substitutions.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot(
        IEnumerable<CharacterObjectSabotageSubstitution> substitutions)
    {
        ArgumentNullException.ThrowIfNull(substitutions);

        List<CharacterObjectSabotageSubstitution> retainedSubstitutions = [];
        HashSet<ObjectId> distinctTargetIds = [];
        foreach (CharacterObjectSabotageSubstitution? substitution in substitutions)
        {
            if (substitution is null)
            {
                throw new ArgumentException(
                    "Substitution state cannot contain null entries.",
                    nameof(substitutions));
            }

            if (!distinctTargetIds.Add(substitution.Occurrence.Target.Identity.Id))
            {
                throw new ArgumentException(
                    "Substitution state cannot contain duplicate target identifiers.",
                    nameof(substitutions));
            }

            retainedSubstitutions.Add(substitution);
            if (retainedSubstitutions.Count > MaximumSubstitutionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(substitutions),
                    retainedSubstitutions.Count,
                    $"Substitution state accepts at most {MaximumSubstitutionCount} entries.");
            }
        }

        Entries = retainedSubstitutions.AsReadOnly();
    }

    /// <summary>Gets exact substitutions in stable supplied order.</summary>
    public IReadOnlyList<CharacterObjectSabotageSubstitution> Entries { get; }
}
