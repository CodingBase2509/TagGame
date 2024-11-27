global using AutoFixture;
global using FluentAssertions;
global using Moq;
global using Xunit;

namespace TagGameApi.Tests;

public class TestBase
{
    private protected Fixture _fixture;

    public TestBase()
    {
        _fixture = new Fixture();
        var customization = new SupportMutableValueTypesCustomization();
        _fixture.Customize(customization);
    }
}