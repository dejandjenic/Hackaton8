using Microsoft.Azure.SignalR.Management;

public interface IHubContextStore
{
    public ServiceHubContext MessageHubContext { get; }
    public ServiceHubContext ChatHubContext { get; }
}