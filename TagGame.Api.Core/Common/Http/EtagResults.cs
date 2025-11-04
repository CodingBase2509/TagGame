namespace TagGame.Api.Core.Common.Http;

public enum IfNoneMatchDecision
{
    Proceed = 0,
    NotModified,
    InvalidIfNoneMatch
}

public enum IfMatchCheckResult
{
    Ok = 0,
    MissingIfMatch,
    InvalidIfMatch,
    EtagMismatch,
    Wildcard
}
