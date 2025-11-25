using TagGame.Api.Core.Abstractions.Auth;
using TagGame.Api.Core.Common.Exceptions;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Api.Endpoints;

public sealed class AuthModule : EndpointBase, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var auth = app
            .MapV1()
            .MapGroup("/auth");

        auth.MapPost("/initial", HandleInitialAsync)
            .Accepts<InitialRequestDto>(MediaTypeNames.Application.Json)
            .Produces<InitialResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .WithName("Auth_Initial")
            .IncludeInOpenApi();

        auth.MapPost("/login", HandleLoginAsync)
            .Accepts<LoginRequestDto>(MediaTypeNames.Application.Json)
            .Produces<LoginResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .WithName("Auth_Login")
            .IncludeInOpenApi();

        auth.MapPost("/refresh", HandleRefreshAsync)
            .Accepts<RefreshRequestDto>(MediaTypeNames.Application.Json)
            .Produces<RefreshResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .WithName("Auth_Refresh")
            .IncludeInOpenApi();

        auth.MapPost("/logout", HandleLogoutAsync)
            .Accepts<LogoutRequestDto>(MediaTypeNames.Application.Json)
            .Produces<LogoutResponseDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)
            .WithName("Auth_Logout")
            .IncludeInOpenApi();
    }

    private static async Task<IResult> HandleInitialAsync(
        [FromBody] InitialRequestDto request,
        [FromServices] IValidator<InitialRequestDto> validator,
        [FromServices] IAuthUoW authUoW,
        [FromServices] IAuthTokenService tokenService,
        [FromServices] TimeProvider clock,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var existing = await authUoW.Users.FirstOrDefaultAsync(
            u => Equals(u.DeviceId, request.DeviceId),
            new QueryOptions<User>
            {
                AsNoTracking = true,
            },
            ct);

        if (existing is not null)
            return Conflict("Errors.User.ExistsForDevice", code: "user_exists");

        var now = clock.GetUtcNow();
        var user = new User()
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            AvatarColor = request.AvatarColor,
            DisplayName = request.DisplayName,
            CreatedAt = now,
            Flags = 0,
        };

        await authUoW.Users.AddAsync(user, ct);
        await authUoW.SaveChangesAsync(ct);

        var tokens = await tokenService.IssueTokenAsync(user, ct);

        return Ok(new InitialResponseDto
        {
            UserId = user.Id,
            Tokens = tokens
        });
    }

    private static async Task<IResult> HandleLoginAsync(
        [FromBody] LoginRequestDto request,
        [FromServices] IAuthUoW authUoW,
        [FromServices] IAuthTokenService tokenService,
        [FromServices] TimeProvider clock,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
            return BadRequest("Errors.Auth.DeviceIdMissing", code: "device_id_missing");

        var user = await authUoW.Users.FirstOrDefaultAsync(
            u => Equals(u.DeviceId, request.DeviceId),
            new QueryOptions<User>
            {
                AsNoTracking = true,
            },
            ct);

        if (user is null)
            return NotFound("Errors.User.NotFoundForDevice", "user_not_found");

        var tokens = await tokenService.IssueTokenAsync(user, ct);

        return Ok(new LoginResponseDto
        {
            UserId = user.Id,
            Tokens = tokens
        });
    }

    private static async Task<IResult> HandleRefreshAsync(
        [FromBody] RefreshRequestDto request,
        [FromServices] IAuthTokenService tokenService,
        [FromServices] ILogger<AuthModule> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Errors.Auth.RefreshTokenMissing", code: "refresh_token_missing");

        try
        {
            var tokens = await tokenService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(new RefreshResponseDto { Tokens = tokens });
        }
        catch (RefreshTokenReuseException)
        {
            return Unauthorized("Errors.Auth.RefreshReuse", code: "refresh_reuse");
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Errors.Auth.RefreshInvalid", code: "refresh_invalid");
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("expired", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Errors.Auth.RefreshExpired", code: "refresh_expired");
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("revoked", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("Errors.Auth.RefreshRevoked", code: "refresh_revoked");
        }
    }

    private static async Task<IResult> HandleLogoutAsync(
        [FromBody] LogoutRequestDto request,
        [FromServices] IAuthTokenService tokenService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Errors.Auth.RefreshTokenMissing", code: "refresh_token_missing");

        await tokenService.RevokeTokenAsync(request.RefreshToken, ct);

        return Ok(new LogoutResponseDto { Revoked = true });
    }
}
