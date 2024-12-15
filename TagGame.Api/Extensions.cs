using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TagGame.Shared.Constants;
using TagGame.Shared.Domain.Common;

namespace TagGame.Api;

public static class Extensions
{
    public static IResult ToHttpResult<TObject>(this TObject obj) where TObject : class
    {
        if (obj is Error error)
            return error.Code switch
            {
                400 => Results.BadRequest((Response<TObject>)error),
                404 => Results.NotFound((Response<TObject>)error),
                500 => Results.Content(
                    JsonSerializer.Serialize((Response<TObject>)error, MappingOptions.JsonSerializerOptions),
                    MediaTypeNames.Application.Json,
                    Encoding.UTF8,
                    StatusCodes.Status500InternalServerError),
                _ => Results.NoContent()
            }; ;
        
        if (obj is Response<object> response)
            return Results.Ok(response);
            
        return Results.Ok((Response<TObject>)obj);
    }
}