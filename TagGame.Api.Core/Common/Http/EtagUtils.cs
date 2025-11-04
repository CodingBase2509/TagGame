using System.Text;

namespace TagGame.Api.Core.Common.Http;

public static class EtagUtils
{
    private const string WeakPrefix = "W/";
    private const string Wildcard = "*";

    ///<summary> Builds a strong ETag header value (quoted), using Base64 of a simple version payload (e.g., v{token}).</summary>
    public static string ToEtag(uint concurrencyToken)
    {
        var payload = $"v{concurrencyToken}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        var b64 = Convert.ToBase64String(bytes);
        return Quote(b64);
    }

    ///<summary>Parses a single strong ETag header value to a concurrency token. Rejects weak or wildcard.</summary>
    public static bool TryParseStringEtag(string? raw, out uint concurrencyToken)
    {
        concurrencyToken = 0;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var s = raw.Trim();
        if (s.StartsWith(WeakPrefix, StringComparison.Ordinal))
            return false;

        s = Unquote(s);
        if (string.Equals(s, Wildcard, StringComparison.Ordinal))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(s);
            var payload = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(payload) || payload[0] != 'v')
                return false;

            if (!uint.TryParse(payload.AsSpan(1), out var token))
                return false;

            concurrencyToken = token;
            return true;
        }
        catch
        {
            return false;
        }
    }

    ///<summary>Parses an If-Match header into numeric tokens (strong) and wildcard flag.</summary>
    public static bool TryParseIfMatch(string? header, out IReadOnlyList<uint> tokens, out bool wildcard)
        => TryParseEtagList(header, out tokens, out wildcard);

    ///<summary>Parses an If-None-Match header into numeric tokens (strong) and wildcard flag.</summary>
    public static bool TryParseIfNoneMatch(string? header, out IReadOnlyList<uint> tokens, out bool wildcard)
        => TryParseEtagList(header, out tokens, out wildcard);

    private static bool TryParseEtagList(string? header, out IReadOnlyList<uint> tokens, out bool wildcard)
    {
        var result = new List<uint>();
        tokens = result;
        wildcard = false;

        if (string.IsNullOrWhiteSpace(header))
            return false;

        var any = false;
        foreach (var part in header.Split(','))
        {
            var item = part.Trim();
            if (item.Length == 0)
                continue;

            any = true;
            if (item.Equals(Wildcard, StringComparison.Ordinal))
            {
                wildcard = true;
                continue;
            }

            if (item.StartsWith(WeakPrefix, StringComparison.Ordinal))
                continue; // ignore weak

            if (TryParseStringEtag(item, out var token))
                result.Add(token);
        }

        return any;
    }

    private static string Quote(string value)
        => string.IsNullOrEmpty(value) ? "\"\"" : (value is ['"', _, ..] && value[^1] == '"' ? value : $"\"{value}\"");

    private static string Unquote(string value)
        => string.IsNullOrEmpty(value) ? value : (value is ['"', _, ..] && value[^1] == '"' ? value[1..^1] : value);

    // Returns Ok if proceed, MissingIfMatch when header absent, InvalidIfMatch for bad header,
    // EtagMismatch when no tag matches, Wildcard when '*' supplied (also proceed semantics)
    public static IfMatchCheckResult CheckIfMatch(string? ifMatchHeader, uint currentToken)
    {
        if (string.IsNullOrWhiteSpace(ifMatchHeader))
            return IfMatchCheckResult.MissingIfMatch;

        if (!TryParseIfMatch(ifMatchHeader, out var tokens, out var wildcard))
            return IfMatchCheckResult.InvalidIfMatch;

        var res = wildcard
            ? IfMatchCheckResult.Wildcard
            : (tokens.Count == 0
                ? IfMatchCheckResult.InvalidIfMatch
                : (tokens.Contains(currentToken)
                    ? IfMatchCheckResult.Ok
                    : IfMatchCheckResult.EtagMismatch));
        return res;
    }

    // Returns NotModified if wildcard or any tag matches, InvalidIfNoneMatch for bad header, otherwise Proceed
    public static IfNoneMatchDecision CheckIfNoneMatch(string? ifNoneMatchHeader, uint currentToken)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatchHeader))
            return IfNoneMatchDecision.Proceed;

        if (!TryParseIfNoneMatch(ifNoneMatchHeader, out var tokens, out var wildcard))
            return IfNoneMatchDecision.InvalidIfNoneMatch;

        var decision = wildcard
            ? IfNoneMatchDecision.NotModified
            : (tokens.Count == 0
                ? IfNoneMatchDecision.InvalidIfNoneMatch
                : (tokens.Contains(currentToken)
                    ? IfNoneMatchDecision.NotModified
                    : IfNoneMatchDecision.Proceed));
        return decision;
    }
}
