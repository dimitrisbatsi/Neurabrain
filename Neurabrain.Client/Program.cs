using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Neurabrain.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Εδώ βάζουμε το URL του Neurabrain.API (άλλαξέ το με το δικό σου port)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7089/") });

await builder.Build().RunAsync();