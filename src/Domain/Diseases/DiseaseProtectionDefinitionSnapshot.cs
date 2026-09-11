using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Diseases;

/// <summary>Contains bounded authored protective measures in stable supplied order.</summary>
public sealed class DiseaseProtectionDefinitionSnapshot
{
    /// <summary>Gets the maximum supported number of protective-measure definitions.</summary>
    public const int MaximumDefinitionCount = 128;

    /// <summary>Creates an immutable protective-measure definition snapshot.</summary>
    public DiseaseProtectionDefinitionSnapshot(
        DiseaseDefinitionSnapshot diseases,
        IEnumerable<DiseaseProtectionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(diseases);
        ArgumentNullException.ThrowIfNull(definitions);

        List<DiseaseProtectionDefinition> retainedDefinitions = new();
        HashSet<(DiseaseId DiseaseId, EvidenceId EvidenceId)> identities = new();
        foreach (DiseaseProtectionDefinition definition in definitions)
        {
            if (definition is null)
            {
                throw new ArgumentException(
                    "Disease-protection definitions cannot contain null values.",
                    nameof(definitions));
            }

            if (!diseases.Definitions.Any(disease =>
                    ReferenceEquals(disease, definition.Disease)))
            {
                throw new ArgumentException(
                    "Each protection must retain an exact authored disease definition.",
                    nameof(definitions));
            }

            if (!identities.Add((definition.Disease.DiseaseId, definition.EvidenceId)))
            {
                throw new ArgumentException(
                    "A disease and evidence identity can define only one protective measure.",
                    nameof(definitions));
            }

            retainedDefinitions.Add(definition);
            if (retainedDefinitions.Count > MaximumDefinitionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(definitions),
                    $"A snapshot accepts at most {MaximumDefinitionCount} protection definitions.");
            }
        }

        Diseases = diseases;
        Definitions = retainedDefinitions.AsReadOnly();
    }

    /// <summary>Gets the exact authoritative disease definitions.</summary>
    public DiseaseDefinitionSnapshot Diseases { get; }

    /// <summary>Gets exact protective-measure definitions in stable supplied order.</summary>
    public IReadOnlyList<DiseaseProtectionDefinition> Definitions { get; }
}
