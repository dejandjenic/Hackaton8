using API.Configuration;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace API.Extensions;

public static class WebApplicationBuilderExtension
{
    public static void RegisterAzureVault(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsAzureAppService())
        {
            TokenCredential cred = builder.Environment.IsAzureAppService() ?
                new DefaultAzureCredential(false) : new AzureCliCredential();

            var keyvaultUri = new Uri($"https://belgrade.vault.azure.net/");
            var secretClient = new SecretClient(keyvaultUri, cred);
            builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    
        }
    }
    
    public static void AddAuthenticationAndAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
        builder.Services.AddAuthorization();
    }


    public static void AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(x => 
            x.AddDefaultPolicy(p=>
                p.AllowAnyHeader().AllowAnyMethod().
                    SetIsOriginAllowed(origin => true)
                    .AllowCredentials()
            )
        );
    }

    public static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void AddSignalR(this WebApplicationBuilder builder,AppSettings appSettings)
    {
        builder.Services.AddSignalR().AddAzureSignalR(options =>
        {
            options.ConnectionString = appSettings.SignalRConnectionString;
        });


        builder.Services
            .AddSingleton<SignalRService>()
            .AddHostedService(sp => sp.GetService<SignalRService>())
            .AddSingleton<IHubContextStore>(sp => sp.GetService<SignalRService>());

    }
}