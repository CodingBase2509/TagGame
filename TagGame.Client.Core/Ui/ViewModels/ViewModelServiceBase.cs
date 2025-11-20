using TagGame.Client.Core.Http;
using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Ui.ViewModels;

public abstract class ViewModelServiceBase
{
    protected async Task<TResult?> RequestSafeHandle<TResult>(Func<Task<TResult>> request)
    {
        try
        {
            return await request();
        }
        catch (ApiProblemException problemResponse)
        {
            var loc = SpUtils.GetRequiredService<ILocalizer>();
            var toast = SpUtils.GetRequiredService<IToastPublisher>();

            var errorMessage = loc.GetFormat("http.error",
                problemResponse.Problem?.Title ?? "Unknown error",
                problemResponse.Problem?.Detail ?? string.Empty);

            await toast.Error(errorMessage, false);
        }
        catch (HttpRequestException httpError)
        {
            var loc = SpUtils.GetRequiredService<ILocalizer>();
            var toast = SpUtils.GetRequiredService<IToastPublisher>();

            var errorMessage = loc.GetFormat("http.error",
                httpError.StatusCode.ToString() ?? "Unknown error",
                httpError.Message);

            await toast.Error(errorMessage, false);
        }
        catch (TaskCanceledException)
        {
        }

        return default;
    }
}
