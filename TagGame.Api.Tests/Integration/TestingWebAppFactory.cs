using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TagGame.Api.Tests.Integration;

public sealed class TestingWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TagGame",
                ["Jwt:Audience"] = "TagGameClient",
                ["Jwt:SigningKey"] = "it-test-signing-key-1234567890"
            };
            cfg.AddInMemoryCollection(dict);
        });
    }
}

