using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Edge.UI;
using Edge.UI.ApiClients;
using Edge.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

builder.Services.AddSingleton(appSettings);


builder.Services.AddSingleton<ConnectionService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddHttpClient<BackendApiClient>(x =>
{
    x.BaseAddress = new Uri(appSettings.BaseAddress);
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();