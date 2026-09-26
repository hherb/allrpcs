using System.Collections.ObjectModel;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains only content-safe journal values for persistence.</summary>
public sealed class JournalSaveState
{
    /// <summary>Gets the maximum persisted entries accepted in any one section.</summary>
    public const int MaximumEntriesPerSection = 256;

    private static readonly string[] CanonicalSectionKeys =
    [
        "observations",
        "testimony",
        "hypotheses",
        "experiments",
        "open-questions",
    ];

    /// <summary>Creates one immutable content-safe journal persistence state.</summary>
    public JournalSaveState(
        CharacterId seekerId,
        IEnumerable<JournalObservationSaveEntry> observations,
        IEnumerable<JournalTestimonySaveEntry> testimony,
        IEnumerable<JournalHypothesisSaveEntry> hypotheses,
        IEnumerable<JournalExperimentSaveEntry> experiments,
        IEnumerable<JournalOpenQuestionSaveEntry> openQuestions,
        string? selectedSectionKey)
    {
        ArgumentNullException.ThrowIfNull(seekerId);

        SeekerId = seekerId;
        Observations = Copy(observations, nameof(observations));
        Testimony = Copy(testimony, nameof(testimony));
        Hypotheses = Copy(hypotheses, nameof(hypotheses));
        Experiments = Copy(experiments, nameof(experiments));
        OpenQuestions = Copy(openQuestions, nameof(openQuestions));
        if (selectedSectionKey is not null
            && !CanonicalSectionKeys.Contains(selectedSectionKey, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "A selected journal section must use one canonical section key.",
                nameof(selectedSectionKey));
        }

        SelectedSectionKey = selectedSectionKey;
    }

    /// <summary>Gets the exact stable identifier of the journal owner.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets independent visibility-safe observations in stable order.</summary>
    public IReadOnlyList<JournalObservationSaveEntry> Observations { get; }

    /// <summary>Gets independent audience-free testimony in stable order.</summary>
    public IReadOnlyList<JournalTestimonySaveEntry> Testimony { get; }

    /// <summary>Gets independent authored hypotheses in stable order.</summary>
    public IReadOnlyList<JournalHypothesisSaveEntry> Hypotheses { get; }

    /// <summary>Gets independent authored experiments in stable order.</summary>
    public IReadOnlyList<JournalExperimentSaveEntry> Experiments { get; }

    /// <summary>Gets independent authored open questions in stable order.</summary>
    public IReadOnlyList<JournalOpenQuestionSaveEntry> OpenQuestions { get; }

    /// <summary>Gets the selected canonical section key, or null for no selection.</summary>
    public string? SelectedSectionKey { get; }

    private static ReadOnlyCollection<T> Copy<T>(IEnumerable<T> entries, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entries, parameterName);
        List<T> retained = entries.ToList();
        if (retained.Count > MaximumEntriesPerSection || retained.Any(entry => entry is null))
        {
            throw new ArgumentException(
                $"A journal section must contain at most {MaximumEntriesPerSection} non-null entries.",
                parameterName);
        }

        return retained.AsReadOnly();
    }
}

/// <summary>Retains one visibility-safe persisted observation.</summary>
public sealed class JournalObservationSaveEntry
{
    /// <summary>Creates one immutable persisted observation.</summary>
    public JournalObservationSaveEntry(ObjectId objectId, string displayName, string description)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ObjectId = objectId;
        DisplayName = JournalSaveText.RequireNonBlank(
            displayName,
            WorldObjectIdentity.MaximumDisplayNameLength,
            nameof(displayName));
        Description = JournalSaveText.RequireBounded(
            description,
            WorldObjectDescription.MaximumLength,
            nameof(description));
    }

    /// <summary>Gets the exact stable observed-object identifier.</summary>
    public ObjectId ObjectId { get; }

    /// <summary>Gets the exact authored display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the exact authored description.</summary>
    public string Description { get; }
}

/// <summary>Retains one audience-free persisted testimony entry.</summary>
public sealed class JournalTestimonySaveEntry
{
    /// <summary>Creates one immutable persisted testimony entry.</summary>
    public JournalTestimonySaveEntry(
        CharacterId speakerCharacterId,
        string text,
        DialogueDeliveryMode deliveryMode)
    {
        ArgumentNullException.ThrowIfNull(speakerCharacterId);
        ArgumentNullException.ThrowIfNull(deliveryMode);
        SpeakerCharacterId = speakerCharacterId;
        Text = JournalSaveText.RequireNonBlank(
            text,
            DialogueUtterance.MaximumTextLength,
            nameof(text));
        DeliveryMode = deliveryMode;
    }

    /// <summary>Gets the exact stable speaker identifier.</summary>
    public CharacterId SpeakerCharacterId { get; }

    /// <summary>Gets the exact heard authored text.</summary>
    public string Text { get; }

    /// <summary>Gets the exact provider-neutral delivery mode.</summary>
    public DialogueDeliveryMode DeliveryMode { get; }
}

/// <summary>Retains one authored persisted hypothesis.</summary>
public sealed class JournalHypothesisSaveEntry
{
    /// <summary>Creates one immutable persisted hypothesis.</summary>
    public JournalHypothesisSaveEntry(
        InvestigationHypothesisId hypothesisId,
        InvestigationQuestionId questionId,
        string questionPrompt,
        string statement)
    {
        ArgumentNullException.ThrowIfNull(hypothesisId);
        ArgumentNullException.ThrowIfNull(questionId);
        HypothesisId = hypothesisId;
        QuestionId = questionId;
        QuestionPrompt = JournalSaveText.RequireNonBlank(
            questionPrompt,
            InvestigationQuestion.MaximumPromptLength,
            nameof(questionPrompt));
        Statement = JournalSaveText.RequireNonBlank(
            statement,
            InvestigationHypothesis.MaximumStatementLength,
            nameof(statement));
    }

    /// <summary>Gets the exact canonical hypothesis identifier.</summary>
    public InvestigationHypothesisId HypothesisId { get; }

    /// <summary>Gets the exact canonical question identifier.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the exact authored question prompt.</summary>
    public string QuestionPrompt { get; }

    /// <summary>Gets the exact authored possible answer.</summary>
    public string Statement { get; }
}

/// <summary>Retains one authored persisted experiment without its authoritative event.</summary>
public sealed class JournalExperimentSaveEntry
{
    /// <summary>Creates one immutable persisted experiment.</summary>
    public JournalExperimentSaveEntry(
        ExperimentId experimentId,
        string procedure,
        string observedOutcome)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ExperimentId = experimentId;
        Procedure = JournalSaveText.RequireNonBlank(
            procedure,
            ExperimentJournalRecord.MaximumProcedureLength,
            nameof(procedure));
        ObservedOutcome = JournalSaveText.RequireNonBlank(
            observedOutcome,
            ExperimentJournalRecord.MaximumObservedOutcomeLength,
            nameof(observedOutcome));
    }

    /// <summary>Gets the exact canonical experiment identifier.</summary>
    public ExperimentId ExperimentId { get; }

    /// <summary>Gets the exact authored procedure.</summary>
    public string Procedure { get; }

    /// <summary>Gets the exact authored qualitative outcome.</summary>
    public string ObservedOutcome { get; }
}

/// <summary>Retains one authored persisted open question.</summary>
public sealed class JournalOpenQuestionSaveEntry
{
    /// <summary>Creates one immutable persisted open question.</summary>
    public JournalOpenQuestionSaveEntry(InvestigationQuestionId questionId, string prompt)
    {
        ArgumentNullException.ThrowIfNull(questionId);
        QuestionId = questionId;
        Prompt = JournalSaveText.RequireNonBlank(
            prompt,
            InvestigationQuestion.MaximumPromptLength,
            nameof(prompt));
    }

    /// <summary>Gets the exact canonical question identifier.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the exact authored question prompt.</summary>
    public string Prompt { get; }
}

internal static class JournalSaveText
{
    internal static string RequireNonBlank(string text, int maximumLength, string parameterName)
    {
        string retained = RequireBounded(text, maximumLength, parameterName);
        if (string.IsNullOrWhiteSpace(retained))
        {
            throw new ArgumentException("Persisted authored text must be nonblank.", parameterName);
        }

        return retained;
    }

    internal static string RequireBounded(string text, int maximumLength, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(text, parameterName);
        if (text.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return text;
    }
}
