namespace API.Hubs;

public interface IAdminHub
{
	Task ChatEvent(object data);
	Task NewChatEvent(object data);
	Task NeedAssistant(object data);
}