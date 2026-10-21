using System.Reflection;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

public partial class SaveSnapshotRestorationControllerSmokeCheck
{
    private bool CheckPublicationOrdering(SaveSnapshot snapshot)
    {
        FirstPatientPreparationController preparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        object? expectedIngredients = preparation.Ingredients;
        JournalOpenCloseInputController journalInput = GetNode<JournalOpenCloseInputController>(
            "../JournalOpenCloseInputController");
        JournalSectionPanel panel = GetNode<JournalSectionPanel>("../JournalSectionPanel");
        bool opened = (bool)typeof(JournalOpenCloseInputController).GetMethod(
            "TryOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(journalInput, null)!;
        Allrpcs.Domain.Ingredients.IngredientState herb = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource").Context.Scenario.GatheredIngredients.States[0];
        if (!opened || !preparation.TryStart(preparation.Recipe!, herb)
            || ReferenceEquals(expectedIngredients, preparation.Ingredients))
        {
            Fail("Restore publication fixture did not change the final ingredient owner.");
            return false;
        }
        bool notified = false;
        bool sawCompleteRoot = false;
        void ObserveVisibility()
        {
            if (panel.Visible) { return; }
            notified = true;
            sawCompleteRoot = preparation.Activity is null
                && ReferenceEquals(expectedIngredients, preparation.Ingredients);
        }
        panel.VisibilityChanged += ObserveVisibility;
        bool restored;
        try { restored = controller.TryRestore(snapshot); }
        finally { panel.VisibilityChanged -= ObserveVisibility; }
        if (!restored || !notified || !sawCompleteRoot)
        {
            Fail("Restore publication: modal visibility observer saw the final ingredient owner before commit.");
            return false;
        }

        RestoreParticipantRegistry registry = new();
        bool finalCommitted = false;
        bool cancelled = false;
        bool laterPublished = false;
        CancellationTokenSource cancellation = new();
        _ = cancellation.Token.Register(() => cancelled = true);
        Task retired = Task.FromException(new InvalidOperationException("expected retired task fault"));
        registry.Register(new RestoreParticipant("failing-view", _ => new PreparedRestore(
            () => { }, () => throw new InvalidOperationException("expected isolated publish ordering probe"))));
        registry.Register(new RestoreParticipant("retirement", _ => new PreparedRestore(
            () => { }, () => RetiredRestoreTask.Retire(retired, cancellation))));
        registry.Register(new RestoreParticipant("final-owner", _ => new PreparedRestore(
            () => finalCommitted = true, () => laterPublished = finalCommitted)));
        if (!registry.TryRestore(snapshot) || !cancelled || !laterPublished)
        {
            Fail("Restore publication failure prevented later retirement or publication.");
            return false;
        }
        return true;
    }
}
