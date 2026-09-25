using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one exact typed Delay request from ordinary-turn acknowledgement.</summary>
public partial class CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestCoordinator request = null!;
    private CharacterSabotageActionSpecificRequestStartCoordinator start = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained by typed start.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact typed Delay request started through the existing seam.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        request = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestCoordinator");
        start = GetNode<CharacterSabotageActionSpecificRequestStartCoordinator>(
            "../CharacterSabotageActionSpecificRequestStartCoordinator");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
    }

    /// <summary>Starts once only while the exact composed ordinary-turn Delay graph is current.</summary>
    public bool TryStart(CharacterTargetSabotageDelayConsiderationRequest? expectedRequest)
    {
        if (Request is not null
            || expectedRequest is null
            || start.StartedAuthorRequest is not null
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || task.Result is not null
            || !request.IsCurrentRequest(expectedRequest))
        {
            return false;
        }

        if (!start.TryStart()
            || !ReferenceEquals(start.StartedAuthorRequest, binding.SelectedAuthorRequest)
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Running
            || task.Result is not null)
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay request must start exactly once.");
        }

        Request = expectedRequest;
        return true;
    }

    /// <summary>Starts once only while the exact reserved Delay graph remains current.</summary>
    public bool TryStart(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayConsiderationRequest? expectedRequest)
    {
        if (Reservation is not null
            || Request is not null
            || expectedReservation is null
            || expectedRequest is null
            || start.StartedAuthorRequest is not null
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || task.Result is not null
            || !request.IsCurrentRequest(expectedReservation, expectedRequest))
        {
            return false;
        }

        if (!start.TryStart()
            || !ReferenceEquals(start.StartedAuthorRequest, binding.SelectedAuthorRequest)
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Running
            || task.Result is not null)
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay request must start exactly once.");
        }

        Reservation = expectedReservation;
        Request = expectedRequest;
        return true;
    }

    /// <summary>Checks one exact completed result against the retained ordinary-turn start graph.</summary>
    internal bool IsCurrentCompletedResult(
        CharacterTargetSabotageDelayConsiderationResult? expectedResult) =>
        Request is not null
        && expectedResult is not null
        && request.IsCurrentRequest(Request)
        && task.Status == CharacterTargetSabotageDelayTaskStatus.Completed
        && ReferenceEquals(task.Result, expectedResult)
        && ReferenceEquals(expectedResult.Request, Request)
        && ReferenceEquals(start.StartedAuthorRequest, binding.SelectedAuthorRequest);

    /// <summary>Checks one exact completed result against the retained reserved start graph.</summary>
    internal bool IsCurrentCompletedResult(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayConsiderationResult? expectedResult) =>
        Reservation is not null
        && ReferenceEquals(Reservation, expectedReservation)
        && Request is not null
        && expectedResult is not null
        && request.IsCurrentRequest(expectedReservation, Request)
        && task.Status == CharacterTargetSabotageDelayTaskStatus.Completed
        && ReferenceEquals(task.Result, expectedResult)
        && ReferenceEquals(expectedResult.Request, Request)
        && ReferenceEquals(start.StartedAuthorRequest, binding.SelectedAuthorRequest);
}
