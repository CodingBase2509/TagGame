using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services.Abstractions;

public interface IHttpResilienceConfigurator
{
    void Configure(IHttpClientBuilder builder, NetworkResilienceOptions.HttpOptions httpOptions);
}
