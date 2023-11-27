using System.Security.Claims;
using API.Configuration;
using API.Extensions;
using API.Hubs;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.RegisterAzureVault();

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings);

builder.AddSwagger();

builder.Services.AddSingleton<IGPTService, GPTService>();
builder.Services.AddSingleton<IChatRepository, ChatRepository>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IAdminHubManager, AdminHubManager>();
builder.Services.AddSingleton<IUserHubManager, UserHubManager>();
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddHttpContextAccessor();

builder.AddAuthenticationAndAuthorization();
builder.AddCors();
builder.AddSignalR(appSettings);

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/login", async ([FromServices] IHubContextStore store, HttpRequest request, HttpResponse response) =>
{
    var cookie = request.EnsureUserCookie(response);
    return await store.ChatHubContext.NegotiateAsync(new NegotiationOptions()
    {
        Claims = new List<Claim>()
        {
            new (ClaimTypes.NameIdentifier, cookie)
        }
    });
});
app.MapGet("/login-admin", async ([FromServices] IHubContextStore store) => await store.AdminHubContext.NegotiateAsync()).RequireAuthorization();

app.MapGet("/new-session/{userId}", async (string userId, [FromServices] IChatService chatService, HttpRequest request) => await chatService.CreateNewChatSessionAsync(userId));
app.MapGet("/chat-history/{userId}", async (string userId, [FromServices] IChatService chatService) => await chatService.GetAllChatSessionsAsync(userId));
app.MapGet("/chat-history-session/{sessionId}", async (string sessionId, [FromServices] IChatService chatService) => await chatService.GetChatSessionMessagesAsync(sessionId));
app.MapGet("/chat-title/{sessionId}", async ([FromRoute] string sessionId, [FromQuery(Name = "text")] string conversationText, [FromServices] IChatService chatService) => new {Text = await chatService.SummarizeChatSessionNameAsync(sessionId, conversationText) });

app.UseAzureSignalR(routes =>
{
    routes.MapHub<ChatHub>("/chat");
});

app.UseAzureSignalR(routes =>
{
    routes.MapHub<AdminHub>("/admin");
});

app.Run();

