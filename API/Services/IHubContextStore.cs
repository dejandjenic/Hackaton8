using Microsoft.Azure.SignalR.Management;

public interface IHubContextStore
{
    public ServiceHubContext AdminHubContext { get; }
    public ServiceHubContext ChatHubContext { get; }
}