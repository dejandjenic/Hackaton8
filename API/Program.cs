using System.Security.Claims;
using API.Configuration;
using API.Extensions;
using API.Hubs;
using API.Repositories;
using API.Services;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);



if (builder.Environment.IsAzureAppService())
{
    TokenCredential cred = builder.Environment.IsAzureAppService() ?
        new DefaultAzureCredential(false) : new AzureCliCredential();

    var keyvaultUri = new Uri($"https://belgrade.vault.azure.net/");
    var secretClient = new SecretClient(keyvaultUri, cred);
    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    
}

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IGPTService,GPTService>();
builder.Services.AddSingleton<IChatRepository,ChatRepository>();
builder.Services.AddSingleton<IChatService,ChatService>();
builder.Services.AddSingleton<AdminHubManager>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddAuthorization();

builder.Services.AddCors(x => 
    x.AddDefaultPolicy(p=>
        p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
    )
);

builder.Services.AddSignalR().AddAzureSignalR(options =>
{
    options.ConnectionString = appSettings.SignalRConnectionString;
    //  This is a tircky way to associate user name with connection for sample purpose.
    //  For PROD, we suggest to use authentication and authorization, see here:
    //  https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz
    options.ClaimsProvider = context => new Claim[]
    {
        //new Claim(ClaimTypes.NameIdentifier, context.User?.Identity.IsAuthenticated.ToString())
        new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddSingleton<SignalRService>()
    .AddHostedService(sp => sp.GetService<SignalRService>())
    .AddSingleton<IHubContextStore>(sp => sp.GetService<SignalRService>());

// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
// });

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/login", async ([FromServices]IHubContextStore store,HttpRequest request,HttpResponse response) =>
{
    var cookie = request.Cookies["id"];
    if (cookie == null)
    {
        cookie = Guid.NewGuid().ToString();
        response.Cookies.Append("id",cookie,new CookieOptions()
        {
            Expires = DateTimeOffset.UtcNow.AddYears(10),
            Path = "/",
            Secure = false, // Use "false" if not using HTTPS
            HttpOnly = true,
        });
    }
    return await store.ChatHubContext.NegotiateAsync(new NegotiationOptions()
    {
        Claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, cookie)
        }
    });
});
app.MapGet("/login-admin", async ([FromServices]IHubContextStore store) => await store.AdminHubContext.NegotiateAsync()).RequireAuthorization();
app.MapGet("/test" ,()=> Guid.NewGuid()).RequireAuthorization();

app.UseAzureSignalR(routes =>
{
    routes.MapHub<ChatHub>("/chat");
});

app.UseAzureSignalR(routes =>
{
    routes.MapHub<AdminHub>("/admin");
});

app.Run();


//  [JsonSerializable(typeof(Test[]))]
// internal partial class AppJsonSerializerContext : JsonSerializerContext
// {
// }
