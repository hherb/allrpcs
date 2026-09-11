using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Contains hidden authoritative disease definitions in stable supplied order.</summary>
public sealed class DiseaseDefinitionSnapshot
{
    /// <summary>Gets the maximum supported number of disease definitions.</summary>
    public const int MaximumDefinitionCount = 32;

    /// <summary>Creates an immutable authoritative disease-definition snapshot.</summary>
    public DiseaseDefinitionSnapshot(IEnumerable<DiseaseDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        List<DiseaseDefinition> retainedDefinitions = new();
        HashSet<DiseaseId> identifiers = new();
        foreach (DiseaseDefinition definition in definitions)
        {
            if (definition is null)
            {
                throw new ArgumentException(
                    "Disease definitions cannot contain null values.",
                    nameof(definitions));
            }

            if (!identifiers.Add(definition.DiseaseId))
            {
                throw new ArgumentException(
                    "Disease definitions cannot contain duplicate identities.",
                    nameof(definitions));
            }

            retainedDefinitions.Add(definition);
            if (retainedDefinitions.Count > MaximumDefinitionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(definitions),
                    $"A snapshot accepts at most {MaximumDefinitionCount} disease definitions.");
            }
        }

        Definitions = retainedDefinitions.AsReadOnly();
    }

    /// <summary>Gets exact definitions in stable supplied order.</summary>
    public IReadOnlyList<DiseaseDefinition> Definitions { get; }
}
