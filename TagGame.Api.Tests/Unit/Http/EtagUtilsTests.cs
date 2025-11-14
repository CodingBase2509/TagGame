using TagGame.Api.Core.Common.Http;

namespace TagGame.Api.Tests.Unit.Http;

public class EtagUtilsTests
{
    private static readonly uint[] MixedStrongTokens = [101u, 202u];
    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(42u)]
    [InlineData(123456u)]
    [InlineData(uint.MaxValue)]
    public void ToEtag_ProducesQuotedValue_And_TryParse_RoundTrips(uint token)
    {
        var etag = EtagUtils.ToEtag(token);
        etag.Should().NotBeNullOrWhiteSpace();
        etag![0].Should().Be('"');
        etag[^1].Should().Be('"');

        EtagUtils.TryParseStringEtag(etag, out var parsed).Should().BeTrue();
        parsed.Should().Be(token);
    }

    [Fact]
    public void TryParseStringEtag_Rejects_Weak_And_Wildcard()
    {
        var strong = EtagUtils.ToEtag(5);
        var weak = $"W/{strong}";

        EtagUtils.TryParseStringEtag(weak, out _).Should().BeFalse();
        EtagUtils.TryParseStringEtag("*", out _).Should().BeFalse();
        EtagUtils.TryParseStringEtag("not-base64", out _).Should().BeFalse();
    }

    [Fact]
    public void TryParseIfMatch_Parses_Single_And_Multiple_Strong_Tags()
    {
        var t1 = 10u; var t2 = 11u;
        var h1 = EtagUtils.ToEtag(t1);
        var h2 = EtagUtils.ToEtag(t2);

        EtagUtils.TryParseIfMatch(h1, out var tokens1, out var wc1).Should().BeTrue();
        wc1.Should().BeFalse();
        tokens1.Should().ContainSingle().Which.Should().Be(t1);

        var combined = $"{h1}, {h2}";
        EtagUtils.TryParseIfMatch(combined, out var tokens2, out var wc2).Should().BeTrue();
        wc2.Should().BeFalse();
        tokens2.Should().BeEquivalentTo([t1, t2]);
    }

    [Fact]
    public void TryParseIfMatch_Parses_Wildcard()
    {
        EtagUtils.TryParseIfMatch("*", out var tokens, out var wc).Should().BeTrue();
        wc.Should().BeTrue();
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void CheckIfMatch_Missing_And_Invalid()
    {
        EtagUtils.CheckIfMatch(null, 1).Should().Be(IfMatchCheckResult.MissingIfMatch);
        EtagUtils.CheckIfMatch(string.Empty, 1).Should().Be(IfMatchCheckResult.MissingIfMatch);

        // Invalid: garbage or only weak tag
        EtagUtils.CheckIfMatch("garbage", 1).Should().Be(IfMatchCheckResult.InvalidIfMatch);
        var weak = $"W/{EtagUtils.ToEtag(1)}";
        EtagUtils.CheckIfMatch(weak, 1).Should().Be(IfMatchCheckResult.InvalidIfMatch);
    }

    [Fact]
    public void CheckIfMatch_Mismatch_Ok_Wildcard()
    {
        var t = 123u;
        var other = EtagUtils.ToEtag(321);
        var same = EtagUtils.ToEtag(t);

        EtagUtils.CheckIfMatch(other, t).Should().Be(IfMatchCheckResult.EtagMismatch);
        EtagUtils.CheckIfMatch(same, t).Should().Be(IfMatchCheckResult.Ok);
        EtagUtils.CheckIfMatch("*", t).Should().Be(IfMatchCheckResult.Wildcard);
    }

    [Fact]
    public void CheckIfNoneMatch_Variants()
    {
        var t = 7u;
        EtagUtils.CheckIfNoneMatch(null, t).Should().Be(IfNoneMatchDecision.Proceed);
        EtagUtils.CheckIfNoneMatch(string.Empty, t).Should().Be(IfNoneMatchDecision.Proceed);

        // Invalid header
        EtagUtils.CheckIfNoneMatch("not-base64", t).Should().Be(IfNoneMatchDecision.InvalidIfNoneMatch);

        // Not modified for wildcard or matching tag
        EtagUtils.CheckIfNoneMatch("*", t).Should().Be(IfNoneMatchDecision.NotModified);
        var same = EtagUtils.ToEtag(t);
        EtagUtils.CheckIfNoneMatch(same, t).Should().Be(IfNoneMatchDecision.NotModified);

        // Proceed when none match
        var other = EtagUtils.ToEtag(8);
        EtagUtils.CheckIfNoneMatch(other, t).Should().Be(IfNoneMatchDecision.Proceed);
    }

    [Fact]
    public void TryParseIfMatch_Mixed_IgnoresWeak_IncludesStrong_And_Wildcard()
    {
        var weak = $"W/{EtagUtils.ToEtag(1)}";
        var s1 = EtagUtils.ToEtag(101);
        var s2 = EtagUtils.ToEtag(202);
        var header = $"{weak}, {s1}, *, {weak}, {s2}";

        EtagUtils.TryParseIfMatch(header, out var tokens, out var wildcard).Should().BeTrue();
        wildcard.Should().BeTrue();
        tokens.Should().BeEquivalentTo(MixedStrongTokens);
    }

    [Fact]
    public void CheckIfMatch_Mixed_WithWildcard_TakesWildcard()
    {
        var current = 999u;
        var header = $"*, {EtagUtils.ToEtag(1)}, W/{EtagUtils.ToEtag(current)}";
        EtagUtils.CheckIfMatch(header, current).Should().Be(IfMatchCheckResult.Wildcard);
    }

    [Fact]
    public void CheckIfMatch_Mixed_OnlyWeak_Invalid()
    {
        var header = $"W/{EtagUtils.ToEtag(1)}";
        // TryParseIfMatch returns true (header present), but CheckIfMatch should mark invalid because no strong tokens
        EtagUtils.TryParseIfMatch(header, out var tokens, out var wc).Should().BeTrue();
        wc.Should().BeFalse();
        tokens.Should().BeEmpty();

        EtagUtils.CheckIfMatch(header, 1).Should().Be(IfMatchCheckResult.InvalidIfMatch);
    }

    [Fact]
    public void CheckIfMatch_Mixed_WeakAndStrong_Matching_Ok()
    {
        var current = 55u;
        var header = $"W/{EtagUtils.ToEtag(123)}, {EtagUtils.ToEtag(current)}";
        EtagUtils.CheckIfMatch(header, current).Should().Be(IfMatchCheckResult.Ok);
    }

    [Fact]
    public void CheckIfNoneMatch_Mixed_WeakIgnored_StrongDifferent_Proceed()
    {
        var current = 77u;
        var header = $"W/{EtagUtils.ToEtag(current)}, {EtagUtils.ToEtag(88)}";
        // since strong 88 != current, Proceed; weak part is ignored by parser
        EtagUtils.CheckIfNoneMatch(header, current).Should().Be(IfNoneMatchDecision.Proceed);
    }
}
