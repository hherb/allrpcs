using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Reserves the shared turn slot for only the exact current sabotage opportunity.</summary>
public partial class CharacterSabotageTurnReservationCoordinator : Node
{
    private CharacterSabotageTurnOpportunityController opportunities = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact current reservation installed in the shared turn owner.</summary>
    public CharacterSabotageTurnReservation? Reservation => turns.SabotageReservation;

    /// <inheritdoc />
    public override void _Ready()
    {
        opportunities = GetNode<CharacterSabotageTurnOpportunityController>(
            "../CharacterSabotageTurnOpportunityController");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Reserves only the exact current opportunity once without invoking authoring.</summary>
    public bool TryReserve(CharacterSabotageTurnOpportunity? expectedOpportunity)
    {
        CharacterSabotageTurnOpportunity? currentOpportunity = opportunities.Opportunity;
        return expectedOpportunity is not null
            && currentOpportunity is not null
            && ReferenceEquals(expectedOpportunity, currentOpportunity)
            && turns.TryReserveSabotageTurn(expectedOpportunity, currentOpportunity);
    }
}
