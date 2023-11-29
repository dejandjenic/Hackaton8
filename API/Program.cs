using System.Security.Claims;
using API.Configuration;
using API.Extensions;
using API.Hubs;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Shared;

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
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IAISearchService, AISearchService>();
builder.Services.AddSingleton<ITextService, TextService>();
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


app.MapGet("/login", async ([FromServices] IHubContextStore store,[FromServices] ICosmosDbService database, HttpRequest request, HttpResponse response) =>
{
	var (cookie,isNew) = request.EnsureUserCookie(response);
	if (isNew)
	{
		await database.CreateUser(cookie);
	}
	return await store.ChatHubContext.NegotiateAsync(new NegotiationOptions()
	{
		Claims = new List<Claim>()
		{
			new (ClaimTypes.NameIdentifier, cookie)
		}
	});
});
app.MapGet("/login-admin", async ([FromServices] IHubContextStore store) => await store.AdminHubContext.NegotiateAsync()).RequireAuthorization();
app.MapGet("/chat-history/{userId}", async ([FromRoute] string userId, [FromServices] IChatService service) => await service.GetHistory(userId)).RequireAuthorization();
app.MapGet("/chat-history", async ([FromServices] IChatService service, HttpRequest request, HttpResponse response) =>
{
	var (userId,_) = request.EnsureUserCookie(response);
	return await service.GetHistory(userId);
});

app.MapPost("/pages", async ([AsParameters][FromBody]KnowledgeBasePage page,[FromServices]ISettingsService database) =>
{
	await database.AddPage(page.Id, page.Name,page.Content);
}).RequireAuthorization();

app.MapPatch("/pages/{id}", async ([FromRoute]string id,[AsParameters][FromBody]KnowledgeBasePage page,[FromServices]ISettingsService database) =>
{
	await database.UpdatePage(id, page.Name,page.Content);
}).RequireAuthorization();

app.MapDelete("/pages/{id}", async ([FromRoute]string id,[FromServices]ISettingsService database) =>
{
	await database.DeletePage(id);
}).RequireAuthorization();

app.MapGet("/pages", async ([FromServices]ISettingsService database) =>
{
	return await database.GetPages();
}).RequireAuthorization();

app.UseAzureSignalR(routes =>
{
	routes.MapHub<ChatHub>("/chat");
});

app.UseAzureSignalR(routes =>
{
	routes.MapHub<AdminHub>("/admin");
});

app.Run();

