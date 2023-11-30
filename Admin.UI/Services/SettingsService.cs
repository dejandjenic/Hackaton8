using Admin.UI.ApiClients;
using Shared;

namespace Admin.UI.Services;

public class SettingsService(BackendApiClient apiClient)
{
    public async Task SaveBotSettings(ChatSettings settings)
    {
        await apiClient.SaveSettings(settings);
    }

    public async Task<ChatSettings> GetBotSettings()
    {
        return await apiClient.GetSettings();
    }
}