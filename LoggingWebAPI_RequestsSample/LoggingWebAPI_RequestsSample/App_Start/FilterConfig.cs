using System.Web;
using System.Web.Mvc;

namespace LoggingWebAPI_RequestsSample
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
