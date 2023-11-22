using API.Configuration;
using Microsoft.Azure.SignalR.Management;

public class SignalRService(ILoggerFactory loggerFactory, AppSettings appSettings)
    : IHostedService, IHubContextStore
{
    private const string ChatHub = "ChatHub";
    private const string MessageHub = "AdminHub";

    public ServiceHubContext AdminHubContext { get; private set; }
    public ServiceHubContext ChatHubContext { get; private set; }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        using var serviceManager = new ServiceManagerBuilder()
            .WithOptions(o=>o.ConnectionString = appSettings.SignalRConnectionString)
            .WithLoggerFactory(loggerFactory)
            .BuildServiceManager();
        AdminHubContext = await serviceManager.CreateHubContextAsync(MessageHub, cancellationToken);
        ChatHubContext = await serviceManager.CreateHubContextAsync(ChatHub, cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(Dispose(AdminHubContext), Dispose(ChatHubContext));
    }

    private static Task Dispose(ServiceHubContext hubContext)
    {
        if (hubContext == null)
        {
            return Task.CompletedTask;
        }
        return hubContext.DisposeAsync();
    }
}