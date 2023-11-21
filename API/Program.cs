using System.Security.Claims;
using API.Configuration;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings);

builder.Services.AddSingleton<IGPTService,GPTService>();

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
        //new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
    };
});

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

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/login", async ([FromServices]IHubContextStore store) => await store.ChatHubContext.NegotiateAsync());
app.MapGet("/test" ,()=> Guid.NewGuid()).RequireAuthorization();

app.UseAzureSignalR(routes =>
{
    routes.MapHub<Chat>("/chat");
});

app.Run();


//  [JsonSerializable(typeof(Test[]))]
// internal partial class AppJsonSerializerContext : JsonSerializerContext
// {
// }
