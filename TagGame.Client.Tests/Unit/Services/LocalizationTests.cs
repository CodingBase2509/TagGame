using System.Globalization;
using System.Reflection;
using System.Resources;
using TagGame.Client.Services;

namespace TagGame.Client.Tests.Unit.Services;

public class LocalizationTests : TestBase
{
    [Fact]
    public void Get_ShouldReturnLocalizedString_WhenKeyExists()
    {
        // Arrange
        const string key = "WelcomeMessage";
        const string pageName = "StartPage";
        const string expectedValue = "Welcome!";

        var loc = new Localization(typeof(TestBase).Assembly);
        var resourceManagerMock = new Mock<ResourceManager>("TagGame.Client.Resources.Localization.StartPage", typeof(Localization).Assembly);
        resourceManagerMock
            .Setup(rm => rm.GetString(key, CultureInfo.CurrentUICulture))
            .Returns(expectedValue);

        ReplaceResourceManager(loc, pageName, resourceManagerMock.Object);

        // Act
        var result = loc.Get(key, pageName);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void Get_ShouldReturnKeyInBrackets_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "InvalidKey";
        const string pageName = "StartPage";

        var loc = new Localization(typeof(TestBase).Assembly);
        var resourceManagerMock = new Mock<ResourceManager>("TagGame.Client.Resources.Localization.StartPage", typeof(Localization).Assembly);
        resourceManagerMock
            .Setup(rm => rm.GetString(key, CultureInfo.CurrentUICulture))
            .Returns((string?)null);

        ReplaceResourceManager(loc, pageName, resourceManagerMock.Object);

        // Act
        var result = loc.Get(key, pageName);

        // Assert
        result.Should().Be($"[{key}]");
    }

    [Fact]
    public void Get_ShouldReturnKeyInBrackets_WhenPageNameDoesNotExist()
    {
        // Arrange
        const string key = "AnyKey";
        const string pageName = "NonExistentPage";
        var loc = new Localization(typeof(TestBase).Assembly);

        // Act
        var result = loc.Get(key, pageName);

        // Assert
        result.Should().Be($"[{key}]");
    }

    private static void ReplaceResourceManager(Localization loc, string pageName, ResourceManager mockResourceManager)
    {
        var field = typeof(Localization)
            .GetField("_resourceManagers", BindingFlags.NonPublic | BindingFlags.Instance);

        if (field is null)
            return;

        var resourceManagers = (Dictionary<string, ResourceManager>)field.GetValue(loc)!;
        resourceManagers[pageName] = mockResourceManager;
    }
}
