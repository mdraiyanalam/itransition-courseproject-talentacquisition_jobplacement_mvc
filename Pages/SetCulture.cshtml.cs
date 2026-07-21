using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class SetCultureModel : PageModel
{
    public IActionResult OnGet(string culture, string returnUrl = "/")
    {
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, cookieValue, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            Path = "/"
        });
        return LocalRedirect(returnUrl);
    }
}