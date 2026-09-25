using System.Numerics;
using Allrpcs.Domain.Diseases;

namespace Allrpcs.Application.Navigation;

/// <summary>Binds the exact first deep-sleeping patient to the exact sickhouse scene anchor.</summary>
public sealed class OpeningRegionFirstPatientSceneBinding
{
    /// <summary>Creates one bounded patient placement without changing disease or scene state.</summary>
    public OpeningRegionFirstPatientSceneBinding(
        OpeningRegionProjection projection,
        OpeningRegionAnchor sickhouseAnchor,
        FirstPatientTreatmentContext treatmentContext,
        Vector2 patientPosition,
        float maximumPatientDistancePixels)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(sickhouseAnchor);
        ArgumentNullException.ThrowIfNull(treatmentContext);
        if (!float.IsFinite(maximumPatientDistancePixels)
            || maximumPatientDistancePixels <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumPatientDistancePixels),
                maximumPatientDistancePixels,
                "The first-patient scene distance must be finite and positive.");
        }

        if (!projection.Anchors.Any(anchor => ReferenceEquals(anchor, sickhouseAnchor))
            || !ReferenceEquals(
                sickhouseAnchor.Location,
                projection.Topology.EntryLocation))
        {
            throw new ArgumentException(
                "The first-patient scene requires the projection's exact sickhouse entry anchor.",
                nameof(sickhouseAnchor));
        }

        if (!FirstPatientScenarioStructuralPolicy.IsAcceptable(treatmentContext.Scenario)
            || !treatmentContext.Scenario.DiseaseTruth.Infections.Any(infection =>
                ReferenceEquals(infection.CharacterId, treatmentContext.Scenario.PatientId)
                && ReferenceEquals(infection.Stage, DiseaseStage.DeepSleep)))
        {
            throw new ArgumentException(
                "The first-patient scene requires its exact structurally valid deep-sleeping patient.",
                nameof(treatmentContext));
        }

        _ = NavigationRequestValidator.Create(
            projection.WorldBounds,
            sickhouseAnchor.Position,
            patientPosition);
        if (Vector2.Distance(sickhouseAnchor.Position, patientPosition)
            > maximumPatientDistancePixels)
        {
            throw new ArgumentException(
                "The first patient must remain inside the bounded sickhouse scene.",
                nameof(patientPosition));
        }

        Projection = projection;
        SickhouseAnchor = sickhouseAnchor;
        TreatmentContext = treatmentContext;
        PatientPosition = patientPosition;
        MaximumPatientDistancePixels = maximumPatientDistancePixels;
    }

    /// <summary>Gets the exact opening-region projection containing the sickhouse.</summary>
    public OpeningRegionProjection Projection { get; }

    /// <summary>Gets the projection's exact sickhouse entry anchor.</summary>
    public OpeningRegionAnchor SickhouseAnchor { get; }

    /// <summary>Gets the exact authored first-patient treatment context.</summary>
    public FirstPatientTreatmentContext TreatmentContext { get; }

    /// <summary>Gets the bounded scene position of the sleeping patient.</summary>
    public Vector2 PatientPosition { get; }

    /// <summary>Gets the largest accepted patient-to-sickhouse-anchor distance.</summary>
    public float MaximumPatientDistancePixels { get; }
}
