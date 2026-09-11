using System.Text;
using System.Text.Json;
using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Infrastructure.Investigations;

/// <summary>Writes only bounded candidate state and explicitly untrusted in-world text.</summary>
internal static class InvestigationHypothesisRequestJsonSerializer
{
    internal static string Serialize(InvestigationHypothesisRecognitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("sourceUtterance");
            writer.WriteStartObject();
            writer.WriteString("trust", "untrusted-in-world-text");
            writer.WriteString("text", request.SourceUtterance.Text);
            writer.WriteEndObject();

            writer.WritePropertyName("openQuestions");
            writer.WriteStartArray();
            foreach (InvestigationQuestion question in request.OpenQuestions.Questions)
            {
                writer.WriteStartObject();
                writer.WriteString("questionId", question.QuestionId.Value);
                writer.WriteString("statement", question.Prompt);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName("hypotheses");
            writer.WriteStartArray();
            foreach (InvestigationHypothesis hypothesis in request.Hypotheses.Hypotheses)
            {
                writer.WriteStartObject();
                writer.WriteString("hypothesisId", hypothesis.HypothesisId.Value);
                writer.WriteString("questionId", hypothesis.Question.QuestionId.Value);
                writer.WriteString("statement", hypothesis.Statement);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
