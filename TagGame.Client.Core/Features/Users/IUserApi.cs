using TagGame.Shared.DTOs.Users;

namespace TagGame.Client.Core.Features.Users;

public interface IUserApi
{
    Task<UserProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);
}
