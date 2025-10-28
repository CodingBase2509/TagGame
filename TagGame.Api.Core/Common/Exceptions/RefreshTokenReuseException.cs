namespace TagGame.Api.Core.Common.Exceptions;

public class RefreshTokenReuseException(Guid userId, Guid tokenId, Guid familyId) : Exception
{
    public Guid UserId { get; init; } = userId;

    public Guid TokenId { get; init; } = tokenId;

    public Guid FamilyId { get; init; } = familyId;
}
