using System.Web.Mvc;

namespace DBSportStore.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
                return false;

            return httpContext.Session["UserID"] != null &&
                   string.Equals(
                       System.Convert.ToString(httpContext.Session["Role"]),
                       "Admin",
                       System.StringComparison.OrdinalIgnoreCase);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            filterContext.Result = new HttpStatusCodeResult(403, "Bạn không có quyền truy cập.");
        }
    }
}