namespace TagGame.Api.Core.Common.Exceptions;

public sealed class ConflictException(string resource, string? reason = null) : DomainException(
    reason is null ? $"Conflict on '{resource}'." : $"Conflict on '{resource}': {reason}", "conflict")
{
    public string Resource { get; } = resource;
    public string? Reason { get; } = reason;
}

