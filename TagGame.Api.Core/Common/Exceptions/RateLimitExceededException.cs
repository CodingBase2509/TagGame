namespace TagGame.Api.Core.Common.Exceptions;

public sealed class RateLimitExceededException(TimeSpan? retryAfter = null)
    : DomainException("Rate limit exceeded.", "rate_limit")
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

