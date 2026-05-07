using System.Web;
using _02_MVC.Models;

namespace _02_MVC.Helpers
{
    public static class SessionHelper
    {
        public static ApplicationUser CurrentUser
        {
            get { return HttpContext.Current?.Session?["User"] as ApplicationUser; }
        }

        public static string CurrentUserId
        {
            get { return CurrentUser?.Id ?? ""; }
        }

        public static string CurrentUserName
        {
            get { return CurrentUser?.UserName ?? ""; }
        }

        public static string CurrentEmail
        {
            get { return CurrentUser?.Email ?? ""; }
        }
    }
}
