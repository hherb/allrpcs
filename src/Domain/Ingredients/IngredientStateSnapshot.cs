using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Contains bounded ingredient state in stable supplied order.</summary>
public sealed class IngredientStateSnapshot
{
    /// <summary>Gets the maximum number of distinct ingredient states.</summary>
    public const int MaximumStateCount = 64;

    /// <summary>Creates an immutable copy of supplied ingredient states.</summary>
    public IngredientStateSnapshot(IEnumerable<IngredientState> states)
    {
        ArgumentNullException.ThrowIfNull(states);

        List<IngredientState> retainedStates = [];
        HashSet<IngredientId> ingredientIds = [];
        foreach (IngredientState? state in states)
        {
            if (state is null)
            {
                throw new ArgumentException("Ingredient states cannot contain null.", nameof(states));
            }

            if (!ingredientIds.Add(state.Definition.IngredientId))
            {
                throw new ArgumentException(
                    "Ingredient states cannot contain duplicate ingredient identities.",
                    nameof(states));
            }

            retainedStates.Add(state);
            if (retainedStates.Count > MaximumStateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(states),
                    $"A snapshot accepts at most {MaximumStateCount} ingredient states.");
            }
        }

        States = retainedStates.AsReadOnly();
    }

    /// <summary>Gets exact ingredient states in stable supplied order.</summary>
    public IReadOnlyList<IngredientState> States { get; }
}
