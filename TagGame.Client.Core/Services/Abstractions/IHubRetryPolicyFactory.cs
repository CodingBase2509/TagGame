using Microsoft.AspNetCore.SignalR.Client;
using TagGame.Client.Core.Options;

namespace TagGame.Client.Core.Services.Abstractions;

public interface IHubRetryPolicyFactory
{
    IRetryPolicy Create(NetworkResilienceOptions.HubOptions hubOptions);
}
