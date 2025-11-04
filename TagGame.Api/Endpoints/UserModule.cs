using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Common.Http;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Endpoints;

public class UserModule : EndpointBase, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var users = app.MapV1()
            .MapGroup("users");

        users.MapPatch("/me", PatchUserAccount)
            .WithName("Users_UpdateOwnProfile")
            .Accepts<PatchUserAccountDto>(MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status409Conflict, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status428PreconditionRequired, MediaTypeNames.Application.Json)
            .RequireAuthorization()
            .WithOpenApi();
    }

    private static async Task<IResult> PatchUserAccount(
        [FromBody] PatchUserAccountDto request,
        [FromServices] IAuthUoW authUoW,
        [FromServices] IValidator<PatchUserAccountDto> validator,
        [FromServices] IHttpContextAccessor httpAccessor,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken ct = default)
    {
        var http = httpAccessor.HttpContext;
        await validator.ValidateAndThrowAsync(request, ct);

        if (!AuthUtils.TryGetUserId(http!, out var userId))
            return Unauthorized("Missing subject claim.", "auth.missing_sub");

        var user = await authUoW.Users.GetByIdAsync([userId], new()
        {
            AsNoTracking = false
        }, ct);
        if (user is null)
            return NotFound("User not found.", "user.not_found");

        var ccToken = await authUoW.Users.GetConcurrencyToken(user, ct);
        var result = EtagUtils.CheckIfMatch(ifMatch, ccToken);
        switch (result)
        {
            case IfMatchCheckResult.MissingIfMatch:
                return PreconditionRequired("if-match", "missing.if-match");
            case IfMatchCheckResult.InvalidIfMatch:
                return BadRequest("if-match", "invalid.if-match");
            case IfMatchCheckResult.EtagMismatch:
                http?.Response.SetEtag(ccToken);
                return PreconditionFailed("if-match", "mismatch.if-match");
            case IfMatchCheckResult.Ok:
            case IfMatchCheckResult.Wildcard:
            default:
                break;
        }

        if (!string.IsNullOrWhiteSpace(request.AvatarColor))
            user.AvatarColor = request.AvatarColor;
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;
        if (!string.IsNullOrWhiteSpace(request.DeviceId))
            user.DeviceId = request.DeviceId;
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        try
        {
            await authUoW.SaveChangesAsync(ct);
            var newCcToken = await authUoW.Users.GetConcurrencyToken(user, ct);
            http?.Response.SetEtag(newCcToken);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            var latest = await authUoW.Users.GetConcurrencyToken(user, ct);
            http?.Response.SetEtag(latest);
            return PreconditionFailed("if-match", "mismatch.if-match");
        }
        catch (DbUpdateException ex) when (IsUniqueViolationOnEmail(ex))
        {
            return Conflict("Email already in use.", "email.in_use");
        }
    }

    private static bool IsUniqueViolationOnEmail(DbUpdateException ex)
    {
        // Walk inner exceptions to find provider-specific details (Npgsql)
        Exception? current = ex;
        while (current is not null)
        {
            var type = current.GetType();
            if (string.Equals(type.FullName, "Npgsql.PostgresException", StringComparison.Ordinal))
            {
                // Use reflection to avoid a hard dependency on Npgsql in this project
                var sqlState = type.GetProperty("SqlState")?.GetValue(current) as string;
                if (string.Equals(sqlState, "23505", StringComparison.Ordinal)) // unique_violation
                {
                    var constraint = type.GetProperty("ConstraintName")?.GetValue(current) as string;
                    if (!string.IsNullOrWhiteSpace(constraint))
                    {
                        if (string.Equals(constraint, "IX_users_Email", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(constraint, "users_email_key", StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    // Fallback: inspect message for the Email column indicator
                    if (current.Message.Contains("(Email)", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            current = current.InnerException;
        }

        // Final fallback by message (provider-agnostic but brittle)
        var msg = ex.GetBaseException().Message;
        return msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
               && msg.Contains("(Email)", StringComparison.OrdinalIgnoreCase);
    }
}
