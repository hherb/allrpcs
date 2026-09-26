namespace Allrpcs.Application.Journal;

/// <summary>Projects one safe selected journal section into stable plain presentation text.</summary>
public static class JournalSectionPresentationTextProjection
{
    /// <summary>Gets the neutral text shown when the selected section has no entries.</summary>
    public const string EmptyContent = "No entries.";

    /// <summary>Returns stable plain text containing only the supplied presentation's safe arm.</summary>
    public static JournalSectionPresentationText Project(
        JournalSectionPresentationSnapshot presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        string content = presentation.Observations is { } observations
            ? Render(
                observations.Entries,
                entry => $"{entry.DisplayName}\n{entry.Description}")
            : presentation.Testimony is { } testimony
                ? Render(
                    testimony.Entries,
                    entry => $"{entry.SpeakerCharacterId.Value} · {entry.DeliveryMode.Value}\n{entry.Text}")
                : presentation.Hypotheses is { } hypotheses
                    ? Render(
                        hypotheses.Entries,
                        entry => $"{entry.QuestionPrompt}\n{entry.Statement}")
                    : presentation.Experiments is { } experiments
                        ? Render(
                            experiments.Entries,
                            entry => $"Procedure: {entry.Procedure}\n"
                                + $"Observed outcome: {entry.ObservedOutcome}")
                        : Render(
                            presentation.OpenQuestions!.Entries,
                            entry => entry.Prompt);

        return new(presentation.Section.DisplayLabel, content);
    }

    private static string Render<T>(
        IReadOnlyList<T> entries,
        Func<T, string> renderEntry) => entries.Count == 0
            ? EmptyContent
            : string.Join("\n\n", entries.Select(renderEntry));
}
