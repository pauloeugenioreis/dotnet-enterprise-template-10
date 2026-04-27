using MudBlazor;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Client.Services.Handlers;

public class ErrorHandler(IServiceProvider serviceProvider) : DelegatingHandler
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
        try 
        {
            // No servidor durante o pre-rendering, o ISnackbar pode falhar ou não ser desejado
            var snackbar = serviceProvider.GetService<ISnackbar>();
            if (snackbar == null) return;
            
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
        catch 
        {
            // Ignora falhas de UI durante pre-rendering ou em estados instáveis
        }
    }
}
