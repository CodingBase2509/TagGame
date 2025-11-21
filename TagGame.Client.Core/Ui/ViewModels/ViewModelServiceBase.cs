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
            var toast = SpUtils.GetRequiredService<IToastPublisher>();

            // Prefer explicit detail key
            var key = problemResponse.Problem?.Detail;
            if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("Errors.", StringComparison.Ordinal))
            {
                await toast.Error(key!); // localized key
            }
            // Next, look for validation errors and use the first key
            else if (problemResponse.Problem?.Errors is { Count: > 0 })
            {
                var first = problemResponse.Problem.Errors.Values.FirstOrDefault(v => v?.Length > 0);
                var candidate = first?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(candidate) && candidate!.StartsWith("Errors.", StringComparison.Ordinal))
                {
                    await toast.Error(candidate!);
                }
                else
                {
                    await toast.Error("Errors.Http.Generic");
                }
            }
            else
            {
                await toast.Error("Errors.Http.Generic");
            }
        }
        catch (HttpRequestException)
        {
            var toast = SpUtils.GetRequiredService<IToastPublisher>();
            await toast.Error("Errors.Http.Network");
        }
        catch (ArgumentException)
        {
            var toast = SpUtils.GetRequiredService<IToastPublisher>();
            await toast.Error("Errors.Validation.InvalidInput");
        }
        catch (TaskCanceledException)
        {
            var toast = SpUtils.GetRequiredService<IToastPublisher>();
            await toast.Error("Errors.Http.Canceled");
        }

        return default;
    }
}
