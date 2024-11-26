namespace TagGame.Api.Middleware;

public class ExceptionHandlerMiddleware : IMiddleware
{
    const string exceptionResponse = "Message:{0}\r\nStackTrace:{1}"; 
    
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
            context.Response.WriteAsync(string.Format(exceptionResponse, e.Message, e.StackTrace));
        }
    }
}