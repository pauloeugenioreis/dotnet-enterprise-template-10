using MudBlazor;
using System.Net;

namespace BlazorApp.Client.Services.Handlers;

public class ErrorHandler(ISnackbar snackbar) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response);
        }

        return response;
    }

    private async Task HandleErrorAsync(HttpResponseMessage response)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                snackbar.Add("Sessão expirada. Por favor, faça login novamente.", Severity.Error);
                break;
            case HttpStatusCode.Forbidden:
                snackbar.Add("Você não tem permissão para realizar esta ação.", Severity.Warning);
                break;
            case HttpStatusCode.BadRequest:
                var message = await response.Content.ReadAsStringAsync();
                snackbar.Add($"Dados inválidos: {message}", Severity.Warning);
                break;
            case HttpStatusCode.InternalServerError:
                snackbar.Add("Ocorreu uma falha interna no servidor. Tente novamente mais tarde.", Severity.Error);
                break;
            default:
                snackbar.Add("Ocorreu um erro inesperado na comunicação com o servidor.", Severity.Error);
                break;
        }
    }
}
