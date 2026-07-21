using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace talentacquisition_jobplacement_mvc.Pages
{
    public class SetCultureModel : PageModel
    {
        public IActionResult OnGet(string culture, string returnUrl = "/")
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            };

            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, cookieValue, cookieOptions);

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}