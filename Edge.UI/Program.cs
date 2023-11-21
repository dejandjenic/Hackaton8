using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Edge.UI;
using Edge.UI.ApiClients;
using Edge.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<ConnectionService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddHttpClient<BackendApiClient>(x =>
{
    x.BaseAddress = new Uri("http://localhost:5250/");
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();