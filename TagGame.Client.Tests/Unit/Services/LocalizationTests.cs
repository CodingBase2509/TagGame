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

        var resourceManagerMock = new Mock<ResourceManager>("TagGame.Client.Resources.Localization.StartPage", typeof(Localization).Assembly);
        resourceManagerMock
            .Setup(rm => rm.GetString(key, CultureInfo.CurrentUICulture))
            .Returns(expectedValue);

        ReplaceResourceManager(pageName, resourceManagerMock.Object);

        // Act
        var result = Localization.Get(key, pageName);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void Get_ShouldReturnKeyInBrackets_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "InvalidKey";
        const string pageName = "StartPage";

        var resourceManagerMock = new Mock<ResourceManager>("TagGame.Client.Resources.Localization.StartPage", typeof(Localization).Assembly);
        resourceManagerMock
            .Setup(rm => rm.GetString(key, CultureInfo.CurrentUICulture))
            .Returns((string?)null);

        ReplaceResourceManager(pageName, resourceManagerMock.Object);

        // Act
        var result = Localization.Get(key, pageName);

        // Assert
        result.Should().Be($"[{key}]");
    }

    [Fact]
    public void Get_ShouldReturnKeyInBrackets_WhenPageNameDoesNotExist()
    {
        // Arrange
        const string key = "AnyKey";
        const string pageName = "NonExistentPage";

        // Act
        var result = Localization.Get(key, pageName);

        // Assert
        result.Should().Be($"[{key}]");
    }

    private static void ReplaceResourceManager(string pageName, ResourceManager mockResourceManager)
    {
        var field = typeof(Localization)
            .GetField("_resourceManagers", BindingFlags.NonPublic | BindingFlags.Static);

        if (field is null) 
            return;
        
        var resourceManagers = (Dictionary<string, ResourceManager>)field.GetValue(null)!;
        resourceManagers[pageName] = mockResourceManager;
    }
}
