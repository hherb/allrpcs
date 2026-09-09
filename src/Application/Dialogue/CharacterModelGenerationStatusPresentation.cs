namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Provides one fixed, content-free player-visible status for a character-model stream snapshot.
/// </summary>
public sealed class CharacterModelGenerationStatusPresentation
{
    private CharacterModelGenerationStatusPresentation(
        string text,
        CharacterModelStreamTerminationStatus? terminationStatus)
    {
        Text = text;
        TerminationStatus = terminationStatus;
    }

    /// <summary>Gets the non-terminal generation presentation.</summary>
    public static CharacterModelGenerationStatusPresentation Generating { get; } =
        new("Character response: GENERATING", null);

    /// <summary>Gets the completed generation presentation.</summary>
    public static CharacterModelGenerationStatusPresentation Completed { get; } =
        new("Character response: COMPLETE", CharacterModelStreamTerminationStatus.Completed);

    /// <summary>Gets the caller-cancelled generation presentation.</summary>
    public static CharacterModelGenerationStatusPresentation Cancelled { get; } =
        new("Character response: CANCELLED", CharacterModelStreamTerminationStatus.Cancelled);

    /// <summary>Gets the deadline-expired generation presentation.</summary>
    public static CharacterModelGenerationStatusPresentation TimedOut { get; } =
        new("Character response: TIMED OUT", CharacterModelStreamTerminationStatus.TimedOut);

    /// <summary>Gets the provider-neutral failed generation presentation.</summary>
    public static CharacterModelGenerationStatusPresentation Failed { get; } =
        new("Character response: FAILED", CharacterModelStreamTerminationStatus.Failed);

    /// <summary>Gets every presentation in stable lifecycle order.</summary>
    public static IReadOnlyList<CharacterModelGenerationStatusPresentation> All { get; } =
        Array.AsReadOnly(new[] { Generating, Completed, Cancelled, TimedOut, Failed });

    /// <summary>Gets fixed player-visible text containing no model or response content.</summary>
    public string Text { get; }

    /// <summary>Gets the terminal status, or <see langword="null"/> while generating.</summary>
    public CharacterModelStreamTerminationStatus? TerminationStatus { get; }

    /// <summary>Gets whether the presentation represents a terminal stream.</summary>
    public bool IsTerminal => TerminationStatus is not null;

    /// <summary>
    /// Selects the fixed presentation for the supplied exact stream prefix without retaining it.
    /// </summary>
    public static CharacterModelGenerationStatusPresentation Project(
        CharacterModelStreamSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Updates.Count == 0
            || snapshot.Updates[^1] is not CharacterModelStreamTermination termination)
        {
            return Generating;
        }

        return termination.Status switch
        {
            CharacterModelStreamTerminationStatus.Completed => Completed,
            CharacterModelStreamTerminationStatus.Cancelled => Cancelled,
            CharacterModelStreamTerminationStatus.TimedOut => TimedOut,
            CharacterModelStreamTerminationStatus.Failed => Failed,
            _ => throw new InvalidOperationException("Unknown generation termination status."),
        };
    }

    /// <inheritdoc />
    public override string ToString() => Text;
}
