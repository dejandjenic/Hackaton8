namespace API.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsAzureAppService(this IWebHostEnvironment env)
    {
        var websiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        return string.IsNullOrEmpty(websiteName) is not true;
    }
}