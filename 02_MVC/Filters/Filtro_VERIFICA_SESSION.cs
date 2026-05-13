using _02_MVC.Controllers;
using _02_MVC.Models;
using System.Web;
using System.Web.Mvc;

namespace _02_MVC.Filters
{
    public class Filtro_VERIFICA_SESSION : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var objUser = (ApplicationUser)HttpContext.Current.Session["User"];

            if (objUser == null)
            {
                if (filterContext.Controller is AccountController == false)
                {
                    filterContext.HttpContext.Response.RedirectToRoutePermanent("Default",
                        new { controller = "Account", action = "Login" });
                }
            }
            else
            {
                if (filterContext.Controller is AccountController == true)
                {
                    filterContext.HttpContext.Response.RedirectToRoutePermanent("Default",
                        new { controller = "Home", action = "Index" });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
