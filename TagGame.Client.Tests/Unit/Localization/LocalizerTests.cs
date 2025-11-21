using System.Globalization;
using TagGame.Client.Core.Localization;

namespace TagGame.Client.Tests.Unit.Localization;

public class LocalizerTests
{
    private sealed class FakeCatalog : ILocalizationCatalog
    {
        private readonly Dictionary<(string Culture, string Key), string> _data = new();
        public void Seed(string culture, string key, string value) => _data[(culture, key)] = value;
        public bool TryGet(string key, CultureInfo info, out string? value)
        {
            return _data.TryGetValue((info.Name, key), out value);
        }
    }

    private static (ILocalizer loc, FakeCatalog cat) CreateLocalizer()
    {
        var cat = new FakeCatalog();
        var loc = new Localizer(cat);
        return (loc, cat);
    }

    [Fact]
    public async Task GetString_uses_resx_and_culture()
    {
        var (loc, cat) = CreateLocalizer();
        cat.Seed("de-DE", "App.Hello", "Hallo");
        cat.Seed("en-US", "App.Hello", "Hello");

        await loc.SetCultureAsync(CultureInfo.GetCultureInfo("de-DE"));
        loc.GetString("App.Hello").Should().Be("Hallo");

        await loc.SetCultureAsync(CultureInfo.GetCultureInfo("en-US"));
        loc.GetString("App.Hello").Should().Be("Hello");
    }

    [Fact]
    public void GetString_returns_key_in_brackets_when_missing()
    {
        var (loc, _) = CreateLocalizer();
        loc.GetString("Does.Not.Exist").Should().Be("[Does.Not.Exist]");
    }

    [Fact]
    public async Task GetFormat_formats_with_args_and_culture()
    {
        var (loc, cat) = CreateLocalizer();
        cat.Seed("en-US", "App.CountFmt", "{0:plural:one|#} item{0:plural:|s}");

        await loc.SetCultureAsync(CultureInfo.GetCultureInfo("en-US"));
        loc.GetFormat("App.CountFmt", 1).Should().Be("one item");
        loc.GetFormat("App.CountFmt", 3).Should().Be("# items");
    }
}
