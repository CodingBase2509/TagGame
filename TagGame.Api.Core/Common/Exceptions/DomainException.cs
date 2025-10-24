namespace TagGame.Api.Core.Common.Exceptions;

public abstract class DomainException(string message, string? code = null, Exception? inner = null)
    : Exception(message, inner)
{
    public string? Code { get; } = code;
}

