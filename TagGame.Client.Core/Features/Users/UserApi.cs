using TagGame.Client.Core.Http;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Client.Core.Features.Users;

public class UserApi(IApiClient api) : ApiImplementationBase, IUserApi
{
    public async Task<UserProfileDto> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        await EnsureValidToken(cancellationToken);
        var response = await api.GetAsync<UserProfileDto>("v1/users/me", cancellationToken);

        return response ?? throw new InvalidOperationException("User API returned no content for profile.");
    }
}
