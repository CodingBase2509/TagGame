namespace TagGame.Api.Core.Common.Exceptions;

public sealed class ForbiddenException(string? permission = null) : DomainException(
    permission is null ? "Forbidden." : $"Forbidden: missing '{permission}' permission.", "forbidden")
{
    public string? Permission { get; } = permission;
}

