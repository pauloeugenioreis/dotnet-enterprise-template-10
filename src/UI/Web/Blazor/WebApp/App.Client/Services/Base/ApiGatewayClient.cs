namespace BlazorApp.Client.Services.Base;

public class ApiGatewayClient(HttpClient client)
{
    public HttpClient HttpClient { get; } = client;
}
