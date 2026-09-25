using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Diseases;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks the exact sickhouse and deep-sleeping-patient scene binding when enabled.</summary>
public partial class OpeningRegionFirstPatientSceneSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_OPENING_REGION_FIRST_PATIENT_SCENE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const int MaximumReadyFrames = 60;

    private int readyFrames;

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        readyFrames++;
        OpeningRegionFirstPatientSceneSource source =
            GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        OpeningRegionFirstPatientSceneBinding? binding = source.Binding;
        if (binding is null && readyFrames < MaximumReadyFrames)
        {
            return;
        }

        SetProcess(false);
        if (binding is null)
        {
            Fail("The first-patient scene binding did not become ready.");
            return;
        }

        OpeningRegionProjection projection =
            GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
        FirstPatientTreatmentContext context =
            GetNode<TemporaryFirstPatientTreatmentContextSource>(
                "../TemporaryFirstPatientTreatmentContextSource").Context;
        bool hasDeepSleepingPatient = context.Scenario.DiseaseTruth.Infections.Any(infection =>
            ReferenceEquals(infection.CharacterId, context.Scenario.PatientId)
            && ReferenceEquals(infection.Stage, DiseaseStage.DeepSleep));
        if (!ReferenceEquals(binding.Projection, projection)
            || !ReferenceEquals(binding.TreatmentContext, context)
            || !ReferenceEquals(binding.SickhouseAnchor.Location, projection.Topology.EntryLocation)
            || binding.SickhouseAnchor.Location.Identity.Id.Value != "sickhouse"
            || binding.PatientPosition.X != source.Position.X
            || binding.PatientPosition.Y != source.Position.Y
            || !hasDeepSleepingPatient)
        {
            Fail("The first-patient scene lost exact sickhouse, patient, or placement provenance.");
            return;
        }

        GD.Print(
            "Opening-region first-patient scene smoke passed: exact sickhouse entry, exact "
            + "deep-sleeping patient, and bounded greybox placement were retained without "
            + "changing disease state.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
