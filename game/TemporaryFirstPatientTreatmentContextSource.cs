using Allrpcs.Domain.Diseases;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authors one exact temporary first-patient treatment context for the scene.</summary>
public partial class TemporaryFirstPatientTreatmentContextSource : Node
{
    /// <summary>Gets the exact retained first-patient treatment context.</summary>
    public FirstPatientTreatmentContext Context { get; private set; } = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Context = RepresentativeFirstPatientTreatmentContextSource.Create();
    }
}
