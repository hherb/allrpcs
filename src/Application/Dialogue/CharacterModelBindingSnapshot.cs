namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains a non-empty immutable set of caller-composed canonical provider bindings.
/// </summary>
public sealed class CharacterModelBindingSnapshot
{
    /// <summary>Gets the maximum number of canonical provider bindings.</summary>
    public const int MaximumBindingCount = 3;

    /// <summary>Copies and validates bindings in stable caller-supplied order.</summary>
    public CharacterModelBindingSnapshot(IEnumerable<CharacterModelBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        List<CharacterModelBinding> retainedBindings = new();
        HashSet<CharacterModelProvider> providers = new();
        foreach (CharacterModelBinding binding in bindings)
        {
            if (binding is null)
            {
                throw new ArgumentException(
                    "Character-model bindings cannot contain null values.",
                    nameof(bindings));
            }

            if (!CharacterModelProvider.All.Any(
                    provider => ReferenceEquals(provider, binding.Provider)))
            {
                throw new ArgumentException(
                    "Character-model bindings must use a canonical provider.",
                    nameof(bindings));
            }

            if (!ReferenceEquals(binding.Provider, binding.Settings.Provider))
            {
                throw new ArgumentException(
                    "Character-model binding settings must match the registered provider.",
                    nameof(bindings));
            }

            if (!providers.Add(binding.Provider))
            {
                throw new ArgumentException(
                    "Character-model bindings cannot contain duplicate providers.",
                    nameof(bindings));
            }

            retainedBindings.Add(binding);
            if (retainedBindings.Count > MaximumBindingCount)
            {
                throw new ArgumentException(
                    $"Character-model bindings cannot exceed {MaximumBindingCount} entries.",
                    nameof(bindings));
            }
        }

        if (retainedBindings.Count == 0)
        {
            throw new ArgumentException(
                "Character-model bindings cannot be empty.",
                nameof(bindings));
        }

        Bindings = retainedBindings.AsReadOnly();
    }

    /// <summary>Gets the exact validated bindings in stable supplied order.</summary>
    public IReadOnlyList<CharacterModelBinding> Bindings { get; }
}
