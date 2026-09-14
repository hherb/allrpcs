using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks stale abstention acknowledgement for the concealment presentation coordinator.</summary>
public partial class CharacterObjectSabotageConcealmentPresentationCoordinatorSmokeCheck
{
    private void ObserveStaleAbstention()
    {
        if (scenario!.Task.Status == CharacterObjectSabotageConcealmentTaskStatus.Running)
        {
            return;
        }

        CharacterObjectSabotageConcealmentConsiderationResult? result = scenario.Task.Result;
        if (scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || result is not { SelectedCandidate: null }
            || scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Coordinator.TryPresentCompletedResult()
            || !scenario.State.TryApply(
                scenario.Input.Character,
                scenario.Input.Request.PhysicalAccess,
                scenario.Input.Request.WorldObjects,
                scenario.Input.Concealment,
                scenario.Input.Evidence,
                scenario.Input.Candidate))
        {
            Fail("The stale abstention state could not be prepared.");
            return;
        }

        retainedConcealment = scenario.State.Concealment;
        retainedEvidence = scenario.State.Evidence;
        if (scenario.Coordinator.TryAcknowledgeCompletedResult()
            || scenario.Task.Status != CharacterObjectSabotageConcealmentTaskStatus.Completed
            || !ReferenceEquals(scenario.Task.Result, result)
            || !ReferenceEquals(scenario.State.Concealment, retainedConcealment)
            || !ReferenceEquals(scenario.State.Evidence, retainedEvidence)
            || !scenario.Body.Visible)
        {
            Fail("A stale completed abstention changed exact task, scene, or presentation state.");
            return;
        }

        GD.Print(
            "Concealment result presentation coordinator smoke passed: exact selected and abstaining "
            + "results acknowledged once, later work started, and invalid acknowledgement preserved "
            + "task, scene, and presentation state.");
        DisposeScenario();
        GetTree().Quit();
    }
}
