using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

public class AuthorizationMessageHandler: Microsoft.AspNetCore.Components.WebAssembly.Authentication.AuthorizationMessageHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="AuthorizationMessageHandler"/>.
    /// </summary>
    /// <param name="provider">The <see cref="Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider"/> to use for provisioning tokens.</param>
    /// <param name="navigation">The <see cref="Microsoft.AspNetCore.Components.NavigationManager"/> to use for performing redirections.</param>
    public AuthorizationMessageHandler(global::Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider provider, global::Microsoft.AspNetCore.Components.NavigationManager navigation,AppSettings config) : base(provider, navigation)
    {
        ConfigureHandler(new[] { config.BaseAddress });
    }
}