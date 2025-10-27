namespace TagGame.Api.Core.Common.Exceptions;

public sealed class NotFoundException(string resource, string key)
    : DomainException($"'{resource}' with key '{key}' was not found.", "not_found")
{
    public string Resource { get; } = resource;
    public string Key { get; } = key;
}

