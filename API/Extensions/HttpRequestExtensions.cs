namespace API.Extensions;

public static class HttpRequestExtensions
{
    public static string EnsureUserCookie(this HttpRequest request, HttpResponse response)
    {
        var cookie = request.Cookies["id"];
        if (cookie == null)
        {
            cookie = Guid.NewGuid().ToString();
            response.Cookies.Append("id",cookie,new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(10),
                Path = "/",
                Secure = true,
                HttpOnly = true,
            });
        }

        return cookie;
    }
}