using BlazorApp.Client;
using BlazorApp.Components;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; });
builder.Services.AddAuthorization();

// Usar o método de extensão compartilhado
// No servidor, podemos passar a URL da API (Aspire ou Docker)
var apiUrl = builder.Configuration["ApiBaseUrl"] 
             ?? builder.Configuration["AppSettings:ApiBaseUrl"]
             ?? builder.Configuration["SERVICES:API:HTTPS:0"] 
             ?? "https://localhost:7196";
builder.Services.AddClientServices(apiUrl);

// Serviços adicionais específicos do servidor (se houver)

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp.Client.DependencyInjection).Assembly)
    .AllowAnonymous();

app.Run();
