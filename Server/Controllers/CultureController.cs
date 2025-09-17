using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Vanigam.CRM.Server.Controllers
{
    [Route("Culture/[action]")]
    public partial class CultureController : Controller
    {
        public IActionResult SetCulture(string culture, string redirectUri)
        {
            try
            {
                if (culture != null)
                {
                    Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)));
                }
                return LocalRedirect(redirectUri);
            }
            catch(Exception ex)
            {
                Log.Error(ex, ex.Message);
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}

