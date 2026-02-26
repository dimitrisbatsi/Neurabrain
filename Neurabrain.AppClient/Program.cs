using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Neurabrain.AppClient;
using Neurabrain.AppClient.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 1. Ρυθμίζουμε τον Http Client να περνάει από το CookieHandler
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddHttpClient("API", client =>
    client.BaseAddress = new Uri("https://localhost:7089/"))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// 2. Ενεργοποιούμε το σύστημα Authorization του Blazor
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
