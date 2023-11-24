using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Admin.UI;
using Admin.UI.ApiClients;
using Admin.UI.Services;
using Blazored.Toast;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(appSettings);
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddBlazoredToast();
builder.Services.AddScoped<SettingsService>();

builder.Services.AddSingleton<ConnectionService>();
builder.Services.AddSingleton<ChatService>();


builder.Services.AddHttpClient<BackendApiClient>(client =>
    {
        client.BaseAddress = new Uri(appSettings.BaseAddress);
    })
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://belgradehackaton.onmicrosoft.com/bc429d0c-0aa0-4e60-9e33-4007a15d029a/API.Access");
});

await builder.Build().RunAsync();