namespace Allrpcs.Application.Characters;

/// <summary>Fences one exact request and publishes its bounded decision only at completion.</summary>
public sealed class StructuredDecisionPipeline<TRequest, TDecision>
    where TRequest : class
    where TDecision : class
{
    private readonly TRequest request;
    private readonly Func<TRequest, string, TDecision> deserialize;
    private readonly int maximumDocumentLength;
    private readonly bool requireLifecycle;
    private readonly bool allowMetadataAfterTextDone;
    private TDecision? decision;
    private int deltaLength;
    private bool started;
    private bool inProgress;
    private bool textDone;
    private bool terminal;

    /// <summary>Retains the exact request, decision policy and existing stream lifecycle policy.</summary>
    public StructuredDecisionPipeline(
        TRequest request,
        Func<TRequest, string, TDecision> deserialize,
        int maximumDocumentLength,
        bool requireLifecycle = true,
        bool allowMetadataAfterTextDone = true)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(deserialize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumDocumentLength);
        this.request = request;
        this.deserialize = deserialize;
        this.maximumDocumentLength = maximumDocumentLength;
        this.requireLifecycle = requireLifecycle;
        this.allowMetadataAfterTextDone = allowMetadataAfterTextDone;
    }

    /// <summary>Consumes one semantic event, returning the exact decision only on successful completion.</summary>
    public TDecision? Apply(StructuredDecisionEvent kind, string? text = null)
    {
        if (terminal)
        {
            throw new InvalidOperationException("The structured-decision stream is already terminal.");
        }

        switch (kind)
        {
            case StructuredDecisionEvent.Started:
                if (requireLifecycle && started) throw Fail();
                started = true;
                break;
            case StructuredDecisionEvent.InProgress:
                if (requireLifecycle && (!started || inProgress)) throw Fail();
                inProgress = true;
                break;
            case StructuredDecisionEvent.Metadata:
                RequireProgress();
                if (textDone && !allowMetadataAfterTextDone) throw Fail();
                break;
            case StructuredDecisionEvent.TextDelta:
                RequireProgress();
                if (textDone || text is null || text.Length > maximumDocumentLength - deltaLength)
                    throw Fail();
                deltaLength += text.Length;
                break;
            case StructuredDecisionEvent.TextDone:
                RequireProgress();
                if (textDone || text is null || text.Length > maximumDocumentLength) throw Fail();
                try
                {
                    decision = deserialize(request, text) ?? throw Fail();
                }
                catch (ArgumentException)
                {
                    throw Fail();
                }
                textDone = true;
                break;
            case StructuredDecisionEvent.Completed:
                RequireProgress();
                if (!textDone || decision is null) throw Fail();
                terminal = true;
                return decision;
            default:
                throw Fail();
        }

        return null;
    }

    private void RequireProgress()
    {
        if (requireLifecycle && !inProgress) throw Fail();
    }

    private InvalidDataException Fail()
    {
        terminal = true;
        return new InvalidDataException("The structured-decision stream is not a complete supported response.");
    }
}
