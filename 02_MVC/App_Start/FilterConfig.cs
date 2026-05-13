using System.Web;
using System.Web.Mvc;
using _02_MVC.Filters;

namespace _02_MVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Filtro_VERIFICA_SESSION());
        }
    }
}
