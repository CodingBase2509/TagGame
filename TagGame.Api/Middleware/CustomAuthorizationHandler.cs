using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Persistence;
using TagGame.Api.Services;
using TagGame.Shared.Domain.Players;

namespace TagGame.Api.Middleware;

public class CustomAuthorizationHandler(IServiceProvider services) : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        var httpContext = (context.Resource as DefaultHttpContext)?.HttpContext;
        if (httpContext is null)
        {
            context.Fail();
            return;
        }

        const string authorizationHeader = "Authorization";
        const string authType = "Basic ";
        string? idString = null;
        
        if (!httpContext.Request.Headers.TryGetValue(authorizationHeader, out var authHeader))
        {
            context.Fail();
            return;
        }
        
        var authHeaderValue = authHeader.ToString();
        if (authHeaderValue.StartsWith(authType, StringComparison.OrdinalIgnoreCase))
        {
            var encodedValue = authHeaderValue.Substring(authType.Length).Trim();
            idString = DecodeBase64(encodedValue);
        }

        if (!Guid.TryParse(idString, out Guid userId) || !await IsUserValidAsync(userId))
        {
            context.Fail();
            return;
        }
        
        httpContext.Items["UserId"] = idString;
        context.Succeed(requirement);
    }

    private string? DecodeBase64(string base64String)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> IsUserValidAsync(Guid userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDataAccess>();
        
        var user = await db.Users.GetByIdAsync(userId);
        return user is not null;
    }
}