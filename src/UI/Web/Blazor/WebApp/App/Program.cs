using BlazorApp.Client;
using BlazorApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Usar o método de extensão compartilhado
// No servidor, podemos passar a URL da API do Aspire
var apiUrl = builder.Configuration["SERVICES:API:HTTPS:0"] ?? "https://localhost:7196";
builder.Services.AddClientServices(apiUrl);

// Serviços adicionais específicos do servidor (se houver)

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp.Client.DependencyInjection).Assembly);

app.Run();
