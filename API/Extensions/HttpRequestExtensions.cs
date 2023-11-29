namespace API.Extensions;

public static class HttpRequestExtensions
{
    public static (string,bool) EnsureUserCookie(this HttpRequest request, HttpResponse response)
    {
        var cookie = request.Cookies["id"];
        bool isNew = false;
        if (cookie == null)
        {
            cookie = Guid.NewGuid().ToString();
            isNew = true;
            response.Cookies.Append("id",cookie,new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(10),
                Path = "/",
                Secure = true,
                HttpOnly = true,
            });
        }

        return (cookie,isNew);
    }
}