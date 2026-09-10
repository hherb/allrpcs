using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

/// <summary>Completes one character response from a bounded Chat Completions document.</summary>
public static class OpenAiChatCompletionJsonCompletion
{
    private const int MaximumJsonDepth = 10;

    /// <summary>Gets the maximum accepted response-envelope length in UTF-16 code units.</summary>
    public const int MaximumDocumentLength = 64_000;

    /// <summary>Reads one complete response while retaining only bounded terminal usage.</summary>
    public static async Task<OpenAiCharacterModelSseResult> CompleteWithUsageAsync(
        CharacterTurnRequest request,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(stream);

        string serialized;
        try
        {
            serialized = await ReadBoundedAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidDataException)
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }
        catch (IOException)
        {
            return Result(Failure(CharacterModelFailureCode.Transport));
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                serialized,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth,
                });
            return ReadResult(request, document.RootElement);
        }
        catch (JsonException)
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }
    }

    private static OpenAiCharacterModelSseResult ReadResult(
        CharacterTurnRequest request,
        JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !TryGetSingleProperty(root, "choices", out JsonElement choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() != 1)
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }

        JsonElement choice = choices[0];
        if (choice.ValueKind != JsonValueKind.Object
            || !TryGetSingleString(choice, "finish_reason", out string? finishReason)
            || !string.Equals(finishReason, "stop", StringComparison.Ordinal)
            || !TryGetSingleProperty(choice, "message", out JsonElement message)
            || message.ValueKind != JsonValueKind.Object
            || !TryGetSingleString(message, "role", out string? role)
            || !string.Equals(role, "assistant", StringComparison.Ordinal)
            || !TryGetSingleString(message, "content", out string? content))
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }

        if (!TryReadUsage(root, out int? input, out int? output, out int? total))
        {
            return Result(Failure(CharacterModelFailureCode.InvalidResponse));
        }

        CharacterModelStreamTermination termination =
            CharacterModelJsonResponseCompletion.Create(request, content!);
        return new OpenAiCharacterModelSseResult(termination, input, output, total);
    }

    private static bool TryReadUsage(
        JsonElement root,
        out int? input,
        out int? output,
        out int? total)
    {
        input = null;
        output = null;
        total = null;
        int count = root.EnumerateObject().Count(
            property => string.Equals(property.Name, "usage", StringComparison.Ordinal));
        if (count == 0)
        {
            return true;
        }

        if (count != 1
            || !root.TryGetProperty("usage", out JsonElement usage)
            || usage.ValueKind == JsonValueKind.Null)
        {
            return count == 1;
        }

        if (usage.ValueKind != JsonValueKind.Object
            || !TryGetTokenCount(usage, "prompt_tokens", out int inputValue)
            || !TryGetTokenCount(usage, "completion_tokens", out int outputValue)
            || !TryGetTokenCount(usage, "total_tokens", out int totalValue)
            || (long)inputValue + outputValue != totalValue)
        {
            return false;
        }

        input = inputValue;
        output = outputValue;
        total = totalValue;
        return true;
    }

    private static bool TryGetTokenCount(
        JsonElement element,
        string propertyName,
        out int value)
    {
        value = 0;
        int count = 0;
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            count++;
            if (count != 1
                || property.Value.ValueKind != JsonValueKind.Number
                || !property.Value.TryGetInt32(out value)
                || value < 0
                || value > CharacterModelGenerationDiagnostic.MaximumTokenCount)
            {
                return false;
            }
        }

        return count == 1;
    }

    private static bool TryGetSingleString(
        JsonElement element,
        string propertyName,
        out string? value)
    {
        value = null;
        return TryGetSingleProperty(element, propertyName, out JsonElement property)
            && property.ValueKind == JsonValueKind.String
            && (value = property.GetString()) is not null;
    }

    private static bool TryGetSingleProperty(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        value = default;
        int count = 0;
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            count++;
            value = property.Value;
        }

        return count == 1;
    }

    private static async Task<string> ReadBoundedAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using StreamReader reader = new(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1_024,
            leaveOpen: true);
        StringBuilder builder = new();
        char[] buffer = new char[1_024];
        while (true)
        {
            int read;
            try
            {
                read = await reader.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch (DecoderFallbackException exception)
            {
                throw new InvalidDataException("The response is not valid UTF-8.", exception);
            }

            if (read == 0)
            {
                return builder.ToString();
            }

            if (builder.Length + read > MaximumDocumentLength)
            {
                throw new InvalidDataException("The response exceeds the transport bound.");
            }

            builder.Append(buffer, 0, read);
        }
    }

    private static CharacterModelStreamTermination Failure(CharacterModelFailureCode code) =>
        new(
            CharacterModelStreamTerminationStatus.Failed,
            null,
            new CharacterModelFailure(code));

    private static OpenAiCharacterModelSseResult Result(
        CharacterModelStreamTermination termination) => new(termination, null, null, null);
}
