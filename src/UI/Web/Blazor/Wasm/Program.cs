using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<BlazorWasm.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Usar o método de extensão compartilhado
var apiUrl = "https://localhost:7196";
builder.Services.AddClientServices(apiUrl);

await builder.Build().RunAsync();
