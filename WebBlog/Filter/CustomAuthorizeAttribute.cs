using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebBlog.Filter
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        // Custom Authentication without using ASP.NET Core Identity
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Redirect to login page if user is not authenticated
                context.Result = new RedirectToActionResult("SignIn", "Account", null);
                return;
            }

            if (!context.HttpContext.User.IsInRole(Roles))
            {
                // Redirect to access denied page if user is not authorized
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }
    }
}
