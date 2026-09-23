using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact completed sabotage turn admitted to ordinary scheduling.</summary>
public partial class CharacterSabotageTurnAdmissionCoordinator : Node
{
    private CharacterSabotageTurnCompletionCoordinator completion = null!;

    /// <summary>Gets the exact completed-turn token admitted once by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        completion = GetNode<CharacterSabotageTurnCompletionCoordinator>(
            "../CharacterSabotageTurnCompletionCoordinator");
    }

    /// <summary>Retains only the exact current completion accepted by the scheduling gate.</summary>
    public bool TryAdmit(CharacterSabotageTurnCompletion? expectedCompletion)
    {
        CharacterSabotageTurnCompletion? currentCompletion = completion.Completion;
        if (Admission is not null
            || !CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                currentCompletion,
                expectedCompletion))
        {
            return false;
        }

        Admission = currentCompletion;
        return true;
    }
}
