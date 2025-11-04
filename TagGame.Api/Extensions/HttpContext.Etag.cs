using TagGame.Api.Core.Common.Http;

namespace TagGame.Api.Extensions;

public static class HttpContextEtagExtensions
{
    // Sets the current ETag header on the response.
    public static void SetEtag(this HttpResponse response, uint token) =>
        response.Headers.ETag = EtagUtils.ToEtag(token);
}
