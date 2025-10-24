namespace TagGame.Api.Core.Common.Exceptions;

public sealed class DomainRuleViolationException(
    string message,
    IDictionary<string, string[]>? errors = null,
    string? code = null)
    : DomainException(message, code ?? "domain_rule_violation")
{
    public IReadOnlyDictionary<string, string[]>? Errors { get; } = errors is null ? null : new Dictionary<string, string[]>(errors);
}

