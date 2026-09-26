namespace Allrpcs.Application.Journal;

/// <summary>Retains exactly one selected visibility-safe journal section for presentation.</summary>
public sealed class JournalSectionPresentationSnapshot
{
    internal JournalSectionPresentationSnapshot(
        JournalSectionDefinition section,
        JournalObservationSectionSnapshot? observations,
        JournalTestimonySectionSnapshot? testimony,
        JournalHypothesisSectionSnapshot? hypotheses,
        JournalExperimentSectionSnapshot? experiments,
        JournalOpenQuestionSectionSnapshot? openQuestions)
    {
        ArgumentNullException.ThrowIfNull(section);

        object?[] contents = [observations, testimony, hypotheses, experiments, openQuestions];
        int contentCount = contents.Count(content => content is not null);
        bool sectionMatchesContent = ReferenceEquals(observations?.Section, section)
            || ReferenceEquals(testimony?.Section, section)
            || ReferenceEquals(hypotheses?.Section, section)
            || ReferenceEquals(experiments?.Section, section)
            || ReferenceEquals(openQuestions?.Section, section);
        if (contentCount != 1 || !sectionMatchesContent)
        {
            throw new ArgumentException(
                "A journal presentation must retain exactly its selected safe section.",
                nameof(section));
        }

        Section = section;
        Observations = observations;
        Testimony = testimony;
        Hypotheses = hypotheses;
        Experiments = experiments;
        OpenQuestions = openQuestions;
    }

    /// <summary>Gets the exact selected canonical section definition.</summary>
    public JournalSectionDefinition Section { get; }

    /// <summary>Gets the selected observations content, or null for another section.</summary>
    public JournalObservationSectionSnapshot? Observations { get; }

    /// <summary>Gets the selected testimony content, or null for another section.</summary>
    public JournalTestimonySectionSnapshot? Testimony { get; }

    /// <summary>Gets the selected hypotheses content, or null for another section.</summary>
    public JournalHypothesisSectionSnapshot? Hypotheses { get; }

    /// <summary>Gets the selected experiments content, or null for another section.</summary>
    public JournalExperimentSectionSnapshot? Experiments { get; }

    /// <summary>Gets the selected open-questions content, or null for another section.</summary>
    public JournalOpenQuestionSectionSnapshot? OpenQuestions { get; }
}
