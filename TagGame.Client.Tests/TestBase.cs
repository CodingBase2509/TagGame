global using AutoFixture;
global using FluentAssertions;
global using Moq;
global using Xunit;

namespace TagGame.Client.Tests;

public class TestBase
{
    private protected Fixture _fixture;
    
    public TestBase()
    {
        _fixture = new Fixture();
        _fixture.Customize(new SupportMutableValueTypesCustomization());
    }
}