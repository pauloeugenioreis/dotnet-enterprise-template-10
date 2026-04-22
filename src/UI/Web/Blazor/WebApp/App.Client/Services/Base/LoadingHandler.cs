
namespace BlazorApp.Client.Services.Base;

public class LoadingHandler(LoadingService loadingService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            loadingService.StartLoading();
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            loadingService.StopLoading();
        }
    }
}
