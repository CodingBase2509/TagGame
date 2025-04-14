using System.Text.Json;
using TagGame.Shared.Domain.Common;

namespace TagGame.Api.Middleware;

public class ExceptionHandlerMiddleware : IMiddleware
{
    private const string exceptionResponse = "Message:{0}\r\nStackTrace:{1}"; 
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            context.Response.StatusCode = 500;
            var response = new Response<Error>()
            {
                Value = null,
                IsSuccess = false,
                Error = new Error()
                {
                    Code = 500,
                    Message = string.Format(exceptionResponse, e.Message, e.StackTrace),
                }
            };
            var jsonResponse = JsonSerializer.Serialize<Response<Error>>(response);
            
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}