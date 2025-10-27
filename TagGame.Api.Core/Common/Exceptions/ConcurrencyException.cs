namespace TagGame.Api.Core.Common.Exceptions;

public sealed class ConcurrencyException(string resource, string? detail = null) : DomainException(
    detail is null ? $"Concurrency conflict on '{resource}'." : $"Concurrency conflict on '{resource}': {detail}", "concurrency")
{
    public string Resource { get; } = resource;
    public string? Detail { get; } = detail;
}

