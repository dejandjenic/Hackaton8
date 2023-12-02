using Edge.UI.ApiClients;
using Microsoft.AspNetCore.SignalR.Client;

namespace Edge.UI.Services;

public class ConnectionService(BackendApiClient apiClient)
{
    HubConnection connection;

    public async Task Send(string method, object? arg1, object? arg2)
    {
        await connection.InvokeAsync(method, arg1, arg2);
    }

    public async Task Send(string method, object? arg1)
    {
        await connection.InvokeAsync(method, arg1);
    }

    public IDisposable Bind<T>(string name, Action<T> handler)
    {
        return connection.On(name, handler);
    }

    public IDisposable Bind<T, K>(string name, Action<T, K> handler)
    {
        return connection.On(name, handler);
    }

    public async Task Connect()
    {
        var response = await apiClient.Login();

        connection = new HubConnectionBuilder()
            .WithUrl(response.Url + "&username=dejan", x => { x.AccessTokenProvider = async () => response.AccessToken; })
            .WithAutomaticReconnect()
            .Build();

        connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await connection.StartAsync();
        };


        await connection.StartAsync();
    }
}