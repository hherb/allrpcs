using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains what a continued session needs beyond world truth: the Seeker profile and bodies.</summary>
public sealed class SaveContinuationState
{
    /// <summary>Gets the maximum number of saved body positions.</summary>
    public const int MaximumPositionCount = 32;

    /// <summary>Creates one continuation state; a missing profile means none was selected.</summary>
    public SaveContinuationState(
        SeekerCustomizationSaveState? seekerProfile,
        IEnumerable<CharacterPositionSaveState> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);

        List<CharacterPositionSaveState> retained = new();
        HashSet<CharacterId> characterIds = new();
        foreach (CharacterPositionSaveState position in positions)
        {
            if (position is null || !characterIds.Add(position.CharacterId))
            {
                throw new ArgumentException(
                    "Saved positions must be non-null and unique per character.",
                    nameof(positions));
            }

            retained.Add(position);
            if (retained.Count > MaximumPositionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(positions),
                    retained.Count,
                    $"A continuation accepts at most {MaximumPositionCount} positions.");
            }
        }

        SeekerProfile = seekerProfile;
        Positions = retained.AsReadOnly();
    }

    /// <summary>Gets the saved Seeker profile, or null when none was selected.</summary>
    public SeekerCustomizationSaveState? SeekerProfile { get; }

    /// <summary>Gets saved body positions in stable supplied order.</summary>
    public IReadOnlyList<CharacterPositionSaveState> Positions { get; }
}
