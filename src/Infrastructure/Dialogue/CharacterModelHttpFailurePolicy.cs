using System.Net;
using Allrpcs.Application.Dialogue;

namespace Allrpcs.Infrastructure.Dialogue;

internal static class CharacterModelHttpFailurePolicy
{
    internal static CharacterModelStreamTermination Termination(HttpResponseMessage response, TimeProvider clock)
    {
        CharacterModelFailureCode code = Classify(response.StatusCode);
        TimeSpan? delay = null;
        if (code is CharacterModelFailureCode.Unavailable or CharacterModelFailureCode.RateLimited)
        {
            var retryAfter = response.Headers.RetryAfter;
            delay = retryAfter?.Delta ?? (retryAfter?.Date - clock.GetUtcNow());
            if (delay is null || delay < TimeSpan.Zero)
            {
                delay = CharacterModelRetryPolicy.DefaultDelay;
            }
        }

        return new CharacterModelStreamTermination(
            CharacterModelStreamTerminationStatus.Failed, null, new CharacterModelFailure(code, delay));
    }

    internal static CharacterModelFailureCode Classify(HttpStatusCode statusCode)
    {
        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return CharacterModelFailureCode.Authentication;
        }

        if ((int)statusCode == 429)
        {
            return CharacterModelFailureCode.RateLimited;
        }

        int numericStatus = (int)statusCode;
        return statusCode == HttpStatusCode.RequestTimeout
            || numericStatus is >= 500 and <= 599
            ? CharacterModelFailureCode.Unavailable
            : CharacterModelFailureCode.ProviderRejected;
    }
}
